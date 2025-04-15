using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VISAInstrument.AutoTest
{
    [Serializable]
    public class PowerTest_1
    {
        [Serializable]
        public struct PowerTest_1_struct
        {
            public string name;   //测试名
            public string index;  //唯一标识&序号
            public string time;   //运行时间分钟
            public string danwei; //结果单位

            public string volt;    //电压
            public string wendu;   //温度
            public string shidu;   //湿度

            public bool user_string_enable;//自动生成串口数据(false)-自定义串口数据(true)
            //true
            public string[] user_string; //用户自定义配置串口数据
            //false
            public string source;  //时钟源
            public string hclk;    //HCLK
            public string power_state; //电源模式
            public string waisheshineng_which;//采用哪一种外设使能
            public bool[] waisheshineng; //使能的外设
            
     
        }
        public List<PowerTest_1_struct> data=new List<PowerTest_1_struct>();
    }
  
}
