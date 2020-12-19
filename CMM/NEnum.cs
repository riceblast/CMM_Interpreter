using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM
{
    /// <summary>
    /// 非终结符号枚举
    /// </summary>
    public enum NEnum
    {
        program,
        stmt_sequence,
        statement,
        stmt_block,
        if_stmt,
        if_stmt_block,
        else_stmt_block,
        while_stmt,
        assign_stmt,
        read_stmt,
        write_stmt,
        declare_stmt,
        variable,
        exp,
        logical_suffix,
        addtive_exp,
        add_op_suffix,
        term,
        mul_op_suffix,
        factor,
        logical_op,
        add_op,
        mul_op,
        /// <summary>
        /// 表示特殊含义的默认值，例如：表示NEnum为空
        /// </summary>
        DEFAULT
    }
}
