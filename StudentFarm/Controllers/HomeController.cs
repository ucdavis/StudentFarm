// using NHibernate;
using StudentFarm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UCDArch.Core.PersistanceSupport;

namespace StudentFarm.Controllers
{
    public class HomeController : ApplicationController // Controller
    {
        private readonly IRepository<Crop> cropRepo;
        private readonly IRepository<Price> priceRepo;
        private readonly IRepository<Unit> unitRepo;

        public HomeController(IRepository<Crop> cropRepo, IRepository<Price> priceRepo, IRepository<Unit> unitRepo)
        {
            this.cropRepo = cropRepo;
            this.priceRepo = priceRepo;
            this.unitRepo = unitRepo;
        }

        //
        // GET: /Home/
        public ActionResult Index()
        {
            /* using (ISession session = MvcApplication.SessionFactory.OpenSession())
            {
                ICriteria obj = session.CreateCriteria<Crop>();
                IList<Crop> lst = obj.List<Crop>();

                return View(lst);
            } */
            
            IQueryable<Crop> crops = this.cropRepo.Queryable;

            /*
            var test = from p in crops
                where p.Name.ToLower().Contains("napus")
                select p;
            */
 
            /*
             * Create test
            var test2 = new CropPrice();
            test2.Unit = this.unitRepo.GetById(1);
            test2.Price = 2.5F;
            test2.PriceDate = DateTime.Now.AddDays(2);
            test2.Crop = test.First();

            this.priceRepo.EnsurePersistent(test2);
            */

            /* Update test
            Crop redRuss = test.First();
            redRuss.Name = "KALE, RED RUSSIAN";
            this.cropRepo.EnsurePersistent(redRuss);
            */
 
            /* Another create test
            Crop dinoKale = new Crop();
            dinoKale.Name = "KALE, DINOSAUR";
            dinoKale.Organic = true;
            dinoKale.AddedDate = DateTime.Now.AddYears(-5);
            dinoKale.Description = "Kale with ruffly leaves.";
            this.cropRepo.EnsurePersistent(dinoKale);
            */

            /* Delete test
            this.cropRepo.Remove(redRuss);
            */

            /* Read test */
            return View(crops);
        }

        [HttpPost]
        public ActionResult Index(String[] product, ICollection<String> packsize, ICollection<String> unitprice, ICollection<int> cpid)
        {
            String result = "";

            foreach (String t in product)
            {
                result += t + "<br />";
            }

            result += "<h3>Packsizes</h3>";

            foreach (String t in packsize)
            {
                result += t + "<br />";
            }
            
            return Content(result);
        }

        public ActionResult CropDescriptions(int id)
        {
            IQueryable<Crop> crops = this.cropRepo.Queryable;

            return View(crops);
        }
    }
}
