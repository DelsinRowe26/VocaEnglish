using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VocaEnglish
{
    public class ListWords
    {
        public static string[] EnWords = new string[40];
        public static string[] RuWords = new string[40];
        public static string[] Transcription = new string[40];
        public static int Values;

        public void List()
        {
            StreamReader fileName = new StreamReader(@"VocaEnglish\Words\ListWords.txt", System.Text.Encoding.UTF8);
            int Nlines = File.ReadAllLines(@"VocaEnglish\Words\ListWords.txt").Length;
            string[] txt = fileName.ReadToEnd().Split(new char[] { '/', ';' }, StringSplitOptions.RemoveEmptyEntries);
            int Nvalues = txt.Length;
            Values = Nlines;

            for (int p = 0, q = 0, r = 1, e = 2; p < Nlines && q < Nvalues && r < Nvalues && e < Nvalues; p++, q += 3, r += 3, e += 3)
            {
                EnWords[p] = txt[e].ToString();
                RuWords[p] = txt[r].ToString();
                Transcription[p] = txt[q].ToString();
            }
        }
    }
}
