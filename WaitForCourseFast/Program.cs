using System;
using System.Threading;
using System.IO;
using System.Linq;

using Locke_CourseSystem;

namespace GetCoursev2
{
    class Program
    {
        static CourseSystem courseSystem;

        static void Main(string[] args)
        {
            courseSystem = new CourseSystem("2011211212", "hamannsun250", str => Console.Write(str), str => { });

            while (true)
            {
                try
                {
                    courseSystem.Login();
                    if (courseSystem.IsLoginIn())
                        break;

                    Thread.Sleep(300);
                }
                catch
                {

                }
            }

            Semester semester;
            while (true)
            {
                try
                {
                    var semesters = courseSystem.GetSemesterList();
                    if (semesters.Length == 0)
                        continue;

                    semester = semesters[0];
                }
                catch
                {
                    continue;
                }
               
                break;
            }

            var helper = courseSystem.GetCourseHelper(semester.id);

            var courseList = File.ReadAllLines("course.txt")
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .Select(l => (l.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)))
                .Select(a => new Course(a[0], a[1]))
                .ToList();

            if (courseList.Count == 0)
            {
                Console.WriteLine("没有需要选的课程");
                return;
            }

            while (true)
            {
                foreach (var course in courseList)
                {
                    Console.WriteLine("正在选课：{0}", course.课程号);
                    try
                    {
                        var result = helper.TryGetCourse(course);
                        if (result)
                        {
                            courseList.Remove(course);
                            Console.WriteLine("选课成功：{0}", course.课程号);

                            if (courseList.Count == 0)
                            {
                                Console.WriteLine("选课结束");
                                return;
                            }
                            continue;
                        }
                    }
                    catch
                    {

                    }
                   
                }


                Thread.Sleep(1000);
            }
        }
    }
}
