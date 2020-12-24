using System;
using System.Collections.Generic;
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

        /// <summary>
        /// 利用CMM源码，初始化解释器
        /// </summary>
        /// <param name="sourceCode">CMM源码</param>
        public Interpreter(string sourceCode)
        {
            this.SourceCode = sourceCode;
        }
    }
}
