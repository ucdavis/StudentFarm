using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace StudentFarm.Models
{
    public class FuzzyDateParser
    {
        List<String> units;
        List<String> wkday_abbr;
        Dictionary<String, int> days, months, years, weekdays;
        NumberParser numParse;

        public FuzzyDateParser() {
            // Kinda hackish, but tues, weds, thurs are supported becaues the Search method supports
            // plurals. "W" for Wednesday isn't supported, because it's also an abbreviation for week.
            // "M" for Monday also isn't supported, because m already stands for month.
            weekdays = new Dictionary<String, int>
            {
                {"sunday", 0}, {"sun", 0}, {"su", 0}, {"u", 0},
                {"monday", 1}, {"mon", 1},
                {"tuesday", 2}, {"tue", 2}, {"tu", 2}, {"t", 2},
                {"wednesday", 3}, {"wed", 3},
                {"thursday", 4}, {"thur", 4}, {"thu", 4}, {"th", 4}, {"r", 4},
                {"friday", 5}, {"fri", 5}, {"f", 5},
                {"saturday", 6}, {"sat", 6}, {"s", 6}
            };
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

            // For searching to see whether or not any of the items in the days, months, and years dictionaries match.
            units = days.Keys.ToList();
            units.AddRange(months.Keys.ToList());
            units.AddRange(years.Keys.ToArray());

            // Same thing, except for day-of-week names
            wkday_abbr = weekdays.Keys.ToList();
        }
        
        // Returns the date of the last day-of-week specified.
        public DateTime WeekDayBefore(int weekday, DateTime before)
        {
            int dow = (int)before.DayOfWeek;
            return before.AddDays(-(dow + 7 - weekday));
        }

        // Returns a key that helps specify the unit of time.
        public String Search(List<String> list, String search)
        {
            // Split the string so we can search individual words in it more easily.
            String[] aSearch = search.Split(' ');
            String found = "";
            List<String> lFound = new List<String>();

            for (int i = 0; i < list.Count(); i++)
            {
                for (int a = 0; a < aSearch.Length; a++)
                {
                    // Each of the items in the days, months, and years dictionaries should be matched
                    // only if they stand alone -- i.e., with spaces or punctuation on either side. They should also
                    // be matched if they're pluralized.
                    if (aSearch[a].Equals(list[i]) || aSearch[a].Equals(list[i] + ".") || aSearch[a].Equals(list[i] + "s.") ||
                        aSearch[a].Equals(list[i] + "s") || aSearch[a].Equals(list[i] + ".s") || aSearch[a].Equals(list[i] + "'s"))
                    {
                        if (found.Length == 0)
                        {
                            found = list[i];
                            lFound.Add(list[i]);
                            break; // Want to make sure there's only one unit of time in this string, so don't return
                                   // yet. If there's more, get Parse to split this string by the found units and try
                                   // again.
                        }
                        else
                        {
                            lFound.Add(list[i]);
                        }
                    }

                    // Abbreviated unit names (i.e., things with lengths less than 3) don't have
                    // to be separated from numbers by spaces, but do have to be
                    // separated from spelled-out numbers by spaces.
                    else if (list[i].Length <= 2 && aSearch[a].Contains(list[i]) && 
                        Regex.IsMatch(aSearch[a], @"\d" + list[i]))
                    {
                        if (found.Length == 0)
                        {
                            found = list[i];
                            lFound.Add(list[i]);
                            break;
                        }
                        else
                        {
                            lFound.Add(list[i]);
                        }
                    }
                }
            }

            if (lFound.Count <= 1) {
                return found;
            }
            else {
                return @"(" + String.Join(@"|", lFound) + ")";
            }
        }

        public DateTime DoUnits(String unit, DateTime from, int multiplier, String search)
        {
            if (days.ContainsKey(unit))
            {
                return from.AddDays(multiplier * days[unit] * numParse.Parse(search));
            }
            else if (months.ContainsKey(unit))
            {
                return from.AddMonths((int)(multiplier * months[unit] * numParse.Parse(search)));
            }
            else if (years.ContainsKey(unit))
            {
                return from.AddYears((int)(multiplier * years[unit] * numParse.Parse(search)));
            }
            else if (weekdays.ContainsKey(unit))
            {
                // Find the date of the given weekday before the given date (e.g., if today is Tuesday
                // and the weekday wanted is Thursday, the date should be five days before the given date).
                from = WeekDayBefore(weekdays[unit], from);

                // 1 Thursday ago should be the same thing as what's outputted from WeekDayBefore,
                // so don't modify the date. 0 thursdays ago doesn't make any sense, so ignore and
                // just pretend the user wants the thursday before.
                double weeks = numParse.Parse(search);
                weeks = weeks <= 1 ? 0 : weeks * 7;

                return from.AddDays(multiplier * weeks);
            }

            return from;
        }

        // Goes through the whole array of unit/number combos and modifies the date using all of 'em.
        public DateTime ModDate(String[] periods, DateTime from, int multiplier)
        {
            // Figure out what the units are for this period and add them to "from"
            // as appropriate.
            for (int pd = 0; pd < periods.Length; pd++)
            {
                from = ParseOneDate(periods[pd], from, multiplier, this.units);
            }

            return from;
        }

        // Modifies the date based on one unit/number combo (e.g., 1 week)
        public DateTime ParseOneDate(String period, DateTime from, int multiplier, List<String> units)
        {
            String unit = Search(units, period);

            if (unit != "" && !unit.Contains('|'))
            {
                return DoUnits(unit, from, multiplier, period);
            }
            else if (unit.Contains('|'))
            {
                String mult = Regex.Replace(period, unit, "$1|");
                return ModDate(mult.Split('|'), from, multiplier);
            }
            else if (units.Equals(this.units))
            {
                return ParseOneDate(period, from, multiplier, wkday_abbr);
            }
            else
            {
                return from;
            }
        }

        public DateTime Parse(String period, DateTime from, bool before = false)
        {
            int[] dmy = new int[3];

            // Subtracts or adds days, depending on if before is specified.
            int multiplier = before ? -1 : 1;

            // Somehow split period string into segments consisting of units, e.g.
            // a couple months and a day would become ["a couple months", "and a day"]
            // The number parser would then hopefully return [2, 1]

            String[] periods = period.Split(new String[] { ",", "and", "&", "+" },
                StringSplitOptions.RemoveEmptyEntries);

            from = ModDate(periods, from, multiplier);

            // Items in stdDays can be matched if something standing alone matches at least
            // three letters -- e.g., "thu" would match Thursday, but "thunk" would not. Might
            // need a special case for Wednesday, because Wednesday's sometimes abbreviated weds. 
            
            return from;
        }
    }
}