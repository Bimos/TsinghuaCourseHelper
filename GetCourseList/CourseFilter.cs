using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TsinghuaCourseHelper
{
    class CourseFilter
    {
        public static IEnumerable<CourseInfo> Filter(IEnumerable<CourseInfo> source)
        {
            return source.Where(c => if选课限制(c.选课文字说明))
                .Where(c => !c.选课文字说明.Contains("网上不选课"))
                .Where(c => !c.kcm课程名.Contains("第二外国语"))
                .Where(c => !c.kcm课程名.Contains("博士生英语"))
                .Where(c => !c.选课文字说明.Contains("MBA课程"))
                //.Where(c=> c.kcm课程名.Contains("英语"))
                ;
        }

        static bool if选课限制(string str)
        {
            if (str.StartsWith("限"))
            {
                if (str.Contains("工物"))
                    return true;
                else
                    return false;
            }
            return true;
        }
    }
}
