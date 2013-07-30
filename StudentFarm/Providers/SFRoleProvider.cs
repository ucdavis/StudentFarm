using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;

namespace StudentFarm.Providers
{
    public struct RoleInfo
    {
        private readonly String _idType;
        private readonly String _table;
        private readonly int _id;

        public String IdType { get { return _idType; } }
        public String Table { get { return _table; } }
        public int Id { get { return _id; } }

        public RoleInfo(String IdType, String Table, int Id)
        {
            this._idType = IdType;
            this._table = Table;
            this._id = Id;
        }
    }

    // Should really write unit tests for this thing. Maybe Microsoft has some?
    public class SFRoleProvider : RoleProvider
    {
        public override string ApplicationName { get; set; }
        private string ConnectionString { get; set; }
        private string ConnectionStringKey { get; set; }
        private SqlConnection Connection;

        /*
         * Copy-Pasted from UCDArch.Web.Providers.CatbertRoleProvider
         */
        public override void Initialize(string name, NameValueCollection config)
        {
            //Make sure we have a valid config collection
            if (config == null)
                throw new ArgumentNullException("config");

            //If no name was given, we'll give it a generic name
            if (string.IsNullOrEmpty(name))
                name = "CAESDORoleProvider";

            //If no description is given, we'll give it a generic one
            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "CAESDO Role Provider");
            }

            //Initialize the RoleProvider base
            base.Initialize(name, config);

            //Loop through the config collection and set our private variables
            foreach (string key in config.Keys)
            {
                switch (key.ToLower())
                {
                    case "applicationname":
                        ApplicationName = config[key];
                        break;
                    case "connectionstring":
                        ConnectionStringKey = config[key];
                        break;
                }
            }

            //Pull the connection string out of the DB through the given connection string key
            ConnectionString = WebConfigurationManager.ConnectionStrings[ConnectionStringKey].ToString();

            //The Application Name and Connection String are required
            if (string.IsNullOrEmpty(ApplicationName)) throw new ArgumentException("Application Name Is Required");
            if (string.IsNullOrEmpty(ConnectionString)) throw new ArgumentException("A Valid Connection Is Required");
        }

        // Concept borrowed from http://stackoverflow.com/questions/7493319/is-the-database-connection-in-this-class-reusable
        // because using (SqlConnection conn = new SqlConnection(ConnectionString) wasn't working for pooling connections
        private void InstConn()
        {
            if (Connection == null || Connection.State == System.Data.ConnectionState.Closed)
            {
                Connection = new SqlConnection(ConnectionString);
                Connection.Open();
            }
        }

        private void ConnClose()
        {
            if (Connection != null && Connection.State != System.Data.ConnectionState.Closed)
            {
                Connection.Close();
            }
        }

        private int? GetIdFor(String table, String col, String name)
        {
            try
            {
                InstConn();

                SqlCommand checkRole = Connection.CreateCommand();
                checkRole.CommandText = "SELECT Id FROM dbo." + table + " WHERE " + col + " = @Name";
                checkRole.Parameters.Add("@Name", System.Data.SqlDbType.NVarChar).Value = name;

                // Connection.Open();
                int id = (int)checkRole.ExecuteScalar();

                ConnClose();

                return id;
            }
            catch
            {
                ConnClose();
                return null;
            }
        }

        private int? GetRoleIdFor(String name)
        {
            if (name.StartsWith("r_"))
                name = name.Substring(2);
            return GetIdFor("Roles", "Role", name);
        }

        private int? GetBuyerIdFor(String name)
        {
            if (name.StartsWith("b_"))
                name = name.Substring(2);
            return GetIdFor("Buyers", "Name", name);
        }

        private int GetUserIdFor(String name)
        {
            int? id = GetIdFor("Users", "Username", name);

            if (!id.HasValue)
                throw new ProviderException("Could not find username.");

            return id.Value;
        }

        private bool NameTest(String name, String type = "Role/User")
        {
            if (name == null)
                throw new ArgumentNullException(type + " name cannot be null.");
            if (name.Length == 0)
                throw new ArgumentException(type + " name cannot be empty.");

            return true;
        }

        /* 
         * Used in AddUsersToRoles to figure out which table, column, and id to use for
         * adding users to the role with name "name." Returns a RoleInfo, which is a struct
         * containing the relevant information.
         * 
         * Since the role could be a Buyer or a Role, the name passed to this method should
         * have a prefix to distinguish between the two types of roles (i.e., r_ for roles and
         * b_ for buyers).
         */
        private RoleInfo GetRoleInfo(String name)
        {
            int? id;

            // Throws the relevant exceptions if necessary.
            NameTest(name, "Role");

            if (name.StartsWith("r_"))
            {
                id = GetRoleIdFor(name);

                if (id != null)
                    return new RoleInfo("RoleId", "UsersInRoles", (int)id);
            }
            else if (name.StartsWith("b_"))
            {
                id = GetBuyerIdFor(name);

                if (id != null)
                    return new RoleInfo("BuyerId", "UsersInBuyers", (int)id);
            }
            else
            {
                if ((id = GetRoleIdFor(name)) != null)
                    return new RoleInfo("RoleId", "UsersInRoles", (int)id);
                else if ((id = GetBuyerIdFor(name)) != null)
                    return new RoleInfo("BuyerId", "UsersInBuyers", (int)id);
            }

            throw new ProviderException("Role name " + name + " could not be found.");
        }

        // Delegate for UserRolesGeneric method to create the right SQL Command
        public delegate string SqlCommText(string table, string type);

        // Method for shared logic for bulk actions on users and roles (i.e., adding and deleting)
        public void UserRolesGeneric(string[] usernames, string[] roleNames, SqlCommText commandText)
        {
            Dictionary<string, int> userIds = new Dictionary<string, int>();
            List<SqlCommand> comms = new List<SqlCommand>();

            // Connection.Open();
            foreach (string uName in usernames)
            {
                int uId = GetUserIdFor(uName);
                userIds.Add(uName, uId);
            }

            foreach (string rName in roleNames)
            {
                RoleInfo roleInfo = GetRoleInfo(rName);

                foreach (string uName in usernames)
                {
                    if (!IsUserInRole(userIds[uName], roleInfo))
                    {
                        SqlCommand userRoleComm = new SqlCommand();
                        userRoleComm.CommandText = commandText(roleInfo.Table, roleInfo.IdType);
                        userRoleComm.Parameters.Add("@RoleId", System.Data.SqlDbType.Int).Value = roleInfo.Id;
                        userRoleComm.Parameters.Add("@UId", System.Data.SqlDbType.Int).Value = userIds[uName];
                        comms.Add(userRoleComm);
                    }
                }
            }


            InstConn();

            using (SqlTransaction transaction = Connection.BeginTransaction())
            {
                try
                {
                    foreach (SqlCommand comm in comms)
                    {
                        comm.Connection = Connection;
                        comm.Transaction = transaction;
                        comm.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
            }

            ConnClose();
        }

        // Roles are both "Buyers" and "Roles," so have to check both tables for everything.
        // Also checks to make sure user isn't in role before adding. Not sure if this is
        // expected behavior for a RoleProvider, but doing it anyway.
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            UserRolesGeneric(usernames, roleNames, (table, type) => "INSERT INTO " + table + " VALUES (@UId, @RoleId)");
        }

        /*
         * This method only creates Roles with a capital R. Not Buyers, which are managed
         * in another part of the application.
         */
        public override void CreateRole(string roleName)
        {
            // Throws the relevant exceptions if necessary.
            NameTest(roleName, "Role");

            if (RoleExists("r_" + roleName))
                throw new ProviderException("Role " + roleName + " already exists.");

            try
            {
                InstConn();

                SqlCommand newRole = Connection.CreateCommand();
                newRole.CommandText = "INSERT INTO Roles (Role) VALUES (@RoleName)";

                newRole.Parameters.Add("@RoleName", System.Data.SqlDbType.NVarChar).Value = roleName;

                // Connection.Open();
                newRole.ExecuteNonQuery();

                ConnClose();
            }
            catch(Exception ex)
            {
                throw new ProviderException(ex.Message);
            }
        }

        private bool PopulatedRole(RoleInfo role)
        {
            try
            {
                InstConn();

                SqlCommand roleCheck = Connection.CreateCommand();
                roleCheck.CommandText = "SELECT * FROM " + role.Table + " where " + role.IdType + " = @Id";

                roleCheck.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = role.Id;

                // Execute scalar returns null if role is empty
                // Connection.Open();
                bool rolePopd = !(roleCheck.ExecuteScalar() == null);
                ConnClose();

                return rolePopd;
            }
            catch
            {
                return false;
            }
        }

        /*
         * Only deletes actual roles. Not buyers.
         */
        public override bool DeleteRole(string role, bool throwOnPopulatedRole)
        {
            // Throws the relevant exceptions.
            NameTest(role, "Role");

            int? nid = GetRoleIdFor(role);
            int id = 0;

            if (!nid.HasValue)
                throw new ArgumentException("Role name does not exist.");
            else
                id = nid.Value;

            InstConn();

            if (throwOnPopulatedRole && PopulatedRole(new RoleInfo("RoleId", "UsersInRoles", id)))
            {
                throw new ProviderException("Role is not empty and throwOnPopulatedRole was specified.");
            }

            // Connection.Open();
            SqlTransaction transact = Connection.BeginTransaction();

            // Don't actually need to delete users from roles, because the database should cascade deletes,
            // but doing it anyway before deleting roles, just in case cascade deletes don't work.
            try
            {
                SqlParameter rIdParam = new SqlParameter("@Id", System.Data.SqlDbType.Int);
                rIdParam.Value = id;

                SqlCommand userRoleDelete = Connection.CreateCommand();
                userRoleDelete.CommandText = "DELETE FROM UsersInRoles WHERE RoleId = @Id";
                userRoleDelete.Parameters.Add(rIdParam);
                userRoleDelete.ExecuteNonQuery();

                SqlCommand roleDelete = Connection.CreateCommand();
                roleDelete.CommandText = "DELETE FROM Roles WHERE RoleId = @Id";
                roleDelete.Parameters.Add(rIdParam);
                roleDelete.ExecuteNonQuery();

                transact.Commit();
            }
            catch
            {
                transact.Rollback();
                ConnClose();
                return false;
            }

            ConnClose();
            return true;
        }


        /*
         * Takes role name with either the r_ or b_ prefix or whatever. Uses GetRoleInfo, so basically
         * whatever that method accepts.
         */
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            RoleInfo role = GetRoleInfo(roleName);
            List<string> users = new List<string>();

            InstConn();
            try
            {
                SqlCommand comm = Connection.CreateCommand();
                comm.CommandText = "SELECT Username FROM " + role.Table + " LEFT JOIN Users ON " + role.IdType + " = Id " +
                    "WHERE Username LIKE @uNameMatch AND " + role.IdType + " = @roleId";
                comm.Parameters.Add("@uNameMatch", System.Data.SqlDbType.NVarChar).Value = usernameToMatch;
                comm.Parameters.Add("@roleId", System.Data.SqlDbType.Int).Value = role.Id;

                // Connection.Open();

                SqlDataReader reader = comm.ExecuteReader();

                while (reader.Read())
                {
                    users.Add((string)reader["Username"]);
                }
            }
            catch
            {
                ConnClose();
                return new string[] { "" };
            }
            ConnClose();

            return users.ToArray();
        }

        /*
         * Just returns the roles in the Roles table
         */
        public override string[] GetAllRoles()
        {
            List<string> roles = new List<string>();

            InstConn();

            try
            {
                SqlCommand allRoles = Connection.CreateCommand();
                allRoles.CommandText = "SELECT Role FROM Roles";

                // Connection.Open();

                SqlDataReader roleReader = allRoles.ExecuteReader();

                while(roleReader.Read())
                {
                    roles.Add((string)roleReader["Role"]);
                }
            }
            catch
            {
                ConnClose();
                return new string[] { "" };
            }

            ConnClose();

            return roles.ToArray();
        }

        // Returns both Buyers and Roles with the b_ or r_ prefix, respectively.
        public override string[] GetRolesForUser(string username)
        {
            List<string> roles = new List<string>();

            InstConn();

            try
            {
                SqlCommand findRoles = Connection.CreateCommand();

                findRoles.CommandText = "SELECT Role FROM Users u " +
                    "LEFT JOIN " +
	                    "(SELECT Role, UserId " +
		                    "FROM " +
			                    "(SELECT Role, UserId FROM Roles LEFT JOIN UsersInRoles ON Id = RoleId) roles " +
		                    "UNION " +
			                    "(SELECT Name As Role, UserId FROM Buyers LEFT JOIN UsersInBuyers ON Id = BuyerId) " +
	                    ") roles ON roles.UserId = u.Id " +
	                    "WHERE u.Username = @Username";

                findRoles.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar).Value = username;

                // Connection.Open();
                SqlDataReader reader = findRoles.ExecuteReader();

                while (reader.Read())
                {
                    roles.Add((string)reader["Role"]);
                }
            }
            catch
            {
                ConnClose();
                return new string[] { "" };
            }

            ConnClose();

            return roles.ToArray();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            return FindUsersInRole(roleName, "%");
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            try
            {
                RoleInfo role = GetRoleInfo(roleName);
                int uId = GetUserIdFor(username);
                return IsUserInRole(uId, role);
            }
            catch
            {
                return false;
            }
        }

        public bool IsUserInRole(int uId, string roleName)
        {
            try
            {
                RoleInfo role = GetRoleInfo(roleName);
                return IsUserInRole(uId, role);
            }
            catch
            {
                return false;
            }
        }

        public bool IsUserInRole(int uId, RoleInfo role)
        {
            try
            {
                InstConn();
                SqlCommand comm = Connection.CreateCommand();
                comm.CommandText = "SELECT UserId FROM " + role.Table + " WHERE " + role.IdType + " = @RoleId AND UserId = @UserId";
                    
                comm.Parameters.Add("@RoleId", System.Data.SqlDbType.Int).Value = role.Id;
                comm.Parameters.Add("@UserId", System.Data.SqlDbType.Int).Value = uId;

                // Connection.Open();

                if (comm.ExecuteScalar() == null)
                {
                    ConnClose();
                    return false;
                }

                ConnClose();
                return true;
            }
            catch
            {
                ConnClose();
                return false;
            }
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            UserRolesGeneric(usernames, roleNames, (table, type) => "DELETE FROM " + table + " WHERE UserId = @UserId AND " + type + " = @RoleId");
            /*
            InstConn();

            // Connection.Open();
            SqlTransaction transact = Connection.BeginTransaction();

            try
            {
                foreach (string role in roleNames)
                {
                    RoleInfo roleInfo = GetRoleInfo(role);

                    foreach (string user in usernames)
                    {
                        int uId = GetUserIdFor(user);

                        if (IsUserInRole(uId, roleInfo))
                        {
                            SqlCommand comm = Connection.CreateCommand();
                            comm.CommandText = "DELETE FROM " + roleInfo.Table + " WHERE UserId = @UserId AND " + roleInfo.IdType + " = @RoleId";
                            comm.Parameters.Add("@UserId", System.Data.SqlDbType.Int).Value = uId;
                            comm.Parameters.Add("@RoleId", System.Data.SqlDbType.Int).Value = roleInfo.Id;

                            comm.ExecuteNonQuery();
                        }
                    }
                }

                transact.Commit();
            }
            catch
            {
                transact.Rollback();
                throw new Exception("Could not remove at least one user from role(s)");
            }

            ConnClose(); */
        }

        public override bool RoleExists(string roleName)
        {
            if (GetRoleIdFor(roleName).HasValue)
                return true;
            return false;
        }

        // Returns roles for all users. Copied query from GetRolesForUser.
        public Dictionary<string, List<string>> GetUserRoles()
        {
            Dictionary<string, List<string>> uroles = new Dictionary<string, List<string>>();

            InstConn();
            try
            {
                SqlCommand comm = Connection.CreateCommand();
                comm.CommandText = "SELECT Username, Role FROM Users u " +
                    "LEFT JOIN " +
                        "(SELECT Role, UserId " +
                            "FROM " +
                                "(SELECT ('r_' + Role) As Role, UserId FROM Roles LEFT JOIN UsersInRoles ON Id = RoleId) roles " +
                            "UNION " +
                                "(SELECT ('b_' + Name) As Role, UserId FROM Buyers LEFT JOIN UsersInBuyers ON Id = BuyerId) " +
                        ") roles ON roles.UserId = u.Id";

                // Connection.Open();

                SqlDataReader read = comm.ExecuteReader();

                while (read.Read())
                {
                    string user = read["Username"] as string;
                    string role = read["Role"] as string;

                    if (user != null && role != null)
                    {
                        if (!uroles.ContainsKey(user))
                            uroles.Add(user, new List<string>());

                        uroles[user].Add(role);
                    }
                }

                ConnClose();
                return uroles;
            }
            catch
            {
                ConnClose();
                return null;
            }
        }
    }
}