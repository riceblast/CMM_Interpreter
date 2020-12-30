using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM
{

    /// <summary>
    /// 预测分析表表项
    /// 包括单个产生式
    /// </summary>
    public class ParsingTableItem
    {
        /// <summary>
        /// 某个预测分析表项中的产生式集合，小标从小到大是尝试的顺序
        /// 一个list存储着一个产生式
        /// 在生成productions时需要把“是否是子树”“终结符号/非终结符号”属性设置完毕
        /// 并将“子树”属性设置初始化
        /// </summary>
        public List<ParseTreeNode> production;

        public ParsingTableItem()
        {
            production = new List<ParseTreeNode>();
        }
    }

    /// <summary>
    /// 预测分析表，以非终结符号为行，终结符号为列
    /// </summary>
    public class ParsingTable
    {
        private int TCount = Enum.GetValues(typeof(TerminalType)).GetLength(0); // 终结符号的数量
        private int NCount = Enum.GetValues(typeof(NEnum)).GetLength(0); // 非终结符号的数量
        private List<ParsingTableItem>[,] parsingTable; // 预测分析表,定长数组

        /// <summary>
        /// 预测分析表构造函数
        /// </summary>
        public ParsingTable()
        {
            parsingTable = new List<ParsingTableItem>[NCount, TCount]; 

            // TODO 利用文本初始化预测分析表
            for(int i = 0; i < parsingTable.GetLength(0); i++)
            {
                for(int j = 0; j < parsingTable.GetLength(1); j++)
                {
                    // 如果没有对应的产生式，则存储使得list长度为0
                    parsingTable[i, j] = new List<ParsingTableItem>();
                }
            }

            // program -> stmt-sequence
            SetParsingTable(NEnum.program, new List<ParseTreeNode>()
            {
                new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.stmt_sequence)
            }, TerminalType.IF, TerminalType.WHILE, TerminalType.READ, TerminalType.WRITE,
            TerminalType.INT, TerminalType.REAL, TerminalType.ID, TerminalType.END);

            // stmt-sequence -> statement stmt-sequence
            SetParsingTable(NEnum.stmt_sequence, new List<ParseTreeNode>()
            {
                new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.statement),
                new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.stmt_sequence)
            }, TerminalType.IF, TerminalType.WHILE, TerminalType.READ, TerminalType.WRITE,
            TerminalType.INT, TerminalType.REAL, TerminalType.ID);

            // stmt-sequence -> ε
            SetParsingTable(NEnum.stmt_sequence, new List<ParseTreeNode>()
            {
                new ParseTreeNode(true, TerminalType.EMPTY, NEnum.DEFAULT)
            }, TerminalType.END, TerminalType.RBRACE);

            // statement -> if
            SetParsingTable(NEnum.statement, NEnum.if_stmt, TerminalType.IF);

            // statemtn -> while
            SetParsingTable(NEnum.statement, NEnum.while_stmt, TerminalType.WHILE);

            // statement -> read
            SetParsingTable(NEnum.statement, NEnum.read_stmt, TerminalType.READ);

            // statement -> write
            SetParsingTable(NEnum.statement, NEnum.write_stmt, TerminalType.WRITE);

            // statement -> declare 
            SetParsingTable(NEnum.statement, NEnum.declare_stmt, TerminalType.INT,
                TerminalType.REAL);

            // statement -> assign
            SetParsingTable(NEnum.statement, NEnum.assign_stmt, TerminalType.ID);

            // stmt-block -> {stmt-sequence}
            SetParsingTable(NEnum.stmt_block, new List<ParseTreeNode>()
            {
                new ParseTreeNode(true, TerminalType.LBRACE, NEnum.DEFAULT),
                new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.stmt_sequence),
                new ParseTreeNode(true, TerminalType.RBRACE, NEnum.DEFAULT)
            }, TerminalType.LBRACE);

            // if-stmt -> if-stmt-block else-stmt-block
            SetParsingTable(NEnum.if_stmt, new List<ParseTreeNode>()
            {
                new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.if_stmt_block),
                new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.else_stmt_block)
            }, TerminalType.IF);

            // if-stmt-block -> if(exp)stmt-block
            SetParsingTable(NEnum.if_stmt_block, new List<ParseTreeNode>()
            {
                new ParseTreeNode(true, TerminalType.IF, NEnum.DEFAULT),
                new ParseTreeNode(true, TerminalType.LPARENT, NEnum.DEFAULT),
                new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.exp),
                new ParseTreeNode(true, TerminalType.RPARENT, NEnum.DEFAULT),
                new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.stmt_block)
            }, TerminalType.IF);

            // else-stmt-block -> else stmt-block
            SetParsingTable(NEnum.else_stmt_block, new List<ParseTreeNode>()
            {
                new ParseTreeNode(true, TerminalType.ELSE, NEnum.DEFAULT),
                new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.stmt_block)
            }, TerminalType.ELSE);

            // else-stmt-block -> ε
            SetParsingTable(NEnum.else_stmt_block, TerminalType.EMPTY, 
                TerminalType.IF,TerminalType.READ, TerminalType.WRITE, TerminalType.WHILE, 
                TerminalType.INT, TerminalType.REAL, TerminalType.ID,TerminalType.RBRACE,
                TerminalType.END);

            // while-stmt -> while(exp)stmt-block
            SetParsingTable(NEnum.while_stmt, new List<ParseTreeNode>()
            {
                new ParseTreeNode(true, TerminalType.WHILE, NEnum.DEFAULT),
                new ParseTreeNode(true, TerminalType.LPARENT, NEnum.DEFAULT),
                new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.exp),
                new ParseTreeNode(true, TerminalType.RPARENT, NEnum.DEFAULT),
                new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.stmt_block)
            }, TerminalType.WHILE);

            // assign-stmt -> variable = exp;
            SetParsingTable(NEnum.assign_stmt, new List<ParseTreeNode>()
            {
                new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.variable),
                new ParseTreeNode(true, TerminalType.ASSIGN, NEnum.DEFAULT),
                new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.exp),
                new ParseTreeNode(true, TerminalType.SEMI, NEnum.DEFAULT)
            }, TerminalType.ID);

            // read-stmt -> read variable;
            SetParsingTable(NEnum.read_stmt, new List<ParseTreeNode>()
            {
                new ParseTreeNode(true, TerminalType.READ, NEnum.DEFAULT),
                new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.variable),
                new ParseTreeNode(true, TerminalType.SEMI, NEnum.DEFAULT)
            }, TerminalType.READ);

            // write-stmt -> write exp;
            SetParsingTable(NEnum.write_stmt, new List<ParseTreeNode>()
            {
                new ParseTreeNode(true, TerminalType.WRITE, NEnum.DEFAULT),
                new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.exp),
                new ParseTreeNode(true, TerminalType.SEMI, NEnum.DEFAULT)
            }, TerminalType.WRITE);

            // declare-stmt -> int variable;
            SetParsingTable(NEnum.declare_stmt, new List<ParseTreeNode>()
            {
                new ParseTreeNode(true, TerminalType.INT, NEnum.DEFAULT),
                new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.variable),
                new ParseTreeNode(true, TerminalType.SEMI, NEnum.DEFAULT)
            }, TerminalType.INT);

            // declare-stmt -> real variable;
            SetParsingTable(NEnum.declare_stmt, new List<ParseTreeNode>()
            {
                new ParseTreeNode(true, TerminalType.REAL, NEnum.DEFAULT),
                new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.variable),
                new ParseTreeNode(true, TerminalType.SEMI, NEnum.DEFAULT)
            }, TerminalType.REAL);

            // variable-stmt -> DEFAULT
            // 需要特殊处理
            SetParsingTable(NEnum.variable, NEnum.DEFAULT, TerminalType.ID);

            // exp -> DEFAULT
            // 需要特殊处理
            SetParsingTable(NEnum.exp, NEnum.DEFAULT, TerminalType.ID,
                TerminalType.INTVAL, TerminalType.REALVAL, TerminalType.LPARENT,
                TerminalType.PLUS, TerminalType.MINUS);

            // addtive-exp ->DEFAULT
            // 需要特殊处理
            SetParsingTable(NEnum.addtive_exp, NEnum.DEFAULT, TerminalType.ID,
                TerminalType.INTVAL, TerminalType.REALVAL, TerminalType.LPARENT,
                TerminalType.PLUS, TerminalType.MINUS);

            // term -> DeFAULT
            // 需要特殊处理
            SetParsingTable(NEnum.term, NEnum.DEFAULT, TerminalType.ID,
                TerminalType.INTVAL, TerminalType.REALVAL, TerminalType.LPARENT,
                TerminalType.PLUS, TerminalType.MINUS);

            // factor -> DEFAULT
            // 需要特殊处理
            SetParsingTable(NEnum.factor, NEnum.DEFAULT, TerminalType.ID,
                TerminalType.INTVAL, TerminalType.REALVAL, TerminalType.LPARENT,
                TerminalType.PLUS, TerminalType.MINUS);

            // logical-op -> > < <> ==
            SetParsingTable(NEnum.logical_op, TerminalType.LESS, TerminalType.LESS);
            SetParsingTable(NEnum.logical_op, TerminalType.GREATER, TerminalType.GREATER);
            SetParsingTable(NEnum.logical_op, TerminalType.NOTEQUAL, TerminalType.NOTEQUAL);
            SetParsingTable(NEnum.logical_op, TerminalType.EQUAL, TerminalType.EQUAL);

            // add-op -> + -
            SetParsingTable(NEnum.add_op, TerminalType.PLUS, TerminalType.PLUS);
            SetParsingTable(NEnum.add_op, TerminalType.MINUS, TerminalType.MINUS);

            // mul-op -> * /
            SetParsingTable(NEnum.mul_op, TerminalType.MUL, TerminalType.MUL);
            SetParsingTable(NEnum.mul_op, TerminalType.DIV, TerminalType.DIV);
        }

        /// <summary>
        /// 利用非终结符号和终结符号获取预测分析表的表项（产生式）
        /// </summary>
        /// <param name="nEnum"></param>
        /// <param name="tokenType"></param>
        /// <returns></returns>
        public List<ParsingTableItem> GetItem(NEnum nEnum, TerminalType tokenType)
        {
            return this.parsingTable[(int)nEnum, (int)tokenType];
        }

        /// <summary>
        /// 为预测分析表设置表项
        /// </summary>
        /// <param name="nEnum">产生式左侧非终结符号</param>
        /// <param name="tokenTypes">所有需要被设置表项的终结符号</param>
        /// <param name="treeNodes">构成产生式的语法分析树结点</param>
        public void SetParsingTable(NEnum nEnum, List<ParseTreeNode> treeNodes
            ,params TerminalType[] tokenTypes)
        {
            ParsingTableItem tableItem; // 用于接收新建的预测分析表表项
            foreach(TerminalType tEnum in tokenTypes)
            {
                // 将树结点添加到产生式中
                tableItem = new ParsingTableItem();
                tableItem.production.AddRange(treeNodes);

                this.GetItem(nEnum, tEnum).Add(tableItem);
            }
        }

        /// <summary>
        /// 为预测分析表设置表项(产生式中由一个终结符号构成)
        /// </summary>
        /// <param name="nEnum">产生式左侧非终结符号</param>
        /// <param name="production">构成产生式的终结符号</param>
        /// <param name="tokenTypes">所有需要被设置表项的终结符号</param>
        public void SetParsingTable(NEnum nEnum, TerminalType production, 
            params TerminalType[] tokenTypes)
        {
            ParsingTableItem tableItem; // 用于接收新建的预测分析表表项
            ParseTreeNode treeNode; // 用于接收新建的语法分析树结点
            foreach (TerminalType tEnum in tokenTypes)
            {
                // 将树结点添加到产生式中
                tableItem = new ParsingTableItem();
                treeNode = new ParseTreeNode(true, production, NEnum.DEFAULT);
                tableItem.production.Add(treeNode);

                this.GetItem(nEnum, tEnum).Add(tableItem);
            }
        }

        /// <summary>
        /// 为预测分析表设置表项(产生式中由一个非终结符号构成)
        /// </summary>
        /// <param name="nEnum">产生式左侧非终结符号</param>
        /// <param name="production">构成产生式的非终结符号</param>
        /// <param name="tokenTypes">所有需要被设置表项的终结符号</param>
        public void SetParsingTable(NEnum nEnum, NEnum production, 
            params TerminalType[] tokenTypes)
        {
            ParsingTableItem tableItem; // 用于接收新建的预测分析表表项
            ParseTreeNode treeNode; // 用于接收新建的语法分析树结点

            foreach (TerminalType tEnum in tokenTypes)
            {
                // 将树结点添加到产生式中
                tableItem = new ParsingTableItem();
                treeNode = new ParseTreeNode(false, TerminalType.DEFAULT, production);
                tableItem.production.Add(treeNode);

                this.GetItem(nEnum, tEnum).Add(tableItem);
            }
        }
    }
}
