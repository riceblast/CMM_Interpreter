using CMM.table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace CMM
{
    class SentenceAnalysis
    {
        /// <summary>
        /// 是否继续运行
        /// </summary>
        public static bool if_continue = true;
        /// <summary>
        ///入口函数
        /// </summary>
        /// <param name="treeNode">根节点</param>
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

        /// <summary>
        ///Statement节点分析函数
        /// </summary>
        /// <param name="node">Statement节点</param>
        public static void StatementAnalysis(ParseTreeNode node)
        {
            //判断是不是断点
            if (false)
            {
                //如果是断点则阻塞线程
                Constant.mreReset();
            }
            //判断是否能运行
            Constant._mre.WaitOne();
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
                //if条件未假
                else if (ifStmt.Childs.Count == 2)
                //else-stmt-block -> else stmt-block | ε
                {
                    if (ifStmt.Childs[1].Childs[0].TSymbol == TerminalType.EMPTY)
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

                    if (IsNumberic(expToValue(assignStmt.Childs[2])))
                    {
                        //声明的为整数时，还需将非整数转化成整数
                        if (scopeTable.type == "int")
                        {
                            string num = expToValue(assignStmt.Childs[2]);
                            if (Regex.IsMatch(num, @"^[+-]?[0-9]+$"))
                            {
                                string value = int.Parse(num).ToString();
                                scopeTable.value = value;
                                Constant.update(scopeTable);
                            }
                            else {
                                ErrorInfo error = new ErrorInfo(assignStmt.Childs[1].LineNum, "赋值类型错误！");
                                Constant.outputAppend(error.ToString());
                                return;
                            }
                        }
                        else //real类型可以直接存入
                        {
                            scopeTable.value = expToValue(assignStmt.Childs[2]);
                            Constant.update(scopeTable);
                        }
                    }
                    else
                    //返回错误
                    {
                        ErrorInfo error = new ErrorInfo(assignStmt.Childs[1].LineNum, "赋值类型错误！");
                        Constant.outputAppend(error.ToString());
                        return;
                    }

                }
                else
                //数组赋值   没有考虑一维数组以外的情况
                {
                    //数组名
                    string name = variable.Childs[0].StringValue;
                    //插入数组位置
                    int leng = int.Parse(variable.Childs[2].StringValue);

                    //查找
                    ScopeTable scopeTable = Constant.check(name);
                    //判断是否越界
                    string type = scopeTable.type;
                    string a = type.Substring(type.IndexOf('[') + 1, type.IndexOf(']') - type.IndexOf('[') - 1);
                    int length = int.Parse(a);
                    if (leng >= length)
                    {
                        ErrorInfo error = new ErrorInfo(variable.Childs[0].LineNum, "数组越界错误！");
                        Constant.outputAppend(error.ToString());
                        return;
                    }

                    else
                    {
                        //value= 0,0,0
                        string value = scopeTable.value;
                        string[] arr1 = value.Split(','); // 以','字符对字符串进行分割，返回字符串数组

                        if (IsNumberic(expToValue(assignStmt.Childs[2])))
                        {
                            //声明的为整数时，还需将非整数转化成整数
                            if (scopeTable.type.IndexOf("int") != -1)
                            {
                                string num = expToValue(assignStmt.Childs[2]);
                                if (Regex.IsMatch(num, @"^[+-]?[0-9]+$"))
                                {
                                    string value2 = int.Parse(num).ToString();
                                    arr1[leng] = value2;
                                    scopeTable.value = String.Join(",", arr1);
                                    Constant.update(scopeTable);
                                }
                                else {
                                    ErrorInfo error = new ErrorInfo(assignStmt.Childs[1].LineNum, "赋值类型错误！");
                                    Constant.outputAppend(error.ToString());
                                    return;
                                }
                            }
                            else //real类型可以直接存入
                            {
                                arr1[leng] = expToValue(assignStmt.Childs[2]);
                                scopeTable.value = String.Join(",", arr1);
                                Constant.update(scopeTable);
                            }
                        }
                        else
                        //返回错误
                        {
                            ErrorInfo error = new ErrorInfo(assignStmt.Childs[1].LineNum, "赋值类型错误！");
                            Constant.outputAppend(error.ToString());
                            return;
                        }

                    }

                }

            }
            else if (node.Childs[0].NSymbol == NEnum.read_stmt)
            //read-stmt -> read variable ;   // read语句
            {
                Constant.mreReset();
                Constant._mre.WaitOne();
                string num = Constant.readstr;
                ParseTreeNode readStmt = node.Childs[0];
                ScopeTable scopeTable = Constant.check(readStmt.Childs[1].Childs[0].StringValue);
                if (scopeTable.type == "int")
                {
                    if (Regex.IsMatch(num, @"^[+-]?[0-9]+$"))
                    {
                        scopeTable.value = num;
                    }
                    else
                    {
                        ErrorInfo error = new ErrorInfo(readStmt.Childs[1].Childs[0].LineNum, "赋值类型错误！");
                        Constant.outputAppend(error.ToString());
                        return;
                    }
                }
                else {
                    scopeTable.value = num;
                }
                
                Constant.update(scopeTable);

            }
            else if (node.Childs[0].NSymbol == NEnum.write_stmt)
            //write-stmt -> write exp ;   // write语句
            {
                ParseTreeNode writeStmt = node.Childs[0];
                ParseTreeNode exp = writeStmt.Childs[1];
                Constant.outputAppend(expToValue(exp));
            }
            else if (node.Childs[0].NSymbol == NEnum.declare_stmt)
            //declare-stmt语句 (int | real) ( (identifier [= exp ]) | (identifier [ exp ]) ) ;
            {
                ParseTreeNode declareStmt = node.Childs[0];

                //声明，且赋值  int a=3;
                //作废
                if (declareStmt.Childs.Count > 3)
                {
                    if (IsNumberic(expToValue(declareStmt.Childs[3])))
                    {
                        //声明的为整数时，还需将非整数转化成整数
                        if (declareStmt.Childs[0].StringValue == "int")
                        {
                            string num = expToValue(declareStmt.Childs[3]);
                            if (Regex.IsMatch(num, @"^[+-]?[0-9]+$"))
                            {
                                string value = int.Parse(num).ToString();
                                ScopeTable scopeTable = new ScopeTable(declareStmt.Childs[1].StringValue, declareStmt.Childs[0].StringValue, value, Constant.currentScope);
                                Constant.scopeTables.Add(scopeTable);
                            }
                            else {
                                ErrorInfo error = new ErrorInfo(0, "声明类型错误！");

                            }
                            
                        }
                        else //real类型可以直接存入
                        {
                            ScopeTable scopeTable = new ScopeTable(declareStmt.Childs[1].StringValue, declareStmt.Childs[0].StringValue, expToValue(declareStmt.Childs[3]), Constant.currentScope);
                            Constant.scopeTables.Add(scopeTable);
                        }
                    }
                    else
                    //返回错误
                    {
                        ErrorInfo error = new ErrorInfo(0, "声明类型错误！");
                    }
                }
                else if ((declareStmt.Childs[1].Childs.Count < 2))
                //只申明，未赋值 int a ;
                {
                    ScopeTable scopeTable = new ScopeTable(declareStmt.Childs[1].Childs[0].StringValue, declareStmt.Childs[0].StringValue, null, Constant.currentScope);
                    Constant.scopeTables.Add(scopeTable);
                }
                else if (declareStmt.Childs[1].Childs.Count > 2)
                //申明一个数组 int a[2];  a  int[3]   0,0,0  Constant.currentScope);

                {
                    int length = int.Parse(declareStmt.Childs[1].Childs[2].StringValue);
                    string value = "";
                    for (int i = 1; i < length; i++)
                    {
                        value += "0,";
                    }
                    value += "0";
                    string n = declareStmt.Childs[1].Childs[0].StringValue;
                    string t = declareStmt.Childs[0].StringValue + declareStmt.Childs[1].Childs[1].StringValue + declareStmt.Childs[1].Childs[2].StringValue + declareStmt.Childs[1].Childs[3].StringValue;
                    ScopeTable scopeTable = new ScopeTable(n, t, value, Constant.currentScope);
                    Constant.scopeTables.Add(scopeTable);
                }
            }
        }
        /// <summary>
        /// exp真假判断函数
        /// </summary>
        /// <param name="node">exp节点</param>
        /// <returns>exp节点是否为真，真返回true，假返回false</returns>
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
        /// <summary>
        /// 字符串计算
        /// </summary>
        /// <param name="node">待计算节点</param>
        /// <returns>返回该节点之下的所有叶子节点组成的字符串的计算结果</returns>
        public static string expToValue(ParseTreeNode node)
        {
            MSScriptControl.ScriptControl scriptControl = new MSScriptControl.ScriptControl();
            scriptControl.UseSafeSubset = true;
            scriptControl.Language = "JScript";
            return scriptControl.Eval(nodeToString(node)).ToString();
        }
        /// <summary>
        /// 节点转字符串
        /// </summary>
        /// <param name="node">待转化节点</param>
        /// <returns>返回该节点之下的所有叶子节点组成的字符串</returns>
        private static string nodeToString(ParseTreeNode node)
        {
            String str = "";
            if (node.IsLeaf)
            {
                if (node.TSymbol == TerminalType.ID)
                {
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
        public static bool IsNumberic(string value)
        {
            return Regex.IsMatch(value, @"^[+-]?[0-9]+\.?[0-9]*$");
        }
    }
}
