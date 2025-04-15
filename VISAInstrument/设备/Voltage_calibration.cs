using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin.Controls;
using MaterialSkin;
using System.Windows.Documents;

namespace VISAInstrument.设备
{
    public partial class Voltage_calibration : MaterialForm
    {
        public Voltage_calibration()
        {
            InitializeComponent();
            materialSkinManager_Config();
        }
        //设置开始风格
        public static MaterialSkinManager materialSkinManager;
        public void materialSkinManager_Config()
        {
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.EnforceBackcolorOnAllComponents = true;
            materialSkinManager.AddFormToManage(this);  
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(
                    Primary.BlueGrey700,
                    Primary.BlueGrey900,
                    Primary.BlueGrey400,
                    Accent.LightBlue200,
                    TextShade.WHITE);
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
        //电压校准
        private void button1_Click(object sender, EventArgs e)
        {

            if (UI_Vol_Cali(textBox2.Text, channel_box.Text, "5", textBox4.Text))
            {
                MessageBox.Show("校准成功！");
            }
            else
            {

                MessageBox.Show("校准失败！");
            }
        }
        public static decimal get_avg_NO_MINMAX(decimal[] decimals)
        {
            decimal max = decimals.Max();
            decimal min = decimals.Min();
            decimal[] newArray = decimals.Where(x => x != max && x != min).ToArray();
            // 计算新数组的平均值
            decimal average = newArray.Average();
            return average;
        }
        //电压校准函数UI
        public bool UI_Vol_Cali(string volt, string chanel, string liangcheng_vol, string jingdu)
        {
            //设置初始电压
            string current = "0.2";
            decimal current_DC_avg = 0;
            decimal OutPut = Convert.ToDecimal(volt);
            decimal[] current_DC = new decimal[10];
            decimal volt_decimal = Convert.ToDecimal(volt);
            decimal jingdu_decimal = Convert.ToDecimal(jingdu);
            if (Math.Abs(OutPut) >= 4)
            {
                power.stop_volt(chanel);
                return false;
            }
            power.set_volt(OutPut.ToString(), current, chanel);
            textBox3.Text  = OutPut.ToString();
            //读取电压
            //设置为电流采集模式
            wanyongbiao.set_volt("DC", liangcheng_vol);
            Task.Delay(100);

            //取10次并计算平均值
            for (int i = 0; i <= 9; i++)
            {
                wanyongbiao.get_screen(ref current_DC[i]);
                Task.Delay(10);
            }
            current_DC_avg = get_avg_NO_MINMAX(current_DC);
            textBox1.Text = current_DC_avg.ToString();
            while (Math.Abs(current_DC_avg - volt_decimal) >= jingdu_decimal)
            {
                OutPut = OutPut + (volt_decimal - current_DC_avg) / 2;
                if (Math.Abs(OutPut) >= 4)
                {
                    power.stop_volt(chanel);
                    return false;
                }
                power.change_volt(OutPut.ToString(), current, chanel);
                textBox3.Text = OutPut.ToString();
                //取10次并计算平均值
                Delay(10);
                for (int i = 0; i <= 9; i++)
                {
                    wanyongbiao.get_screen(ref current_DC[i]);
                    Delay(10);
                }
                current_DC_avg = get_avg_NO_MINMAX(current_DC);
                textBox1.Text = current_DC_avg.ToString();
            }
            return true;
        }
        public  void UI_Vol_Cali_While(string volt, string chanel, string liangcheng_vol, string jingdu, int freq)
        {
            //设置初始电压
            Vol_Cali_While_success = false;
            string current = "0.2";
            decimal current_DC_avg = 0;
            decimal OutPut = Convert.ToDecimal(volt);
            decimal[] current_DC = new decimal[10];
            decimal volt_decimal = Convert.ToDecimal(volt);
            decimal jingdu_decimal = Convert.ToDecimal(jingdu);
            if (Math.Abs(OutPut) >= 4)
            {
                power.stop_volt("1");
                goto End1;
            }
            power.set_volt(OutPut.ToString(), current, chanel);
            textBox3.Text = OutPut.ToString();
            //读取电压
            //设置为电流采集模式
            wanyongbiao.set_volt("DC", liangcheng_vol);
            Task.Delay(100);

            //取10次并计算平均值
            for (int i = 0; i <= 9; i++)
            {
                wanyongbiao.get_screen(ref current_DC[i]);
                Task.Delay(10);
            }
            current_DC_avg = get_avg_NO_MINMAX(current_DC);
            textBox1.Text = current_DC_avg.ToString();
            Vol_Cali_While_start = true;
            while (Vol_Cali_While_start)
            {
                OutPut = OutPut + (volt_decimal - current_DC_avg) / 2;
                if (Math.Abs( OutPut) >= 4)
                {
                    power.stop_volt("1");
                    goto End1;
                }
                power.change_volt(OutPut.ToString(), current, chanel);
                textBox3.Text = OutPut.ToString();
                //取10次并计算平均值
                for (int i = 0; i <= 9; i++)
                {
                    wanyongbiao.get_screen(ref current_DC[i]);
                    Delay(10);
                }
                current_DC_avg = get_avg_NO_MINMAX(current_DC);
                textBox1.Text = current_DC_avg.ToString();

                if (Vol_Cali_While_success)
                {
                    Delay(freq);
                }
                else
                {
                    if (Math.Abs(volt_decimal - current_DC_avg) <= jingdu_decimal)
                    {
                        //是否成功校准
                        Vol_Cali_While_success = true;
                    }
                    Delay(10);
                }
            }
        End1: {
                MessageBox.Show("已停止校准！");
                Vol_Cali_While_start = false;
            }

        }

        //电压序列结构体
        public struct VOL_struct
        {
            public String VOLT_USE;
            public String VOLT_OUTPUT;
        }
        //通用LIST
        public static List<VOL_struct> VOLT_LIST=new List<VOL_struct>();
        //获取电压序列
        public static bool Vol_list(string channel,string[] vols,ref List<VOL_struct> LIST)
        {
            LIST.Clear();
            string out_vol=null;
            VOL_struct voL_Struct = new VOL_struct();
            foreach (string vol in vols)
            {
                if (Vol_Cali(vol, channel ,"5", "0.0001", ref out_vol))
                {
                    voL_Struct.VOLT_USE = vol;
                    voL_Struct.VOLT_OUTPUT = out_vol;
                    LIST.Add(voL_Struct);
                }
                else
                { 
                    return false;
                }
            }
            return true;
        }



        //电压校准函数-一次校准
        public static bool Vol_Cali(string volt,string chanel,string liangcheng_vol, string jingdu,ref string out_vol)
        {
            try
            {
                //设置初始电压
                string current = "0.2";
                decimal current_DC_avg = 0;
                decimal OutPut = Convert.ToDecimal(volt);
                decimal[] current_DC = new decimal[10];
                decimal volt_decimal = Convert.ToDecimal(volt);
                decimal jingdu_decimal = Convert.ToDecimal(jingdu);
                if (Math.Abs(OutPut) >= 4)
                {
                    power.stop_volt("1");
                    return false;
                }
                power.set_volt(OutPut.ToString(), current, chanel);
                //读取电压
                power.Power_ON_OFF("1",true);
                Delay(20);
                wanyongbiao.set_volt("DC", liangcheng_vol);
                Delay(20);
                //取10次并计算平均值
                for (int i = 0; i <= 2; i++)
                {
                    wanyongbiao.get_screen(ref current_DC[i]);
                    Task.Delay(10);
                }
                current_DC_avg = get_avg_NO_MINMAX(current_DC);
                wanyongbiao.get_screen(ref current_DC_avg);
                out_vol = OutPut.ToString();
                while (Math.Abs(current_DC_avg - volt_decimal) >= jingdu_decimal)
                {
                    OutPut = OutPut +(volt_decimal - current_DC_avg);
                    if (Math.Abs(OutPut) >= 4)
                    {
                        power.stop_volt(chanel);
                        return false;
                    }
                    power.change_volt(OutPut.ToString(), current, chanel);
                    out_vol = OutPut.ToString();
                    //取10次并计算平均值
                    for (int i = 0; i <= 2; i++)
                    {
                        wanyongbiao.get_screen(ref current_DC[i]);
                        Delay(10);
                    }
                    current_DC_avg = get_avg_NO_MINMAX(current_DC);
                    //wanyongbiao.get_screen(ref current_DC_avg);
                }
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }
        //电压校准函数-及时校准
        public static bool Vol_Cali_While_start = true;
        public static bool Vol_Cali_While_success = true;
        public static void Vol_Cali_While(string volt, string chanel, string liangcheng_vol, string jingdu,int freq)
        {
            //设置初始电压
            Vol_Cali_While_success = false;
            string current = "0.2";
            decimal current_DC_avg = 0;
            decimal OutPut = Convert.ToDecimal(volt);
            decimal[] current_DC = new decimal[10];
            decimal volt_decimal = Convert.ToDecimal(volt);
            decimal jingdu_decimal = Convert.ToDecimal(jingdu);
            if (Math.Abs(OutPut) >= 4)
            {
                power.stop_volt("1");
                goto End1;
            }
            power.set_volt(OutPut.ToString(), current, chanel);
            //读取电压
            //设置为电流采集模式
            wanyongbiao.set_volt("DC", liangcheng_vol);
            Task.Delay(100);

            //取10次并计算平均值
            for (int i = 0; i <= 9; i++)
            {
                wanyongbiao.get_screen(ref current_DC[i]);
                Task.Delay(10);
            }
            current_DC_avg = get_avg_NO_MINMAX(current_DC);
            Vol_Cali_While_start = true;
            while (Vol_Cali_While_start)
            {
                OutPut = OutPut + (volt_decimal - current_DC_avg) / 2;
                if (Math.Abs(OutPut) >= 4)
                {
                    power.stop_volt("1");
                    goto End1;
                }
                power.change_volt(OutPut.ToString(), current, chanel);
                //取10次并计算平均值
                for (int i = 0; i <= 9; i++)
                {
                    wanyongbiao.get_screen(ref current_DC[i]);
                    Delay(10);
                }
                current_DC_avg = get_avg_NO_MINMAX(current_DC);
                if (Vol_Cali_While_success)
                {
                    Delay(freq);
                }
                else
                {
                    if (Math.Abs(volt_decimal - current_DC_avg) <= jingdu_decimal)
                    {
                        //是否成功校准
                        Vol_Cali_While_success = true;
                    }
                    Delay(10);
                }
         
            }
        End1: {
                Vol_Cali_While_start = false;
            }
            
        }
        //停止即时校准
        public static void Vol_Cali_While_STOP()
        {
            Vol_Cali_While_start=false;
        }
        //持续校准
        private void button2_Click(object sender, EventArgs e)
        {
       
                UI_Vol_Cali_While(textBox2.Text, "1", "5", textBox4.Text,Convert.ToInt32(textBox5.Text,10));
            

        }
        //停止校准
        private void button3_Click(object sender, EventArgs e)
        {
            Vol_Cali_While_start = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //string[] volts;
            string[] volts= {"2.8", "3.1","3.2", "3.3", "3.4"};
            if (Vol_list("1", volts, ref VOLT_LIST))
            {
                MessageBox.Show("OK");
            }
            else
            {
                MessageBox.Show("ERROR");

            }


        }
    }
}
