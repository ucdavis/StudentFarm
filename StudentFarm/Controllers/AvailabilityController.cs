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
        private readonly IRepository<BuyerAvailability> buyerAvailRepo;
        private readonly IRepository<Buyer> buyerRepo;

        public AvailabilityController(IRepository<Availability> availRepo, ICropRepository cropRepo,
            IPriceRepository priceRepo, IRepository<Offered> offerRepo, IUnitRepository unitRepo,
            ICropUnitRepository cuRepo, IRepository<BuyerAvailability> buyerAvailRepo, IRepository<Buyer> buyerRepo)
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
        // GET: /Availability/Create

        public ActionResult Create()
        {
            ViewBag.url = Request.Url.GetLeftPart(UriPartial.Authority) + Url.Content("~/");
            ViewBag.buyers = this.buyerRepo.Queryable;
            ViewBag.printTime = new Del(printTime);
            ViewBag.edit = -1;

            return View("edit");
        }

        //
        // POST: /Availability/Create
        // Creates a new availability.

        [HttpPost]
        public ActionResult Create(String start_d, String end_d, int[] buyers, int[] buyer_id,
            String[] ostart_t, String[] ostart_d, String[] oend_t, String[] oend_d, String[] product,
            int[] product_id,  String[] packsize, int[] packsize_id, double[] unitprice, double[] amount)
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
                Dictionary<int, int> dBuyer = new Dictionary<int, int>();
                for (int i = 0; i < buyer_id.Length; i++)
                {
                    dBuyer.Add(buyer_id[i], i);
                }

                for (int i = 0; i < buyers.Length; i++)
                {
                    // Get the position of start/end time/date for this buyer in the POST input.
                    int pos = dBuyer[buyers[i]];

                    // Actually associate the buyer with this availability
                    BuyerAvailability ba = new BuyerAvailability();
                    ba.Availability = newAvail;
                    ba.Buyer = this.buyerRepo.GetById(buyers[i]);
                    ba.StartTime = DateTime.Parse(ostart_d[pos] + " " + ostart_t[pos]);
                    ba.EndTime = DateTime.Parse(oend_d[pos] + " " + oend_t[pos]);
                    this.buyerAvailRepo.EnsurePersistent(ba);
                }

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
                    Offered offer = new Offered();
                    offer.CropPrice = p;
                    offer.Quantity = amount[i];
                    offer.Availability = newAvail;
                    this.offerRepo.EnsurePersistent(offer);
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
            ViewBag.avail = this.availRepo.GetById(id);

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
