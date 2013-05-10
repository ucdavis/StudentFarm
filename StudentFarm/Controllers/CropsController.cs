using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UCDArch.Core.PersistanceSupport;
using StudentFarm.Models;

// TODO: Look at HomeController and add in usings and dependencies for injection.

namespace StudentFarm.Controllers
{
    public class CropsController : ApplicationController
    {
        private readonly IRepository<Crop> cropRepo;

        public CropsController(IRepository<Crop> cropRepo)
        {
            this.cropRepo = cropRepo;
        }

        //
        // GET: /Crops/

        public ActionResult Index()
        {
            ViewBag.url = Request.Url.GetLeftPart(UriPartial.Authority) + Url.Content("~/");

            return View(this.cropRepo.Queryable);
        }

        //
        // Get: /Crops/Search
        public ActionResult Search(String search)
        {
            IQueryable<Crop> cropq = this.cropRepo.Queryable;

            var crops = from crop in cropq
                        where crop.Name.ToLower().Contains(search.ToLower())
                        orderby crop.Name ascending
                        select crop;

            return Content(base.Search<Crop>(search, "Name", crops));
        }

        //
        // GET: /Crops/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // POST: /Crops/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                Crop newCrop = new Crop();
                newCrop.Name = collection["name"];
                newCrop.Description = collection["desc"];
                newCrop.Organic = collection["organic"] != null ? true : false;
                newCrop.AddedDate = DateTime.Now;

                this.cropRepo.EnsurePersistent(newCrop);

                return Json( new {
                    alert = "<div id='add_crop_alert' class='alert alert-success'>" +
                    "<button type='button' class='close' data-dismiss='alert'>&times;</button>" +
                    "<strong>Crop Added Successfully!</strong>" +
                    "</div>",
                    id = newCrop.Id
                } );
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        //
        // POST: /Crops/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                Crop toEdit = this.cropRepo.GetById(id);

                toEdit.Name = collection["name"];
                toEdit.Description = collection["desc"];
                toEdit.Organic = collection["organic"] != null ? true : false;

                this.cropRepo.EnsurePersistent(toEdit);

                return Json(new
                {
                    alert = "<div id='edit_crop_alert' class='alert alert-success'>" +
                    "<button type='button' class='close' data-dismiss='alert'>&times;</button>" +
                    "<strong>Crop Edited Successfully!</strong>" +
                    "</div>",
                    id = toEdit.Id
                });
            }
            catch
            {
                return Json(new
                {
                    alert = "<div id='edit_crop_alert' class='alert alert-error'>" +
                       "<button type='button' class='close' data-dismiss='alert'>&times;</button>" +
                       "<strong>Can't edit something that doesn't exist!</strong>" +
                       "</div>",
                    id = -1
                });
            }
        }

        //
        // POST: /Crops/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                Crop toDelete = this.cropRepo.GetById(id);
                String name = toDelete.Name;
                this.cropRepo.Remove(toDelete);

                return Content("Deleted " + name);
            }
            catch
            {
                return Content("Failed to delete crop with id " + id);
            }
        }
    }
}
