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
        //入口函数,传入根节点
        public static void nodeAnalysis(ParseTreeNode treeNode)
        {
            foreach (ParseTreeNode node in treeNode.Childs)
            {
                if (node.IsLeaf)
                {
                    if (node.TSymbol == TerminalType.LBRACE)
                    {
                        Constant.currentScopeIncrease();
                    }
                    else if (node.TSymbol == TerminalType.RBRACE)
                    {
                        Constant.currentScopeDecrease();
                    }
                    //是叶子节点直接退出
                    break;
                }

                switch (node.NSymbol)
                {
                    case NEnum.statement:
                        StatementAnalysis(node);
                        break;
                    default:
                        nodeAnalysis(node);
                        break;
                }
            }
        }

        public static void StatementAnalysis(ParseTreeNode node)
        {
            if (node.IsLeaf)
                //是叶子节点直接退出
                return;
            else if (node.Childs[0].NSymbol == NEnum.if_stmt)
            //  if-stmt-> if-stmt - block else -stmt - block   //i f语句
            {
                ParseTreeNode ifStmt = node.Childs[0];

                ParseTreeNode ifStmtBlock = ifStmt.Childs[0];

                //if-stmt-block -> if (exp) stmt-block   // if语句子块
                ParseTreeNode exp = ifStmtBlock.Childs[2];

                //if条件为真
                if (expJudge(exp))
                {
                    //遇到stmtBlock
                    ParseTreeNode stmtBlock = ifStmtBlock.Childs[4];
                    //stmt - block->{statement} | { stmt - sequence }   // 语句块

                    if (stmtBlock.Childs[1].NSymbol == NEnum.statement) {
                        Constant.currentScopeIncrease();
                        StatementAnalysis(stmtBlock.Childs[1]);
                        Constant.currentScopeDecrease();
                    }
                        
                    else if (!stmtBlock.Childs[1].IsLeaf)
                    {
                        Constant.currentScopeIncrease();
                        ParseTreeNode stmtSequence = stmtBlock.Childs[1];
                        //stmt-sequence -> statement stmt-sequence | ε  // 语句序列
                        while (stmtSequence.Childs.Count == 2)
                        {
                            StatementAnalysis(stmtSequence.Childs[0]);
                            stmtSequence = stmtSequence.Childs[1];
                        }
                        StatementAnalysis(stmtSequence.Childs[0]);
                        Constant.currentScopeDecrease();
                    }
                }
                //if条件未假
                else if (ifStmt.Childs.Count == 2)
                //else-stmt-block -> else stmt-block | ε
                {
                    if (ifStmt.Childs[1].Childs[0].TSymbol==TerminalType.EMPTY)
                        //是空直接退出
                        return;
                    //遇到stmtBlock
                    ParseTreeNode elseStmtBlock = ifStmt.Childs[1];
                    ParseTreeNode stmtBlock = elseStmtBlock.Childs[1];
                    //stmt - block->{statement }| { stmt - sequence }   // 语句块

                    if (stmtBlock.Childs[1].NSymbol == NEnum.statement)
                    {
                        Constant.currentScopeIncrease();
                        StatementAnalysis(stmtBlock.Childs[1]);
                        Constant.currentScopeDecrease();
                    }
                    else if (!stmtBlock.Childs[1].IsLeaf)
                    {
                        Constant.currentScopeIncrease();
                        ParseTreeNode stmtSequence = stmtBlock.Childs[1];
                        //stmt-sequence -> statement stmt-sequence | ε  // 语句序列
                        while (stmtSequence.Childs.Count == 2)
                        {
                            StatementAnalysis(stmtSequence.Childs[0]);
                            stmtSequence = stmtSequence.Childs[1];
                        }
                        StatementAnalysis(stmtSequence.Childs[0]);
                        Constant.currentScopeDecrease();
                    }
                }
            }
            else if (node.Childs[0].NSymbol == NEnum.while_stmt)
            {
                //while-stmt -> while ( exp ) stmt-block   // while语句子快
                ParseTreeNode whileStmt = node.Childs[0];
                while (expJudge(whileStmt.Childs[2]))
                {
                    //遇到stmtBlock
                    ParseTreeNode stmtBlock = whileStmt.Childs[4];
                    //stmt - block->{statement} | { stmt - sequence }   // 语句块

                    if (stmtBlock.Childs[1].NSymbol == NEnum.statement) {
                        Constant.currentScopeIncrease();
                        StatementAnalysis(stmtBlock.Childs[1]);
                        Constant.currentScopeDecrease();
                    }

                    else if (!stmtBlock.Childs[1].IsLeaf)
                    {
                        Constant.currentScopeIncrease();
                        ParseTreeNode stmtSequence = stmtBlock.Childs[1];
                        //stmt-sequence -> statement stmt-sequence | ε  // 语句序列
                        while (stmtSequence.Childs.Count == 2)
                        {
                            StatementAnalysis(stmtSequence.Childs[0]);
                            stmtSequence = stmtSequence.Childs[1];
                        }
                        StatementAnalysis(stmtSequence.Childs[0]);
                        Constant.currentScopeDecrease();
                    }
                }

            }
            else if (node.Childs[0].NSymbol == NEnum.assign_stmt)
            //赋值语句 assign-stmt -> variable = exp ; variable -> identifier [ [ exp ] ] 
            {
                ParseTreeNode assignStmt = node.Childs[0];
                ParseTreeNode variable = assignStmt.Childs[0];
                if (variable.Childs.Count == 1)
                //一般变量赋值
                {
                    string name = variable.Childs[0].StringValue;
                    //查找并赋值
                    ScopeTable scopeTable = Constant.check(name);
                    scopeTable.value = expToValue(assignStmt.Childs[2]);
                    Constant.update(scopeTable);
                }
                else
                //数组赋值   没有考虑一维数组以外的情况
                {
                    //数组名
                    string name = variable.Childs[0].StringValue;
                    //数组长度
                    int leng = int.Parse(variable.Childs[2].StringValue);

                    //查找
                    ScopeTable scopeTable = Constant.check(name);
                    //判断是否越界
                    string type = scopeTable.type;
                    int length = int.Parse(type.Substring(type.IndexOf('[') + 1, type.IndexOf('[') - type.IndexOf(']')));
                    if (leng > length)
                        MessageBox.Show("数组越界");
                    else
                    {
                        //value= 0,0,0
                        string value = scopeTable.value;
                        string[] arr1 = value.Split(','); // 以','字符对字符串进行分割，返回字符串数组
                        arr1[leng] = expToValue(assignStmt.Childs[2]);
                        scopeTable.value = String.Join(",", arr1);
                        Constant.update(scopeTable);
                    }

                }

            }
            else if (node.Childs[0].NSymbol == NEnum.read_stmt)
            //read-stmt -> read variable ;   // read语句
            {
                ParseTreeNode readStmt = node.Childs[0];
            }
            else if (node.Childs[0].NSymbol == NEnum.write_stmt)
            //write-stmt -> write exp ;   // write语句
            {
                ParseTreeNode writeStmt = node.Childs[0];
                Console.WriteLine(expToValue(writeStmt.Childs[1]));
            }
            else if (node.Childs[0].NSymbol == NEnum.declare_stmt)
            //declare-stmt语句 (int | real) ( (identifier [= exp ]) | (identifier [ exp ]) ) ;
            {
                ParseTreeNode declareStmt = node.Childs[0];
                if ((declareStmt.Childs.Count < 4))
                //只申明，未赋值 int a ;
                {
                    ScopeTable scopeTable = new ScopeTable(declareStmt.Childs[1].StringValue, declareStmt.Childs[0].StringValue, null, Constant.currentScope);
                    Constant.scopeTables.Add(scopeTable);
                }
                else if (declareStmt.Childs[2].StringValue == "[")
                //申明一个数组 int a[2];  a  int[3]   0,0,0  Constant.currentScope);

                {
                    int length = int.Parse(declareStmt.Childs[3].StringValue);
                    string value = "";
                    for (int i = 1; i < length; i++)
                    {
                        value += "0,";
                    }
                    value += "0";
                    ScopeTable scopeTable = new ScopeTable(declareStmt.Childs[1].StringValue,
                        declareStmt.Childs[0].StringValue + declareStmt.Childs[2].StringValue + declareStmt.Childs[3].StringValue + declareStmt.Childs[4].StringValue,
                        null, Constant.currentScope);
                    Constant.scopeTables.Add(scopeTable);
                }
                else
                //声明，且赋值  int a=3;

                {
                    ScopeTable scopeTable = new ScopeTable(declareStmt.Childs[1].StringValue, declareStmt.Childs[0].StringValue, declareStmt.Childs[3].StringValue, Constant.currentScope);
                    Constant.scopeTables.Add(scopeTable);
                }
            }
        }


        public static bool expJudge(ParseTreeNode node)
        {
            string result = expToValue(node);
            try
            {
                if (int.Parse(result) > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                if (result == "True")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        //exp表达式转化成具体数值
        public static string expToValue(ParseTreeNode node)
        {
            MSScriptControl.ScriptControl scriptControl = new MSScriptControl.ScriptControl();
            scriptControl.UseSafeSubset = true;
            scriptControl.Language = "JScript";
            return scriptControl.Eval(nodeToString(node)).ToString();
        }


        private static string nodeToString(ParseTreeNode node)
        {
            String str = "";
            if (node.IsLeaf) {
                if (node.TSymbol==TerminalType.ID) {
                    return Constant.check(node.StringValue).value;
                }
                return node.StringValue;
            }
                
            else
            {
                for (int i = 0; i < node.Childs.Count; i++)
                {
                    str += nodeToString(node.Childs[i]);
                }
            }
            return str;
        }


    }
}
