using CMM.table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM
{
    class Constant
    {
        public static int currentScope=0;
        public static List<ScopeTable> scopeTables=new List<ScopeTable>();
        public static void currentScopeDecrease() {
            currentScope -= 1;
            foreach(ScopeTable table in scopeTables){
                if (table.scope == currentScope + 1) {
                    scopeTables.Remove(table);
                }
            }
        }
        public static void currentScopeIncrease()
        {
            currentScope += 1;
        }
        //增||改
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
                        Constant.scopeTables[i].scope = scopeTable.scope;
                    }
                }
            }
        }
        //删
        public static void delete(string name)
        {
            for (int i = 0; i < Constant.scopeTables.Count; i++)
            {
                if (Constant.scopeTables[i].name == name) //赋exp表达式的值
                    Constant.scopeTables.RemoveAt(i);
            }
        }
        //查找
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

