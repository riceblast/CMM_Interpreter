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
    }
}

