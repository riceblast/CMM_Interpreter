using CMM.table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CMM
{
    class RowAnalyser
    {
        public static List<RowTabel> run( string input)
        {
            int line = 1;       //第几行
            int n = 1;          //第几个
            int count = 0;    //总的表下标
            List<RowTabel> rowTabel = new List<RowTabel>();
            //总的表
            TokenResult result = WordAnalyser.Analyse(input);
            //每一行字符串
            System.IO.StringReader sr = new System.IO.StringReader(input);
            string str = sr.ReadLine();
            //每一行的表
            TokenResult temp = WordAnalyser.Analyse(str);

            for (; ; )
            {
                //每一个单词逐步对应
                for (int i = 0; i < temp.Tokens.Count; i++)
                {
                    //如果相等，直接打印
                    if (temp.Tokens[i].StrValue == result.Tokens[count].StrValue)
                    {
                        rowTabel.Add(new RowTabel()
                        {
                            Name = result.Tokens[count].StrValue,
                            Id = result.Tokens[count].TokenType,
                            Row = line,
                            Num = n
                        });
                        n++;
                        count++;
                    }
                    //如果不相等
                    //1、说明表中下一个单词是多行的
                    else if (result.Tokens[count].StrValue.IndexOf(temp.Tokens[i].StrValue) == 0)
                    {
                        //获取多行单词的行数
                        Console.WriteLine(result.Tokens[count].StrValue + "    " + line + "    " + n);
                        int t = huanHangCiShu(result.Tokens[count].StrValue);
                        while (t > 1)
                        {
                            str = sr.ReadLine();
                            line++;
                            t--;
                        }
                        count++;
                        break;
                    }
                    //2、上一个多行单词“残留”在本行
                    //①只有该残留的单词
                    else if (temp.Tokens.Count == 1)
                    {
                        break;
                    }
                    else
                    //②残留单词后方还有单词
                    {
                        n++;
                    }
                }
                //重新获取下一行的数据
                str = sr.ReadLine();
                if (str == null)
                    break;
                temp = WordAnalyser.Analyse(str);
                line++;
                n = 1;
                //直到读完才退出
                if (count == result.Tokens.Count)
                    break;
            }
            return rowTabel;
        }
        //返回字符串中的换行次数
        private static int huanHangCiShu(string s)
        {
            int num = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\r' && s[i + 1] == '\n')
                {
                    num++;
                }
            }
            return num;
        }
    }
}
