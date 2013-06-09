using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace StudentFarm.Models
{
    public class NumberParser
    {
        // Somehow add terms like "a", "a couple", "a dozen", "a score" and have things like "one or two" return either 1 or 2, randomly.
        String[] ones = new String[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
        String[] tens = new String[] { "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety", "hundred" };
        String[] above = new String[] { "thousand", "million" }; //, "billion", "trillion", "quadrillion", "quintillion", "sextillion", "septillion", "octillion", "nonillion", "decillion" };

        public NumberParser()
        {
        }

        public int indexOf(String number, String[] arr, int start = 0)
        {
            start = start == -1 ? 0 : start;

            for (int i = 0; i < arr.Length; i++)
            {
                if (number.ToLower().Contains(arr[i]))
                {
                    return number.ToLower().IndexOf(arr[i], start);
                }
            }

            return -1;
        }

        public int find(String number, String[] arr, int pos = 0, int start = -1)
        {
            start = start == -1 ? arr.Length - 1 : start;
            pos = pos == -1 ? 0 : pos;

            // Search backward so things like eighteen don't match eight.
            for (int i = start; i >= 0; i--)
            {
                if (number.ToLower().IndexOf(arr[i], pos) != -1)
                {
                    return i;
                }
            }

            return -1;
        }

        private int Collapse(List<int> stack, int count = 0, int start = 0)
        {
            List<int> newstack = new List<int>();
            newstack.Add(stack[0]);

            int pos = 0;

            for (int i = 1; i < stack.Count; i++)
            {
                if (newstack[pos] < stack[i])
                {
                    newstack[pos] *= stack[i];
                }
                else if (newstack[pos] < 1000 && newstack[pos] > stack[i])
                {
                    newstack[pos] += stack[i];
                }
                else if (newstack[pos] > stack[i])
                {
                    newstack.Add(stack[i]);
                    pos++;
                }
            }

            return newstack.Sum();
        }

        private bool MissingHundred(String part)
        {
            if (((!part.ToLower().Contains("hundred") && find(part, tens) != -1) ||
                        find(part, ones) != -1))
            {
                return true;
            }

            return false;
        }

        public double Parse(String number)
        {
            double num = 0;
            if (double.TryParse(number, out num))
            {
                return num;
            }

            List<int> stack = new List<int>();
            String[] parts = number.Split(' ');

            // Put all the number-parts on a stack, so we can
            // add and/or multiply them all together in the next loop
            // to make them an integer
            for (int i = parts.Length - 1; i >= 0; i--)
            {
                int pos;
                // Not sure if this is a good idea, but changing the numeric parser to parse
                // numbers out of mixed strings.
                if (int.TryParse(Regex.Replace(parts[i], "[^0-9]", String.Empty), out pos))
                {
                    stack.Add(pos);
                }

                // Parse 'the' and 'a'
                if (parts[i].ToLower().Equals("the") || parts[i].ToLower().Equals("a"))
                {
                    stack.Add(1);
                }

                // Parse 'a couple.' Since with something like a couple, 1 would've
                // already been added, we want to make sure we remove that, if necessary, before adding 2.
                if (parts[i].ToLower().Equals("couple"))
                {
                    if (i + 1 < parts.Length - 1 && parts[i + 1].ToLower().Equals("a"))
                    {
                        stack.RemoveAt(stack.Count - 1);
                    }

                    stack.Add(2);
                }

                pos = find(parts[i], ones);
                // Make sure things like "sixty" don't get stored as 60 and 6 (because six
                // matches sixty, as does sixty).
                if (pos != -1 && find(parts[i], tens) != -1)
                {
                    pos = find(parts[i], ones, indexOf(parts[i], tens) + 1);
                }
                if (pos != -1)
                {
                    stack.Add(pos);

                    // Handle things like one ten or two fifty-two
                    if (i - 1 >= 0 && MissingHundred(parts[i - 1]))
                    {
                        stack.Add(100);
                    }
                }

                pos = find(parts[i], tens);
                if (pos != -1)
                {
                    stack.Add((pos + 2) * 10);

                    if (i - 1 >= 0 && !parts[i].ToLower().Contains("hundred") && MissingHundred(parts[i - 1]))
                    {
                        stack.Add(100);
                    }
                }

                pos = find(parts[i], above);
                if (pos != -1)
                {
                    stack.Add((int)Math.Pow(1000, (pos + 1)));
                }
            }

            // Reverse the list so we can actually process it. We do it
            // backward so that things like twenty-two and "twenty two" both
            // produce the same output. We only split on spaces, but ones are
            // processed before everything else, so both cases add 2 and then 20.
            stack.Reverse();
            int stacktwo = Collapse(stack);

            return (double)stacktwo;
        }
    }
}