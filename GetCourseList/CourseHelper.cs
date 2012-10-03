using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

namespace TsinghuaCourseHelper
{
    [Serializable]
    [DebuggerDisplay("{kcm课程名}")]
    public class CourseInfo
    {
        public string kkyx开课院系;
        public string kch课程号;
        public string kxh课序号;
        public string kcm课程名;
        public int xf学分;
        public string zjjs主讲教师;
        public int bkskrl本科生课容量;
        public int bkskyl本科生课余量;
        public int yjskrl研究生课容量;
        public CourseTime sksj上课时间;
        public string 年级;
        public string 课程特色;
        public string 本科文化素质课组;
        public string 选课文字说明;
        public string 重修是否占容量;
        public string 是否选课时限制;
        public string 是否二级选课;
        public string 实验信息;

        public CourseInfo(CourseRawData rawData)
        {
            kkyx开课院系 = rawData.开课院系;
            kch课程号 = rawData.课程号;
            kxh课序号 = rawData.课序号;
            kcm课程名 = rawData.课程名;
            xf学分 = int.Parse(rawData.学分);
            zjjs主讲教师 = rawData.主讲教师;
            bkskrl本科生课容量 = int.Parse(rawData.本科生课容量);
            bkskyl本科生课余量 = int.MaxValue;
            yjskrl研究生课容量 = int.Parse(rawData.研究生课容量);
            sksj上课时间 = CourseTime.Parse(rawData.上课时间);
            年级 = rawData.年级;
            课程特色 = rawData.课程特色;
            本科文化素质课组 = rawData.本科文化素质课组;
            选课文字说明 = rawData.选课文字说明;
            重修是否占容量 = rawData.重修是否占容量;
            是否选课时限制 = rawData.是否选课时限制;
            是否二级选课 = rawData.是否二级选课;
            实验信息 = rawData.实验信息;
        }

        public bool ConflictWith(CourseInfo other)
        {
            return sksj上课时间.ConflictWith(other.sksj上课时间);
        }
    }

    class CourseHelper
    {
        public CourseInfo[] CourseList;
        public DateTime UpdateTime;
        private GetCourseList _getcourselist;

        private const string DataFileName = "CourseList.dat";

        public void Login()
        {
            Console.WriteLine("请输入用户名：");
            var id = Console.ReadLine();
            Console.WriteLine("请输入密码：");

            string password = string.Empty;

            ConsoleKeyInfo info;
            do
            {
                info = Console.ReadKey(true);
                if (info.Key != ConsoleKey.Enter && info.Key != ConsoleKey.Backspace && info.Key != ConsoleKey.Escape &&
                    info.Key != ConsoleKey.Tab && info.KeyChar != '\0')
                {
                    password += info.KeyChar;
                    Console.Write('*');
                }
            } while (info.Key != ConsoleKey.Enter);
            Console.WriteLine();


            _getcourselist = new GetCourseList(id, password, Console.WriteLine);
            _getcourselist.Login();
        }

        public void Login2()
        {
            _getcourselist = new GetCourseList("", "", Console.WriteLine);
            _getcourselist.Login2();
        }

        public void UpdateCourseList()
        {
            var semester = _getcourselist.GetSemesterList()[0];
            UpdateTime = DateTime.Now;
            var rawData = _getcourselist.GetCourses(semester);

            try
            {
                CourseList = rawData.Select(raw => new CourseInfo(raw))

                .ToArray();
            }
            catch (Exception exception)
            {
                Console.WriteLine("发生错误：{0}", exception.Message);

                using (var fs = new FileStream("dump.dat", FileMode.Create))
                using (var stream = new GZipStream(fs, CompressionMode.Compress))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, rawData);
                }

                throw;
            }
        }

        public void UpdateCourseRemainCapacity()
        {
            var semester = _getcourselist.GetSemesterList()[0];
            var remainCapacityData = _getcourselist.GetCourseRemainCapacity(semester);

            try
            {
                CourseList = CourseList
                .Join(remainCapacityData,
                        o => o.kch课程号 + " " + o.kxh课序号,
                        i => i.kch课程号 + " " + i.kxh课序号,
                        (o, i) =>
                        {
                            o.bkskyl本科生课余量 = int.Parse(i.kyl课余量);
                            return o;
                        }
                      )
                .ToArray();
            }
            catch (Exception exception)
            {
                Console.WriteLine("发生错误：{0}", exception.Message);

                using (var fs = new FileStream("dump.dat", FileMode.Create))
                using (var stream = new GZipStream(fs, CompressionMode.Compress))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, CourseList);
                    formatter.Serialize(stream, remainCapacityData);
                }

                throw;
            }

        }

        public void SaveToFile()
        {
            try
            {
                using (var fs = new FileStream(DataFileName, FileMode.Create))
                using (var stream = new GZipStream(fs, CompressionMode.Compress))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, UpdateTime);
                    formatter.Serialize(stream, CourseList);
                }
            }
            catch { }
        }

        public void LoadFromFile()
        {
            using (var fs = new FileStream(DataFileName, FileMode.Open))
            using (var stream = new GZipStream(fs, CompressionMode.Decompress))
            {
                var formatter = new BinaryFormatter();
                UpdateTime = (DateTime)formatter.Deserialize(stream);
                CourseList = (CourseInfo[])formatter.Deserialize(stream);
            }
        }

    }
}
