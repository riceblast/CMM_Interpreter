using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM
{
    class SemanticAnalysis
    {
        public static void nodeAnalysis(ParseTreeNode treeNode) {
            foreach (ParseTreeNode node in treeNode.Childs) {
                if (node.IsLeaf) {
                    if (node.TSymbol == TokenType.LBRACE) {
                        Constant.currentScopeIncrease();
                    } else if (node.TSymbol == TokenType.RBRACE) {
                        Constant.currentScopeDecrease();
                    }
                    //是叶子节点直接退出
                    break;
                }
                    
                switch (node.NSymbol) {
                    case NEnum.statement:
                        SentenceAnalysis.StmtAnalysis(node.Childs[0]);
                        break;
                    default:
                        nodeAnalysis(node);
                        break;                
                }
            }
        }
    }
}
