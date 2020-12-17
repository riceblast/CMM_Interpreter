using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM.table
{
    class ScopeTable
    {
        //变量名
        public string name;
        public string type;
        public string value;
        public int scope;

        public ScopeTable(string name, string type, string value, int scope)
        {
            this.name = name;
            this.type = type;
            this.value = value;
            this.scope = scope; 
        }

    }
}
