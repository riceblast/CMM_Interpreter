using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CMM
{
    class WordAnalyser
    {
        static List<(string, int)> tokens;

        static string[] keywords = { "if", "else", "while", "read", "write", "int", "real" };
        static string[] symbols = { "+", "-", "*", "/", "=", "<", ">", "==", "<>", "(", ")", ";", "{", "}", "/*", "*/", "[", "]", ",", "." };

        static int sStart = keywords.Length;
        static int oStart = symbols.Length;

        static string buffer;
        static int index;
        static char ch;
        static string input;
        static int value;

        static char Peek()
        {
            ++index;
            try
            {
                ch = input[index];
            }
            catch(IndexOutOfRangeException)
            {
                ch = '\0';
            }
            return ch;
        }

        static void Concat()
        {
            buffer += ch;
        }

        static void Retract()
        {
            ch = input[--index];
        }

        static void Read()
        {
            tokens.Add((buffer, value));
            buffer = "";
        }

        static void AnalyseSymbol()
        {

            switch (ch)
            {
                case '=':
                    if (Peek() == '=')
                        Concat();
                    break;
                case '<':
                    if (Peek() == '>')
                        Concat();
                    break;
                case '/':
                    if (Peek() != '*' && ch != '\0')
                    {
                        Retract();
                        break;
                    }
                    buffer = "";
                    while ((Peek() != '*' || Peek() != '/') && ch != '\0') ;
                    return;
            }

            value = Array.IndexOf(symbols, buffer) + sStart;
            if (value == -1)
                throw new Exception();
            Read();

        }

        static void AnalyseNumber()
        {
            int point = 0;

            for (Peek(); Char.IsDigit(ch) || ch == '.'; Peek())
            {
                if (Char.IsDigit(ch))
                {
                    Concat();
                    continue;
                }
                if (buffer.Contains('.'))
                    throw new Exception();
                if (!Char.IsDigit(Peek()))
                    throw new Exception();
                if (ch != '\0')
                    Retract();
                Concat();
            }
            if (ch != '\0')
                Retract();
            value = oStart;
            if (point == 1)
                value = oStart + 1;
            Read();
        }

        static void AnalyseIdAndKey()
        {

            for (Peek(); Char.IsLetterOrDigit(ch) || ch == '_'; Peek())
            {
                Concat();
            }
            if (ch != '\0')
                Retract();
            value = Array.IndexOf(keywords, buffer);
            if (value == -1)
                value = oStart + 1;
            if (buffer.Last() == '_')
                throw new Exception();
            Read();

        }

        public static List<(string, int)> Analyse(string inputStr)
        {
            if (inputStr == "")
                return null;
            input = inputStr;
            tokens = new List<(string, int)>();
            index = 0;
            buffer = "";
            ch = input[index];
            
            do
            {
                Concat();
                try
                {
                    if (Array.IndexOf(symbols, buffer) != -1)
                        AnalyseSymbol();
                    else if (Char.IsDigit(ch))
                        AnalyseNumber();
                    else if (Char.IsLetter(ch) || ch == '_')
                        AnalyseIdAndKey();
                    else if (Char.IsWhiteSpace(ch)) ;
                    else
                        throw new Exception();
                    Peek();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
            } while (ch != '\0');

            return tokens;
        }

    }
}
