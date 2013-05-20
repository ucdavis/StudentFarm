using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StudentFarm.Models
{
    public class FuzzyDateParser
    {
        String[] stdDays;
        String[] ucdDays;
        Dictionary<String, int> days, months, years;
        NumberParser numParse;

        public FuzzyDateParser() {
            stdDays = new String[] { "sunday", "monday", "tuesday", "wednesday", "thursday", "friday", "saturday" };
            ucdDays = new String[] { "u", "m", "t", "w", "r", "f", "s" };
            days = new Dictionary<String, int>();
            months = new Dictionary<String, int>();
            years = new Dictionary<String, int>();
            numParse = new NumberParser();

            days.Add("day", 1);
            days.Add("d", 1);
            days.Add("week", 7);
            days.Add("wk", 7);
            days.Add("w", 7);
            days.Add("fortnight", 14);
            months.Add("month", 1);
            months.Add("mo", 1);
            months.Add("m", 1);
            years.Add("year", 1);
            years.Add("yr", 1);
            years.Add("y", 1);
            years.Add("decade", 10);
        }

        public DateTime Parse(String period, DateTime from)
        {
            int[] dmy = new int[3];

            // Somehow split period string into segments consisting of units, e.g.
            // a couple months and a day would become ["a couple months", "and a day"]
            // The number parser would then hopefully return [2, 1]
            // Each of the items in the days, months, and years dictionaries should be matched
            // only if they stand alone -- i.e., with spaces on either side. They should also
            // be matched if they're pluralized.
            // Items in stdDays can be matched if something standing alone matches at least
            // three letters -- e.g., "thu" would match Thursday, but "thunk" would not. Might
            // need a special case for Wednesday, because Wednesday's sometimes abbreviated weds. 
            
            return from;
        }
    }
}