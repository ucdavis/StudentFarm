using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UCDArch.Core.PersistanceSupport;
using StudentFarm.Models;
using UCDArch.Web.Controller;
using UCDArch.Web.Attributes;

namespace StudentFarm.Controllers
{
    [Version(MajorVersion = 3)]
    //[ServiceMessage("StudentFarm", ViewDataKey = "ServiceMessages", MessageServiceAppSettingsKey = "MessageService")]
    public abstract class ApplicationController : SuperController
    {
        protected String Search<T>(String search, String item, IOrderedQueryable<T> crops)
        {
            String results = "";

            foreach (var crop in crops)
            {
                var property = crop.GetType().GetProperty(item);
                var val = property.GetValue(crop, null);

                // Escape double quotes with htmlentity &#34;
                results += "\"" + val.ToString().Replace("\"", "&#34;") + "\",";
            }

            // Removes the first quotation mark and the last quotation mark + comma, so that
            // we can split on quote-comma-quote (",") in javascript to make an array for
            // typeahead.
            results = results.Length > 0 ? results.Substring(1, results.Length - 3) : "";

            return results;
        }
    }
}