using CMM.table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CMM
{
    class Constant
    {
        /// <summary>
        /// 输出
        /// </summary>
        private static string output="";
        /// <summary>
        /// 当前函数层数
        /// </summary>
        public static int currentScope=0;
        /// <summary>
        /// 层数表
        /// </summary>
        public static List<ScopeTable> scopeTables=new List<ScopeTable>();
        /// <summary>
        /// 输出委托
        /// </summary>
        public static event Action<string> outPutAppend;
        /// <summary>
        /// 清空委托
        /// </summary>
        public static event Action outPutClean;
        /// <summary>
        /// 控制语义分析的运行
        /// </summary>
        public static ManualResetEvent _mre = new ManualResetEvent(true);

        /// <summary>
        /// 唤醒线程
        /// </summary>
        public static void mreSet() {
            _mre.Set();
        }
        /// <summary>
        /// 停止线程
        /// </summary>
        public static void mreReset()
        {
            _mre.Reset();
        }
        /// <summary>
        /// 输出
        /// </summary>
        /// <param name="s"></param>
        public static void outputAppend(string s) {
            output += s;
            outPutAppend(s);
        }
        /// <summary>
        /// 清空
        /// </summary>
        public static void outputClean()
        {
            output ="";
            outPutClean();
        }
        /// <summary>
        /// 当前层数-1，并去掉符号表中部分值
        /// </summary>
        public static void currentScopeDecrease()
        {
            currentScope -= 1;
            List<ScopeTable> tables = new List<ScopeTable>();
            foreach (ScopeTable table in scopeTables)
            {
                if (table.scope == currentScope + 1)
                {
                    tables.Add(table);
                }
            }
            foreach (ScopeTable table in tables)
            {
                scopeTables.Remove(table);
            }

        }
        /// <summary>
        /// 当前层数+1
        /// </summary>
        public static void currentScopeIncrease()
        {
            currentScope += 1;
        }
        /// <summary>
        /// 增||改
        /// </summary>
        /// <param name="scopeTable">层数表</param>
        public static void update(ScopeTable scopeTable)
        {
            if (check(scopeTable.name) == null)
                // 增
                scopeTables.Add(scopeTable);
            else
            //改
            {
                for (int i = 0; i < Constant.scopeTables.Count; i++)
                {
                    if (Constant.scopeTables[i].name == scopeTable.name)
                    {
                        Constant.scopeTables[i].type = scopeTable.type;
                        Constant.scopeTables[i].value = scopeTable.value;
                    }
                }
            }

        }
        /// <summary>
        /// 删
        /// </summary>
        /// <param name="name">名字</param>
        public static void delete(string name)
        {
            for (int i = 0; i < Constant.scopeTables.Count; i++)
            {
                if (Constant.scopeTables[i].name == name) //赋exp表达式的值
                    Constant.scopeTables.RemoveAt(i);
            }
        }
        /// <summary>
        /// 查找
        /// </summary>
        /// <param name="name">名字</param>
        /// <returns>找到的层数表</returns>
        public static ScopeTable check(string name)
        {
            for (int i = 0; i < Constant.scopeTables.Count; i++)
            {
                if (Constant.scopeTables[i].name == name) //赋exp表达式的值
                    return Constant.scopeTables[i];
            }
            Console.WriteLine("不存在");
            return null;
        }
    }
}

