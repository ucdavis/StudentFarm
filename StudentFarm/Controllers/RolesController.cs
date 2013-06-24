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

        public RolesController(IRepository<Buyer> buyerRepo)
        {
            this.buyerRepo = buyerRepo;
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

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                

                return RedirectToAction("Index");
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
