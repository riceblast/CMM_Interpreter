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
        public InterpretResult Run(List<int> bpList)
        {
            // 解释器最终结果
            InterpretResult result = new InterpretResult();

            // 词法分析
            TokenResult tokenResult = WordAnalyse();
            result.WordAnalyseResult = tokenResult;
            result.Period = InterpretPeriod.Word;
            if (!tokenResult.IsSuccess)
            {
                // 如果词法分析失败，则给出失败原因
                result.IsSuccess = false;
                result.ErrorInfos = tokenResult.ErrorInfos;

                return result;
            }

            // 语法分析
            this.syntacticAnalyser = new SyntacticAnalyser();
            ParseTree parseTree = SyntacticAnalyse(tokenResult, bpList);
            result.SyntacticAnalyseResult = parseTree;
            result.Period = InterpretPeriod.Syntactic;
            if (!parseTree.IsSuccess)
            {
                result.IsSuccess = false;
                result.ErrorInfos = parseTree.ErrorInfos;

                return result;
            }

            return result;
        }
    }

    /// <summary>
    /// 解释器运行阶段
    /// </summary>
    public enum InterpretPeriod
    {
        /// <summary>
        /// 词法
        /// </summary>
        Word,
        /// <summary>
        /// 语法
        /// </summary>
        Syntactic,
        /// <summary>
        /// 语义
        /// </summary>
        Sentence,
        /// <summary>
        /// 解释器运行结束
        /// </summary>
        Finish
    }

    /// <summary>
    /// 词法、语法封装类
    /// </summary>
    public class InterpretResult
    {
        /// <summary>
        /// 是否解释运行成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 报错信息列表
        /// </summary>
        public List<ErrorInfo> ErrorInfos { get; set; }

        /// <summary>
        /// 编译器当前运行的阶段
        /// </summary>
        public InterpretPeriod Period { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="errorInfos">报错信息列表</param>
        /// <param name="period">所处阶段</param>
        public InterpretResult(bool isSuccess, InterpretPeriod period)
        {
            this.IsSuccess = isSuccess;
            this.Period = period;
            ErrorInfos = new List<ErrorInfo>();
        }

        /// <summary>
        /// 全参数构造函数
        /// </summary>
        /// <param name="isSuccess">是否解释成功</param>
        /// <param name="period">解释器运行到的阶段</param>
        /// <param name="errorInfos">错误信息</param>
        public InterpretResult(bool isSuccess, InterpretPeriod period, 
            List<ErrorInfo> errorInfos)
        {
            this.IsSuccess = isSuccess;
            this.Period = period;
            this.ErrorInfos = errorInfos;
        }

        /// <summary>
        /// 词法分析结果封装
        /// </summary>
        public TokenResult WordAnalyseResult { get; set; }

        /// <summary>
        /// 语法分析结果封装类
        /// </summary>
        public ParseTree SyntacticAnalyseResult { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public InterpretResult()
        {
            this.IsSuccess = true;
            this.Period = InterpretPeriod.Finish;
            this.ErrorInfos = new List<ErrorInfo>();
            this.WordAnalyseResult = null;
            this.SyntacticAnalyseResult = null;
        }

        /// <summary>
        /// 获取解释器当前报错信息的字符串
        /// </summary>
        /// <returns>解释器当前结果字符串</returns>
        public string GetErrorString()
        {
            if (IsSuccess)
            {
                return "";
            }

            string errorString;
            string periodString; // 程序因为报错而停止运行的阶段
            switch (this.Period)
            {
                case InterpretPeriod.Word:
                    periodString = "词法分析";
                    break;
                case InterpretPeriod.Syntactic:
                    periodString = "语法分析";
                    break;
                case InterpretPeriod.Sentence:
                    periodString = "语义分析";
                    break;
                default:
                    periodString = "";
                    break;
            }
            errorString = $"程序在 {periodString} 阶段出错，报错信息：\n";
            foreach (ErrorInfo errorInfo in ErrorInfos)
            {
                errorString += errorInfo.ToString() + "\n";
            }

            return errorString;
        }
    }
}
