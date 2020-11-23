using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM
{
    //名字表
    class nametab
    {
        //名字标识符串
        private string name;

        //名字种类 例常量、变量、类型、过程
        private string kind;

        //名字所在的程序体的静态层数
        private int lev;

        //名字的类型 例整型、字符型、布尔型、数组类型
        private string typ;

        //用于标明名字是否为变量形参名
        private bool normal;

        //当名字为数组类型或数组变量名 myref指向该数组在数组信息表中的位置；
        //当名字为过程名时，myref指向该过程在程序体表中的位置
        //其他情况myref为0;
        private string myref;

        //当名字为变量名，存入该变量在相应活动记录中分配的存贮单元的相对地址
        //当名字为过程名，填入他们相应代码的入口地址
        //当名字为常量名，填入他们的相应值
        //当名字为类型名，填入该类型数据所需存贮单元的数目
        private string adr_val_size;

        //指向同一程序体中上一个名字在nametab中的位置，第一个的link为0
        private string link;
    }
}
