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
        //private static List<KeyValuePair<string, int>> kv = new List<KeyValuePair<string, int>>();

        private static List<(string, int)> tokens;

        ////标识符
        //private static Regex identifier = new Regex(@"^[A-Za-z]\w*[A-Za-z0-9]|[A-Za-z]$");

        ////整数
        //private static Regex intNumber = new Regex(@"^[-+]?\d+$");

        ////浮点数
        //private static Regex floatNumber = new Regex(@"^[-+]?\d+(\.\d+)?$");

        ////注释及多行注释
        //private static Regex notes = new Regex(@"^/\*[\w\W]*?\*/$");

        ////数组:array[]
        //private static Regex array1 = new Regex(@"^([A-Za-z]\w*[A-Za-z0-9]|[A-Za-z])\[\]$");

        ////数组:array[x]
        //private static Regex array2 = new Regex(@"^([A-Za-z]\w*[A-Za-z0-9]|[A-Za-z])\[(\d+)\]$");

        //private static string[] words = { "+", "-", "*", "/", "=", "<", ">", "==", "<>", "(", ")", ";", "{", "}", "/*", "*/", "[", "]", ",", "If", "else", "while", "read", "write", "int", "real" };

        private static string[] words = { "if", "else", "while", "read", "write", "int", "real" };

        private static string[] special = { "+", "-", "*", "/", "=", "<", ">", "==", "<>", "(", ")", ";", "{", "}", "/*", "*/", "[", "]", ",", "." };


        //public static int analyse(string word)
        //{
        //    if (words.Contains(word))
        //    {
        //        return Array.IndexOf(words, word);
        //    }
        //    else if (identifier.IsMatch(word))
        //    {
        //        return 26;
        //    }
        //    else if (intNumber.IsMatch(word))
        //    {
        //        return 27;
        //    }
        //    else if (floatNumber.IsMatch(word))
        //    {
        //        return 28;
        //    }
        //    else if (notes.IsMatch(word))
        //    {
        //        return 29;
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        //public static void Match(string m)
        //{
        //    if (array1.IsMatch(m))
        //    {
        //        Match result = array1.Match(m);
        //        kv.Add(new KeyValuePair<string, int>(result.Groups[1].Value, analyse(result.Groups[1].Value)));
        //        kv.Add(new KeyValuePair<string, int>("[", analyse("[")));
        //        kv.Add(new KeyValuePair<string, int>("]", analyse("]")));
        //    }
        //    else if (array2.IsMatch(m))
        //    {
        //        Match result = array2.Match(m);
        //        kv.Add(new KeyValuePair<string, int>(result.Groups[1].Value, analyse(result.Groups[1].Value)));
        //        kv.Add(new KeyValuePair<string, int>("[", analyse("[")));
        //        kv.Add(new KeyValuePair<string, int>(result.Groups[2].Value, analyse(result.Groups[2].Value)));
        //        kv.Add(new KeyValuePair<string, int>("]", analyse("]")));
        //    }
        //    else
        //    {
        //        kv.Add(new KeyValuePair<string, int>(m, analyse(m)));
        //    }
        //}

        public static List<(string, int)> analyse(string input)
        {
            //MatchCollection mc = Regex.Matches(input, @"/\*[\w\W]*?\*/|\S+");
            //foreach (Match m in mc)
            //{
            //    Match(m.ToString());
            //}
            //return kv;
            tokens = new List<(string, int)>();
            char currunt;
            string buffer;
            int length = input.Length;
            int sStart = words.Length;
            int oStart = sStart + special.Length;

            for (int index = 0; index < length; index++)
            {
                currunt = input[index];
                buffer = currunt.ToString();

                if (Array.IndexOf(special, buffer) != -1)
                {
                    switch (currunt)
                    {
                        case '=':
                        case '<':
                            if (index + 1 == length)
                                break;
                            currunt = input[index + 1];
                            if (Array.IndexOf(special, buffer + currunt) != -1)
                            {
                                buffer += currunt;
                                index++;
                            }
                            tokens.Add((buffer, Array.IndexOf(special, buffer) + sStart));
                            break;
                        case '/':
                            if (index + 1 == length)
                                break;
                            if (input[index + 1] == '*')
                            {
                                index++;
                                while (++index < length)
                                {
                                    while (input[index] != '*')
                                        continue;
                                    if (input[index + 1] == '/')
                                    {
                                        index++;
                                        break;
                                    }
                                }
                            }
                            break;
                        default:
                            tokens.Add((buffer, Array.IndexOf(special, buffer) + sStart));
                            break;
                    }
                }
                else if (Char.IsDigit(currunt))
                {
                    while (++index < length)
                    {
                        currunt = input[index];
                        if (!Char.IsDigit(currunt) && currunt != '.')
                        {
                            index--;
                            break;
                        }
                        buffer += currunt;
                    }
                    if (buffer.Contains('.'))
                        tokens.Add((buffer, oStart + 1));

                    tokens.Add((buffer, oStart));
                }
                else if (Char.IsLetter(currunt))
                {
                    while (++index < length)
                    {
                        currunt = input[index];
                        if (!Char.IsLetterOrDigit(currunt) && currunt != '_')
                        {
                            index--;
                            break;
                        }
                        buffer += currunt;
                    }
                    int t = Array.IndexOf(words, buffer);
                    if (t != -1)
                        tokens.Add((buffer, t));
                    else if (buffer.Last() == '_') ;
                    else
                        tokens.Add((buffer, oStart + 1));
                }
                else if (Char.IsWhiteSpace(currunt))
                    continue;
                else
                    throw new Exception();

            }

            return tokens;
        }
        
    }
}
