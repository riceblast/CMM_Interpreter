using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM
{
    /// <summary>
    /// 语法分析树，用于存放语法分析的结果
    /// </summary>
    public class ParseTree
    {
        /// <summary>
        /// 语法树根结点
        /// </summary>
        public ParseTreeNode Root { get; set; }

        /// <summary>
        /// 是否语法分析成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 报错信息
        /// </summary>
        public List<ErrorInfo> errorInfos { get; set; }

        /// <summary>
        /// 语法分析树构造函数，初始化根节点
        /// </summary>
        public ParseTree()
        {
            Root = new ParseTreeNode(false, TerminalType.DEFAULT, NEnum.program);
            IsSuccess = true;
            errorInfos = new List<ErrorInfo>();
        }
    }

    /// <summary>
    /// 语法分析树结点类
    /// 非叶子结点中，存放当前分析的非终结符号，且Childs列表非空
    /// 叶子结点中，存放分析得到的终结符号，且Childs列表为null
    /// </summary>
    public class ParseTreeNode
    {
        /// <summary>
        /// 标识是否是叶子结点
        /// </summary>
        public bool IsLeaf { get; set; }

        /// <summary>
        /// 如果该结点是叶子结点，则该属性非空
        /// </summary>
        public TerminalType TSymbol { get; set; }

        /// <summary>
        /// 如果该结点是非叶子结点，则该属性非空
        /// </summary>
        public NEnum NSymbol { get; set; }

        /// <summary>
        /// 该结点中枚举值转换为string对应的值
        /// </summary>
        public string StringValue { get; set; }

        /// <summary>
        /// 该结点的所有孩子结点
        /// </summary>
        public List<ParseTreeNode> Childs { get; set; }

        /// <summary>
        /// 语法分析树结点的构造函数
        /// </summary>
        /// <param name="isLeaf">是否是叶子结点</param>
        /// <param name="tSymbol">如果是叶子结点，则是TokenSymbol的值，否则为default</param>
        /// <param name="nSymbol">如果是非叶子结点，则是NSymbol的值，否则为deault</param>
        public ParseTreeNode(bool isLeaf, TerminalType tSymbol, NEnum nSymbol)
        {
            this.IsLeaf = isLeaf;
            this.TSymbol = tSymbol;
            this.NSymbol = nSymbol;
            this.Childs = new List<ParseTreeNode>();
        }
    }
}
