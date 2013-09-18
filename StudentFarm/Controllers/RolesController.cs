using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UCDArch.Core.PersistanceSupport;
using StudentFarm.Providers;
using StudentFarm.Models;
using System.Web.Security;
using System.Configuration;

namespace StudentFarm.Controllers
{
    public class RolesController : ApplicationController
    {
        private readonly IRepository<Buyer> buyerRepo;
        private readonly IRepository<User> userRepo;

        public RolesController(IRepository<Buyer> buyerRepo, IRepository<User> userRepo)
        {
            this.buyerRepo = buyerRepo;
            this.userRepo = userRepo;
        }

        //
        // GET: /Roles/

        public ActionResult Index()
        {
            SFRoleProvider rp = (SFRoleProvider)Roles.Provider;
            ViewBag.UserRoles = rp.GetUserRoles();
            ViewBag.Roles = rp.GetAllRoles();
            ViewBag.Buyers = this.buyerRepo.Queryable;
            return View();
        }

        private ActionResult AddUserToRoles(FormCollection collection, string message, int id, string username)
        {
            List<string> roles = new List<string>();

            // Make separate lists for roles and groups for output (used in JSON output and
            // ultimately by JS for dynamically adding info to table).
            List<string> r = new List<string>();
            List<string> g = new List<string>();

            foreach (string key in collection.AllKeys)
            {
                if (key != "name" && (key.StartsWith("r_") || key.StartsWith("b_")))
                {
                    roles.Add(key);
                    if (key.StartsWith("r_"))
                        r.Add(key.Substring(2));
                    else if (key.StartsWith("b_"))
                        g.Add(key.Substring(2));
                }
            }

            Roles.AddUserToRoles(username, roles.ToArray());

            return Json(new
            {
                alert = "<div id='add_user_alert' class='alert alert-success'>" +
                    "<button type='button' class='close' data-dismiss='alert'>&times;</button>" +
                    "<strong>" + message + "</strong>" +
                    "</div>",
                id = id,
                username = username,
                roles = String.Join(", ", r),
                groups = String.Join(", ", g)
            });
        }

        //
        // POST: /Roles/Create
        // If we're calling create, the user shouldn't exist yet, so just have to create a user and add roles to the user.

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // Create the new user
                User newUser = new User();
                newUser.Username = (string)collection["name"];
                this.userRepo.EnsurePersistent(newUser);
                this.userRepo.DbContext.CommitTransaction();

                return AddUserToRoles(collection, "User " + newUser.Username + " Added Successfully!", newUser.Id, newUser.Username);
            }
            catch
            {
                return Content("Somethin' ain't workin' right");
            }
        }

        //
        // POST: /Roles/Edit/username
        // string name is the username of the user whose roles we are editing
        // (Username can't be edited, so we only have to worry about changing the roles and groups for the user.)
        // FormCollection collection has all the goodies

        [HttpPost]
        public ActionResult Edit(string id, FormCollection collection)
        {
            try
            {
                // Find info for user
                User curUser = this.userRepo.Queryable.Where(u => u.Username.Equals(id)).First();

                // Delete all the user's current roles
                Roles.RemoveUserFromRoles(id, Roles.GetRolesForUser(id));

                return AddUserToRoles(collection, "User " + id + " Permissions Updated!", curUser.Id, id);
            }
            catch
            {
                return Content("Somethin' ain't workin' right");
            }
        }

        //
        // POST: /Roles/Delete/5

        [HttpPost]
        public EmptyResult Delete(string id)
        {
            try
            {
                // Delete all the user's current roles
                string[] roleNames = Roles.GetRolesForUser(id);
                if (roleNames.Length > 0)
                    Roles.RemoveUserFromRoles(id, roleNames);

                // Delete the user
                User curUser = this.userRepo.Queryable.Where(u => u.Username.Equals(id)).First();
                this.userRepo.Remove(curUser);
                this.userRepo.DbContext.CommitTransaction();

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
