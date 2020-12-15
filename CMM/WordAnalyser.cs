using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CMM
{

    class NameTable
    {
        public static List<nametab> tabs = new List<nametab>();
    }

    public enum TokenType
    {
        IF,
        ELSE,
        WHILE,
        READ,
        WRITE,
        INT,
        REAL,
        PLUS,
        MINUS,
        MUL,
        DIV,
        ASSIGN,
        LESS,
        GREATER,
        EQUAL,
        NOTEQUAL,
        LPARENT,
        RPARENT,
        SEMI,
        LBRACE,
        RBRACE,
        NOTES,
        LBRACKET,
        RBRACKET,
        COMMA,
        INTVAL,
        REALVAL,
        ID,
        ERR
    }

    class WordAnalyser
    {
        
        //单词
        static List<(string, TokenType)> tokens;
        //关键字和特殊符号
        static string[] keywords = { "if", "else", "while", "read", "write", "int", "real" };
        static string[] symbols = { "+", "-", "*", "/", "=", "<", ">", "==", "<>", "(", ")", ";", "{", "}", "/*", "*/", "[", "]", ",", "." };

        static string buffer;
        static int index;
        static char ch;
        static string input;
        static TokenType value;
        static bool flag;

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
                flag = false;
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
            tokens.Add((buffer, value));
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
                        value = TokenType.EQUAL;
                        Concat();
                    }
                    else
                    {
                        Retract();
                        value = TokenType.ASSIGN;
                    }
                    break;
                case '<':
                    if (Peek() == '>')
                    {
                        value = TokenType.NOTEQUAL;
                        Concat();
                    }
                    else
                    {
                        Retract();
                        value = TokenType.LESS;
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
                        value = TokenType.NOTES;
                    }
                    else
                    {
                        Retract();
                        value = TokenType.DIV;
                    }
                    break;
                case '+':
                    value = TokenType.PLUS;
                    break;
                case '-':
                    value = TokenType.MINUS;
                    break;
                case '*':
                    value = TokenType.MUL;
                    break;
                case '(':
                    value = TokenType.LPARENT;
                    break;
                case ')':
                    value = TokenType.RPARENT;
                    break;
                case ';':
                    value = TokenType.SEMI;
                    break;
                case '{':
                    value = TokenType.LBRACE;
                    break;
                case '}':
                    value = TokenType.RBRACE;
                    break;
                case '[':
                    value = TokenType.LBRACKET;
                    break;
                case ']':
                    value = TokenType.RBRACKET;
                    break;
                case ',':
                    value = TokenType.COMMA;
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
            value = TokenType.INTVAL;
            if (ch == '.')
            {
                Concat();
                while (Char.IsDigit(Peek()))
                {
                    Concat();
                }
                value = TokenType.REALVAL;
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
            switch(Array.IndexOf(keywords, buffer))
            {
                case 0:
                    value = TokenType.IF;
                    break;
                case 1:
                    value = TokenType.ELSE;
                    break;
                case 2:
                    value = TokenType.WHILE;
                    break;
                case 3:
                    value = TokenType.READ;
                    break;
                case 4:
                    value = TokenType.WRITE;
                    break;
                case 5:
                    value = TokenType.INT;
                    break;
                case 6:
                    value = TokenType.REAL;
                    break;
                default:
                    value = TokenType.ID;
                    break;
            }
            Retract();
            if (ch == '_')
                throw new Exception("标识符错误");
            Read();

        }
        //词法分析
        public static List<(string, TokenType)> Analyse(string inputStr)
        {
            if (inputStr == "")
                return null;
            input = inputStr;
            tokens = new List<(string, TokenType)>();
            index = 0;
            buffer = "";
            ch = input[index];
            flag = true;

            while (flag)
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
                    buffer = ex.Message;
                    value = TokenType.ERR;
                    Read();
                }
                finally
                {
                    Peek();
                }
            } 

            return tokens;
        }

    }

}
