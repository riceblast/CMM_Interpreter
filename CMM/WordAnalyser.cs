using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CMM
{

    public class NameTable
    {
        public static List<nametab> tabs = new List<nametab>();
    }

    public class WordAnalyser
    {

        //单词
        static TokenResult result;
        static Token token;
        static ErrorInfo error;
        //关键字和特殊符号
        static string[] keywords = { "if", "else", "while", "read", "write", "int", "real" };
        static string[] symbols = { "+", "-", "*", "/", "=", "<", ">", "==", "<>", "(", ")", ";", "{", "}", "/*", "*/", "[", "]", ",", "." };

        static string buffer;
        static int index;
        static char ch;
        static string input;
        static TerminalType value;
        static bool isAnalysing;

        //读取下一个字符
        static char Peek()
        {
            ++index;
            if (index < input.Length)
            {
                ch = input[index];
            }
            else
            {
                isAnalysing = false;
                ch = '\0';
            }
            return ch;
        }
        //加入缓冲
        static void Concat()
        {
            buffer += ch;
        }
        //回退
        static void Retract()
        {
            ch = input[--index];
        }
        //读取单词
        static void Read()
        {
            token = new Token();
            token.StrValue = buffer;
            token.TokenType = value;

            result.Tokens.Add(token);
            nametab tab = new nametab(buffer);
            NameTable.tabs.Add(tab);
            buffer = "";
        }
        //分析特殊符号
        static void AnalyseSymbol()
        {

            switch (ch)
            {
                case '=':
                    if (Peek() == '=')
                    {
                        value = TerminalType.EQUAL;
                        Concat();
                    }
                    else
                    {
                        Retract();
                        value = TerminalType.ASSIGN;
                    }
                    break;
                case '<':
                    if (Peek() == '>')
                    {
                        value = TerminalType.NOTEQUAL;
                        Concat();
                    }
                    else
                    {
                        Retract();
                        value = TerminalType.LESS;
                    }
                    break;
                case '/':
                    if (Peek() == '*')
                    {
                        buffer = "";
                        //跳过注释
                        while (Peek() != '*' || Peek() != '/')
                        {
                            if (ch == '\0')
                                throw new Exception("注释错误");
                        }
                        value = TerminalType.NOTES;
                    }
                    else
                    {
                        Retract();
                        value = TerminalType.DIV;
                    }
                    break;
                case '+':
                    value = TerminalType.PLUS;
                    break;
                case '-':
                    value = TerminalType.MINUS;
                    break;
                case '*':
                    value = TerminalType.MUL;
                    break;
                case '>':
                    value = TerminalType.GREATER;
                    break;
                case '(':
                    value = TerminalType.LPARENT;
                    break;
                case ')':
                    value = TerminalType.RPARENT;
                    break;
                case ';':
                    value = TerminalType.SEMI;
                    break;
                case '{':
                    value = TerminalType.LBRACE;
                    break;
                case '}':
                    value = TerminalType.RBRACE;
                    break;
                case '[':
                    value = TerminalType.LBRACKET;
                    break;
                case ']':
                    value = TerminalType.RBRACKET;
                    break;
                case ',':
                    value = TerminalType.COMMA;
                    break;
                default:
                    throw new Exception("特殊符号错误");
            }
            Read();

        }
        //分析数字
        static void AnalyseNumber()
        {
            while (Char.IsDigit(Peek()))
            {
                Concat();
            }
            value = TerminalType.INTVAL;
            if (ch == '.')
            {
                Concat();
                while (Char.IsDigit(Peek()))
                {
                    Concat();
                }
                value = TerminalType.REALVAL;
            }
            if (Char.IsLetter(ch) || ch == '.')
                throw new Exception("数字错误");
            Retract();
            if (ch == '.')
                throw new Exception("数字错误");
            Read();
        }
        //分析标识符和保留字
        static void AnalyseIdAndKey()
        {

            while (Char.IsLetterOrDigit(Peek()) || ch == '_')
            {
                Concat();
            }
            switch (Array.IndexOf(keywords, buffer))
            {
                case 0:
                    value = TerminalType.IF;
                    break;
                case 1:
                    value = TerminalType.ELSE;
                    break;
                case 2:
                    value = TerminalType.WHILE;
                    break;
                case 3:
                    value = TerminalType.READ;
                    break;
                case 4:
                    value = TerminalType.WRITE;
                    break;
                case 5:
                    value = TerminalType.INT;
                    break;
                case 6:
                    value = TerminalType.REAL;
                    break;
                default:
                    value = TerminalType.ID;
                    break;
            }
            Retract();
            if (ch == '_')
                throw new Exception("标识符错误");
            Read();

        }

        //词法分析
        public static TokenResult Analyse(string inputStr)
        {
            if (inputStr == "")
                return null;
            input = inputStr;
            index = 0;
            buffer = "";
            ch = input[index];

            isAnalysing = true;
            result = new TokenResult();
            result.Tokens = new List<Token>();
            result.ErrorInfos = new List<ErrorInfo>();


            while (isAnalysing)
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
                    else if (Char.IsWhiteSpace(ch))
                        buffer = "";
                    else
                        throw new Exception("未知符号错误");
                }
                catch (Exception ex)
                {
                    error = new ErrorInfo();
                    error.Message = ex.Message;
                    result.ErrorInfos.Add(error);
                }
                finally
                {
                    Peek();
                }
            }

            // 添加结束符
            buffer = "$";
            value = TerminalType.END;
            Read();

            result.IsSuccess = result.ErrorInfos.Count == 0 ? true : false;

            return result;
        }

    }

}
