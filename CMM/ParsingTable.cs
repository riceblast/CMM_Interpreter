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
        public List<ParseTreeNode> production = new List<ParseTreeNode>();
    }

    /// <summary>
    /// 预测分析表，以非终结符号为行，终结符号为列
    /// </summary>
    public class ParsingTable
    {
        private const int TCount = 31; // 终结符号的数量
        private const int NCount = 24; // 非终结符号的数量
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
    }
}
