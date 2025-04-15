using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sunny.UI;
namespace VISAInstrument
{
    public partial class power : UIForm
    {
        public static power form_st = null;
        public power()
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
                catch {
                    return dData;
                }
            }
            return dData;
        }


        //打开电压输出
        private void materialSwitch3_CheckedChanged(object sender, EventArgs e)
        {
            if (comboBox4_flag)
            {
                if (materialSwitch3.Checked == true)
                {
                    
                 //打开电压源
                    string Command = "";
                    //合成命令
                    //Command += "*RST\r\n";
                    if (FrmMain3.Model_name.Contains("E36"))
                    {
                        Command += "APPL Ch" + comboBox4.Text + ", " + textBox8.Text + ", " + textBox4.Text + "\r\n";
                        Command += "OUTP ON,(@" + comboBox4.Text + ")\r\n";
                    }
                    else {
                       // Command += "*RST\r\n";
                        Command += ":SOUR" + comboBox4.Text + ":FUNC:MODE VOLT\r\n";
                        Command += ":SOUR" + comboBox4.Text + ":VOLT " + textBox8.Text + "\r\n";
                        Command += ":SENS" + comboBox4.Text + ":CURR:PROT " + textBox4.Text + "\r\n";
                        Command += ":OUTP" + comboBox4.Text + " ON\r\n";

                    }
                    FrmMain3.form_st.Write(Command, false);

                }
                else
                {
                    //关闭电压源
                    if (FrmMain3.Model_name.Contains("E36"))
                    {
                        FrmMain3.form_st.Write(":OUTP OFF, (@" + comboBox4.Text + ")\r\n", false);
                    }
                    else
                    {
                        FrmMain3.form_st.Write(":OUTP"+ comboBox4.Text + " OFF\r\n", false);
                    }
                }

            }
            else
            {
                comboBox4_flag = true;

            }
        }
        //切换通道
        public bool comboBox4_flag = true;
        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

            //if (FrmMain3.form_st.Write("OUTP? (@" + comboBox4.Text + ")", false))
            //{
            //    string get_string = "";
            //    FrmMain3.form_st.Read(false, 500, ref get_string);
            //    if (get_string.Contains("0"))
            //    {
            //        if (materialSwitch3.Checked == true)
            //        {
            //            comboBox4_flag = false;

            //        }
            //        materialSwitch3.Checked = false;
            //    }
            //    else
            //    {
            //        if (materialSwitch3.Checked == false)
            //        {
            //            comboBox4_flag = false;

            //        }
            //        materialSwitch3.Checked = true;

            //    }
            //}
        }
        //自动刷新
        public bool start_flag = true;
        public bool whitch = true;
        private void materialSwitch1_CheckedChanged(object sender, EventArgs e)
        {
            if (materialSwitch1.Enabled)
            {
                start_flag = true;
                int delay_time = Convert.ToInt32(textBox3.Text, 10);
                string Command1_volt;
                string Command2_current;
                if (FrmMain3.Model_name.Contains("E36"))
                {
                    Command1_volt = "MEAS:VOLT? (@" + comboBox4.Text + ")";
                    Command2_current = "MEAS:CURR ? (@" + comboBox4.Text + ")";
                }
                else
                {
                    Command1_volt = "SOUR:VOLT?";
                    Command2_current = ":SENS:CURR:PROT?";

                }
                    while (start_flag)
                {
                    if (whitch)
                    {
                        if (FrmMain3.form_st.Write(Command1_volt, false))
                        {
                            Decimal temp = 0.0M;
                            string get_string = "";
                            FrmMain3.form_st.Read(true, 500, ref get_string);
                            temp = ChangeDataToD(get_string);
                            textBox1.Text = temp.ToString();
                        }
                    }
                    else
                    {
                        if (FrmMain3.form_st.Write(Command2_current, false))
                        {
                            Decimal temp = 0.0M;
                            string get_string = "";
                            FrmMain3.form_st.Read(true, 500, ref get_string);
                            temp = ChangeDataToD(get_string);
                            textBox2.Text = temp.ToString();
                        }

                    }
                    whitch = !whitch;
                    Delay(delay_time);
                }


            }
            else {



            }
        }

        //设置电压 chanel 1/2/3
        public static bool set_volt(string volt,string current,string chanel)
        {
            try
            {
                //打开电压源
                string Command = "";
                //合成命令
                //Command += "*RST\r\n";
                if (FrmMain3.Model_name.Contains("E36"))
                {
                    Command += "APPL Ch" + chanel + ", " + volt + ", " + current + "\r\n";
                    Command += "OUTP ON,(@" + chanel + ")\r\n";
                }
                else
                {
                    Command += ":SOUR"+chanel+":FUNC:MODE VOLT\r\n";
                    Command += ":SOUR"+chanel+":VOLT " + volt + "\r\n";
                    Command += ":SENS"+chanel+":CURR:PROT " + current + "\r\n";
                    Command += ":OUTP"+chanel+" ON\r\n";

                }
                FrmMain3.form_st.Write(Command, false);
            }
            catch
            {
                return false;
            }
            return true;
        }
        //开启关闭通道
        public static bool Power_ON_OFF(string chanel,bool ON_OFF)
        {
            try
            {
                //打开电压源
                string Command = "";
                //合成命令
                //Command += "*RST\r\n";
                if (FrmMain3.Model_name.Contains("E36"))
                {
                    if (ON_OFF)
                    {
                        Command += "OUTP ON,(@" + chanel + ")\r\n";
                    }
                    else
                    {

                        Command += "OUTP OFF,(@" + chanel + ")\r\n";
                    }
                }
                else
                {
                    if (ON_OFF)
                    {
                        Command += ":OUTP"+chanel+" ON\r\n";
                    }
                    else
                    {
                        Command += ":OUTP"+chanel+" OFF\r\n";
                    }
                }
                FrmMain3.form_st.Write(Command, false);
            }
            catch
            {
                return false;
            }
            return true;
        }


        //改变电压 chanel 1/2/3
        public static bool change_volt(string volt, string current, string chanel)
        {
            try
            {
                //打开电压源
                string Command = "";
                //合成命令
                //Command += "*RST\r\n";
                if (FrmMain3.Model_name.Contains("E36"))
                {
                    Command += "APPL Ch" + chanel + ", " + volt + ", " + current + "\r\n";
                }
                else
                {
                    Command += ":SOUR"+chanel+":VOLT " + volt + "\r\n";

                }
                FrmMain3.form_st.Write(Command, false);
            }
            catch
            {
                return false;
            }
            return true;
        }
        //关闭电源输出 与上面功能重复了
        public static bool stop_volt(string chanel)
        {
            try
            {
                //关闭电压源
                if (FrmMain3.Model_name.Contains("E36"))
                {
                    FrmMain3.form_st.Write("OUTP OFF, (@" + chanel + ")\r\n", false);
                }
                else
                {
                    FrmMain3.form_st.Write(":OUTP"+chanel+" OFF\r\n", false);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
        //重上电,复位MCU
        public static bool power_reset(string channel)
        {
            power.Power_ON_OFF(channel, false);
            Delay(1000);
            power.Power_ON_OFF(channel, true);
            return true;
        }
        private void power_Load(object sender, EventArgs e)
        {

        }
    }
}

