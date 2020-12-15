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
        ParseTreeNode root { get; set; }

        // 语法树其他信息
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
        public TokenType TSymbol { get; set; }

        /// <summary>
        /// 如果该结点是非叶子结点，则该属性非空
        /// </summary>
        public NEnum NSymbol { get; set; }

        /// <summary>
        /// 该结点的所有孩子结点
        /// </summary>
        public List<ParseTreeNode> Childs { get; set; }
    }
}
