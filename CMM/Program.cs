using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CMM
{
    class Program
    {
        private void run()
        {
            int line = 1;       //第几行
            int n = 1;          //第几个
            int count = 0;    //总的表下标

            string test = @"a b c d  e f g h  i j k l /*a*/  /*as
               aa
               a*/ m n o p /*as
               aa
               a*/
               m n o p";
            //总的表
            List<(string name, int)> tokens = WordAnalyser.Analyse(test);
            Console.WriteLine("单词" + "    " + "行数" + "    " + "位置");
            //每一行字符串
            System.IO.StringReader sr = new System.IO.StringReader(test);
            string str = sr.ReadLine();
            //每一行的表
            List<(string name, int)> temp = WordAnalyser.Analyse(str);

            for (; ; )
            {
                //每一个单词逐步对应
                for (int i = 0; i < temp.Count; i++)
                {
                    //如果相等，直接打印
                    if (temp[i].name == tokens[count].name)
                    {
                        Console.WriteLine(tokens[count].name + "    " + line + "    " + n);
                        n++;
                        count++;
                    }
                    //如果不相等
                    //1、说明表中下一个单词是多行的
                    else if (tokens[count].name.IndexOf(temp[i].name) == 0)
                    {
                        //获取多行单词的行数
                        Console.WriteLine(tokens[count].name + "    " + line + "    " + n);
                        int t = huanHangCiShu(tokens[count].name);
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
                    else if (temp.Count == 1)
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
                if (count == tokens.Count)
                    break;
            }
            //foreach (KeyValuePair<string, int> item in kv)
            //{

            //    if (str.Contains(item.Key)))
            //    {
            //        Console.WriteLine(item.Key + "    " + line + "    " + n);
            //        n++;
            //    }

            //    else
            //    {
            //        Console.WriteLine(item.Key + "    " + line + "    " + n);
            //        line += huanHangCiShu(item.Key);
            //        n = 1;
            //    }
            //}

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
