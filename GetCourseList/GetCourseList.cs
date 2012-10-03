using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace TsinghuaCourseHelper
{
    [Serializable]
    [DebuggerDisplay("{课程名}")]
    public class CourseRawData
    {
        public string 开课院系;
        public string 课程号;
        public string 课序号;
        public string 课程名;
        public string 学分;
        public string 主讲教师;
        public string 本科生课容量;
        public string 研究生课容量;
        public string 上课时间;
        public string 年级;
        public string 课程特色;
        public string 本科文化素质课组;
        public string 选课文字说明;
        public string 重修是否占容量;
        public string 是否选课时限制;
        public string 是否二级选课;
        public string 实验信息;
    }

    [Serializable]
    [DebuggerDisplay("{kcm课程名}")]
    public class CourseRemainCapacity
    {
        public string kch课程号;
        public string kxh课序号;
        public string kcm课程名;
        public string krl课容量;
        public string kyl课余量;
        public string zjjs主讲教师;
        public string sks上课时间;
    }

    public class Semester
    {
        public string Id;
        public string Title;
    }

    public class GetCourseList
    {
        public delegate void OutputDelegate(string log);

        readonly HTTPHelper _httpHelper;
        readonly Readjpg _jpgreader;
        readonly string _userId;
        readonly string _userPassword;
        readonly Action<string> _logDelegate;

        public GetCourseList(string id, string password, Action<string> logDelegate)
        {
            _httpHelper = new HTTPHelper(5000);
            _jpgreader = new Readjpg();
            _userId = id;
            _userPassword = password;
            _logDelegate = logDelegate;
        }

        void OutputLog(string str)
        {
            var output = string.Format("{0} {1}",
                DateTime.Now.ToLongTimeString(), str);
            _logDelegate(output);
        }

        public void Login()
        {
            OutputLog("开始登录");

            {
                var loginUrl1 = "http://zhjwxk.cic.tsinghua.edu.cn/xsxk_index.jsp";
                var request = _httpHelper.CreateHTTPGetRequest(loginUrl1, true);
                HTTPHelper.GetResponseString(request);
            }

            {
                var loginUrl2 = "http://zhjwxk.cic.tsinghua.edu.cn/xklogin.do";
                var request = _httpHelper.CreateHTTPGetRequest(loginUrl2, true);
                HTTPHelper.GetResponseString(request);
            }
            
            var login = false;
            int errorcount = 0;
            while (login == false && errorcount < 5)
            {
                string jpgcode = null;
                OutputLog("获取jpg");
                while (jpgcode == null)
                {
                    var loginUrl3 = "http://zhjwxk.cic.tsinghua.edu.cn/login-jcaptcah.jpg?captchaflag=login1";
                    var request = _httpHelper.CreateHTTPGetRequest(loginUrl3, true);
                    jpgcode = HTTPHelper.GetResponseJpgCode(request, _jpgreader);

                    if (jpgcode == null)
                        OutputLog("识别jpg失败");
                    System.Threading.Thread.Sleep(1000);
                }

                OutputLog("开始HTTPS链接");
                {
                    var loginUrl4 = "https://zhjwxk.cic.tsinghua.edu.cn:443/j_acegi_formlogin_xsxk.do";
                    var postStr = string.Format("j_username={0}&j_password={1}&captchaflag=login1&_login_image_={2}",
                        _userId, _userPassword, jpgcode);
                    var request = _httpHelper.CreateHTTPPOSTRequest(loginUrl4, postStr, true);
                    HTTPHelper.GetResponseString(request);
                }
                System.Threading.Thread.Sleep(1000);

                try
                {
                    var loginUrl5 = "http://zhjwxk.cic.tsinghua.edu.cn/xkBks.vxkBksXkbBs.do?m=main";
                    var request = _httpHelper.CreateHTTPGetRequest(loginUrl5, true);
                    HTTPHelper.GetResponseString(request);
                }
                catch(Exception e)
                {
                    errorcount++;
                    OutputLog("登录错误，重新登录。错误信息：" + e.Message);
                    System.Threading.Thread.Sleep(2000);
                    continue;
                }
                login = true;
                OutputLog("登录成功");
            }

        }

        public void Login2()
        {
            var c1 = new Cookie("thuwebcookie", "1141141414.20480.0000", "/", "zhjw.cic.tsinghua.edu.cn");
            var c2 = new Cookie("JSESSIONID", "bacFnMtzWdoQ_86vOOEit", "/", "zhjw.cic.tsinghua.edu.cn");
            _httpHelper.CookieContainer.Add(c1);
            _httpHelper.CookieContainer.Add(c2);
        }

        public Semester[] GetSemesterList()
        {
            string html;

            {
                var url1 = "http://zhjwxk.cic.tsinghua.edu.cn/xkYjs.vxkYjsXkbBs.do?m=main";
                var request = _httpHelper.CreateHTTPGetRequest(url1, true);
                html = HTTPHelper.GetResponseString(request);
            }

            var regexPatternStr = "function showTree(?:[^=]*=){3}([^\"]*)\"";
            var nowSemester = Regex.Match(html, regexPatternStr).Groups[1].ToString();

            {
                var url1 = "http://zhjwxk.cic.tsinghua.edu.cn/xkYjs.vxkYjsXkbBs.do?m=showTree&p_xnxq=" + nowSemester;
                var request = _httpHelper.CreateHTTPGetRequest(url1, true);
                html = HTTPHelper.GetResponseString(request);
            }

            regexPatternStr = "<option value=\"([^\"]*)\"[^>]*>\\s*([^\\s]*)\\s";
            var matchResult = Regex.Matches(html, regexPatternStr);

            return matchResult.Cast<Match>()
                .Select(match => new Semester
                                     {
                                         Id = match.Groups[1].ToString(),
                                         Title = match.Groups[2].ToString()
                                     })
                .ToArray();
        }

        private static string getToken(string html)
        {
            var matchresult = Regex.Match(html, "name=\"token\" value=\"([^\\\"]*)\"");
            return matchresult.Groups[1].ToString();
        }

        private static readonly Regex HTMLToTxtRegex = new Regex("(?:<[^>]*>)?([^<]*)(?:<[^>]*>)?");

        private static string HTMLToTxt(string html)
        {
            return HTMLToTxtRegex.Match(html).Groups[1].ToString();
        }

        private const string ParseCourseRegexPatternStr = "\\[" +
            "\\s*\"([^\"]*)\"\\s*," +
            "\\s*\"([^\"]*)\"\\s*," +
            "\\s*\"([^\"]*)\"\\s*," +
            "\\s*\"([^\"]*)\"\\s*," +
            "\\s*\"([^\"]*)\"\\s*," +
            "\\s*\"([^\"]*)\"\\s*," +
            "\\s*\"([^\"]*)\"\\s*," +
            "\\s*\"([^\"]*)\"\\s*," +
            "\\s*\"([^\"]*)\"\\s*," +
            "\\s*\"([^\"]*)\"\\s*," +
            "\\s*\"([^\"]*)\"\\s*," +
            "\\s*\"([^\"]*)\"\\s*," +
            "\\s*\"([^\"]*)\"\\s*," +
            "\\s*\"([^\"]*)\"\\s*," +
            "\\s*\"([^\"]*)\"\\s*," +
            "\\s*\"([^\"]*)\"\\s*," +
            "\\s*\"([^\"]*)\"\\s*" +
            "\\]";

        private static readonly Regex ParseCourseRegex = new Regex(ParseCourseRegexPatternStr, RegexOptions.Compiled);

        private const string ParseCourseRemainCapacityRegexPatternStr = "\\[" +
            "\\s*\"([^\"]*)\"\\s*," +
            "\\s*\"([^\"]*)\"\\s*," +
            "\\s*\"([^\"]*)\"\\s*," +
            "\\s*\"([^\"]*)\"\\s*," +
            "\\s*\"([^\"]*)\"\\s*," +
            "\\s*\"([^\"]*)\"\\s*," +
            "\\s*\"([^\"]*)\"\\s*" +
            "\\]";

        private static readonly Regex ParseCourseRemainCapacityRegex = new Regex(ParseCourseRemainCapacityRegexPatternStr, RegexOptions.Compiled);


        IEnumerable<CourseRawData> ParseCourse(string html)
        {
            //    \[[^\[\]]*,[^\[\]]*\d+[^\[\]]*\]
            var matchCC = Regex.Matches(html,"\\[[^\\[\\]]*,[^\\[\\]]*\\d+[^\\[\\]]*\\]");
            foreach (Match match0 in matchCC)
            {
                var match = ParseCourseRegex.Match(match0.ToString());
                if (!match.Success)
                {
                    OutputLog("因为解析失败丢弃一个课程信息");
                    continue;
                }
                yield return new CourseRawData
                                 {
                    开课院系 = match.Groups[1].ToString(),
                    课程号 = match.Groups[2].ToString(),
                    课序号 = match.Groups[3].ToString(),
                    课程名 = HTMLToTxt(match.Groups[4].ToString()),
                    学分 = match.Groups[5].ToString(),
                    主讲教师 = HTMLToTxt(match.Groups[6].ToString()),
                    本科生课容量 = match.Groups[7].ToString(),
                    研究生课容量 = match.Groups[8].ToString(),
                    上课时间 = match.Groups[9].ToString(),
                    年级 = match.Groups[10].ToString(),
                    课程特色 = match.Groups[11].ToString(),
                    本科文化素质课组 = match.Groups[12].ToString(),
                    选课文字说明 = match.Groups[13].ToString(),
                    重修是否占容量 = match.Groups[14].ToString(),
                    是否选课时限制 = match.Groups[15].ToString(),
                    是否二级选课 = match.Groups[16].ToString(),
                    实验信息 = HTMLToTxt(match.Groups[17].ToString())
                };
            }
        }

        public List<CourseRawData> GetCourses(Semester semester)
        {
            string html;
            var rand = new Random(5);

            var courseList = new List<CourseRawData>();

            {
                var url = "http://zhjwxk.cic.tsinghua.edu.cn/xkYjs.vxkYjsJxjhBs.do?m=kkxxSearch&p_xnxq=" + semester.Id;
                var request = _httpHelper.CreateHTTPGetRequest(url, true);
                html = HTTPHelper.GetResponseString(request);
            }

            courseList.AddRange(ParseCourse(html));

            var token = getToken(html);
            var match = Regex.Match(html, "第 1 页 / 共 (\\d+) 页");
            var pageCount = int.Parse(match.Groups[1].ToString());

            OutputLog(string.Format("共计 {0} 页", pageCount));

            OutputLog("成功抓取第 1 页");
            
            for(int i = 2 ; i <= pageCount ;i++)
            {
                System.Threading.Thread.Sleep(500 + rand.Next(0, 1000));

                var url = "http://zhjwxk.cic.tsinghua.edu.cn/xkYjs.vxkYjsJxjhBs.do";
                var postStr = string.Format("m=kkxxSearch&page={2}&token={0}&p_sort.p1=&p_sort.p2=&p_sort.asc1=true&p_sort.asc2=true&p_xnxq={1}&pathContent=&showtitle=&p_kkdwnm=&p_kch=&p_kcm=&p_zjjsxm=&p_kcflm=&p_skxq=&p_skjc=&p_xkwzsm=&p_kctsm=&p_kctsm_new_value=false&p_ssnj=&p_rxklxm=&goPageNumber={3}",
                    token, semester.Id, i, i - 1);
            
                var request = _httpHelper.CreateHTTPPOSTRequest(url, postStr, true);
                html = HTTPHelper.GetResponseString(request);

                OutputLog(string.Format("成功抓取第 {0} 页", i));

                courseList.AddRange(ParseCourse(html));
            }
            
            return courseList;
        }

        IEnumerable<CourseRemainCapacity> ParseCourseRemainCapacity(string html)
        {
            //    \[[^\[\]]*,[^\[\]]*\d+[^\[\]]*\]
            var matchCC = Regex.Matches(html, "\\[[^\\[\\]]*,[^\\[\\]]*\\d+[^\\[\\]]*\\]");
            foreach (Match match0 in matchCC)
            {
                var match = ParseCourseRemainCapacityRegex.Match(match0.ToString());
                if (!match.Success)
                {
                    OutputLog("因为解析失败丢弃一个课程信息");
                    continue;
                }

                yield return new CourseRemainCapacity
                                 {
                                     kch课程号 = match.Groups[1].ToString(),
                                     kxh课序号 = match.Groups[2].ToString(),
                                     kcm课程名 = match.Groups[3].ToString(),
                                     krl课容量 = match.Groups[4].ToString(),
                                     kyl课余量 = match.Groups[5].ToString(),
                                     zjjs主讲教师 = match.Groups[6].ToString(),
                                     sks上课时间 = match.Groups[7].ToString()
                                 };
            }
        }

        public List<CourseRemainCapacity> GetCourseRemainCapacity(Semester semester)
        {
            string html;
            var rand = new Random(5);

            var courseList = new List<CourseRemainCapacity>();

            {
                var url = "http://zhjwxk.cic.tsinghua.edu.cn/xkBks.vxkBksXkbBs.do?m=xkqkSearch&p_xnxq=" + semester.Id;
                var request = _httpHelper.CreateHTTPGetRequest(url, true);
                html = HTTPHelper.GetResponseString(request);
            }

            courseList.AddRange(ParseCourseRemainCapacity(html));

            var token = getToken(html);
            var match = Regex.Match(html, "第 1 页 / 共 (\\d+) 页");
            int pageCount = int.Parse(match.Groups[1].ToString());

            OutputLog(string.Format("共计 {0} 页", pageCount));

            OutputLog("成功抓取第 1 页");

            for (int i = 2; i <= pageCount; i++)
            {
                System.Threading.Thread.Sleep(500 + rand.Next(0, 1000));

                var url = "http://zhjwxk.cic.tsinghua.edu.cn/xkBks.vxkBksJxjhBs.do";
                var postStr = string.Format("m=kylSearch&page={2}&token={0}&p_sort.p1=&p_sort.p2=&p_sort.asc1=&p_sort.asc2=&p_xnxq={1}&pathContent=课余量查询&p_kch=&p_kxh=&p_kcm=&p_skxq=&p_skjc=&goPageNumber={3}",
                    token, semester.Id, i, i - 1);

                var request = _httpHelper.CreateHTTPPOSTRequest(url, postStr, true);
                html = HTTPHelper.GetResponseString(request);

                OutputLog(string.Format("成功抓取第 {0} 页", i));

                courseList.AddRange(ParseCourseRemainCapacity(html));
            }

            return courseList;
        }
    }
}
