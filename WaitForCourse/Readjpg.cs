using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace Locke_CourseSystem
{
    public class Readjpg
    {
        List<byte[,]> charBitmapList;
        List<string> charList;
        
        public Readjpg()
        {
            string[] filenames = Directory.GetFiles(Directory.GetCurrentDirectory()+"\\gif\\", "*.gif");
            charBitmapList = new List<byte[,]>();
            charList = new List<string>();
            foreach (string path in filenames)
            {
                Bitmap temp = (Bitmap)Bitmap.FromFile(path);
                byte[,] bmp = new byte[temp.Width, temp.Height];
                for (int i = 0; i < temp.Width; i++)
                    for (int j = 0; j < temp.Height; j++)
                        if (temp.GetPixel(i, j).B == 0)
                            bmp[i, j] = 0;
                        else
                            bmp[i, j] = 255;
                charBitmapList.Add(bmp);
                string name = Path.GetFileName(path).ToUpperInvariant();
                name = Regex.Replace(name, "\\.GIF", "");
                name = Regex.Replace(name, "-.*", "");
                charList.Add(name);
            }
        }

        public string Bmp2Text(Stream picdata)
        {
            Bitmap pic = (Bitmap)Bitmap.FromStream(picdata);
            Color pixel;
            byte[,] bmp = new byte[pic.Width, pic.Height];
            for (int x = 0; x < pic.Width; x++)
                for (int y = 0; y < pic.Height; y++)
                {
                    pixel = pic.GetPixel(x, y);
                    if (pixel.R < 128 && pixel.G < 128)
                        bmp[x, y] = 0;
                    else
                        bmp[x, y] = 255;
                }
            pic.Dispose();

            bmp = bmpTrim(bmp);
            List<byte[,]> bmps = bmpSplit(bmp);
            
            string output = "";
            foreach (byte[,] charbmp in bmps)
            {
                string re = matchBitmap(charbmp);
                if (re == "")
                    return null;
                output += re;
            }
            return output;
        }

        private byte[,] bmpTrim(byte[,] bmp)
        {
            int x0, x1, y0, y1;
            for (x0 = 0; x0 < bmp.GetLength(0); x0++)
            {
                bool check = true;
                for (int j = 0; j < bmp.GetLength(1); j++)
                {
                    if (bmp[x0, j] == 0)//black
                        check = false;
                }
                if (check == false)
                    break;
            }
            for (x1 = bmp.GetLength(0) - 1; x1 >= 0; x1--)
            {
                bool check = true;
                for (int j = 0; j < bmp.GetLength(1); j++)
                {
                    if (bmp[x1, j] == 0)//black
                        check = false;
                }
                if (check == false)
                    break;
            }
            for (y0 = 0; y0 < bmp.GetLength(1); y0++)
            {
                bool check = true;
                for (int j = 0; j < bmp.GetLength(0); j++)
                {
                    if (bmp[j, y0] == 0)//black
                        check = false;
                }
                if (check == false)
                    break;
            }
            for (y1 = bmp.GetLength(1) - 1; y1 >= 0; y1--)
            {
                bool check = true;
                for (int j = 0; j < bmp.GetLength(0); j++)
                {
                    if (bmp[j, y1] == 0)//black
                        check = false;
                }
                if (check == false)
                    break;
            }
            byte[,] result = new byte[x1 - x0 + 1, y1 - y0 + 1];
            for (int i = 0; i < x1 - x0 + 1; i++)
                for (int j = 0; j < y1 - y0 + 1; j++)
                    result[i, j] = bmp[x0 + i, y0 + j];
            return result;
        }

        private List<byte[,]> bmpSplit(byte[,] inputbmp)
        {
            byte[,] bmp = (byte[,])inputbmp.Clone();
            List<byte[,]> result = new List<byte[,]>();
            
            while (true)
            {
                Point none = new Point(-1, -1);
                Point first = none;
                for (int i = 0; i < bmp.GetLength(0); i++)
                {
                    for (int j = 0; j < bmp.GetLength(1); j++)
                        if (bmp[i, j] == 0)
                        {
                            first = new Point(i,j);
                            break;
                        }
                    if (first != none)
                        break;
                }

                if (first == none)//empty
                    break;
                List<Point> blacklist = new List<Point>();
                List<Point> newlist = new List<Point>();
                blacklist.Add(first);
                newlist.Add(first);

                while (newlist.Count > 0)
                {
                    List<Point> nowlist = newlist;
                    newlist = new List<Point>();
                    foreach (Point p in nowlist)
                    {
                        List<Point> ptemp = new List<Point>();

                        if (p.X > 0)
                            ptemp.Add(new Point(p.X - 1, p.Y));

                        if (p.X < bmp.GetLength(0) - 1)
                            ptemp.Add(new Point(p.X + 1, p.Y));

                        if (p.Y > 0)
                            ptemp.Add(new Point(p.X, p.Y - 1));

                        if (p.Y < bmp.GetLength(1) - 1)
                            ptemp.Add(new Point(p.X, p.Y + 1));

                        if (p.X > 0 && p.Y > 0)
                            ptemp.Add(new Point(p.X - 1, p.Y - 1));

                        if (p.X > 0 && p.Y < bmp.GetLength(1) - 1)
                            ptemp.Add(new Point(p.X - 1, p.Y + 1));

                        if (p.X < bmp.GetLength(0) - 1 && p.Y > 0)
                            ptemp.Add(new Point(p.X + 1, p.Y - 1));

                        if (p.X < bmp.GetLength(0) - 1 && p.Y < bmp.GetLength(1) - 1)
                            ptemp.Add(new Point(p.X + 1, p.Y + 1));

                        foreach (Point pp in ptemp)
                        {
                            if (bmp[pp.X, pp.Y] == 0 && blacklist.Contains(pp) == false)
                            {
                                blacklist.Add(pp);
                                newlist.Add(pp);
                            }
                        }
                    }
                }

                int bmpwidth = 0, bmpheight = 0, bmpwidthlow = 255;
                foreach (Point p in blacklist)
                {
                    bmp[p.X, p.Y] = 255;
                    if (p.X > bmpwidth)
                        bmpwidth = p.X;
                    if (p.X < bmpwidthlow)
                        bmpwidthlow = p.X;
                    if (p.Y > bmpheight)
                        bmpheight = p.Y;
                }
                byte[,] newbmp = new byte[bmpwidth + 1 - bmpwidthlow, bmpheight + 1];
                for (int i = 0; i < newbmp.GetLength(0); i++)
                    for (int j = 0; j < newbmp.GetLength(1); j++)
                        newbmp[i, j] = 255;
                foreach (Point p in blacklist)
                    newbmp[p.X - bmpwidthlow, p.Y] = 0;
                result.Add(newbmp);
            }

            return result;
        }

        private string matchBitmap(byte[,] bmp)
        {
            const int limit = 10;
            if (bmp.GetLength(1) > 15)
            {
                if (bmp.GetLength(0) < 12)
                    return "J";
                else if (bmp.GetLength(0) < 20)
                    return "Q";
                else
                    return "";
            }
            else
            {
                int charpos;
                for (charpos = 0; charpos < charBitmapList.Count; charpos++)
                {
                    byte[,] charmap = charBitmapList[charpos];
                    if (bmp.GetLength(0) - charmap.GetLength(0) > 2)
                        continue;
                    if (charmap.GetLength(1) != 15)
                        continue;

                    int dismatchcount = 0;
                    for (int i = 0; i < bmp.GetLength(0) && i < charmap.GetLength(0); i++)
                        for (int j = 0; j < bmp.GetLength(1) && j < charmap.GetLength(1); j++)
                            if (bmp[i, j] != charmap[i, j])
                                dismatchcount++;
                    if (dismatchcount < limit)
                        break;

                    dismatchcount = 0;
                    for (int i = 0; i < bmp.GetLength(0) && i + 1 < charmap.GetLength(0); i++)
                        for (int j = 0; j < bmp.GetLength(1) && j < charmap.GetLength(1); j++)
                            if (bmp[i, j] != charmap[i + 1, j])
                                dismatchcount++;
                    if (dismatchcount < limit)
                        break;

                    dismatchcount = 0;
                    for (int i = 0; i + 1 < bmp.GetLength(0) && i < charmap.GetLength(0); i++)
                        for (int j = 0; j < bmp.GetLength(1) && j < charmap.GetLength(1); j++)
                            if (bmp[i + 1, j] != charmap[i, j])
                                dismatchcount++;
                    if (dismatchcount < limit)
                        break;
                }
                if (charpos < charBitmapList.Count)
                    return charList[charpos];
                else
                    return "";
            }
        }

    }
}
