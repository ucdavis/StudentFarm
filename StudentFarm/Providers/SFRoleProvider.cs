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

    public class SFRoleProvider : RoleProvider
    {
        public override string ApplicationName { get; set; }
        private string ConnectionString { get; set; }
        private string ConnectionStringKey { get; set; }

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

        private int? GetIdFor(String table, String col, String name)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    SqlCommand checkRole = new SqlCommand("SELECT Id FROM " + table + " WHERE " + col + " = @Name", conn);
                    checkRole.Parameters.AddWithValue("@Name", name);
                    return (int)checkRole.ExecuteScalar();
                }
            }
            catch
            {
                return null;
            }
        }

        private int? GetRoleIdFor(String name)
        {
            return GetIdFor("Roles", "Role", name);
        }

        private int? GetBuyerIdFor(String name)
        {
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

        // Roles are both "Buyers" and "Roles," so have to check both tables for everything.
        // Also checks to make sure user isn't in role before adding. Not sure if this is
        // expected behavior for a RoleProvider, but doing it anyway.
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    foreach (string rName in roleNames)
                    {
                        RoleInfo roleInfo = GetRoleInfo(rName);

                        foreach (string uName in usernames)
                        {
                            int uId = GetUserIdFor(uName);
                            if (!IsUserInRole(uId, roleInfo))
                            {
                                SqlCommand addUserRole = conn.CreateCommand();
                                addUserRole.CommandText = "INSERT INTO " + roleInfo.Table + " VALUES (@UId, @RoleId)";
                                addUserRole.Transaction = transaction;
                                addUserRole.Parameters.AddWithValue("@RoleId", roleInfo.Id);
                                addUserRole.Parameters.AddWithValue("@UId", uId);
                                addUserRole.ExecuteNonQuery();
                            }
                        }
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
            }
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
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    SqlCommand newRole = conn.CreateCommand();
                    newRole.CommandText = "INSERT INTO Roles (Role) VALUES (@RoleName)";

                    SqlParameter roleNameParam = newRole.CreateParameter();
                    roleNameParam.ParameterName = "@RoleName";
                    roleNameParam.SqlDbType = System.Data.SqlDbType.NVarChar;
                    roleNameParam.Value = roleName;

                    newRole.ExecuteNonQuery();
                }
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
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    SqlCommand roleCheck = conn.CreateCommand();
                    roleCheck.CommandText = "SELECT * FROM " + role.Table + " where " + role.IdType + " = @Id";

                    SqlParameter roleId = roleCheck.CreateParameter();
                    roleId.SqlDbType = System.Data.SqlDbType.Int;
                    roleId.ParameterName = "@Id";
                    roleId.Value = role.Id;

                    // Execute scalar returns null if role is empty
                    return !(roleCheck.ExecuteScalar() == null);
                }
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

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                if (throwOnPopulatedRole && PopulatedRole(new RoleInfo("RoleId", "UsersInRoles", id)))
                {
                    throw new ProviderException("Role is not empty and throwOnPopulatedRole was specified.");
                }

                SqlTransaction transact = conn.BeginTransaction();

                // Don't actually need to delete users from roles, because the database should cascade deletes,
                // but doing it anyway before deleting roles, just in case cascade deletes don't work.
                try
                {
                    SqlParameter rIdParam = new SqlParameter("@Id", System.Data.SqlDbType.Int);
                    rIdParam.Value = id;

                    SqlCommand userRoleDelete = conn.CreateCommand();
                    userRoleDelete.CommandText = "DELETE FROM UsersInRoles WHERE RoleId = @Id";
                    userRoleDelete.Parameters.Add(rIdParam);
                    userRoleDelete.ExecuteNonQuery();

                    SqlCommand roleDelete = conn.CreateCommand();
                    roleDelete.CommandText = "DELETE FROM Roles WHERE RoleId = @Id";
                    roleDelete.Parameters.Add(rIdParam);
                    roleDelete.ExecuteNonQuery();

                    transact.Commit();
                }
                catch
                {
                    transact.Rollback();
                    conn.Close();
                    return false;
                }
            }

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

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    SqlCommand comm = conn.CreateCommand();
                    comm.CommandText = "SELECT Username FROM " + role.Table + " LEFT JOIN Users ON " + role.IdType + " = Id " +
                        "WHERE Username LIKE @uNameMatch AND " + role.IdType + " = @roleId";
                    comm.Parameters.Add("@uNameMatch", System.Data.SqlDbType.NVarChar).Value = usernameToMatch;
                    comm.Parameters.Add("@roleId", System.Data.SqlDbType.Int).Value = role.Id;

                    SqlDataReader reader = comm.ExecuteReader();

                    while (reader.Read())
                    {
                        users.Add((string)reader["Username"]);
                    }
                }
                catch
                {
                    return new string[] { "" };
                }
            }

            return users.ToArray();
        }

        /*
         * Just returns the roles in the Roles table
         */
        public override string[] GetAllRoles()
        {
            List<string> roles = new List<string>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    SqlCommand allRoles = conn.CreateCommand();
                    allRoles.CommandText = "SELECT Role FROM Roles";

                    SqlDataReader roleReader = allRoles.ExecuteReader();

                    while(roleReader.Read())
                    {
                        roles.Add((string)roleReader["Role"]);
                    }
                }
                catch
                {
                    return new string[] { "" };
                }
            }
            return roles.ToArray();
        }

        // Returns both Buyers and Roles with the b_ or r_ prefix, respectively.
        public override string[] GetRolesForUser(string username)
        {
            List<string> roles = new List<string>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    SqlCommand findRoles = conn.CreateCommand();

                    /// -----------------------------------------------------------------------------------------------------------------------------------------
                    /// -----------------------------------------------------------------------------------------------------------------------------------------
                    /// TODO: Don't know if this query actually works. Should test.
                    /// -----------------------------------------------------------------------------------------------------------------------------------------
                    /// -----------------------------------------------------------------------------------------------------------------------------------------
                    findRoles.CommandText = "SELECT Role FROM Users u " +
                        "LEFT JOIN " +
                        "   (SELECT Role, UserId FROM Roles LEFT JOIN UsersInRoles ON Id = RoleId) roles ON roles.UserId = u.Id " +
                        "LEFT JOIN " +
                        "   (SELECT Name As Role, UserId FROM Buyers LEFT JOIN UsersInBuyers ON Id = BuyerId) buyers ON buyers.UserId = u.Id " +
                        "WHERE u.Username = @Username";

                    findRoles.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar).Value = username;

                    SqlDataReader reader = findRoles.ExecuteReader();

                    while (reader.Read())
                    {
                        roles.Add((string)reader["Role"]);
                    }
                }
                catch
                {
                    return new string[] { "" };
                }
            }

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
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    SqlCommand comm = conn.CreateCommand();
                    comm.CommandText = "SELECT UserId FROM " + role.Table + " WHERE " + role.IdType + " = @RoleId AND UserId = @UserId";
                    
                    SqlParameter roleid = comm.CreateParameter();
                    roleid.ParameterName = "@RoleId";
                    roleid.SqlDbType = System.Data.SqlDbType.Int;
                    roleid.Value = role.Id;

                    SqlParameter userid = comm.CreateParameter();
                    userid.ParameterName = "@UserId";
                    userid.SqlDbType = System.Data.SqlDbType.Int;
                    userid.Value = uId;

                    conn.Open();

                    if (comm.ExecuteScalar() == null)
                    {
                        return false;
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                SqlCommand comm = conn.CreateCommand();
                SqlTransaction transact = conn.BeginTransaction();

                foreach (string role in roleNames)
                {
                    RoleInfo roleInfo = GetRoleInfo(rName);
                }
            }
        }

        public override bool RoleExists(string roleName)
        {
            if (GetRoleIdFor(roleName).HasValue)
                return true;
            return false;
        }
    }
}