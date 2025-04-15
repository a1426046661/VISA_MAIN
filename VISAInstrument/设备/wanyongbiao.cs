using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NPOI.SS.Formula.Functions;
using Sunny.UI;
namespace VISAInstrument
{
    public partial class wanyongbiao : UIForm
    {
        public static wanyongbiao form_st = null;
        public wanyongbiao()
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

        public static Decimal ChangeDataToD(string strData)
        {
            Decimal dData = 0.0M;
            if (strData.Contains("E"))
            {
                try
                {
                    dData = Convert.ToDecimal(Decimal.Parse(strData.ToString(), System.Globalization.NumberStyles.Float));
                }
                catch {
                    return dData;
                }
            }
            return dData;
        }
        //开启电流采样
        public static bool start_flag=false;
        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text.Contains("开启电流采样"))
            {
                button1.Text = "关闭电流采样";
                button2.Enabled = false;

                string Command = "";
                //计算单位
                
                textBox2.Text = comboBox2.Text;
                int beilv = 1;
                if (textBox2.Text == "mA")
                {
                    beilv = 1000;
                }
                else if (textBox2.Text == "uA")
                {
                    beilv = 1000000;
                }
                //合成命令
                Command += "*RST\r\n";
                Command += @"FUNC ""CURRENT:" + comboBox1.Text + @"""" + "\r\n";
                Command += @"CURRENT:RANG " + textBox7.Text + "\r\n";
                FrmMain2.form_st.Write(Command, false);
                start_flag = true;
                int delay_time = Convert.ToInt32(textBox3.Text, 10);
                while (start_flag)
                {
                    if (FrmMain2.form_st.Write(":READ?", false))
                    {
                        Decimal temp = 0.0M;
                        string get_string = "";
                        FrmMain2.form_st.Read(true, 500, ref get_string);
                        temp = ChangeDataToD(get_string)*beilv;
                        textBox1.Text = temp.ToString();
                    }
                    Delay(delay_time);
                }
            }
            else {

                start_flag = false;
                button2.Enabled = true;
                button1.Text = "开启电流采样";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text.Contains("开启电压采样"))
            {
                button2.Text = "关闭电压采样";

                button1.Enabled = true;
                string Command = "";
                textBox2.Text = comboBox3.Text;
                int beilv = 1;
                if (textBox3.Text == "mV")
                {
                    beilv = 1000;
                }
                else if (textBox3.Text == "uV")
                {
                    beilv = 1000000;
                }

                Command += "*RST\r\n";
                Command += @"FUNC ""VOLT:" + comboBox4.Text + @"""" + "\r\n";
                Command += @"VOLT:RANG " + textBox8.Text + "\r\n";
                FrmMain2.form_st.Write(Command, false);
                start_flag = true;
                int delay_time = Convert.ToInt32(textBox4.Text, 10);
                while (start_flag)
                {
                    if (FrmMain2.form_st.Write(":READ?", false))
                    {
                        Decimal temp = 0.0M;
                        string get_string = "";
                        FrmMain2.form_st.Read(true, 500, ref get_string);
                        temp = ChangeDataToD(get_string)*beilv;
                        textBox1.Text = temp.ToString();
                    }
                    Delay(delay_time);
                }
            }
            else
            {
                start_flag = false;
                button1.Enabled = true;
                button2.Text = "开启电压采样";
            }
        }


        //设置为电压采集模式
        public static bool set_volt(string DC_AC, string liangcheng)
        {
            try
            {
                string Command = "";
                //合成命令
                Command += "*RST\r\n";
                Command += @"FUNC ""VOLT:" + DC_AC + @"""" + "\r\n";
                Command += @"VOLT:RANG " + liangcheng + "\r\n";
                FrmMain2.form_st.Write(Command, false);
            }
            catch
            {
                return false;
            }
            return true;
        }

        //设置为电流采集模式
        public static bool set_current(string DC_AC,string liangcheng)
        {
            try
            {
                string Command = "";
                //合成命令
                Command += "*RST\r\n";
                Command += @"FUNC ""CURRENT:" + DC_AC + @"""" + "\r\n";
                Command += @"CURRENT:RANG " + liangcheng + "\r\n";
                FrmMain2.form_st.Write(Command, false);
            }
            catch
            { 
                return false;
            }
            return true;
        }
        //获取当前屏幕显示数值
        public static bool get_screen(ref decimal current)
        {
            try
            {
                if (FrmMain2.form_st.Write(":READ?", false))
                {
                    Decimal temp = 0.0M;
                    string get_string = "";
                    FrmMain2.form_st.Read(true, 500, ref get_string);
                    temp = ChangeDataToD(get_string);
                    current = temp;
                    return true;
                }
                else
                {
                    return false;

                }
            }
            catch
            {

                return false;
            }
        }

        //获取当前屏幕平均值
        public static decimal get_screen_data(int dely, int avg_num)
        {
            List<decimal> list = new List<decimal>();
            decimal current_now=0;
            list.Clear();
            for (int i = 0; i < avg_num; i++)
            {
                if (wanyongbiao.get_screen(ref current_now))
                {
                    if (current_now >= 0)
                    {
                        list.Add(current_now);
                    }
                }
            }
            return list.Average();
        }
           
        


    }
}

