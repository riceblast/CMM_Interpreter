using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM
{
    public enum TerminalType
    {
        IF,
        ELSE,
        WHILE,
        READ,
        WRITE,
        INT,
        REAL,
        PLUS,
        MINUS,
        MUL,
        DIV,
        ASSIGN,
        LESS,
        GREATER,
        EQUAL,
        NOTEQUAL,
        LPARENT,
        RPARENT,
        SEMI,
        LBRACE,
        RBRACE,
        NOTES,
        LBRACKET,
        RBRACKET,
        COMMA,
        INTVAL,
        REALVAL,
        ID,
        ERR,   // 表示词法分析中的错误情况
        END,
        EMPTY,   // 即“空”符号
        BREAKPOINT,  // 特殊的“断点”终结符号
        DEFAULT // 表示特殊含义的default
    }
}
