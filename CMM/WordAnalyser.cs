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

        //词法分析结果
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

        static int line;


        /// <summary>
        /// 读取下一个字符，若读到末尾返回\0
        /// </summary>
        /// <returns>字符值</returns>
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

        /// <summary>
        /// 当前字符加入缓冲
        /// </summary>
        static void Concat()
        {
            buffer += ch;
        }

        /// <summary>
        /// 回退到上一字符
        /// </summary>
        static void Retract()
        {
            ch = input[--index];
        }

        /// <summary>
        /// 读取单词到词法分析结果
        /// </summary>
        static void Read()
        {
            token = new Token
            {
                StrValue = buffer,
                TokenType = value,
                LineNum = line
            };
            result.Tokens.Add(token);
            nametab tab = new nametab(buffer);
            NameTable.tabs.Add(tab);
            buffer = "";
        }

        /// <summary>
        /// 分析特殊符号开头的单词
        /// </summary>
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
                        value = TerminalType.NOTES;
                        Read();
                        //跳过注释
                        while (Peek() != '*' || Peek() != '/')
                        {
                            AnalyseRow();
                            if (ch == '\0')
                                throw new Exception("注释错误：缺少注释结尾");
                        }
                        return;
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
                    throw new Exception("符号错误：未知符号");
            }
            Read();

        }

        /// <summary>
        /// 分析数字
        /// </summary>
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
                throw new Exception("数字错误：存在多个小数点/数字不应包含字符串");
            Retract();
            if (ch == '.')
                throw new Exception("数字错误：小数点后应该是数字");
            Read();
        }

        /// <summary>
        /// 分析标识符和保留字
        /// </summary>
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
                throw new Exception("标识符错误：标识符格式不正确");
            Read();

        }

        static void AnalyseRow()
        {
            buffer = "";
            if (ch == '\r')
            {
                if (Peek() == '\n')
                    line++;
                else
                    Retract();
            }
        }

        /// <summary>
        /// 词法分析程序主体
        /// </summary>
        /// <param name="inputStr">要分析的文本</param>
        /// <returns>词法分析结果</returns>
        public static TokenResult Analyse(string inputStr)
        {
            if (inputStr == null || inputStr == "")
                return new TokenResult();
            input = inputStr;
            index = 0;
            line = 1;
            buffer = "";
            ch = input[index];

            isAnalysing = true;
            result = new TokenResult();

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
                        AnalyseRow();
                    else
                        throw new Exception("符号错误：未知符号");
                }
                catch (Exception ex)
                {
                    error = new ErrorInfo(line, ex.Message);
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

            result.IsSuccess = result.ErrorInfos.Count == 0;

            return result;
        }

        /// <summary>
        /// 当前用户输入的所有变量
        /// </summary>
        /// <param name="inputStr"></param>
        /// <returns></returns>
        public static List<string> GetAllVariables(string inputStr)
        {
            // 用于接收结果的string
            List<string> variables = new List<string>();

            TokenResult result = Analyse(inputStr);
            List<Token> tokens = result.Tokens;
            tokens.RemoveAt(tokens.Count - 1);
            if(tokens.Count > 0)
            {
                tokens.RemoveAt(tokens.Count - 1);
            }

            for(int i = 0; i < tokens.Count; i++)
            {
                if(tokens[i].TokenType == TerminalType.ID)
                {
                    variables.Add(tokens[i].StrValue);
                }
            }

            return variables;
        }

    }

}
