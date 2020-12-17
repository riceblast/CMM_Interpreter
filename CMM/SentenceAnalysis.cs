using CMM.table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CMM
{
    class SentenceAnalysis
    {
        public static void StmtAnalysis(ParseTreeNode node)
        {
            if (node.IsLeaf)
                //是叶子节点直接退出
                return;
            else if(node.NSymbol== NEnum.assign_stmt)
            //赋值语句 assign-stmt -> variable = exp ; variable -> identifier [ [ exp ] ] 
            // 未考虑 [ [ exp ] ]部分
            {
                string name = node.Childs[0].Childs[0].ToString();
                //查找字母
                for(int i = 0; i < Constant.scopeTables.Count; i++)
                {
                    if(Constant.scopeTables[i].name==name)
                    {
                        //赋exp表达式的值
                        Constant.scopeTables[i].value = node.Childs[2].ToString();
                        break;
                    }
                        
                }
            }
            else if (node.NSymbol == NEnum.read_stmt)
            //read语句 read variable;
            {

            }
            else if (node.NSymbol == NEnum.write_stmt)
            //write语句 write exp;
            {
                Console.WriteLine(node.Childs[1].ToString());
            }
            else if (node.NSymbol == NEnum.declare_stmt)
            //declare-stmt语句 (int | real) ( (identifier [= exp ]) | (identifier [ exp ]) ) ;
            {
                if((node.Childs.Count<4))
                    //只申明，未赋值
                {
                    ScopeTable scopeTable = new ScopeTable(node.Childs[1].ToString(), node.Childs[0].ToString(), null, 0);
                    Constant.scopeTables.Add(scopeTable);
                }
                else
                //申明且赋值
                {
                    ScopeTable scopeTable = new ScopeTable(node.Childs[1].ToString(), node.Childs[0].ToString(),
                        node.Childs[3].ToString(), 0);
                    Constant.scopeTables.Add(scopeTable);
                }

            }

        }

        public static string expToValue(ParseTreeNode node)
        {
            string sum;

            
            MSScriptControl.ScriptControl scriptControl = new MSScriptControl.ScriptControl();
            scriptControl.UseSafeSubset = true;
            scriptControl.Language = "JScript";
            return scriptControl.Eval("((2*3)-5+(3*4))+6/2").ToString();
        }
       

    }
}
