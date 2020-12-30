using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM
{
    /// <summary>
    /// 解释器类
    /// 负责词法、语法、语义的综合分析
    /// </summary>
    public class Interpreter
    {
        public string SourceCode { get; set; }

        public SyntacticAnalyser syntacticAnalyser { get; set; }

        /// <summary>
        /// 利用CMM源码，初始化解释器
        /// </summary>
        /// <param name="sourceCode">CMM源码</param>
        public Interpreter(string sourceCode)
        {
            this.SourceCode = sourceCode;
            this.syntacticAnalyser = new SyntacticAnalyser();
        }

        /// <summary>
        /// 调用词法分析程序
        /// </summary>
        /// <returns>词法分析的结果</returns>
        public TokenResult WordAnalyse()
        {
            return WordAnalyser.Analyse(this.SourceCode);
        }

        /// <summary>
        /// 调用语法分析程序
        /// </summary>
        /// <param name="tokenResult">词法分析结果</param>
        /// <param name="bpList">breakPoint列表，断点所在行数</param>
        /// <returns>语法分析树</returns>
        public ParseTree SyntacticAnalyse(TokenResult tokenResult, List<int> bpList)
        {
            return this.syntacticAnalyser.SyntacticAnalysis(tokenResult,
                bpList);
        }

        /// <summary>
        /// 解释器核心运行
        /// 包括调用词法、语法、语义分析
        /// 以及出错处理程序
        /// </summary>
        /// <param name="bpList">breakPoint列表，所有断点所在行数</param>
        public void Run(List<int> bpList)
        {
            TokenResult tokenResult = WordAnalyse();

            // TODO 词法.语法分析出错处理程序待做

            ParseTree parseTree = SyntacticAnalyse(tokenResult, bpList);

        }
    }
}
