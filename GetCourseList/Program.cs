using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;

namespace TsinghuaCourseHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            var helper = new CourseHelper();
            //helper.Login();
            //helper.Login2();//需要Cookie
            //helper.UpdateCourseList();
            //helper.UpdateCourseRemainCapacity();
            //helper.SaveToFile();

            //return;

            helper.LoadFromFile();

            var exceptlines = File.ReadAllLines("except.txt")
                .Select(s=>s.Trim())
                .Where(s=>s.Length > 0)
                .Where(s => s[0] != '%');

            var except = new HashSet<string>(exceptlines);

            var contain = File.ReadAllLines("contain.txt")
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .Where(s => s[0] != '%')
                .Select(s => GetCourseID(s))
                .Join(helper.CourseList, s => s, c => c.kch课程号 + " " + c.kxh课序号, (s, c) => c)
                .ToArray();

            except.UnionWith(contain.Select(c => c.kch课程号));

            var courses = helper.CourseList
                .Where(c => !except.Contains(c.kch课程号))
                .Where(c => contain.All(have => !have.ConflictWith(c)))
                .Where(c => c.yjskrl研究生课容量 > 0)
                .OrderBy(c => c.kkyx开课院系)
                ;

            var courseList = CourseFilter.Filter(courses)
                .ToList();
            

            using (var fs = File.CreateText("output.txt"))
            {
                foreach (var c in courseList)
                {
                    fs.WriteLine(CourseToString(c));
                }
            }
        }

        private static string GetCourseID(string s)
        {
            var l = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return l[0] + ' ' + l[1];
        }

        static string CourseToString(CourseInfo c)
        {
            return c.kch课程号 + "$" + c.kxh课序号 + "$" + c.kkyx开课院系 + "$" + c.kcm课程名 + "$" + c.zjjs主讲教师
                + "$" + c.sksj上课时间.DebugToString() + "$" + c.xf学分 + "$" + c.yjskrl研究生课容量 + "$"
                 + c.课程特色 + "$" + c.选课文字说明;
        }
    }
}
