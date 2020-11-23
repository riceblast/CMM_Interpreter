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
            int line=1;       //第几行
            int n=1;          //第几个
            int count = 0;    //总的表下标

            string test = @"a b c d  e f g h  i j k l /*a*/  /*as
               aa
               a*/ m n o p /*as
               aa
               a*/
               m n o p";
            //总的表
            List<KeyValuePair<string, int>> kv = WordAnalyse.input(test);
            Console.WriteLine("单词" + "    " + "行数" + "    "+"位置");
            //每一行字符串
            System.IO.StringReader sr = new System.IO.StringReader(test);
            string str = sr.ReadLine();
            //每一行的表
            List<KeyValuePair<string, int>> Tempkv = WordAnalyse.input(str);

            for (; ; )
            {
                //每一个单词逐步对应
                for (int i = 0; i < Tempkv.Count; i++)
                {
                    //如果相等，直接打印
                    if (Tempkv[i].Key == kv[count].Key)
                    {
                        Console.WriteLine(kv[count].Key + "    " + line + "    " + n);
                        n++;
                        count++;
                    }
                    //如果不相等
                    //1、说明表中下一个单词是多行的
                    else if (kv[count].Key.IndexOf(Tempkv[i].Key) == 0)
                    {
                        //获取多行单词的行数
                        Console.WriteLine(kv[count].Key + "    " + line + "    " + n);
                        int t = huanHangCiShu(kv[count].Key);
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
                    else if (Tempkv.Count == 1)
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
                Tempkv = WordAnalyse.input(str);
                line++;
                n = 1;
                //直到读完才退出
                if (count == kv.Count)
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
