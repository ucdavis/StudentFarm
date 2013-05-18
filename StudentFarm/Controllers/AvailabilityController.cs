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
        private readonly IRepository<Price> priceRepo;
        private readonly IRepository<Offered> offerRepo;
        private readonly IRepository<Unit> unitRepo;
        private readonly IRepository<CropUnit> cuRepo;
        private readonly IRepository<BuyerAvailability> buyerAvailRepo;
        private readonly IRepository<Buyer> buyerRepo;

        public AvailabilityController(IRepository<Availability> availRepo, IRepository<Crop> cropRepo,
            IRepository<Price> priceRepo, IRepository<Offered> offerRepo, IRepository<Unit> unitRepo,
            IRepository<CropUnit> cuRepo, IRepository<BuyerAvailability> buyerAvailRepo, IRepository<Buyer> buyerRepo)
        {
            this.availRepo = availRepo;
            this.cropRepo = cropRepo;
            this.priceRepo = priceRepo;
            this.offerRepo = offerRepo;
            this.unitRepo = unitRepo;
            this.cuRepo = cuRepo;
            this.buyerAvailRepo = buyerAvailRepo;
            this.buyerRepo = buyerRepo;
        }

        //
        // GET: /Availability/

        public ActionResult Index()
        {
            ViewBag.url = Request.Url.GetLeftPart(UriPartial.Authority) + Url.Content("~/");
            ViewBag.buyers = this.buyerRepo.Queryable;
            ViewBag.printTime = new Del(printTime);

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
        // Creates a new availability.

        [HttpPost]
        public ActionResult Create(String[] product, int[] product_id, String[] packsize, int[] packsize_id, double[] unitprice, double[] amount)
        {
            try
            {
                // Create new Availability
                Availability newAvail = new Availability();
                this.availRepo.EnsurePersistent(newAvail);

                // Check for existing CropUnits and associated prices. Create Offered records for everything, creating
                // new CropUnit records and Price records if necessary.
                for (int i = 0; i < product.Length; i++)
                {
                    Crop crop = this.cropRepo.GetNullableById(product_id[i]);
                    Unit unit = this.unitRepo.GetNullableById(packsize_id[i]);

                    if (crop == null || crop.Name.ToLower() != product[i].ToLower())
                    {
                        crop = new Crop();
                        crop.Name = product[i];
                        crop.Organic = true; // Assume everything's organic unless specified otherwise 
                                             // (and one can specify otherwise via the crop controller)
                        this.cropRepo.EnsurePersistent(crop);
                    }

                    if (unit == null || unit.Name.ToLower() != packsize[i].ToLower())
                    {
                        unit = new Unit();
                        unit.Name = packsize[i];
                        this.unitRepo.EnsurePersistent(unit);
                    }

                    // Look for an associated CropUnit record
                    IQueryable<CropUnit> cropunits = this.cuRepo.Queryable;
                    var cuq = from cropunit in cropunits
                             where cropunit.Crop.Id == crop.Id &&
                                   cropunit.Unit.Id == unit.Id
                             select cropunit;
                    
                    // Create one if it doesn't exist.
                    CropUnit cu;
                    if (cuq.Count() > 0)
                    {
                        cu = cuq.First();
                    }
                    else
                    {
                        cu = new CropUnit();
                        cu.Crop = crop;
                        cu.Unit = unit;
                        this.cuRepo.EnsurePersistent(cu);
                    }

                    // Look for a Price record
                    IQueryable<Price> prices = this.priceRepo.Queryable;
                    var pq = from price in prices
                             where price.CropUnit.Id == cu.Id &&
                                   price.UnitPrice == unitprice[i]
                             select price;

                    // Create one if it doesn't exist.
                    Price p;
                    if (pq.Count() > 0)
                    {
                        p = pq.First();
                    }
                    else
                    {
                        p = new Price();
                        p.CropUnit = cu;
                        p.UnitPrice = unitprice[i];
                        this.priceRepo.EnsurePersistent(p);
                    }

                    // Offer
                    Offered offer = new Offered();
                    offer.CropPrice = p;
                    offer.Quantity = amount[i];
                    offer.Availability = newAvail;
                    this.offerRepo.EnsurePersistent(offer);
                }

                // Return Offered Ids, so we can update the displayed table with them and
                // know what to update if the user clicks save again.
                return RedirectToAction("Index");
            }
            catch
            {
                return Content("");
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

        public delegate MvcHtmlString Del(int i, bool selected = false);
        public static MvcHtmlString printTime(int i, bool selected = false)
        {
            String option = "";

            String dispTime = "";
            String sendTime = "";
            int iMinutes = i % 60;
            String sMinutes = iMinutes < 10 ? '0' + iMinutes.ToString() : iMinutes.ToString();

            double fHour = i / 60;
            int hour = (int)Math.Floor(fHour);

            String amPm = hour < 12 ? " AM" : " PM";
            if (hour > 12)
            {
                hour = hour - 12;
            }
            if (hour == 0)
            {
                hour = 12;
            }

            String sHour = hour < 10 ? "\u00a0\u00a0" + hour.ToString() : hour.ToString();

            dispTime = sHour + ':' + sMinutes + amPm;
            sendTime = hour.ToString() + ':' + sMinutes + amPm;

            if (selected)
            {
                option = "<option selected=\"selected\" ";
            }
            else
            {
                option = "<option ";
            }

            if (dispTime.Equals("12:00 AM"))
            {
                option += "value='" + sendTime + "'>Midnight</option>";
            }
            else if (dispTime.Equals("12:00 PM"))
            {
                option += "value='" + sendTime + "'>Noon</option>";
            }
            else
            {
                option += " value='" + sendTime + "'>" + dispTime + "</option>";
            }

            return new MvcHtmlString(option);
        }
    }
}
