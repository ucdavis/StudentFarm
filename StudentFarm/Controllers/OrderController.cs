using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UCDArch.Core.PersistanceSupport;
using StudentFarm.Models;
using System.Web.Security;

namespace StudentFarm.Controllers
{
    // Let's use availability ids as inputs for Details and other actions, and we'll
    // display the orders for the availability/ies based on who's logged in.
    // [Authorize]
    public class OrderController : ApplicationController
    {
        private readonly IRepository<Availability> availRepo;
        private readonly IBuyerAvailabilityRepository buyerAvailRepo;
        private readonly IRepository<Buyer> buyerRepo;
        private readonly ICropRepository cropRepo;
        private readonly ICropUnitRepository cuRepo;
        private readonly IRepository<Offered> offerRepo;
        private readonly IRepository<Ordered> orderedRepo;
        private readonly IRepository<Order> orderRepo;
        private readonly IPriceRepository priceRepo;
        private readonly IUnitRepository unitRepo;

        public OrderController(IRepository<Availability> availRepo, IBuyerAvailabilityRepository buyerAvailRepo,
            IRepository<Buyer> buyerRepo, ICropRepository cropRepo, ICropUnitRepository cuRepo,
            IRepository<Offered> offerRepo, IRepository<Ordered> orderedRepo, IRepository<Order> orderRepo,
            IPriceRepository priceRepo, IUnitRepository unitRepo)
        {
            this.availRepo = availRepo;
            this.buyerAvailRepo = buyerAvailRepo;
            this.buyerRepo = buyerRepo;
            this.cropRepo = cropRepo;
            this.cuRepo = cuRepo;
            this.offerRepo = offerRepo;
            this.orderedRepo = orderedRepo;
            this.orderRepo = orderRepo;
            this.priceRepo = priceRepo;
            this.unitRepo = unitRepo;
        }

        //
        // GET: /Order/

        // Puts a variable in the viewbag called Buyers, which holds a dictionary containing Buyers (to which the
        // current user has access) and their time-relevant (i.e., available for them to order from right now) Availability IDs
        public ActionResult Index()
        {
            // string[] uRoles = Roles.GetRolesForUser();
            string[] uRoles = Roles.GetRolesForUser("ericflin");
            Dictionary<string, List<int>> buyers = new Dictionary<string, List<int>>();
            IQueryable<BuyerAvailability> buyAvailQ = this.buyerAvailRepo.Queryable;

            // Have this part so people can view past orders, like viewing past availabilities in the availability controller??
            // Past orders should be by buyer.
            DateTime today = DateTime.Now;
            ViewBag.WeekBegin = today.AddDays((int)today.DayOfWeek * -1);

            if (uRoles.Contains("r_Admin"))
            {
                IQueryable<Buyer> buyerQ = this.buyerRepo.Queryable;

                // Loop through every single buyer, because this user's an Admin.
                foreach (Buyer buyer in buyerQ)
                {
                    List<int> avails = new List<int>();

                    // Basically, we're putting in all the (time-)relevant availability ids here for the current buyer.
                    foreach (BuyerAvailability baaaaa in buyAvailQ.Where(ba => ba.Buyer == buyer).Where(ba => DateTime.Now.CompareTo(ba.StartTime) >= 0 && DateTime.Now.CompareTo(ba.EndTime) <= 0))
                    {
                        avails.Add(baaaaa.Availability.Id);
                    }

                    buyers.Add(buyer.Name, avails);
                }

                ViewBag.Buyers = buyers;
                return View(this.buyerAvailRepo.Queryable);
            }

            foreach (string role in uRoles)
            {
                // Loop through the buyers this user is authorized to access/order for
                if (role.StartsWith("b_"))
                {
                    List<int> avails = new List<int>();

                    // Repetitive code, but what the heck. I'm trying to get this finished.
                    foreach (BuyerAvailability baaaaa in buyAvailQ.Where(ba => ba.Buyer.Name.Equals(role.Substring(2))).Where(ba => DateTime.Now.CompareTo(ba.StartTime) >= 0 && DateTime.Now.CompareTo(ba.EndTime) <= 0))
                    {
                        avails.Add(baaaaa.Availability.Id);
                    }

                    buyers.Add(role.Substring(2), avails);
                }
            }

            ViewBag.Buyers = buyers;

            // See above about being able to view past orders
            return View(this.buyerAvailRepo.Queryable);
        }

        //
        // GET: /Order/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Order/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Order/Create

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
        // GET: /Order/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Order/Edit/5

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
        // GET: /Order/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Order/Delete/5

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
