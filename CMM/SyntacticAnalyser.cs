﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM
{
    /// <summary>
    /// 编译器核心的语法分析部分
    /// 由于语法分析器需要保存分析时的符号栈，故采用非静态类
    /// </summary>
    public class SyntacticAnalyser
    {

        /// <summary>
        /// 构造函数，初始化两个栈，并初始化预测分析表
        /// </summary>
        public SyntacticAnalyser()
        {
            this.symbolStack = new Stack<ParseTreeNode>();
            this.inputStack = new Stack<(string, TerminalType)>();

            // 初始化预测分析表
            this.parsingTable = new ParsingTable();
        }

        /// <summary>
        /// 记录自顶向下分析过程的符号表，以语法分析树的叶子结点作为标识
        /// </summary>
        private Stack<ParseTreeNode> symbolStack;

        /// <summary>
        /// 由词法分析得到的token列表
        /// </summary>
        private Stack<(string, TerminalType)> inputStack;

        /// <summary>
        /// 由该语法分析器构造的语法分析树
        /// </summary>
        private ParseTree targetTree;

        /// <summary>
        /// 该语法分析器所用到的语法分析表
        /// </summary>
        private ParsingTable parsingTable;

        /// <summary>
        /// 利用词法分析的结果进行语法分析
        /// </summary>
        /// <param name="tokens">由词法分析得到的token列表</param>
        /// <returns>语法分析树</returns>
        public ParseTree SyntacticAnalysis(List<(string, TerminalType)> tokens)
        {
            bool isSuccess = true; // 表示语法分析是否成功

            // 将所有token读入栈
            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                inputStack.Push(tokens[i]);
            }

            // 将结束符号放入符号栈中
            ParseTreeNode EndNode = new ParseTreeNode(true, TerminalType.END, NEnum.DEFAULT);
            symbolStack.Push(EndNode);

            // 开始构造语法分析树
            this.targetTree = new ParseTree();

            // 每次从输入栈中读取一个符号并更新符号栈的值
            // 当没有读到$时进行循环，如果下一个是结束符号$则结束循环
            (string, TerminalType) token; // 输入串中的当前讨论的token
            ParseTreeNode symbolNode; // 符号表中当前讨论的结点

            while(inputStack.Count > 0)
            {
                token = inputStack.Peek();
                symbolNode = symbolStack.Peek();

                #region 特殊非终结符号处理
                // 遇到标识符特殊考虑
                List<ErrorInfo> variableErrorInfos;
                if (symbolNode.NSymbol == NEnum.variable)
                {
                    // TODO 考虑后续的出错处理
                    this.VariableAnalyse(out variableErrorInfos);
                    continue;
                }

                // 遇到表达式特殊考虑
                if(symbolNode.NSymbol == NEnum.exp)
                {
                    // TODO 特殊考虑表达式

                    continue;
                }
                #endregion

                #region 查看M[U, a]表
                // 由于只有exp和标识符的情况会出现递归，所以其他非终结符号直接读取产生式即可
                List<ErrorInfo> NonErrorInfos;   // 在查找M[U,a]表时出现的错误
                if (symbolNode.IsLeaf)
                {
                    // 如果符号栈栈顶是终结符号
                    // 如果是结束符号
                    if(symbolNode.TSymbol == TerminalType.END)
                    {
                        if(token.Item2 == TerminalType.END)
                        {
                            // 语法分析结束，语法分析成功
                            break;
                        }
                        else
                        {
                            // TODO 语法分析出错处理

                            isSuccess = false;
                        }
                    }

                    // 如果终结符号不是结束符号，则判断与输入表栈顶是否相同
                    if(symbolNode.TSymbol == token.Item2)
                    {
                        // 将符号表和输入表栈顶元素出栈
                        symbolStack.Pop();
                        inputStack.Pop();
                    }
                    else
                    {
                        // TODO　语法分析出错处理

                        isSuccess = false;
                    }
                }
                else
                {
                    // 如果查找表失败则进行错误处理
                    if(!LookUpTable(symbolNode, out NonErrorInfos))
                    {
                        // TODO 出错处理

                        isSuccess = false;
                    }
                }
                #endregion
            }

            // 返回构建好的语法分析树
            this.targetTree.IsSuccess = isSuccess;
            return this.targetTree;
        }

        /// <summary>
        /// 单独对标识符进行语法分析
        /// </summary>
        /// <param name="errorInfo">待传出的错误信息</param>
        /// <returns>是否语法分析成功，如果返回值为false，则errorInof不为空</returns>
        private bool VariableAnalyse(out List<ErrorInfo> errorInfos)
        {
            errorInfos = new List<ErrorInfo>();
            bool isSuccess = true; // 是否语法分析成功

            (string, TerminalType) token = inputStack.Peek(); // 输入串中的当前讨论的token
            ParseTreeNode symbolNode = symbolStack.Peek(); // 符号表中当前讨论的树结点
            ParseTreeNode treeNode; // 用于接收新建的树结点

            // 首先判断是不是ID
            if(!ProcessNextTerminal(symbolNode, TerminalType.ID))
            {
                // TODO 出错处理

                isSuccess = false;
            }

            // 再判断之后有没有[
            if(isSuccess && ProcessNextTerminal(symbolNode, TerminalType.LBRACKET))
            {
                // 再继续判断接下来是不是intVal和]
                if (isSuccess && !ProcessNextTerminal(symbolNode, TerminalType.INTVAL))
                {
                    // TODO 出错处理

                    isSuccess = false;
                }

                if(isSuccess && !ProcessNextTerminal(symbolNode, TerminalType.RBRACKET))
                {
                    // TODO 出错处理

                    isSuccess = false;
                }
            }

            // 如果没有[则直接返回true，或者
            return isSuccess;
        }

        /// <summary>
        /// 判断下一个输入符号是否是目标终结符号
        /// 如果是，则构建当前语法树的子结点，并将inputStack中栈顶元素出栈
        /// 如果不是，则返回fasle，不修改inputStack的栈顶元素
        /// </summary>
        /// <param name="fatherNode">当前正在研究的符号表的栈顶元素</param>
        /// <param name="tEnum">待判断的终结符号</param>
        /// <returns>是否语法分析成功</returns>
        private bool ProcessNextTerminal(ParseTreeNode fatherNode, TerminalType targetTerminal)
        {
            (string, TerminalType) token = inputStack.Peek(); // 输入串中的当前讨论的token
            ParseTreeNode treeNode; // 用于接收新建的语法树结点

            // 如果下一个token不满足要求
            if (token.Item2 != targetTerminal)
            {
                return false;
            }

            // 将目标终结符号加入到语法分析树中
            treeNode = new ParseTreeNode(true, targetTerminal, NEnum.DEFAULT);
            fatherNode.Childs.Add(treeNode);

            // 将inputStack栈顶元素出栈
            this.inputStack.Pop();
            return true;
        }

        /// <summary>
        /// 利用当前栈顶元素查找预测分析表并修改符号表
        /// 该函数无法处理标识符类型和表达式类型
        /// </summary>
        /// <param name="symbolNode">当前正在研究的符号表的栈顶元素</param>
        /// <returns>是否语法分析成功</returns>
        private bool LookUpTable(ParseTreeNode symbolNode, out List<ErrorInfo> errorInfos)
        {
            errorInfos = new List<ErrorInfo>();
            bool isSuccess = true; // 是否语法分析成功

            (string, TerminalType) token = inputStack.Peek(); // 输入串中的当前讨论的token
            ParseTreeNode treeNode; // 用于接收新建的语法树结点

            // 对于非终结符号，则参照预测分析表找到产生式
            // 在没有遇到exp或者variable的情况下，只会有一条产生式满足规则
            // 产生式列表
            List<ParsingTableItem> productions =
                this.parsingTable.GetItem(symbolNode.NSymbol, token.Item2);
            if (productions == null || productions.Count == 0)
            {
                // TODO 语法分析错误处理

                isSuccess = false;
            }
            else
            {
                List<ParseTreeNode> production = productions[0].production;
                // 生成对应的树结点，并设置子树
                for (int i = 0; i < production.Count; i++)
                {
                    // 对于“空”终结符号，则不产生任何树结点
                    if (production[i].TSymbol != TerminalType.EMPTY)
                    {
                        treeNode = new ParseTreeNode(production[i].IsLeaf,
                            production[i].TSymbol, production[i].NSymbol);

                        symbolNode.Childs.Add(treeNode);
                    }
                }

                // 将产生式对应的(非)终结符号入栈, 入栈顺序与childs顺序相反
                for (int i = symbolNode.Childs.Count - 1; i >= 0; i--)
                {
                    // 对于“空”终结符号，则不push任何符号
                    if (symbolNode.Childs[i].TSymbol != TerminalType.EMPTY)
                    {
                        symbolStack.Push(symbolNode.Childs[i]);
                    }
                }
            }

            return isSuccess;
        }

        /// <summary>
        /// 单独对表达式进行语法分析
        /// </summary>
        /// <param name="fatherNode">当前符号栈正在研究的栈顶结点，该结点的NEnum为exp</param>
        /// <param name="errorInfos">待传出的错误信息</param>
        /// <returns>是否语法分析成功，如果返回值为false，则errorInof不为空</returns>
        private bool ExpAnalyse(ParseTreeNode fatherNode, out List<ErrorInfo> errorInfos)
        {
            errorInfos = new List<ErrorInfo>();
            bool isSuccess = true; // 是否语法分析成功
            ParseTreeNode treeNode; // 用于接收新建的语法树结点

            #region addtive-exp
            treeNode = new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.addtive_exp); // 新建addtive_exp的子结点
            fatherNode.Childs.Add(treeNode);

            List<ErrorInfo> errorInfosAddtive_1;
            if(!AddtiveExpAnalyse(treeNode, out errorInfosAddtive_1))
            {
                // 出错处理

                isSuccess = false;
            }
            #endregion

            #region logical-op
            (string, TerminalType) token = inputStack.Peek();
            if (token.Item2 == TerminalType.GREATER)
                ;
            #endregion

            #region addtive-exp

            #endregion

            return true;
        }

        /// <summary>
        /// 单独对addtive-exp作语法分析
        /// </summary>
        /// <param name="fatherNode">当前符号栈正在研究的栈顶结点，该结点的NEnum为addtive-exp</param>
        /// <param name="errorInfos">待传出的错误列表</param>
        /// <returns>是否语法分析成功</returns>
        private bool AddtiveExpAnalyse(ParseTreeNode fatherNode, out List<ErrorInfo> errorInfos)
        {
            errorInfos = new List<ErrorInfo>();
            return true;
        }

        /// <summary>
        /// 单独对term作语法分析
        /// </summary>
        /// <param name="fatherNode">当前符号栈正在研究的栈顶结点，该结点的NEnum为term</param>
        /// <param name="errorInfos">错误列表</param>
        /// <returns>是否语法分析成功</returns>
        private bool TermAnalyse(ParseTreeNode fatherNode, out List<ErrorInfo> errorInfos)
        {
            errorInfos = new List<ErrorInfo>();
            return true;
        }

        /// <summary>
        /// 单独对Factor作语法分析
        /// </summary>
        /// <param name="fatherNode">当前符号栈正在研究的栈顶结点，该结点的NEnum为facor</param>
        /// <param name="errorInfos">待传出的错误信息</param>
        /// <returns>是否语法分析成功</returns>
        private bool FactorAnalyse(ParseTreeNode fatherNode, out List<ErrorInfo> errorInfos)
        {
            errorInfos = new List<ErrorInfo>();
            return true;
        }
    }
}