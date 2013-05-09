using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UCDArch.Core.PersistanceSupport;
using StudentFarm.Models;

namespace StudentFarm.Controllers
{
    public class AvailabilityController : ApplicationController
    {
        private readonly IRepository<Availability> availRepo;
        private readonly IRepository<Crop> cropRepo;
        private readonly IRepository<CropPrice> priceRepo;
        private readonly IRepository<Offered> offerRepo;
        private readonly IRepository<Unit> unitRepo;
        private readonly IRepository<BuyerAvailability> buyerAvailRepo;

        public AvailabilityController(IRepository<Availability> availRepo, IRepository<Crop> cropRepo,
            IRepository<CropPrice> priceRepo, IRepository<Offered> offerRepo, IRepository<Unit> unitRepo,
            IRepository<BuyerAvailability> buyerAvailRepo)
        {
            this.availRepo = availRepo;
            this.cropRepo = cropRepo;
            this.priceRepo = priceRepo;
            this.offerRepo = offerRepo;
            this.unitRepo = unitRepo;
            this.buyerAvailRepo = buyerAvailRepo;
        }

        //
        // GET: /Availability/

        public ActionResult Index()
        {
            ViewBag.url = Request.Url.GetLeftPart(UriPartial.Authority) + Url.Content("~/");

            return View();
        }

        //
        // GET: /Availability/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Availability/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Availability/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Availability/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Availability/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Availability/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Availability/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
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
