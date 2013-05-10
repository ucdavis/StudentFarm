using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UCDArch.Core.PersistanceSupport;
using StudentFarm.Models;

namespace StudentFarm.Controllers
{
    public class UnitsController : ApplicationController
    {
        private readonly IRepository<Unit> uRepo;

        public UnitsController(IRepository<Unit> uRepo)
        {
            this.uRepo = uRepo;
        }

        //
        // GET: /Units/Search

        public ActionResult Search(String search)
        {
            IQueryable<Unit> units = this.uRepo.Queryable;

            var u = from unit in units
                        where unit.Name.ToLower().Contains(search.ToLower())
                        orderby unit.Name ascending
                        select unit;

            return Content(base.Search<Unit>(search, "Name", u));
        }

    }
}
