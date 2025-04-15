using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin.Controls;
using MaterialSkin;
using System.Windows.Forms.DataVisualization.Charting;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.IO.Ports;
using Sunny.UI.Win32;
using NPOI.SS.Formula.Functions;
using static NPOI.HSSF.Util.HSSFColor;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace VISAInstrument
{

    public partial class MCU_Calib : MaterialForm
    {

        //com定义
        private IController controller;
        private int sendBytesCount = 0;
        private int receiveBytesCount = 0;
        //---------------
        public static MaterialSkinManager materialSkinManager;
        public static List<FileInfo> arbs = new List<FileInfo>();
        public static List<int> DataList = new List<int>();
        public static int[] run_data = new int[100];
        //字符串arb
        public static List<string> arbs_set = new List<string>();
        public static int[] data_set = new int[0];
        // 定义两个全局变量
        public bool isMouseDown = false;
        public int lastMove = 0; // 用于记录鼠标上次移动的点，用于判断是左移还是右移
                                 // 初始化ScaleView，可根据首次出现在chart中的数据点数修改合适的值
        public static int chart_length = 0;

        public MCU_Calib()
        {

            InitializeComponent();
            comboBox3.SelectedIndex = 1;
            //设置开始风格
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

        //取串口数据
        public bool GET_STRING(int delay, string[] Command, ref string[] string_get)
        {
            try
            {
                Megahunt_USART.Data_Recved = "";
                Megahunt_USART.Data_Recved_ON = true;
                foreach (string cmd in Command)
                {
                    Megahunt_USART.Send(cmd, true);
                    Delay(delay);
                }
                double value;
                List<double> values = new List<double>();
                if (Megahunt_USART.Data_Recved == "" || !Megahunt_USART.Data_Recved.Contains("\r\n"))
                {
                    Megahunt_USART.Data_Recved_ON = false;
                    return false;
                }
                string[] recvs = Megahunt_USART.Data_Recved.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                Megahunt_USART.Data_Recved_ON = false;
                string_get = recvs;
                return true;
            }
            catch
            {
                Megahunt_USART.Data_Recved_ON = false;
                return false;
            }
        }
        //取串口数据
        public bool GET_STRING(int delay, string Command, ref string[] string_get)
        {
            try
            {
                Megahunt_USART.Data_Recved = "";
                Megahunt_USART.Data_Recved_ON = true;
                Megahunt_USART.Send(Command, true);
                Delay(delay);
                double value;
                List<double> values = new List<double>();
                if (Megahunt_USART.Data_Recved == "" || !Megahunt_USART.Data_Recved.Contains("\r\n"))
                {
                    Megahunt_USART.Data_Recved_ON = false;
                    return false;
                }
                string[] recvs = Megahunt_USART.Data_Recved.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                Megahunt_USART.Data_Recved_ON = false;
                string_get = recvs;
                return true;
            }
            catch
            {
                Megahunt_USART.Data_Recved_ON = false;
                return false;
            }
        }
        //清除
        public void clearText(TextBox box1, int baoliu)
        {
            string[] lines = box1.Lines; //获取所有行
            if (lines.Length > baoliu) //如果超过500行
            {
                int excess = lines.Length - baoliu; //计算多余的行数
                box1.Lines = lines.Skip(excess).ToArray(); //删除多余的行
            }
        }
        //更新LOG 只保留后200行
        public void Play_LOG(string text)
        {
            if (LogText.Lines.Length >= 500)
            {
                clearText(LogText, 200);

            }
            LogText.Text += text + "\r\n";
            LogText.Focus();
            //设置光标的位置到文本尾
            LogText.Select(LogText.TextLength, 0);
            //滚动到控件光标处
            LogText.ScrollToCaret();

        }
        public  struct C27_Calib_struct
        {
            public string BGO;
            public string LPLDO;
            public string TEMP;
            public string HSI14;
            public string HSI;
            public string LSI;
            public string ADC;
            public string ADC_CODE;
            public string[] OPA;
            public string LPLV_M;
            public string LPLDO_M;
            public string HSI_M;
            public string HSI14_M;
            public string LSI_M;
        }
        public C27_Calib_struct C0027_Data;
        public struct D16_Calib_struct
        {
            public string BGO;
            public string LPLDO;
            public string TEMP;
            public string HSI14;
            public string HSI;
            public string LSI;
            public string ADC;
            public string ADC_CODE;
            public string[] OPA;
            public string LPLV_M;
            public string LPLDO_M;
            public string HSI_M;
            public string HSI14_M;
            public string LSI_M;
        }
        public D16_Calib_struct D0016_Data;


        public void struct_reset()
        {
            C0027_Data = new C27_Calib_struct
            {
                BGO = "",
                LPLDO = "",
                TEMP = "",
                HSI14 = "",
                HSI = "",
                LSI = "",
                ADC = "",
                ADC_CODE = "",
                OPA = new string[12],
                LPLV_M = "",
                LPLDO_M = "",
                HSI_M = "",
                HSI14_M = "",
                LSI_M = ""
            };
            for (int i = 0; i < C0027_Data.OPA.Length; i++)
            {
                C0027_Data.OPA[i] = "";
            }
        }
        public string[] GetData;

        public bool Reset_Start()
        {
            LogText.Text = "正在连接MCU...\r\n";
            struct_reset();
            if (GET_STRING(500, "calib signal pa1", ref GetData))
            {
                foreach (string s in GetData)
                {
                    if (s.Contains("calib input signal is pa1"))
                    {
                        Play_LOG("MCU连接成功->校准模式");
                        return true;
                    }
                }
            }
            else
            {
                Play_LOG("未检测到MCU");
                return false;
            }
            return false;

        }
        //重上电,复位MCU
        public void power_reset()
        {
            power.Power_ON_OFF("1", false);
            Delay(500);
            power.Power_ON_OFF("1", true);
            Delay(1500);
            Play_LOG("已复位MCU");
        }
        private void Reset_Click(object sender, EventArgs e)
        {
            Reset_Start();
        }
        //不同时钟源
        public void Sourse(string sourse_HZ)
        {
            if (comboBox3.SelectedIndex == 0)
            {
                Wave.PWM(sourse_HZ, "50", "1");
                Delay(500);
            }
            else
            {
                MessageBox.Show(sourse_HZ + "HZ");
            }
        
        }

        public bool BGP_Calib()
        {
            //输出为100hz
            Sourse("100");
            Play_LOG("BGP校准...");
            string[] Command = new string[] { "calib lpldo", "calib bgp", "calib temperature" };
            if (GET_STRING(200, Command, ref GetData))
            {
                int num = 0;
                foreach (string s in GetData)
                {
                    if (s.Contains("lpldo is cal "))
                    {
                        Play_LOG("LPLDO:\t" + s.Replace("lpldo is cal ", ""));
                        num++;
                    }
                    if (s.Contains("bgp is cal "))
                    {
                        Play_LOG("BGP:\t" + s.Replace("bgp is cal ", ""));
                        num++;
                    }
                    if (s.Contains("temperature is cal "))
                    {
                        Play_LOG("TEMP:\t" + s.Replace("temperature is cal ", ""));
                        num++;
                        break;
                    }
                }
                if (num == 3)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        //时钟校准
        public bool HSI_HSI14_LSI_Calib()
        {
            //输出为5000hz
            Sourse("5000");
            string[] Command = new string[] { "calib hsi", "calib hsi14" };
            int num = 0;
            if (GET_STRING(200, Command, ref GetData))
            {

                foreach (string s in GetData)
                {
                    if (s.Contains("hsi is cal "))
                    {
                        Play_LOG("HSI:\t" + s.Replace("hsi is cal ", ""));
                        num++;
                    }
                    if (s.Contains("hsi14 is cal "))
                    {
                        Play_LOG("HSI14:\t" + s.Replace("hsi14 is cal ", ""));
                        num++;
                    }
                }

            }
            else
            {
                return false;
            }
            //输出为125hz
            Sourse("125");
            if (GET_STRING(200, "calib lsi", ref GetData))
            {

                foreach (string s in GetData)
                {
                    if (s.Contains("lsi is cal "))
                    {
                        Play_LOG("LSI:\t" + s.Replace("lsi is cal ", ""));
                        num++;
                    }
                }
            }
            else
            {
                return false;
            }

            if (num == 3)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //BGP校准按钮
        private void button2_Click(object sender, EventArgs e)
        {
            if (BGP_Calib())
            {
                Play_LOG("BGP/Temp校准成功！");
            }
        }
        //时钟校准按钮
        private void button6_Click(object sender, EventArgs e)
        {
            if (HSI_HSI14_LSI_Calib())
            {
                Play_LOG("HSI/LSI校准成功！");
            }
        }
        
        public bool OPA_Calib()
        {
            Play_LOG("OPA校准...");
            string[] Command = new string[] { "calib opa1", "calib opa1-h", "calib opa2", "calib opa2-h", "calib opa3", "calib opa3-h" };
            if (GET_STRING(1000, Command, ref GetData))
            {
                int num = 0;
                foreach (string s in GetData)
                {
                    if (s.Contains("opa1 nmos cal is "))
                    {
                        Play_LOG("1:\t" + s.Replace("opa1 nmos cal is ", ""));
                        C0027_Data.OPA[num] = s.Replace("opa1 nmos cal is ", "");
                        num++;
                    }
                    if (s.Contains("opa1 pmos cal is "))
                    {
                        Play_LOG("2:\t" + s.Replace("opa1 pmos cal is ", ""));
                        C0027_Data.OPA[num] = s.Replace("opa1 pmos cal is ", "");
                        num++;
                    }
                    if (s.Contains("opa1 h nmos cal is "))
                    {
                        Play_LOG("3:\t" + s.Replace("opa1 h nmos cal is ", ""));
                        C0027_Data.OPA[num] = s.Replace("opa1 h nmos cal is ", "");
                        num++;
                    }
                    if (s.Contains("opa1 h pmos cal is "))
                    {
                        Play_LOG("4:\t" + s.Replace("opa1 h pmos cal is ", ""));
                        C0027_Data.OPA[num] = s.Replace("opa1 h pmos cal is ", "");
                        num++;
                    }
                    //---------------------------------------
                    if (s.Contains("opa2 nmos cal is "))
                    {
                        Play_LOG("5:\t" + s.Replace("opa2 nmos cal is ", ""));
                        C0027_Data.OPA[num] = s.Replace("opa2 nmos cal is ", "");
                        num++;
                    }
                    if (s.Contains("opa2 pmos cal is "))
                    {
                        Play_LOG("6:\t" + s.Replace("opa2 pmos cal is ", ""));
                        C0027_Data.OPA[num] = s.Replace("opa2 pmos cal is ", "");
                        num++;
                    }
                    if (s.Contains("opa2 h nmos cal is "))
                    {
                        Play_LOG("7:\t" + s.Replace("opa2 h nmos cal is ", ""));
                        C0027_Data.OPA[num] = s.Replace("opa2 h nmos cal is ", "");
                        num++;
                    }
                    if (s.Contains("opa2 h pmos cal is "))
                    {
                        Play_LOG("8:\t" + s.Replace("opa2 h pmos cal is ", ""));
                        C0027_Data.OPA[num] = s.Replace("opa2 h pmos cal is ", "");
                        num++;
                    }
                    //--------------------------------------------------
                    if (s.Contains("opa3 nmos cal is "))
                    {
                        Play_LOG("9:\t" + s.Replace("opa3 nmos cal is ", ""));
                        C0027_Data.OPA[num] = s.Replace("opa3 nmos cal is ", "");
                        num++;
                    }
                    if (s.Contains("opa3 pmos cal is "))
                    {
                        Play_LOG("10:\t" + s.Replace("opa3 pmos cal is ", ""));
                        C0027_Data.OPA[num] = s.Replace("opa3 pmos cal is ", "");
                        num++;
                    }
                    if (s.Contains("opa3 h nmos cal is "))
                    {
                        Play_LOG("11:\t" + s.Replace("opa3 h nmos cal is ", ""));
                        C0027_Data.OPA[num] = s.Replace("opa3 h nmos cal is ", "");
                        num++;
                    }
                    if (s.Contains("opa3 h pmos cal is "))
                    {
                        Play_LOG("12:\t" + s.Replace("opa3 h pmos cal is ", ""));
                        C0027_Data.OPA[num] = s.Replace("opa3 h pmos cal is ", "");
                        num++;
                    }
                }
                if (num == 12)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        //OPA校准按钮
        private void button5_Click(object sender, EventArgs e)
        {
            if (OPA_Calib())
            {
                Play_LOG("OPA校准成功！");
            }
        }
        //写入校准
        public bool Write_Calib()
        {
            Play_LOG("写入校准值...");
            string[] Command = new string[] { "calib write" };
            if (GET_STRING(1500, Command, ref GetData))
            {
                foreach (string s in GetData)
                {
                    if (s.Contains("write compatible success"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        //写入校准按钮
        private void button1_Click(object sender, EventArgs e)
        {
            if (Write_Calib())
            {
                Play_LOG("校准值写入成功！");
            }
        }
        //读取校准
        public bool Read_Calib()
        {
            Play_LOG("读取校准值...");
            string[] Command = new string[] { "calib read" };
            string Command_string="";
            int num = 0;
            if (GET_STRING(1500, Command, ref GetData))
            {
                foreach (string s in GetData)
                {
                    Command_string = "hsi cal data is ";
                    if (s.Contains(Command_string))
                    {
                        string VALUE = s.Replace(Command_string, "");
                        Play_LOG("HSI:\t" + VALUE);
                        C0027_Data.HSI = VALUE;
                        num++;
                    }

                    Command_string = "hsi14 cal data is ";
                    if (s.Contains(Command_string))
                    {
                        string VALUE = s.Replace(Command_string, "");
                        Play_LOG("HSI14:\t" + VALUE);
                        C0027_Data.HSI14 = VALUE;
                        num++;
                    }

                    Command_string = "lsi cal data is ";
                    if (s.Contains(Command_string))
                    {
                        string VALUE = s.Replace(Command_string, "");
                        Play_LOG("LSI:\t" + VALUE);
                        C0027_Data.LSI = VALUE;
                        num++;
                    }

                    Command_string = "bgo cal data is ";
                    if (s.Contains(Command_string))
                    {
                        string VALUE = s.Replace(Command_string, "");
                        Play_LOG("BGO:\t" + VALUE);
                        C0027_Data.BGO = VALUE;
                        num++;
                    }

                    Command_string = "lpldo cal data is ";
                    if (s.Contains(Command_string))
                    {
                        string VALUE = s.Replace(Command_string, "");
                        Play_LOG("LPLDO:\t" + VALUE);
                        C0027_Data.LPLDO = VALUE;
                        num++;
                    }

                    Command_string = "temper cal data is ";
                    if (s.Contains(Command_string))
                    {
                        string VALUE = s.Replace(Command_string, "");
                        Play_LOG("Temper:\t" + VALUE);
                        C0027_Data.TEMP = VALUE;
                        num++;
                    }

                    Command_string = "adc analog cal data is ";
                    if (s.Contains(Command_string))
                    {
                        string VALUE = s.Replace(Command_string, "");
                        Play_LOG("ADC:\t" + VALUE);
                        C0027_Data.ADC = VALUE;
                        num++;
                    }

                }
                if (num == 7)
                {
                    return true;
                }
            }
            return false;
        }
        //读校准值
        private void button3_Click(object sender, EventArgs e)
        {
            if (Read_Calib())
            {
                Play_LOG("校准值读取成功！");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                C0027_Data.LPLV_M = textBox4.Text;
                C0027_Data.LPLDO_M = textBox3.Text;
                C0027_Data.HSI_M = textBox6.Text;
                C0027_Data.HSI14_M = textBox5.Text;
                C0027_Data.LSI_M = textBox7.Text;
            }
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\TEST DATA\\Calib");
            Excel.ADD_ONE_EXCEL_Calib_C27("TEST DATA\\Calib\\"+ textBox1.Text+ ".xlsx", C0027_Data, textBox2.Text);
            Play_LOG("写入完成");
            power.Power_ON_OFF("2", false);
            power.Power_ON_OFF("1", false);
        }
        //自动化校准
        private void button7_Click(object sender, EventArgs e)
        {
            power.Power_ON_OFF("2", true);
            power.Power_ON_OFF("1", true);
            power_reset();
            if (!Reset_Start())
            {
                goto end1;
            }
            if (!BGP_Calib())
            {
                goto end1;
            }
            if (!HSI_HSI14_LSI_Calib())
            {
                goto end1;
            }
            if (!ADC_Calib())
            {
                goto end1;
            }
            if (!OPA_Calib())
            {
                goto end1;
            }
            if (!Write_Calib())
            {
                goto end1;
            }
            power_reset();
            //MessageBox.Show("Reset MCU");
            if (!Read_Calib())
            {
                goto end1;
            }
            if (checkBox1.Checked)
            {
                Megahunt_USART.Send("gpio config afpp s3 a 8", true);
                Delay(100);
                //Megahunt_USART.Send("mco hsi", true);
                //MessageBox.Show("hsi 采样中");
                //Megahunt_USART.Send("mco hsi14", true);
                //MessageBox.Show("hsi14 采样中");
                //Megahunt_USART.Send("mco lsi", true);
                //MessageBox.Show("lsi 采样中");
                Megahunt_USART.Send("mco lsi", true);
                Play_LOG("LSI Out");
            }
            Play_LOG("校准完成");
          
        end1: { }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string Path = "TEST DATA\\Calib\\" + textBox1.Text + ".xlsx";
            if (File.Exists(Path))
            {
                Process myProcess = new Process();
                myProcess.StartInfo.FileName =Path;
                myProcess.StartInfo.Verb = "Open";
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.Start();

            }
            else
            {
                Play_LOG("未发现目标文件，为你打开目录！");
                Process.Start("explorer.exe", "TEST DATA\\Calib");
            }
        }
        //校准ADC
        public bool ADC_Calib()
        {
            try
            {
                Play_LOG("读取校准值...");
                string[] Command = new string[] { "calib read" };
                string Command_string = "";
                int num = 0;
            TEMP:
                num++;
                Megahunt_USART.Data_Recved = "";
                Megahunt_USART.Data_Recved_ON = true;
                Megahunt_USART.Send("calib adc", true);
                while (true)
                {
                    Delay(500);
                    if (Megahunt_USART.Data_Recved.Contains("adc analog calib fail"))
                    {
                        Play_LOG("重新校准ADC！");
                        Megahunt_USART.Data_Recved_ON = false;
                        if (num == 6)
                        {
                            Play_LOG("ADC校准出现问题！");
                            return false;
                        }
                        goto TEMP;
                    }
                    if (Megahunt_USART.Data_Recved.Contains("adc analog calib ok"))
                    {
                        Megahunt_USART.Data_Recved_ON = false;
                        string[] recvs = Megahunt_USART.Data_Recved.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        int NUM = 0;
                        foreach (var item in recvs)
                        {
                            if (item.Contains("adc analog calib value is "))
                            {
                                break;
                            }
                            NUM++;
                        }
                        string temp = recvs[NUM-1].Replace("adc calib value is ", "");
                        temp = temp.Replace(" adc value is ", "");
                        string[] values = temp.Split(new string[] { "," }, StringSplitOptions.None);
                        C0027_Data.ADC_CODE = values[1];
                        Play_LOG("ADC:\t"+ values[0]);
                        Play_LOG("Code:\t" + values[1]);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return false;
        }

        //ADC校准
        private void one_test_button_Click(object sender, EventArgs e)
        {
            if (ADC_Calib())
            {
                Play_LOG("ADC校准成功");
            }
        }


    }
}
