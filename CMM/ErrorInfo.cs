using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM
{
    /// <summary>
    /// 报错信息类，用于记录词法/语法/语义分析中的报错
    /// </summary>
    public class ErrorInfo
    {
        /// <summary>
        /// 报错的行号
        /// </summary>
        public int LineNum { get; set; }

        /// <summary>
        /// 报错的信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public ErrorInfo()
        {
            LineNum = -1;
            Message = "";
        }

        /// <summary>
        /// 全参构造函数
        /// </summary>
        /// <param name="lineNum">出现错误的行号</param>
        /// <param name="message">报错信息</param>
        public ErrorInfo(int lineNum, string message)
        {
            this.LineNum = lineNum;
            this.Message = message;
        }
    }
}
