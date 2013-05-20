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
        private readonly ICropRepository cropRepo;
        private readonly IPriceRepository priceRepo;
        private readonly IRepository<Offered> offerRepo;
        private readonly IUnitRepository unitRepo;
        private readonly ICropUnitRepository cuRepo;
        private readonly IBuyerAvailabilityRepository buyerAvailRepo;
        private readonly IRepository<Buyer> buyerRepo;

        public AvailabilityController(IRepository<Availability> availRepo, ICropRepository cropRepo,
            IPriceRepository priceRepo, IRepository<Offered> offerRepo, IUnitRepository unitRepo,
            ICropUnitRepository cuRepo, IBuyerAvailabilityRepository buyerAvailRepo, IRepository<Buyer> buyerRepo)
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
            return View(this.availRepo.Queryable);
        }

        //
        // GET: /Availability/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // Get: /Availability/Create/5 or /Availability/Create
        
        public ActionResult Create(int id = -1)
        {
            ViewBag.url = Request.Url.GetLeftPart(UriPartial.Authority) + Url.Content("~/");
            ViewBag.buyers = this.buyerRepo.Queryable;
            ViewBag.printTime = new Del(printTime);
            ViewBag.edit = id;
            ViewBag.create = true;
            ViewBag.avail = this.availRepo.GetById(id);

            return View("edit");
        }

        // Takes an array and reverses the key and item, returning a Dictionary.
        private Dictionary<K, int> reverse<K>(K[] arr)
        {
            Dictionary<K, int> reversed = new Dictionary<K, int>();
            for (int i = 0; i < arr.Length; i++)
            {
                reversed.Add(arr[i], i);
            }

            return reversed;
        }

        //
        // POST: /Availability/Create
        // Creates a new availability.

        [HttpPost]
        public ActionResult Create(String start_d, String end_d, int[] buyers, int[] buyer_id,
            String[] ostart_t, String[] ostart_d, String[] oend_t, String[] oend_d, String[] product,
            int[] product_id, String[] packsize, int[] packsize_id, double[] unitprice, double[] amount)
        {
            try
            {
                // Create new Availability
                Availability newAvail = new Availability();
                newAvail.DateStart = DateTime.Parse(start_d + " 12:00:00 AM");
                newAvail.DateEnd = DateTime.Parse(end_d + " 11:59:59 PM");
                this.availRepo.EnsurePersistent(newAvail);

                // Add Buyers. Index their positions in the form, keyed on id, first
                // so we can use the right start/end datetimes.
                Dictionary<int, int> dBuyer = reverse<int>(buyer_id);

                newAvail.UpdateBuyers(buyers, dBuyer, ostart_d, ostart_t, oend_d, oend_t);

                // Check for existing CropUnits and associated prices. Create Offered records for everything, creating
                // new CropUnit records and Price records if necessary.
                for (int i = 0; i < product.Length; i++)
                {
                    Crop crop = this.cropRepo.GetOrCreate(product_id[i], product[i]);
                    Unit unit = this.unitRepo.GetOrCreate(packsize_id[i], packsize[i]);

                    // Look for an associated CropUnit record
                    CropUnit cu = this.cuRepo.GetOrCreate(crop, unit);

                    // Look for a Price record
                    Price p = this.priceRepo.GetOrCreate(cu, unitprice[i]);

                    // Offer
                    newAvail.CreateOrUpdateOffer(cu, p, amount[i]);
                }

                // Return Offered Ids, so we can update the displayed table with them and
                // know what to update if the user clicks save again.
                return Content(newAvail.Id.ToString());
            }
            catch
            {
                return Content("Nope");
            }
        }

        //
        // GET: /Availability/Edit/5

        public ActionResult Edit(int id)
        {
            ViewBag.url = Request.Url.GetLeftPart(UriPartial.Authority) + Url.Content("~/");
            ViewBag.buyers = this.buyerRepo.Queryable;
            ViewBag.printTime = new Del(printTime);
            ViewBag.edit = id;
            ViewBag.create = false;
            ViewBag.avail = this.availRepo.GetById(id);

            return View();
        }

        //
        // POST: /Availability/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, String start_d, String end_d, int[] buyers, int[] buyer_id,
            String[] ostart_t, String[] ostart_d, String[] oend_t, String[] oend_d, String[] product,
            int[] product_id, String[] packsize, int[] packsize_id, double[] unitprice, double[] amount,
            int[] cp_id, int[] offered_id)
        {
            try
            {
                Availability avail = this.availRepo.GetById(id);
                avail.DateStart = DateTime.Parse(start_d + " 12:00:00 AM");
                avail.DateEnd = DateTime.Parse(end_d + " 11:59:59 PM");
                this.availRepo.EnsurePersistent(avail);

                // Index buyer positions
                Dictionary<int, int> dBuyer = reverse<int>(buyer_id);

                // Modify buyer information as necessary. Prefer modifying over deleting/re-adding, because
                // we want to be able to edit availabilities even after people have kind of made orders.
                avail.UpdateBuyers(buyers, dBuyer, ostart_d, ostart_t, oend_d, oend_t);

                // Delete/modify/add offered records as necessary
                // Check for existing CropUnits and associated prices. Create Offered records for everything, creating
                // new CropUnit records and Price records if necessary.
                for (int i = 0; i < product.Length; i++)
                {
                    Crop crop = this.cropRepo.GetOrCreate(product_id[i], product[i]);
                    Unit unit = this.unitRepo.GetOrCreate(packsize_id[i], packsize[i]);

                    // Look for an associated CropUnit record
                    CropUnit cu = this.cuRepo.GetOrCreate(crop, unit);

                    // Look for a Price record
                    Price p = this.priceRepo.GetOrCreate(cu, unitprice[i]);

                    // Check whether or not this offer existed before editing.
                    avail.CreateOrUpdateOffer(cu, p, amount[i], offered_id[i]);
                }

                return Content("Yup");
            }
            catch
            {
                return Content("Nope");
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
