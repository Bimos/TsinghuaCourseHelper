using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace TsinghuaCourseHelper
{
    [Serializable]
    [DebuggerDisplay("{DebugToString()}")]
    public class CourseTime
    {
        public Period[] Periods;

        private CourseTime() { }

        private static readonly Regex ParseRegex1 = new Regex("^(?:[-\\d]*\\([^)]*\\),)*[-\\d]*\\([^)]*\\)$");
        private static readonly Regex ParseRegex2 = new Regex("[-\\d]*\\([^)]*\\)");

        public static CourseTime Parse(string str)
        {
            if (str.Length == 0)
                return new CourseTime {Periods = new Period[0]};

            if (!ParseRegex1.IsMatch(str))
                throw new ArgumentException();

            var result = new CourseTime();

            result.Periods = ParseRegex2.Matches(str)
                .Cast<Match>()
                .Select(m => Period.Parse(m.ToString()))
                .ToArray();

            return result;
        }

        public bool ConflictWith(CourseTime other)
        {
            foreach (Period period1 in Periods)
                foreach (Period period2 in other.Periods)
                    if (period1.ConflictWith(period2))
                        return true;

            return false;
        }

        public string DebugToString()
        {
            return Periods.Select(p => p.DebugToString())
                .OrderBy(s => s)
                .Aggregate((a, b) => a + ";" + b);
        }
    }

    [Serializable]
    [DebuggerDisplay("{DebugToString()}")]
    public class Period
    {
        public int DayOfWeek;
        public int Time;
        public int Week;
        public string WeekStr;

        Period() { }

        static Regex ParseRegex = new Regex("(\\d?)-?(\\d?)\\(([^)]*)周\\)");
        public static Period Parse(string str)
        {
            Match match = ParseRegex.Match(str);
            if (!match.Success)
                throw new ArgumentException();

            Period result = new Period();
            string dayofweekStr = match.Groups[1].ToString();
            result.DayOfWeek = dayofweekStr == "" ? 0 : int.Parse(dayofweekStr);
            string timeStr = match.Groups[2].ToString();
            result.Time = timeStr == "" ? 0 : int.Parse(timeStr);
            result.WeekStr = match.Groups[3].ToString();
            result.Week = ParseWeek(result.WeekStr);

            return result;
        }

        static Regex ParseWeekRegex = new Regex("(?:(?:\\d+|\\d+-\\d+),)*(?:\\d+|\\d+-\\d+)");

        static int ParseWeek(string str)
        {
            if (str == "全")
                return 0xFFFF;
            else if (str == "单")
                return 0x5555;
            else if (str == "双")
                return 0xAAAA;
            else if (str == "前八")
                return 0x00FF;
            else if (str == "后八")
                return 0xFF00;
            else if (ParseWeekRegex.IsMatch(str))
            {
                int result = 0;
                foreach (string s in str.Split(','))
                {
                    if (s.Contains('-'))
                    {
                        var nums = s.Split('-')
                            .Select(ss=>int.Parse(ss))
                            .ToArray();

                        for (int i = nums[0]; i <= nums[1]; i++)
                            result += 1 << (i - 1);
                    }
                    else
                    {
                        int num = int.Parse(s);
                        result += 1 << (num - 1);
                    }
                }

                return result;
            }

            throw new ArgumentException();
        }

        public bool ConflictWith(Period other)
        {
            if (DayOfWeek == 0 || other.DayOfWeek == 0)
                return false;

            return DayOfWeek == other.DayOfWeek && Time == other.Time && (Week & other.Week) != 0;
        }

        public string DebugToString()
        {
            return string.Format("{0}-{1}({2})", DayOfWeek, Time, WeekStr);
        }
    }
}
