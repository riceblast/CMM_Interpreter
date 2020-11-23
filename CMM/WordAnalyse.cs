using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CMM
{
    class WordAnalyse
    {
        private static List<KeyValuePair<string, int>> kv = new List<KeyValuePair<string, int>>();

        //标识符
        private static Regex identifier = new Regex(@"^[A-Za-z]\w*[A-Za-z0-9]|[A-Za-z]$");

        //整数
        private static Regex intNumber = new Regex(@"^[-+]?\d+$");

        //浮点数
        private static Regex floatNumber = new Regex(@"^[-+]?\d+(\.\d+)?$");

        //注释及多行注释
        private static Regex notes = new Regex(@"^/\*[\w\W]*?\*/$");

        //数组:array[]
        private static Regex array1 = new Regex(@"^([A-Za-z]\w*[A-Za-z0-9]|[A-Za-z])\[\]$");

        //数组:array[x]
        private static Regex array2 = new Regex(@"^([A-Za-z]\w*[A-Za-z0-9]|[A-Za-z])\[(\d+)\]$");

        private static string[] words = { "+", "-", "*", "/", "=", "<", ">", "==", "<>", "(", ")", ";", "{", "}", "/*", "*/", "[", "]", ",", "If", "else", "while", "read", "write", "int", "real" };

        public static int analyse(string word)
        {
            if (words.Contains(word))
            {
                return Array.IndexOf(words, word);
            }
            else if (identifier.IsMatch(word))
            {
                return 26;
            }
            else if (intNumber.IsMatch(word))
            {
                return 27;
            }
            else if (floatNumber.IsMatch(word))
            {
                return 28;
            }
            else if (notes.IsMatch(word))
            {
                return 29;
            }
            else
            {
                return -1;
            }
        }

        public static void Match(string m)
        {
            if (array1.IsMatch(m))
            {
                Match result = array1.Match(m);
                kv.Add(new KeyValuePair<string, int>(result.Groups[1].Value, analyse(result.Groups[1].Value)));
                kv.Add(new KeyValuePair<string, int>("[", analyse("[")));
                kv.Add(new KeyValuePair<string, int>("]", analyse("]")));
            }
            else if (array2.IsMatch(m))
            {
                Match result = array2.Match(m);
                kv.Add(new KeyValuePair<string, int>(result.Groups[1].Value, analyse(result.Groups[1].Value)));
                kv.Add(new KeyValuePair<string, int>("[", analyse("[")));
                kv.Add(new KeyValuePair<string, int>(result.Groups[2].Value, analyse(result.Groups[2].Value)));
                kv.Add(new KeyValuePair<string, int>("]", analyse("]")));
            }
            else
            {
                kv.Add(new KeyValuePair<string, int>(m, analyse(m)));
            }
        }

        public static List<KeyValuePair<string, int>> input(string input)
        {
            MatchCollection mc = Regex.Matches(input, @"/\*[\w\W]*?\*/|\S+");
            foreach (Match m in mc)
            {
                Match(m.ToString());
            }
            return kv;
        }
    }
}
