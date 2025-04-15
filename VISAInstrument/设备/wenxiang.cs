using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IoTClient;
using IoTClient.Tool;
using Sunny.UI;
namespace VISAInstrument
{
    public partial class wenxiang : UIForm
    {
        public static wenxiang form_st = null;
        public wenxiang()
        {
            InitializeComponent();
            form_st = this;
        }
        #region 毫秒延时 界面不会卡死
        public static void Delay(int mm)
        {
            DateTime current = DateTime.Now;
            while (current.AddMilliseconds(mm) > DateTime.Now)
            {
                Application.DoEvents();
            }
            return;
        }
        #endregion

        private Decimal ChangeDataToD(string strData)
        {
            Decimal dData = 0.0M;
            if (strData.Contains("E"))
            {
                try
                {
                    dData = Convert.ToDecimal(Decimal.Parse(strData.ToString(), System.Globalization.NumberStyles.Float));
                }
                catch
                {
                    return dData;
                }
            }
            return dData;
        }



        //设置温箱
        public static bool start_flag = false;
        private void button1_Click(object sender, EventArgs e)
        {
            dynamic result = null;

            ////写
            //result = ModbusTcpControl.form_st.client.Write(address, short.Parse(txt_value.Text?.Trim()), ModbusTcpControl.form_st.station_Number);
            ////读
            //result = ModbusTcpControl.form_st.client.ReadInt16(txt_address.Text, ModbusTcpControl.form_st.station_Number);
            ////成功
            //
            // string result_string = ($"[读取 {txt_address.Text?.Trim()} 成功]：{result.Value}\t\t耗时：{result.TimeConsuming}ms");

            //写温度
            result = ModbusTcpControl.form_st.client.Write("43", short.Parse(((int)(Convert.ToDouble(textBox7.Text) * 10)).ToString()?.Trim()), ModbusTcpControl.form_st.station_Number);
            if (!result.IsSucceed)
            {
                goto end1;
            }
            //写湿度
            result = ModbusTcpControl.form_st.client.Write("44", short.Parse(((int)(Convert.ToDouble(textBox1.Text) * 10)).ToString()?.Trim()), ModbusTcpControl.form_st.station_Number);
            if (!result.IsSucceed)
            {
                goto end1;
            }
            //启动预设
            result = ModbusTcpControl.form_st.client.Write("47", short.Parse("1"?.Trim()), ModbusTcpControl.form_st.station_Number);
            if (!result.IsSucceed)
            {
                goto end1;
            }
            else
            {

                MessageBox.Show("Start Success");
            }

            checkBox1_CheckedChanged(null, null);




        end1:
            {
                //MessageBox.Show("启动失败");
            }

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            dynamic result = null;
            while (checkBox1.Checked)
            {
                //读温度   
                result = ModbusTcpControl.form_st.client.ReadInt16("53", ModbusTcpControl.form_st.station_Number);
                if (result.IsSucceed)
                {
                  
                    textBox4.Text = (Convert.ToDouble(String.Format($"{result.Value}")) / 10).ToString();                                      ;
                }
                //读湿度   
                result = ModbusTcpControl.form_st.client.ReadInt16("54", ModbusTcpControl.form_st.station_Number);
                if (result.IsSucceed)
                {
                    textBox2.Text = (Convert.ToDouble(String.Format($"{result.Value}")) / 10).ToString();
                }
                //延时
                Delay(300);
            }
        }

        //停止
        private void button2_Click(object sender, EventArgs e)
        {
            dynamic result = null;
            result = ModbusTcpControl.form_st.client.Write("47", short.Parse("0"?.Trim()), ModbusTcpControl.form_st.station_Number);
            if (result.IsSucceed)
            {
                MessageBox.Show("Stopped OK");
            }
            checkBox1_CheckedChanged(null, null);
        }
        //保持
        private void button3_Click(object sender, EventArgs e)
        {
            dynamic result = null;
            result = ModbusTcpControl.form_st.client.Write("47", short.Parse("0"?.Trim()), ModbusTcpControl.form_st.station_Number);
            if (result.IsSucceed)
            {
                MessageBox.Show("保持温度 OK");
            }
            checkBox1_CheckedChanged(null, null);
        }

        //温度控制
        public static bool temp_contrl(string wendu, string shidu)
        {
            dynamic result = null;
            bool wendu_flag = false;
            bool shidu_flag = false;
            //判断是否执行温度或湿度
            if (Convert.ToUInt16(wendu,10)==0)
            { 
                wendu_flag = true;  
            }
            if (Convert.ToUInt16(shidu, 10) == 0)
            {
                shidu_flag = true;
            }

            //写温度
            result = ModbusTcpControl.form_st.client.Write("43", short.Parse(wendu?.Trim()), ModbusTcpControl.form_st.station_Number);
            if (!result.IsSucceed)
            {
                return false;
            }
            if (shidu != "NULL")
            { //写湿度
                result = ModbusTcpControl.form_st.client.Write("44", short.Parse(shidu?.Trim()), ModbusTcpControl.form_st.station_Number);
                if (!result.IsSucceed)
                {
                    return false;
                }
            }
            //启动预设
            result = ModbusTcpControl.form_st.client.Write("47", short.Parse("1"?.Trim()), ModbusTcpControl.form_st.station_Number);
            if (!result.IsSucceed)
            {
                return false;
            }
            //进入温度监控
            while (true)
            {
                Delay(500);
                //读温度   
                result = ModbusTcpControl.form_st.client.ReadInt16("53", ModbusTcpControl.form_st.station_Number);
                if (result.IsSucceed)
                {
                    if (String.Format($"{result.Value}") == wendu)
                    {
                        wendu_flag=true;
                    }
                }

                if (shidu != "NULL")
                { //读湿度   
                    result = ModbusTcpControl.form_st.client.ReadInt16("54", ModbusTcpControl.form_st.station_Number);
                    if (result.IsSucceed)
                    {
                        if (String.Format($"{result.Value}") == shidu)
                        {
                            shidu_flag = true;
                        }
                    }
                }

                //判断是否都达到对应温度
                if (wendu_flag && (shidu_flag|| shidu == "NULL"))
                {
                    return true;
                }
            }
        }


        //外部温度控制
        public static bool set_wendu(string wendu)
        {
            dynamic result = null;
            //写温度
            result = ModbusTcpControl.form_st.client.Write("43", short.Parse(wendu?.Trim()), ModbusTcpControl.form_st.station_Number);
            if (!result.IsSucceed)
            {
                return false;
            }
            //启动预设
            result = ModbusTcpControl.form_st.client.Write("47", short.Parse("1"?.Trim()), ModbusTcpControl.form_st.station_Number);
            if (!result.IsSucceed)
            {
                return false;
            }
            return true;
        }


    //获取当前温度
        public static string get_wendu()
        {
            dynamic result = null;  
            result = ModbusTcpControl.form_st.client.ReadInt16("53", ModbusTcpControl.form_st.station_Number);
            if (result.IsSucceed)
            {
                return String.Format($"{result.Value}");
            }
            else 
                return "NULL";
     

        }
        //停止温度
        public static bool stop_wendu()
        {
            dynamic result = null;
            result = ModbusTcpControl.form_st.client.Write("47", short.Parse("0"?.Trim()), ModbusTcpControl.form_st.station_Number);
            return result.IsSucceed;
        }

        //退出时就把温箱停止了
        private void wenxiang_FormClosing(object sender, FormClosingEventArgs e)
        {
            checkBox1.Checked = false;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

