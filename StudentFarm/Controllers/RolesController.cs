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

                // Add roles to said user.
                List<string> roles = new List<string>();
                foreach (string key in collection.AllKeys) {
                    if (key != "name" && (key.StartsWith("r_") || key.StartsWith("b_"))) {
                        roles.Add(key);
                    }
                }
                Roles.AddUsersToRoles(new string[] { (string)collection["name"] }, roles.ToArray());

                return Json(new
                {
                    alert = "<div id='add_user_alert' class='alert alert-success'>" +
                        "<button type='button' class='close' data-dismiss='alert'>&times;</button>" +
                        "<strong>User " + newUser.Username + " Added Successfully!</strong>" +
                        "</div>"
                });
            }
            catch
            {
                return View();
            }
        }

        //
        // POST: /Roles/Edit/5

        [HttpPost]
        public ActionResult Edit(string name, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return Json(new string[0] {});
            }
            catch
            {
                return View();
            }
        }

        //
        // POST: /Roles/Delete/5

        [HttpPost]
        public ActionResult Delete(string name, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
