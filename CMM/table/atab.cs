using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM
{
    //用于记录每个数组的详细信息
    class atab
    {
        //数组的下标类型，整数、枚举
        private string inxtyp;

        //数组元素的类型
        private string eltyp;

        //当元素为数组时，它指向该元素数组信息在atab中的位置；其他情况为0
        private int elref;

        //数组下标的下限
        private int low;

        //数组下标的上限
        private int high;

        //一个元素的所占的空间
        private string elsize;

        //整个数组的空间
        private string size;

    }
}
