using System;
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
            this.inputStack = new Stack<Token>();

            // 初始化预测分析表
            this.parsingTable = new ParsingTable();

            // 新建语法分析树
            this.targetTree = new ParseTree();
        }

        /// <summary>
        /// breakPoint断点行号列表
        /// </summary>
        private List<int> bPLineNums;

        /// <summary>
        /// 记录自顶向下分析过程的符号表，以语法分析树的叶子结点作为标识
        /// </summary>
        private Stack<ParseTreeNode> symbolStack;

        /// <summary>
        /// 由词法分析得到的token列表
        /// </summary>
        private Stack<Token> inputStack;

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
        /// <param name="tokenResult">由词法分析得到的token结果</param>
        /// <param name="bpList">断点行号列表</param>
        /// <returns>语法分析树</returns>
        public ParseTree SyntacticAnalysis(TokenResult tokenResult, List<int> bpList)
        {
            #region 语法分析初始化

            // 初始化断点列表
            this.bPLineNums = bpList;

            bool isSuccess = true; // 表示语法分析是否成功
            List<ErrorInfo> totalErrorInfos = new List<ErrorInfo>(); // 总的报错信息

            // 将所有token读入栈
            for (int i = tokenResult.Tokens.Count - 1; i >= 0; i--)
            {
                if (tokenResult.Tokens[i].TokenType != TerminalType.NOTES)
                {
                    inputStack.Push(tokenResult.Tokens[i]);
                }
            }

            // 将结束符号放入符号栈中
            ParseTreeNode EndNode = new ParseTreeNode(true, TerminalType.END, NEnum.DEFAULT);
            symbolStack.Push(EndNode);

            // 将program放入符号栈
            symbolStack.Push(targetTree.Root);

            #endregion

            #region 语法分析主体程序
            // 每次从输入栈中读取一个符号并更新符号栈的值
            // 当没有读到$时进行循环，如果下一个是结束符号$则结束循环
            Token token; // 输入串中的当前讨论的token
            ParseTreeNode symbolNode; // 符号表中当前讨论的结点

            while (inputStack.Count > 0 && symbolStack.Count > 0)
            {
                token = inputStack.Peek();
                symbolNode = symbolStack.Peek();

                // 遇到注释则跳过
                if (symbolNode.TSymbol == TerminalType.NOTES)
                {
                    continue;
                }

                #region 特殊非终结符号处理
                // 遇到标识符特殊考虑
                List<ErrorInfo> variableErrorInfos, expErrorInfos;
                if (symbolNode.NSymbol == NEnum.variable)
                {
                    this.VariableAnalyse(symbolNode, out variableErrorInfos);
                    totalErrorInfos.AddRange(variableErrorInfos);
                    continue;
                }

                // 遇到表达式特殊考虑
                if (symbolNode.NSymbol == NEnum.exp)
                {
                    this.ExpAnalyse(symbolNode, out expErrorInfos);
                    // 将exp出栈
                    PopSymbolStack();
                    totalErrorInfos.AddRange(expErrorInfos);
                    continue;
                }
                #endregion

                #region 查看M[U, a]表
                // 由于只有exp和标识符的情况会出现递归，所以其他非终结符号直接读取产生式即可
                List<ErrorInfo> NonErrorInfos = new List<ErrorInfo>();   // 在查找M[U,a]表时出现的错误
                if (symbolNode.IsLeaf)
                {
                    // 如果符号栈栈顶是终结符号
                    // 如果是结束符号
                    if (symbolNode.TSymbol == TerminalType.END)
                    {
                        if (token.TokenType == TerminalType.END)
                        {
                            // 语法分析结束，语法分析成功
                            break;
                        }
                        else
                        {
                            // TODO 语法分析出错处理
                            ErrorEncapsulation("程序结尾存在多余字符串");
                            isSuccess = false;
                        }
                    }

                    // 如果终结符号不是结束符号，则判断与输入表栈顶是否相同
                    if (symbolNode.TSymbol == token.TokenType)
                    {
                        // 将符号表和输入表栈顶元素出栈
                        // 如果符号栈栈顶元素是后一个元素子结点中的最后一个节点，则
                        // 应该将上一个结点也出栈
                        symbolNode.StringValue = token.StrValue;
                        symbolNode.LineNum = token.LineNum;
                        PopSymbolStack();
                        inputStack.Pop();
                    }
                    else
                    {
                        // TODO　语法分析出错处理
                        ErrorEncapsulation($"缺少符号: {T2String(symbolNode.TSymbol)}");
                    }
                }
                else
                {
                    // 如果查找表失败则进行错误处理
                    if (!LookUpTable(symbolNode, out NonErrorInfos))
                    {
                        // TODO 出错处理
                        ErrorEncapsulation($"语法成分: {N2String(symbolNode.NSymbol)}" +
                            $" 不能以 {inputStack.Peek().StrValue} 符号开头");
                    }
                }
                #endregion
            }
            #endregion

            // 返回构建好的语法分析树
            this.targetTree.IsSuccess = isSuccess;
            this.targetTree.ErrorInfos = totalErrorInfos.OrderBy(e => e.LineNum).ToList();

            return this.targetTree;
        }

        /// <summary>
        /// 出栈符号栈栈顶的元素，如果当前出栈元素是下一个子树节点的最后一个元素
        /// 则将下一个元素也一起出栈
        /// </summary>
        /// <param name="isEmpty">待出栈的符号是否为空，默认为false</param>
        private void PopSymbolStack(bool isEmpty = false)
        {
            ParseTreeNode topSymbol; // 栈顶元素
            ParseTreeNode secondTopSymbol; // 栈顶里面一个的元素

            // 如果当前元素为空，则还应该将其在父结点的childs列表中删去
            if (isEmpty)
            {
                // 根据目前文法可知，两个取空的情况下，‘空’均位于父结点中的最后一个
                // 所以在将当前结点出栈后，再删除父结点childs列表中最后一个值即可
                topSymbol = symbolStack.Pop();
                bool flag = false; // 是否需要将当前栈顶的符号出栈

                // 如果还有下一个元素，则进行判断
                if (symbolStack.Count > 0)
                {
                    secondTopSymbol = symbolStack.Peek();
                    if (secondTopSymbol.Childs.Count == 0)
                    {
                        return;
                    }
                    if (secondTopSymbol.Childs.Last().TSymbol == topSymbol.TSymbol &&
                        secondTopSymbol.Childs.Last().NSymbol == topSymbol.NSymbol)
                    {
                        flag = true;
                    }
                }

                Stack<ParseTreeNode> tempStack = new Stack<ParseTreeNode>();
                while(symbolStack.Count > 0)
                {
                    tempStack.Push(symbolStack.Pop());
                    if (tempStack.Peek().Childs.Contains(topSymbol))
                    {
                        tempStack.Peek().Childs.Remove(topSymbol);
                        break;
                    }
                }

                // 将pop出来的元素重新push回符号栈
                while(tempStack.Count > 0)
                {
                    symbolStack.Push(tempStack.Pop());
                }

                // 根据之前判断的结果选择是否将栈顶元素出栈
                if (flag)
                {
                    PopSymbolStack(symbolStack.Peek().Childs.Count == 0);
                }

                return;
            }
            if (symbolStack.Count > 0)
            {
                topSymbol = symbolStack.Pop();

                // 如果还有下一个元素，则进行判断
                if(symbolStack.Count > 0 )
                {
                    secondTopSymbol = symbolStack.Peek();
                    if (secondTopSymbol.Childs.Count == 0)
                    {
                        return;
                    }
                    if(secondTopSymbol.Childs.Last().TSymbol == topSymbol.TSymbol &&
                        secondTopSymbol.Childs.Last().NSymbol == topSymbol.NSymbol)
                    {
                        PopSymbolStack();
                    }
                }
            }
        }

        /// <summary>
        /// 单独对标识符进行语法分析
        /// </summary>
        /// <param name="symbolNode">符号表栈顶结点</param>
        /// <param name="errorInfos">待传出的错误信息</param>
        /// <returns>是否语法分析成功，如果返回值为false，则errorInof不为空</returns>
        private bool VariableAnalyse(ParseTreeNode symbolNode, out List<ErrorInfo> errorInfos)
        {
            errorInfos = new List<ErrorInfo>();
            bool isSuccess = true; // 是否语法分析成功

            Token token = inputStack.Peek(); // 输入串中的当前讨论的token
            //ParseTreeNode symbolNode = symbolStack.Peek(); // 符号表中当前讨论的树结点
            ParseTreeNode treeNode; // 用于接收新建的树结点

            // 首先判断是不是ID
            if (!ProcessNextTerminal(symbolNode, TerminalType.ID))
            {
                // TODO 出错处理
                ErrorEncapsulation($"源码此处是: {inputStack.Peek().StrValue} 缺少语法成分: 标识符");
            }

            // 再判断之后有没有[
            if (isSuccess && ProcessNextTerminal(symbolNode, TerminalType.LBRACKET))
            {
                // 出栈

                // 再继续判断接下来是不是intVal和]
                if (isSuccess && !ProcessNextTerminal(symbolNode, TerminalType.INTVAL))
                {
                    // TODO 出错处理
                    ErrorEncapsulation("数组索引应为正整数");
                    isSuccess = false;
                }

                if (isSuccess && !ProcessNextTerminal(symbolNode, TerminalType.RBRACKET))
                {
                    // TODO 出错处理
                    ErrorEncapsulation("数组索引右括号 ']' 缺失");
                    isSuccess = false;
                }
            }

            // 将symbolStack栈顶的variable出栈
            if(symbolStack.Peek().NSymbol == NEnum.variable)
            {
                symbolStack.Pop();
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
        /// <param name="tEnums">待判断的终结符号集，只要满足其中一个即视作语法分析成功</param>
        /// <returns>是否语法分析成功</returns>
        private bool ProcessNextTerminal(ParseTreeNode fatherNode, params TerminalType[] tEnums)
        {
            Token token = inputStack.Peek(); // 输入串中的当前讨论的token
            ParseTreeNode treeNode; // 用于接收新建的语法树结点
            TerminalType targetTerminal; // 带判断的终结符号

            for(int i = 0; i < tEnums.Length; i++)
            {
                targetTerminal = tEnums[i];

                // 如果终结符号不满足要求则尝试下一个终结符号
                if (token.TokenType != targetTerminal)
                {
                    continue;
                }

                // 将目标终结符号加入到语法分析树中
                treeNode = new ParseTreeNode(true, targetTerminal, NEnum.DEFAULT);
                treeNode.StringValue = token.StrValue;
                treeNode.LineNum = token.LineNum;
                fatherNode.Childs.Add(treeNode);

                // 将inputStack栈顶元素出栈
                this.inputStack.Pop();
                return true;
            }

            return false; // 所有终结符号均不满足要求

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

            Token token = inputStack.Peek(); // 输入串中的当前讨论的token
            ParseTreeNode treeNode; // 用于接收新建的语法树结点

            // 对于非终结符号，则参照预测分析表找到产生式
            // 在没有遇到exp或者variable的情况下，只会有一条产生式满足规则
            // 产生式列表
            List<ParsingTableItem> productions =
                this.parsingTable.GetItem(symbolNode.NSymbol, token.TokenType);
            if (productions == null || productions.Count == 0)
            {
                // TODO 语法分析错误处理
                ErrorEncapsulation($"语法成分: {N2String(symbolNode.NSymbol)} 不能以" +
                    $" {T2String(token.TokenType)} 符号开头");
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
                    // 对于“断点”，不push任何符号
                    if (symbolNode.Childs[i].TSymbol != TerminalType.EMPTY &&
                        symbolNode.Childs[i].TSymbol != TerminalType.BREAKPOINT)
                    {
                        symbolStack.Push(symbolNode.Childs[i]);
                    }
                }

                // 断点处理
                // 如果对应的token是断点所在行，则加入断点
                // 如果是stmtblock和stmtsequence均需要断点处理
                treeNode = new ParseTreeNode(false, TerminalType.DEFAULT,
                    NEnum.statement);
                treeNode.Childs.Add(new ParseTreeNode(true, TerminalType.BREAKPOINT,
                    NEnum.DEFAULT));
                if (symbolNode.NSymbol == NEnum.stmt_sequence &&
                    production[0].TSymbol != TerminalType.EMPTY &&
                    this.bPLineNums.Contains(token.LineNum))
                {
                    // stmt-sequence
                    symbolNode.Childs.Insert(0, treeNode);
                }
                if (symbolNode.NSymbol == NEnum.stmt_block &&
                    production[0].TSymbol != TerminalType.EMPTY &&
                    (this.bPLineNums.Contains(token.LineNum) || 
                    this.bPLineNums.Contains(token.LineNum + 1)))
                {
                    // stmt-block
                    symbolNode.Childs.Insert(1, treeNode);
                }

                // 如果是Empty，则还应将符号栈栈顶元素出栈
                if (production[0].TSymbol == TerminalType.EMPTY)
                {
                    PopSymbolStack(true);
                }
            }

            return isSuccess;
        }

        /// <summary>
        /// 单独对表达式进行语法分析
        /// 包括了对输入栈和符号栈对应元素的出栈
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
            if (!AddtiveExpAnalyse(treeNode, out errorInfosAddtive_1))
            {
                // TODO 出错处理

                isSuccess = false;
            }
            #endregion

            #region logical-op
            // 判断是否有逻辑运算符
            if (ProcessNextTerminal(fatherNode, TerminalType.GREATER,
                TerminalType.LESS,TerminalType.EQUAL,TerminalType.NOTEQUAL))
            {
                // 如果有再判断后续的addtive-exp
                #region addtive-exp
                treeNode = new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.addtive_exp); // 新建addtive_exp的子结点
                fatherNode.Childs.Add(treeNode);

                List<ErrorInfo> errorInfosAddtive_2;
                if (!AddtiveExpAnalyse(treeNode, out errorInfosAddtive_2))
                {
                    // TODO 出错处理

                    isSuccess = false;
                }
                #endregion
            }
            #endregion

            return isSuccess;
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

            bool isSuccess = true; // 是否语法分析成功
            ParseTreeNode treeNode; // 用于接收新建的语法树结点

            #region term
            treeNode = new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.term);
            fatherNode.Childs.Add(treeNode);

            List<ErrorInfo> termErrorInfos;
            if(!TermAnalyse(treeNode, out termErrorInfos))
            {
                // TODO 错误处理
                //Error();
                isSuccess = false;
            }
            #endregion

            #region add-op
            // 判断有没有加减号
            if(ProcessNextTerminal(fatherNode, TerminalType.PLUS, TerminalType.MINUS))
            {
                // 如果有加减号需要继续判断是否有addtive-exp
                #region addtive-exp
                treeNode = new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.addtive_exp);
                fatherNode.Childs.Add(treeNode);

                List<ErrorInfo> addtiveExpErrorInfos;
                if(!AddtiveExpAnalyse(treeNode, out addtiveExpErrorInfos))
                {
                    // TODO 出错处理
                    //Error();
                    isSuccess = false;
                }

                #endregion
            }
            #endregion

            return isSuccess;
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

            bool isSuccess = true;
            ParseTreeNode treeNode; // 用于接收新建的语法树结点

            #region factor
            treeNode = new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.factor);
            fatherNode.Childs.Add(treeNode);

            // 进行factor部分的语法分析
            List<ErrorInfo> factorErrorInfos;
            if(!FactorAnalyse(treeNode, out factorErrorInfos))
            {
                // 出错处理
                //Error();
                isSuccess = false;
            }
            #endregion

            #region mul-op
            // 判断之后是否有mul-op
            if(ProcessNextTerminal(fatherNode, TerminalType.MUL, TerminalType.DIV))
            {
                #region term
                treeNode = new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.term);
                fatherNode.Childs.Add(treeNode);

                List<ErrorInfo> termErrorInfos;
                if(!TermAnalyse(treeNode, out termErrorInfos))
                {
                    // 出错处理
                    //Error();
                    isSuccess = false;
                }
                #endregion
            }
            #endregion

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

            bool isSuccess = true; // 是否语法分析成功
            ParseTreeNode treeNode; // 用于接收新建的语法树结点

            List<ErrorInfo> expErrorInfos, idErrorInfos, addOpErrorInfos;

            if(ProcessNextTerminal(fatherNode, TerminalType.LPARENT))
            {
                // (exp)
                if(!ExpAnalyse(fatherNode, out expErrorInfos))
                {
                    isSuccess = false;
                }

                if(!ProcessNextTerminal(fatherNode, TerminalType.RPARENT))
                {
                    // TODO 出错处理
                    ErrorEncapsulation("表达式中右括号未闭合");
                    isSuccess = false;
                }

            }else if(ProcessNextTerminal(fatherNode, TerminalType.INTVAL,
                TerminalType.REALVAL))
            {
                // number
                // factor此时语法分析结束

            }else if(ProcessNextTerminal(fatherNode, TerminalType.PLUS, TerminalType.MINUS))
            {
                // add-op exp
                if(!ExpAnalyse(fatherNode, out addOpErrorInfos))
                {
                    // TODO 出错处理
                    //Error();
                    isSuccess = false;
                }
            }
            else
            {
                // variable
                if(!VariableAnalyse(fatherNode, out idErrorInfos))
                {
                    // 出错处理
                    isSuccess = false;
                }
            }

            return isSuccess;
        }

        /// <summary>
        /// 出错处理程序的封装
        /// 1. 封装出错信息和行数(默认为inputStack栈顶token的行号)
        /// 2. 进行"式后字"寻找的出错处理程序
        /// </summary>
        /// <param name="message">出错处理信息</param>
        /// <param name="lineNum">可以手动添加错误所在行数</param>
        private void ErrorEncapsulation(string message, int lineNum = -1)
        {

            // 添加到语法分析树的ErrorInfos列表
            int actualLineNum = lineNum;
            if(lineNum == -1)
            {
                actualLineNum = inputStack.Peek().LineNum;
            }

            this.targetTree.ErrorInfos.Add(new ErrorInfo(actualLineNum, message));

            // 进行"式后字"寻找和处理
            Error();

            // 更新IsSuccess值
            this.targetTree.IsSuccess = false;
        }

        /// <summary>
        /// 出错处理程序
        /// </summary>
        private void Error()
        {
            // 找到当前符号表栈顶元素对应的"式后字"
            // 将符号表栈顶元素出栈
            // 将"式后字"之前的所有非终结符号出栈
            
            ParseTreeNode symbolNode = this.symbolStack.Peek();
            this.targetTree.IsSuccess = false;

            // 如果是终结符号则直接出栈
            if (symbolNode.IsLeaf)
            {
                //symbolStack.Pop();
                PopSymbolStack();
                if(inputStack.Count > 0)
                {
                    inputStack.Pop();
                }

            }
            else
            {
                // 如果是非终结符号则找对应的follow集合
                switch (symbolNode.NSymbol)
                {
                    case NEnum.program:
                        // $
                        ProcessFollowTerminal(TerminalType.END);
                        break;
                    case NEnum.stmt_sequence:
                        // $ }
                        ProcessFollowTerminal(TerminalType.END, TerminalType.RBRACE);
                        break;
                    case NEnum.statement:
                        // if while id read write int real } $
                        ProcessFollowTerminal(TerminalType.IF, TerminalType.WHILE, TerminalType.ID,
                            TerminalType.READ, TerminalType.WRITE, TerminalType.INT, TerminalType.REAL,
                            TerminalType.RBRACE, TerminalType.END);
                        break;
                    case NEnum.stmt_block:
                        // if while id read write int real } else
                        ProcessFollowTerminal(TerminalType.IF, TerminalType.WHILE, TerminalType.ID,
                            TerminalType.READ, TerminalType.WRITE, TerminalType.INT, TerminalType.REAL,
                            TerminalType.RBRACE, TerminalType.END);
                        break;
                    case NEnum.if_stmt:
                        // if while id read write int real } else
                        ProcessFollowTerminal(TerminalType.IF, TerminalType.WHILE, TerminalType.ID,
                            TerminalType.READ, TerminalType.WRITE, TerminalType.INT, TerminalType.REAL,
                            TerminalType.RBRACE, TerminalType.END);
                        break;
                    case NEnum.if_stmt_block:
                        // else
                        ProcessFollowTerminal(TerminalType.ELSE);
                        break;
                    case NEnum.else_stmt_block:
                        // if while id read write int real } else
                        ProcessFollowTerminal(TerminalType.IF, TerminalType.WHILE, TerminalType.ID,
                            TerminalType.READ, TerminalType.WRITE, TerminalType.INT, TerminalType.REAL,
                            TerminalType.RBRACE, TerminalType.END);
                        break;
                    case NEnum.while_stmt:
                        // if while id read write int real } else
                        ProcessFollowTerminal(TerminalType.IF, TerminalType.WHILE, TerminalType.ID,
                            TerminalType.READ, TerminalType.WRITE, TerminalType.INT, TerminalType.REAL,
                            TerminalType.RBRACE, TerminalType.END);
                        break;
                    case NEnum.assign_stmt:
                        // if while id read write int real } else
                        ProcessFollowTerminal(TerminalType.IF, TerminalType.WHILE, TerminalType.ID,
                            TerminalType.READ, TerminalType.WRITE, TerminalType.INT, TerminalType.REAL,
                            TerminalType.RBRACE, TerminalType.END);
                        break;
                    case NEnum.read_stmt:
                        // if while id read write int real } else
                        ProcessFollowTerminal(TerminalType.IF, TerminalType.WHILE, TerminalType.ID,
                            TerminalType.READ, TerminalType.WRITE, TerminalType.INT, TerminalType.REAL,
                            TerminalType.RBRACE, TerminalType.END);
                        break;
                    case NEnum.write_stmt:
                        // if while id read write int real } else
                        ProcessFollowTerminal(TerminalType.IF, TerminalType.WHILE, TerminalType.ID,
                            TerminalType.READ, TerminalType.WRITE, TerminalType.INT, TerminalType.REAL,
                            TerminalType.RBRACE, TerminalType.END);
                        break;
                    case NEnum.declare_stmt:
                        // if while id read write int real } else
                        ProcessFollowTerminal(TerminalType.IF, TerminalType.WHILE, TerminalType.ID,
                            TerminalType.READ, TerminalType.WRITE, TerminalType.INT, TerminalType.REAL,
                            TerminalType.RBRACE, TerminalType.END);
                        break;
                    case NEnum.variable:
                        // < > <> == ) ; ] * / + -
                        ProcessFollowTerminal(TerminalType.LESS, TerminalType.GREATER, TerminalType.EQUAL,
                            TerminalType.RPARENT, TerminalType.SEMI, TerminalType.RBRACKET, TerminalType.MUL,
                            TerminalType.DIV, TerminalType.PLUS, TerminalType.MINUS,TerminalType.NOTEQUAL);
                        break;
                    case NEnum.exp:
                        // + - * / < > <> == ) ;
                        ProcessFollowTerminal(TerminalType.PLUS, TerminalType.MINUS, TerminalType.MUL,
                            TerminalType.DIV, TerminalType.LESS, TerminalType.GREATER, TerminalType.EQUAL,
                            TerminalType.NOTEQUAL, TerminalType.RPARENT, TerminalType.SEMI);
                        break;
                    case NEnum.addtive_exp:
                        // + - * / < > <> == ) ;
                        ProcessFollowTerminal(TerminalType.PLUS, TerminalType.MINUS, TerminalType.MUL,
                            TerminalType.DIV, TerminalType.LESS, TerminalType.GREATER, TerminalType.EQUAL,
                            TerminalType.NOTEQUAL, TerminalType.RPARENT, TerminalType.SEMI);
                        break;
                    case NEnum.term:
                        // + - * / < > <> == ) ;
                        ProcessFollowTerminal(TerminalType.PLUS, TerminalType.MINUS, TerminalType.MUL,
                            TerminalType.DIV, TerminalType.LESS, TerminalType.GREATER, TerminalType.EQUAL,
                            TerminalType.NOTEQUAL, TerminalType.RPARENT, TerminalType.SEMI);
                        break;
                    case NEnum.factor:
                        // + - * / < > <> == ) ;
                        ProcessFollowTerminal(TerminalType.PLUS, TerminalType.MINUS, TerminalType.MUL,
                            TerminalType.DIV, TerminalType.LESS, TerminalType.GREATER, TerminalType.EQUAL,
                            TerminalType.NOTEQUAL, TerminalType.RPARENT, TerminalType.SEMI);
                        break;
                    case NEnum.logical_op:
                        // ( intVal realVal id + -
                        ProcessFollowTerminal(TerminalType.LPARENT, TerminalType.INTVAL, TerminalType.REALVAL,
                            TerminalType.ID, TerminalType.PLUS, TerminalType.MINUS);
                        break;
                    case NEnum.add_op:
                        // ( intVal realVal id + -
                        ProcessFollowTerminal(TerminalType.LPARENT, TerminalType.INTVAL, TerminalType.REALVAL,
                            TerminalType.ID, TerminalType.PLUS, TerminalType.MINUS);
                        break;
                    case NEnum.mul_op:
                        // ( intVal realVal id + -
                        ProcessFollowTerminal(TerminalType.LPARENT, TerminalType.INTVAL, TerminalType.REALVAL,
                            TerminalType.ID, TerminalType.PLUS, TerminalType.MINUS);
                        break;

                }
            }
        }

        /// <summary>
        /// 处理式后字的程序
        /// 在inputStack中寻找当前非终结符号的式后字
        /// 将式后字之前的所有的输入字符串出栈
        /// 将符号栈栈顶出栈
        /// </summary>
        private void ProcessFollowTerminal(params TerminalType[] tEnums)
        {
            Token token; // 记录输入串栈顶的符号

            // 将symbolStack栈顶出栈
            //this.symbolStack.Pop();
            PopSymbolStack();

            // 寻找式后字并将输入符号串出栈
            while(inputStack.Count > 0)
            {
                token = inputStack.Pop();

                // 如果找到了式后字，则将输入串放回栈顶
                if (tEnums.Contains(token.TokenType))
                {
                    inputStack.Push(token);
                }
            }
        }

        /// <summary>
        /// 将终结符号的枚举转换为字符串
        /// </summary>
        /// <param name="T">终结符号枚举</param>
        /// <returns>对应的字符串</returns>
        public string T2String(TerminalType T)
        {
            string resultString;
            switch (T)
            {
                case TerminalType.IF:
                    resultString = "if";
                    break;
                case TerminalType.ELSE:
                    resultString = "else";
                    break;
                case TerminalType.WHILE:
                    resultString = "while";
                    break;
                case TerminalType.READ:
                    resultString = "read";
                    break;
                case TerminalType.WRITE:
                    resultString = "write";
                    break;
                case TerminalType.INT:
                    resultString = "int";
                    break;
                case TerminalType.REAL:
                    resultString = "real";
                    break;
                case TerminalType.PLUS:
                    resultString = "+";
                    break;
                case TerminalType.MINUS:
                    resultString = "-";
                    break;
                case TerminalType.MUL:
                    resultString = "*";
                    break;
                case TerminalType.DIV:
                    resultString = "/";
                    break;
                case TerminalType.ASSIGN:
                    resultString = "=";
                    break;
                case TerminalType.LESS:
                    resultString = "<";
                    break;
                case TerminalType.GREATER:
                    resultString = ">";
                    break;
                case TerminalType.EQUAL:
                    resultString = "==";
                    break;
                case TerminalType.NOTEQUAL:
                    resultString = "<>";
                    break;
                case TerminalType.LPARENT:
                    resultString = "(";
                    break;
                case TerminalType.RPARENT:
                    resultString = ")";
                    break;
                case TerminalType.SEMI:
                    resultString = ";";
                    break;
                case TerminalType.LBRACE:
                    resultString = "{";
                    break;
                case TerminalType.RBRACE:
                    resultString = "}";
                    break;
                case TerminalType.NOTES:
                    resultString = "/**/";
                    break;
                case TerminalType.LBRACKET:
                    resultString = "[";
                    break;
                case TerminalType.RBRACKET:
                    resultString = "]";
                    break;
                case TerminalType.COMMA:
                    resultString = ",";
                    break;
                case TerminalType.INTVAL:
                    resultString = "整数类型的值";
                    break;
                case TerminalType.REALVAL:
                    resultString = "浮点类型的值";
                    break;
                case TerminalType.ID:
                    resultString = "标识符";
                    break;
                case TerminalType.ERR:
                    resultString = "错误信息";
                    break;
                case TerminalType.END:
                    resultString = "程序结束标志";
                    break;
                case TerminalType.EMPTY:
                    resultString = "'空'";
                    break;
                case TerminalType.BREAKPOINT:
                    resultString = "断点";
                    break;
                case TerminalType.DEFAULT:
                    resultString = "默认终结符号";
                    break;
                default:
                    resultString = "";
                    break;
            }
            return resultString;
        }

        /// <summary>
        /// 将非终结符号串转换为字符串
        /// </summary>
        /// <param name="N">待转换的非终结符号串</param>
        /// <returns>终结符号串对应的字符串</returns>
        public string N2String(NEnum N)
        {
            string resultString;
            switch (N)
            {
                case NEnum.program:
                    resultString = "主程序";
                    break;
                case NEnum.stmt_sequence:
                    resultString = "语句序列";
                    break;
                case NEnum.statement:
                    resultString = "语句";
                    break;
                case NEnum.stmt_block:
                    resultString = "语句块";
                    break;
                case NEnum.if_stmt:
                    resultString = "if语句";
                    break;
                case NEnum.if_stmt_block:
                    resultString = "if语句块";
                    break;
                case NEnum.else_stmt_block:
                    resultString = "else语句块";
                    break;
                case NEnum.while_stmt:
                    resultString = "while语句块";
                    break;
                case NEnum.assign_stmt:
                    resultString = "赋值语句";
                    break;
                case NEnum.read_stmt:
                    resultString = "read语句";
                    break;
                case NEnum.write_stmt:
                    resultString = "write语句";
                    break;
                case NEnum.declare_stmt:
                    resultString = "声明语句";
                    break;
                case NEnum.variable:
                    resultString = "变量";
                    break;
                case NEnum.exp:
                    resultString = "表达式";
                    break;
                case NEnum.addtive_exp:
                    resultString = "加法表达式";
                    break;
                case NEnum.term:
                    resultString = "term";
                    break;
                case NEnum.factor:
                    resultString = "表达式计算因子";
                    break;
                case NEnum.logical_op:
                    resultString = "逻辑表达式符号";
                    break;
                case NEnum.add_op:
                    resultString = "加减运算符号";
                    break;
                case NEnum.mul_op:
                    resultString = "乘法符号";
                    break;
                case NEnum.DEFAULT:
                    resultString = "默认";
                    break;
                default:
                    resultString = "";
                    break;
            }

            return resultString;
        }
    }
}
