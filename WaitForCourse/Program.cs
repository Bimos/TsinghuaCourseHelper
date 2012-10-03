using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;
using Locke_CourseSystem;

namespace GetCoursev2
{
    class Program
    {
        static CourseSystem courseSystem;

        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("刷课机V2");
                Console.WriteLine("Locke编写");
                Console.WriteLine();

                Console.WriteLine("请选择：");
                Console.WriteLine("1、配置刷课信息。");
                Console.WriteLine("2、按配置信息刷课。");
                Console.WriteLine("3、退出。");
                Console.WriteLine();
                Console.Write("请选择序号：");
                var input = Console.ReadLine();
                int index = 0;
                if (int.TryParse(input, out index))
                {
                    switch (index)
                    {
                        case 1:
                            Configure();
                            break;

                        case 2:
                            Scan();
                            break;

                        case 3:
                            return;

                        default:
                            //null;
                            break;
                    }
                }

            }
        }

        static void Configure()
        {
            string id;
            string password;
            while (true)
            {
                Console.Clear();
                Console.WriteLine("当前位置：");
                Console.WriteLine("  配置刷课信息");
                Console.WriteLine("    设置账户");
                Console.WriteLine();

                Console.WriteLine("请输入用户名：");
                id = Console.ReadLine();

                Console.WriteLine("请输入密码：");
                password = Console.ReadLine();

                Console.Clear();
                Console.WriteLine("当前位置：");
                Console.WriteLine("  配置刷课信息");
                Console.WriteLine();

                courseSystem = new CourseSystem(id, password, str => Console.Write(str), str => { });
                try
                {
                    courseSystem.Login();
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("发生错误：" + e.Message);
                    Console.Write("按任意键重新设置账户...");
                    Console.Read();
                }
            }

            var semesterList = courseSystem.GetSemesterList();

            Semester semester;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("当前位置：");
                Console.WriteLine("  配置刷课信息");
                Console.WriteLine("    选择学期");
                Console.WriteLine();

                Console.WriteLine("请选择学期：");
                for (int i = 0; i < semesterList.Length; i++)
                {
                    Console.WriteLine("{0} {1}", i, semesterList[i].name);
                }
                Console.WriteLine();
                Console.Write("请选择：");
                string input = Console.ReadLine();
                int index;
                if (int.TryParse(input, out index))
                {
                    if (index >= 0 && index < semesterList.Length)
                    {
                        semester = semesterList[index];
                        break;
                    }
                }
            }

            Console.Clear();
            Console.WriteLine("当前位置：");
            Console.WriteLine("  配置刷课信息");
            Console.WriteLine("    选择学期");
            Console.WriteLine();

            var courseHelper = courseSystem.GetCourseHelper(semester.id);
            var courseArray = courseHelper.GetMyCourseList();

            Console.Clear();
            Console.WriteLine("当前位置：");
            Console.WriteLine("  配置刷课信息");
            Console.WriteLine("    选择学期");
            Console.WriteLine();

            Console.WriteLine("该学期已选上如下课程：");
            Console.WriteLine("{0}\t{1}\t{2}\t{3}", "课程号  ", "课序号", "主讲教师", "课程名");
            foreach (var course in courseArray)
            {
                Console.WriteLine("{0}\t{1}\t{2}\t{3}", course.课程号, course.课序号, course.主讲教师 + "  ", course.课程名);
            }
            Console.WriteLine("共计 {0} 门。", courseArray.Length);
            Console.WriteLine();
            Console.Write("按任意键继续...");
            Console.Read();

            var courseList = new List<Course>();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("当前位置：");
                Console.WriteLine("  配置刷课信息");
                Console.WriteLine("    选择带刷课程列表文件");
                Console.WriteLine();

                Console.WriteLine("请按如下格式把待选课程保存在文件中");
                Console.WriteLine();
                Console.WriteLine("课程号1 课序号1");
                Console.WriteLine("课程号2 课序号2");
                Console.WriteLine("....");
                Console.WriteLine();
                Console.Write("请输入文件名：");
                var filename = Console.ReadLine();

                if (!File.Exists(filename))
                {
                    Console.Write("文件 {0} 不存在，按任意键重新输入....");
                    continue;
                }

                var filelines = File.ReadAllLines(filename);
                var regex = new Regex("^([^ ]+) ([^ ]+)");
                foreach (string line in filelines)
                {
                    var match = regex.Match(line);
                    if (match.Success)
                    {
                        courseList.Add(new Course(match.Groups[1].ToString(), match.Groups[2].ToString()));
                    }
                }

                Console.WriteLine();
                Console.WriteLine("在文件中识别出如下课程信息：");
                foreach (var course in courseList)
                {
                    Console.WriteLine("{0} {1}", course.课程号, course.课序号);
                }
                Console.WriteLine();
                Console.Write("按 R 重新选择文件，按其他键继续...");
                var key = Console.ReadKey();
                if (key.KeyChar == 'r' || key.KeyChar == 'R')
                    continue;

                Console.Clear();
                Console.WriteLine("当前位置：");
                Console.WriteLine("  配置刷课信息");
                Console.WriteLine("    获取课程信息");
                Console.WriteLine();

                Console.WriteLine("正在获取课程信息...");
                foreach (var course in courseList)
                {
                    courseHelper.FillCourseInfo(course);
                }

                for (int i = courseList.Count - 1; i >= 0; i--)
                {
                    if (courseList[i].课程名 == null)
                    {
                        Console.WriteLine("课程 {0} {1} 不存在。", courseList[i].课程号, courseList[i].课序号);
                        courseList.RemoveAt(i);
                    }
                }

                Console.WriteLine();
                Console.WriteLine("您选择了如下课程：");
                Console.WriteLine("{0}\t{1}\t{2}\t{3}", "课程号  ", "课序号", "主讲教师", "课程名");
                foreach (var course in courseList)
                {
                    Console.WriteLine("{0}\t{1}\t{2}\t{3}", course.课程号, course.课序号, course.主讲教师 + "  ", course.课程名);
                }
                Console.WriteLine("共计 {0} 门。", courseList.Count);
                Console.WriteLine();
                Console.Write("按 R 重新选择文件，按其他键继续...");
                key = Console.ReadKey();
                if (key.KeyChar == 'r' || key.KeyChar == 'R')
                    continue;

                break;
            }

            var strBuilder = new StringBuilder();
            strBuilder.AppendLine(id);
            strBuilder.AppendLine(password);
            strBuilder.AppendLine(semester.id);
            strBuilder.AppendLine(courseList.Count.ToString());
            foreach (var course in courseList)
                strBuilder.AppendLine(course.课程号 + " " + course.课序号);
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strBuilder.ToString());

            using (var file = File.Create("userdata.dat"))
            {
                var gz = new GZipStream(file, CompressionMode.Compress);
                gz.Write(buffer, 0, buffer.Length);
                gz.Close();
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("成功写入配置信息到 userdata.dat");
            Console.WriteLine("按任意键返回主菜单...");
            Console.Read();
        }

        static void Scan()
        {
            Console.Clear();
            Console.WriteLine("当前位置：");
            Console.WriteLine("  按配置信息刷课");
            Console.WriteLine("    读取配置文件");
            Console.WriteLine();

            string id;
            string password;
            string semester;
            var courseList = new List<Course>();
            try
            {
                using (var file = File.OpenRead("userdata.dat"))
                {
                    using (var gz = new GZipStream(file, CompressionMode.Decompress))
                    {
                        var reader = new StreamReader(gz, Encoding.UTF8);
                        id = reader.ReadLine();
                        password = reader.ReadLine();
                        semester = reader.ReadLine();
                        int count = int.Parse(reader.ReadLine());
                        for (int i = 0; i < count; i++)
                        {
                            var line = reader.ReadLine()
                                .Split(' ');
                            courseList.Add(new Course(line[0], line[1]));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("读取配置文件错误，按任意键返回主菜单...");
                return;
            }

            int time;
            while (true)
            {
                Console.Clear();
                Console.WriteLine("当前位置：");
                Console.WriteLine("  按配置信息刷课");
                Console.WriteLine("    设置刷课频率");
                Console.WriteLine();

                Console.Write("请输入每次刷课间隔秒数：");
                var input = Console.ReadLine();
                
                if (int.TryParse(input, out time))
                {
                    if (time < 5)
                    {
                        Console.WriteLine();
                        Console.WriteLine();
                        Console.Write("时间必须大于等于5秒，按任意键重新输入...");
                        Console.ReadKey();
                    }
                    else
                    {
                        time *= 1000;
                        break;
                    }
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.Write("请输入整数，按任意键重新输入...");
                    Console.ReadKey();
                }
            }

            Console.Clear();
            Console.WriteLine("当前位置：");
            Console.WriteLine("  按配置信息刷课");
            Console.WriteLine("    正在登录");
            Console.WriteLine();

            courseSystem = new CourseSystem(id, password, str => Console.Write(str), str => { });
            courseSystem.Login();
            var courseHelper = courseSystem.GetCourseHelper(semester);
            courseHelper.GetMyCourseList();
            foreach (var course in courseList)
            {
                courseHelper.FillCourseInfo(course);
            }

            var courseScanner = courseHelper.GetCourseScanner(courseList);
            
            var rand = new Random(5);
            int scanCount = 0;
            while (true)
            {
                courseScanner.ScanAll();

                scanCount++;

                Console.Clear();
                Console.WriteLine("当前位置：");
                Console.WriteLine("  按配置信息刷课");
                Console.WriteLine("    正在刷课");
                Console.WriteLine();

                Console.WriteLine();
                Console.WriteLine("已完成 {0} 轮检测", scanCount);
                Console.WriteLine();

                Console.WriteLine("已选上课程：", scanCount);
                Console.WriteLine();

                foreach (var state in courseScanner.CourseStateList)
                {
                    if(state.State == CoureseStateEnum.Got)
                        Console.WriteLine("{0} {1} {2}  {3}", state.Course.课程号, state.Course.课序号, state.Course.课程名, state.Course.主讲教师);
                }

                Console.WriteLine();
                Console.WriteLine("未选上课程：", scanCount);
                Console.WriteLine();

                foreach (var state in courseScanner.CourseStateList)
                {
                    if (state.State == CoureseStateEnum.Wait)
                        Console.WriteLine("{0} {1} {2}  {3}", state.Course.课程号, state.Course.课序号, state.Course.课程名, state.Course.主讲教师);
                }

                Console.WriteLine();
                Console.WriteLine("按任意键在当前扫描完成后暂停...");
                if (Console.KeyAvailable)
                {
                    while (Console.KeyAvailable)
                        Console.ReadKey();
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.Write("按 Q 键返回主菜单，按其他任意键继续...");
                    var key = Console.ReadKey();
                    if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                        return;
                }

                System.Threading.Thread.Sleep(time + rand.Next(1000 + time / 10));

                if (Console.KeyAvailable)
                {
                    while (Console.KeyAvailable)
                        Console.ReadKey();
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.Write("按 Q 键返回主菜单，按其他任意键继续...");
                    var key = Console.ReadKey();
                    if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                        return;
                }
            }
        }
    }
}
