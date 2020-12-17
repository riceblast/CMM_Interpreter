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
        /// 该token的字符串字面值
        /// </summary>
        public string StrValue { get; set; }

        /// <summary>
        /// token对应的类型信息
        /// </summary>
        public TerminalType tokenType { get; set; }

        /// <summary>
        /// 该token所在的行数
        /// </summary>
        public int LineNum { get; set; }
    }
}
