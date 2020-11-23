using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMM
{
    //记录各程序体的总信息
    //用于对源程序中定义的名字的作用域进行分析
    //对名字表进行管理
    class btab
    {
        //指向本程序体中最后一个形式参数在nametab中的位置
        private string lastpar;

        //指向本程序体中最后一个名字在nametab中的位置
        private string last;

        //本程序体所有形参所需要的存贮单元
        private string psize;
        
        //本程序体所有局部数据需要的存贮空间
        private string vsize;
    }
}
