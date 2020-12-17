using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM
{
    /// <summary>
    /// 词法分析结果类，用于封装词法分析的结果
    /// </summary>
    public class TokenResult
    {
        /// <summary>
        /// 是否词法分析成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 词法分析结果的token串
        /// </summary>
        public List<Token> Tokens { get; set; }

        /// <summary>
        /// 词法分析中的错误信息
        /// </summary>
        public List<ErrorInfo> ErrorInfos { get; set; }

        public TokenResult()
        {
            Tokens = new List<Token>();
            ErrorInfos = new List<ErrorInfo>();
        }
    }

    /// <summary>
    /// 词法分析结果中的单词
    /// </summary>
    public class Token
    {
        /// <summary>
        /// 该token的字符串字面值
        /// </summary>
        public string StrValue { get; set; }

        /// <summary>
        /// token对应的类型信息
        /// </summary>
        public TerminalType TokenType { get; set; }

        /// <summary>
        /// 该token所在的行数
        /// </summary>
        public int LineNum { get; set; }
    }
}
