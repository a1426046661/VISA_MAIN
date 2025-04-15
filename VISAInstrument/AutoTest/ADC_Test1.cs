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
using static MaterialSkin.MaterialSkinManager;
using VISAInstrument.设备;
using static Org.BouncyCastle.Math.EC.ECCurve;
using NPOI.SS.Formula.Functions;
using static VISAInstrument.设备.Voltage_calibration;
using System.Windows.Documents;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Windows.Media.TextFormatting;
using static NPOI.HSSF.Util.HSSFColor;
using System.Security.Cryptography.X509Certificates;
using VISAInstrument.Utility.Extension.UI;
using static NPOI.POIFS.Crypt.CryptoFunctions;
using Sunny.UI.Win32;
using System.Runtime.Remoting.Contexts;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.Remoting.Channels;

namespace VISAInstrument.AutoTest
{
    public partial class ADC_Test1 : MaterialForm
    {
        public static MaterialSkinManager materialSkinManager;
        public static bool wenxiang_link = false;
        public static bool wenyayuan_link = false;
        public ADC_Test1(bool wenxiang, bool wenyayuan)
        {

            InitializeComponent();
            //设置开始风格
            materialSkinManager_Config();
            //温箱和稳压源的连接状态

            wenxiang_link = wenxiang;
            wenyayuan_link = wenyayuan;
            //wenxiang_link = true;
            //wenyayuan_link = true;
            wenxiang_wenyayuan_LINKCHECK();
            Config_to_select();
            ADC_MODEL_LIST();

        }
        ControlChange cc = new ControlChange();
        //自适应页面
        private void ADC_Test1_Load(object sender, EventArgs e)
        { //窗体自适应
            cc.x = this.Width;
            cc.y = this.Height;
            cc.setTag(this);
            //让控件在父控件中居中
            // cc.CenterCtr(panel1, true, true);
        }

        private void ADC_Test1_Resize(object sender, EventArgs e)
        {
            //控件随着窗体改变大小
            float newx = this.Width / cc.x;
            float newy = this.Height / cc.y;
            cc.setControls(newx, newy, this);
            //让控件在父控件中居中
            // cc.CenterCtr(panel1, true, true);
        }

        //温箱和稳压源的连接状态
        public void wenxiang_wenyayuan_LINKCHECK()
        {
            wendu_check.Checked = wenxiang_link;
            wendu_check.Enabled = wenxiang_link;
            wendu_text.Enabled = wenxiang_link;
            vol_check.Enabled = wenyayuan_link;
            vol_check.Checked = wenyayuan_link;
            dianya_text.Enabled = wenyayuan_link;
        }
        //设置开始风格
        public void materialSkinManager_Config()
        {
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.EnforceBackcolorOnAllComponents = true;
            ////materialSkinManager.e= fontType.H1;
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
        //计算平均值
        public static decimal get_avg_NO_MINMAX(decimal[] decimals)
        {
            decimal max = decimals.Max();
            decimal min = decimals.Min();
            decimal[] newArray = decimals.Where(x => x != max && x != min).ToArray();
            // 计算新数组的平均值
            decimal average = newArray.Average();
            return average;
        }
        //计算平均值
        public static double get_avg_NO_MINMAX(double[] decimals)
        {
            double max = decimals.Max();
            double min = decimals.Min();
            double[] newArray = decimals.Where(x => x != max && x != min).ToArray();
            // 计算新数组的平均值
            double average = newArray.Average();
            return average;
        }
        //测试向量存储
        public struct TestItem
        {
            public string volt;
            public string wendu;
            public string ADC;
            public string channel;
            public string pin;
            public string div;
            public string samp;
            public double DAC_MIN;
            public double DAC_MAX;
            public double DAC_Step;
        }
        public static List<TestItem> TestItems = new List<TestItem>();
        public static TestItem this_Item;
        //一次测试
        public static TestItem Auto_One;
        //----------------------------------------



        //自由串口输出
        private void checkBox12_CheckedChanged(object sender, EventArgs e)

        {
            button2.Enabled = !checkBox12.Checked;
            button5.Enabled = !checkBox12.Checked;
            button6.Enabled = !checkBox12.Checked;
            groupBox1.Enabled = !checkBox12.Checked;
            groupBox6.Enabled = !checkBox12.Checked;
            comboBox1.Enabled = !checkBox12.Checked;
            textBox2.Enabled = !checkBox12.Checked;
        }
        public void wendu_dianya_LIST()
        {
            string volt;
            string wendu;
            string[] volts = dianya_text.Text.Split(new[] { ',', '，', ';' }, StringSplitOptions.RemoveEmptyEntries);
            string[] wendus = wendu_text.Text.Split(new[] { ',', '，', ';' }, StringSplitOptions.RemoveEmptyEntries);
            //查看开启还是关闭了ADC
            LogText.Text = "";
            if (volts.Length == 0 || vol_check.Checked == false)
            {
                volts = new string[1] { "NULL" };
                vol_check.Checked = false;
            }
            if (wendus.Length == 0 || wendu_check.Checked == false)
            {
                wendus = new string[1] { "NULL" };
                wendu_check.Checked = false;
            }

            foreach (string vol_for in volts)
            {
                foreach (string wendu_for in wendus)
                {
                    TestItems.Clear();
                    this_Item.volt = vol_for;
                    this_Item.wendu = wendu_for;
                    TestItems.Add(this_Item);
                    goto end3;
                }
            }
        end3: { }
        }

        //测试结束
        public void testfinish()
        {
            if (wenyayuan_link)
            {
                power.Power_ON_OFF("1", false);
                power.Power_ON_OFF("2", false);
            }
            if (wenxiang_link)
            {
                wenxiang.stop_wendu();
            }
        }

        public void UI_LIST(bool clear, bool screen)
        {
            //清除已经存储的序列
            if (clear)
            {
                TestItems.Clear();
            }
            //ADC设置序列是否为空
            if (listView2_Checked() == -1)
            {
                listView2.Items[0].Checked = true;
            }
            //电压设置超标
            if (Convert.ToDouble(adc_min.Text) >= 4 || Convert.ToDouble(adc_max.Text) >= 4)
            {
                MessageBox.Show("电压设置有误！");
                goto end2;
            }

            string volt;
            string wendu;
            string[] volts = dianya_text.Text.Split(new[] { ',', '，', ';' }, StringSplitOptions.RemoveEmptyEntries);
            string[] wendus = wendu_text.Text.Split(new[] { ',', '，', ';' }, StringSplitOptions.RemoveEmptyEntries);
            string adc = comboBox2.Text;
            string channel = listView2.Items[listView2_Checked()].SubItems[0].Text;
            string pin = listView2.Items[listView2_Checked()].SubItems[1].Text;
            //查看开启还是关闭了DAC
            LogText.Text = "";
            if (volts.Length == 0 || vol_check.Checked == false)
            {
                volts = new string[1] { "NULL" };
                vol_check.Checked = false;
            }
            if (wendus.Length == 0 || wendu_check.Checked == false)
            {
                wendus = new string[1] { "NULL" };
                wendu_check.Checked = false;
            }

        //电压遍历结构
        MUTI_dianya:
            //若未勾选
            foreach (string wendu_for in wendus)
            {
                foreach (string vol_for in volts)
                {
                    this_Item.volt = vol_for;
                    this_Item.wendu = wendu_for;
                    this_Item.ADC = adc;
                    this_Item.channel = channel;
                    this_Item.pin = pin;
                    this_Item.div = comboBox1.Text;
                    this_Item.samp = textBox2.Text;
                    this_Item.DAC_MIN = Convert.ToDouble(adc_min.Text);
                    this_Item.DAC_MAX = Convert.ToDouble(adc_max.Text);
                    this_Item.DAC_Step = Convert.ToDouble(adc_step.Text);
                    TestItems.Add(this_Item);
                }

            }
        //显示结构
        end1:
            if (screen)
            {
                int num = 0;
                listView1.Items.Clear();
                foreach (TestItem Item in TestItems)
                {
                    listView1.Items.Add(new ListViewItem(new String[] {num.ToString("d2"), Item .wendu,Item.ADC,Item.channel,
                    Item.pin, Item.volt,Item.samp,Item.div,Item.DAC_MIN.ToString(),Item.DAC_MAX.ToString(),Item.DAC_Step.ToString()
                    }));
                    listView1.Items[num].Checked = true;
                    num++;
                }
                panel1.VerticalScroll.Value = panel1.VerticalScroll.Maximum;
                //是否存在测试向量
                listView1_ItemChecked(null, null);
            }
        end2: { }

        }

        //查看测试向量
        private void button2_Click(object sender, EventArgs e)
        {
            UI_LIST(true, true);
        }
        //追加自动化向量
        private void button5_Click(object sender, EventArgs e)
        {
            UI_LIST(false, true);
        }
        //全选
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            int Count_all = listView1.Items.Count;
            for (int i = 0; i < Count_all; i++)
            {
                listView1.Items[i].Checked = checkBox1.Checked;
            }
        }
        //某选项更改
        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            int Count_all = listView1.Items.Count;
            int num = 0;
            for (int i = 0; i < Count_all; i++)
            {
                if (listView1.Items[i].Checked)
                {
                    num++;
                }
            }
            if (num == 0)
            {
                auto_test_button.Enabled = false;
            }
            else
            {
                auto_test_button.Enabled = true;
            }

        }

        //是否使用温箱
        private void wendu_check_CheckedChanged(object sender, EventArgs e)
        {
            wendu_now.Enabled = wendu_check.Checked;
            if (!wendu_now.Enabled)
            {
                wendu_now.Text = "NULL";
            }
            else
            {
                wendu_now.Text = "";
            }
        }
        //是否使用稳压源
        private void vol_check_CheckedChanged(object sender, EventArgs e)
        {
            dianya_now.Enabled = vol_check.Checked;
            if (!dianya_now.Enabled)
            {
                dianya_now.Text = "NULL";
            }
            else
            {
                dianya_now.Text = "";
            }
        }
        //重上电,复位MCU
        public void power_reset()
        {
            power.Power_ON_OFF("1", false);
            Delay(500);
            power.Power_ON_OFF("1", true);
            Delay(500);
            Play_LOG("已复位MCU");
        }

        //自动化测试按键控制
        public void AUTO_Test_Button_contrl(bool testing)
        {
            button4.Enabled = testing;
            button1.Enabled = testing;
            button3.Enabled = testing;
            auto_test_button.Enabled = !testing;
            Auto_One.ADC = "NULL";
        }
        //单条测试按键控制
        public void ONCE_Test_Button_contrl(bool testing)
        {
            button4.Enabled = testing;
            button1.Enabled = testing;
            button3.Enabled = false;
            auto_test_button.Enabled = false;
            Auto_One.ADC = "NULL";
        }

        //判断是不是字符
        public bool isPureNum(string str)
        {
            bool isNumeric = int.TryParse(str, out _);
            return isNumeric;
        }
        //取串口数据
        //"AdcSingle:Adc1Channel00"
        public double GET_ADC(string Command, int delay, bool avg_ON)
        {
            try
            {
                Megahunt_USART.Data_Recved = "";
                Megahunt_USART.Data_Recved_ON = true;
                Megahunt_USART.Send(Command, Enabled);
                Delay(delay);
                double value;
                List<double> values = new List<double>();
                if (Megahunt_USART.Data_Recved == "" || !Megahunt_USART.Data_Recved.Contains("\r\n"))
                {
                    return -1;
                }
                string[] recvs = Megahunt_USART.Data_Recved.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                foreach (string recv in recvs)
                {
                    if (isPureNum(recv))
                    {
                        if (avg_ON)
                        {
                            values.Add(Convert.ToDouble(recv));
                        }
                        else
                        {
                            return Convert.ToDouble(recv);
                        }
                    }
                }
                value = values.ToArray().Average();
                return value;
            }
            catch {

                return -1;
            }
        }
        //取串口数据
        //"AdcSingle:Adc1Channel00"
        public double GET_ADC(int delay, bool avg_ON)
        {
            try
            {
                Megahunt_USART.Data_Recved = "";
                Megahunt_USART.Data_Recved_ON = true;
                int temp_num = 0;
                Get_send(Auto_One.ADC.Replace("ADC", ""), Auto_One.channel.Replace("通道", ""), Auto_One.div, Auto_One.samp, delay,false);
            TEMP:
                temp_num++;
                double value;
                List<double> values = new List<double>();
                if (Megahunt_USART.Data_Recved == "" || !Megahunt_USART.Data_Recved.Contains("\r\n"))
                {
                    Delay(100);
                    goto TEMP;
                }
                string[] recvs = Megahunt_USART.Data_Recved.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                foreach (string recv in recvs)
                {
                    if (isPureNum(recv))
                    {
                        values.Add(Convert.ToDouble(recv));
                    }
                }
                if (values.Count == 10)
                {
                    if (avg_ON)
                    {
                        value = values.ToArray().Average();
                        return value;
                    }
                    else
                    {
                        return values[0];
                    }

                }
                else
                {
                    if (temp_num == 20)
                    {
                        return -1;
                    }
                    Delay(100);
                    goto TEMP;
                }

            }
            catch
            {
                return -1;
            }
        }

        public void clearText(TextBox box1, int baoliu)
        {
            string[] lines = box1.Lines; //获取所有行
            if (lines.Length > baoliu) //如果超过500行
            {
                int excess = lines.Length - baoliu; //计算多余的行数
                box1.Lines = lines.Skip(excess).ToArray(); //删除多余的行
            }
        }


    //开始自动化测试
    public static List<Voltage_calibration.VOL_struct> VOL_LIST = new List<Voltage_calibration.VOL_struct>();
        private void auto_test_button_Click(object sender, EventArgs e)
        {
            //暂停
            zanting = false;
            //结束所有
            all_testing = true;
            LogText.Text = "";
            AUTO_Test_Button_contrl(true);
            file_name = "TEST DATA\\ADC_TEST_DATA\\ADC_" + DateTime.Now.ToString("yy_MMdd_hhmm_ss") + ".xlsx";
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\TEST DATA\\ADC_TEST_DATA");

            int Count_all = listView1.Items.Count;

            if (vol_check.Checked == true)
            {
                MessageBox.Show("请完成连接!\r\n请将万用表与电源输出通道1连接\r\n连接完成后，点击按钮继续");
                string[] dianyas = dianya_text.Text.Split(new[] { ',', '，', ';' }, StringSplitOptions.RemoveEmptyEntries);
                Play_LOG("正在执行电压校准");
                if (Voltage_calibration.Vol_list("1",dianyas, ref VOL_LIST))
                {
                    foreach (Voltage_calibration.VOL_struct vol in VOL_LIST)
                    {
                        Play_LOG(vol.VOLT_USE.ToString() + "<-->" + vol.VOLT_OUTPUT.ToString());
                    }
                    MessageBox.Show("电压校准成功!");
                }
                else
                {
                    MessageBox.Show("电压校准失败！");
                }
            }
            for (int i = 0; i < Count_all; i++)
            {
                if (listView1.Items[i].Checked)
                {
                    //测试控制/结束全部
                    if (TestItems[i].ADC != Auto_One.ADC)
                    {
                      
                         MessageBox.Show("连接ADC通道!\r\n请将电源Channel2与ADC输入"+ TestItems[i].pin+ "连接\r\n连接完成后，点击按钮继续");
                        
                  
                    }
                    Auto_One = TestItems[i];

                    if (!all_testing)
                    {
                        Play_LOG("已手动退出测试！");
                        goto end2;
                    }
                    Play_LOG("向量：" + (i + 1).ToString() + " 开始测试");
                    Play_LOG(Auto_One.ADC+"\t"+Auto_One.channel);
                    Play_LOG("温度："+Auto_One.wendu + "\t电压：" + Auto_One.volt);
                    Play_LOG("MIN:"+Auto_One.DAC_MIN);
                    Play_LOG("MAX:" + Auto_One.DAC_MAX);
                    Play_LOG("STEP:" + Auto_One.DAC_Step);
                    one_test();
                    //若跳出了一条
                    if (this_testing)
                    {
                        listView1.Items[i].Checked = false;
                    }
                    else
                    {
                        listView1.Items[i].Checked = true;
                    }
                }

            }
        end1:
            AUTO_Test_Button_contrl(false);
            OPEN_FILE(file_name);
        end2:
            Play_LOG("测试结束！");
        }
        //开始单条测试
        private void one_test_button_Click(object sender, EventArgs e)
        {

            //未使用自由串口
            if (!checkBox12.Checked)
            {
                UI_LIST(true, false);
                if (TestItems.Count != 1)
                {
                    MessageBox.Show("测试项数量 > 1,请重新编辑测试配置");
                    goto end1;
                }
                LogText.Text = "";
                zanting = false;
                all_testing = true;
                ONCE_Test_Button_contrl(true);
                file_name = "TEST DATA\\DAC_TEST_DATA\\DACONCE_" + DateTime.Now.ToString("yy_MM_hh_mm_ss") + ".xlsx";
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\TEST DATA\\ADC_TEST_DATA");
                if (vol_check.Checked == true)
                {
                    MessageBox.Show("请完成连接!\r\n请将万用表与电源输出通道1连接\r\n连接完成后，点击按钮继续");
                    string[] dianyas = dianya_text.Text.Split(new[] { ',', '，', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    Play_LOG("正在执行电压校准");
                    if (Voltage_calibration.Vol_list("1",dianyas, ref VOL_LIST))
                    {
                        foreach (Voltage_calibration.VOL_struct vol in VOL_LIST)
                        {
                            Play_LOG(vol.VOLT_USE.ToString() + "<-->" + vol.VOLT_OUTPUT.ToString());
                        }
                        MessageBox.Show("电压校准成功!");
                    }
                    else
                    {
                        MessageBox.Show("电压校准失败!");
                    }
                }
                //测试控制/结束全部
                MessageBox.Show("连接ADC通道!\r\n请将电源Channel2与ADC输入" + TestItems[0].pin + "连接\r\n连接完成后，点击按钮继续");
                Auto_One = TestItems[0];
                Play_LOG("向量开始测试");
                Play_LOG(Auto_One.ADC + "\t" + Auto_One.channel);
                Play_LOG("温度：" + Auto_One.wendu + "\t电压：" + Auto_One.volt);
                Play_LOG("MIN:" + Auto_One.DAC_MIN);
                Play_LOG("MAX:" + Auto_One.DAC_MAX);
                Play_LOG("STEP:" + Auto_One.DAC_Step);
                one_test();
                //测试主函数
                one_test();
                ONCE_Test_Button_contrl(false);
                OPEN_FILE(file_name);
            end2:
                Play_LOG("测试结束！");
            }
            //自由串口
            else if (checkBox12.Checked)
            {
                if (text8.Text == "")
                {
                    MessageBox.Show("GetADC不能为空");
                    goto end1;
                }
                wendu_dianya_LIST();
                zanting = false;
                all_testing = true;
                ONCE_Test_Button_contrl(true);
                Auto_One = TestItems[0];
                if (vol_check.Checked == true)
                {
                    MessageBox.Show("请完成连接!\r\n请将万用表与电源输出连接\r\n连接完成后，点击按钮继续");
                    string[] dianyas = dianya_text.Text.Split(new[] { ',', '，', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    Play_LOG("正在执行电压校准");
                    if (Voltage_calibration.Vol_list("1",dianyas, ref VOL_LIST))
                    {
                        foreach (Voltage_calibration.VOL_struct vol in VOL_LIST)
                        {
                            Play_LOG(vol.VOLT_USE.ToString() + "<-->" + vol.VOLT_OUTPUT.ToString());
                        }
                        MessageBox.Show("电压校准成功!");
                    }
                    else
                    {
                        MessageBox.Show("电压校准失败！");
                    }
                }
                Play_LOG("向量开始测试");
                one_test();
            }
        end1:
            {

            }
        }
        //单条测试函数
        public struct TestItem2
        {
            public string volt;
            public string wendu;
            public string DAC;
            public bool buff;
            public bool MIX;
            public int DAC_MIN;
            public int DAC_MAX;
            public int DAC_Step;
        }
        //自动化测试的一条
        public float[] vol_get = new float[32];
        public float[] dac_set = new float[32];
        public float[] dac_get = new float[32];
        public float[] dac_shoud = new float[32];
        public List<float> dac_set_list = new List<float>();
        public string file_name = "TEST1";
        string last_wendu = "NULL";
        public void one_test()
        {
            //线性度测试 or 单点测试
            int Test_number = 0;
            if (Auto_One.DAC_MIN != Auto_One.DAC_MAX)
            {
                Test_number = 1;
            }
            else
            {
                Test_number = 2;
            }
            if (checkBox12.Checked == true)
            {
                Test_number = 3;
            }
            this_testing = true;
            //温度
            if (Auto_One.wendu != "NULL")
            {
                int count = 0;
                string get_wendu_str;
                string set_wendu_str = ((int)(Convert.ToDouble(Auto_One.wendu) * 10)).ToString();
                Play_LOG("Set:" + Auto_One.wendu);
                Play_LOG("正在调整温度！");
                if (wenxiang.set_wendu(set_wendu_str) == true)
                {
                    while (true)
                    {
                        Delay(500);
                        get_wendu_str = wenxiang.get_wendu();
                        if (get_wendu_str != "NULL")
                        {
                            wendu_now.Text = (Convert.ToDouble(get_wendu_str) / 10).ToString();
                            if (Math.Abs(Convert.ToInt32(get_wendu_str, 10) - Convert.ToInt32(set_wendu_str, 10)) <= 10)
                            {
                                Play_LOG("已到达预期温度");
                                break;
                            }
                        }
                    }
                }
                if (last_wendu != set_wendu_str)
                {

                    Play_LOG("等待5 min 使温度稳定");
                    while (count <= 300)
                    {
                        count++;
                        get_wendu_str = wenxiang.get_wendu();
                        Delay(1000);

                    }
                    last_wendu = set_wendu_str;
                    Play_LOG("开始测试");
                }
            }
            //电压
            if (Auto_One.volt != "NULL")
            {
                //复位操作
                power_reset();
                dianya_now.Text = Auto_One.volt;
                foreach (Voltage_calibration.VOL_struct vol in VOL_LIST)
                {
                    if (Convert.ToDecimal(vol.VOLT_USE) == Convert.ToDecimal(Auto_One.volt))
                    {
                        power.set_volt(vol.VOLT_OUTPUT, "200", "1");
                        break;
                    }
                }
            }

            //生成测试列表
            dac_set_list.Clear();
            if (Test_number == 1)//线性度测试
            {
                double step = (Auto_One.volt == "NULL" ? 3.3 / 4095 : Convert.ToDouble(Auto_One.volt) / 4095) * Convert.ToDouble(Auto_One.DAC_Step);
                for (double i = Auto_One.DAC_MIN; i <= Auto_One.DAC_MAX; i = i + step)
                {
                    dac_set_list.Add((float)i);
                }
            }
            else//单点测试
            {

                double step = Convert.ToDouble(Auto_One.DAC_Step);
                for (double i = 0; i <= step; i++)
                {
                    dac_set_list.Add((float)Auto_One.DAC_MIN);
                }
            }

            dac_set = dac_set_list.ToArray();
            vol_get = new float[dac_set_list.Count];
            dac_get = new float[dac_set_list.Count];
            dac_shoud = new float[dac_set_list.Count];

            //若没有配置
            if (!Config_file_USE || Test_number == 3)
            {
                List<string> DAC_config = new List<string>();
                DAC_config.Clear();
                if (Test_number == 3)
                {
                    DAC_config = freedom_usart_getstring().ToList();
                    Play_LOG("正在配置MCU参数（根据自由串口配置)");
                }
                else
                {
                    DAC_config.Add("AdcSingle:Adc1Channel00");
                    Play_LOG("正在配置MCU参数（根据UI配置)");
                }
                foreach (string configs in DAC_config)
                {
                    Play_LOG(configs);
                    Megahunt_USART.Send(configs, Enabled);
                    Delay(100);
                }
            }
            else
            {
                //从配置文件中取数
                Config_send(Auto_One.ADC.Replace("ADC",""),Auto_One.channel.Replace("通道",""),Auto_One.div,Auto_One.samp,500,true);
            }
            //进度条控制
            progressBar1.Minimum = 0;
            progressBar1.Maximum = dac_set.Length - 1;
            //一个DAC测试
            int index = 0;
            string ref_volt = "";
            foreach (float ADC_IN in dac_set)
            {
                progressBar1.Value = index;
                progressBar1.Refresh();
                Double[] current_DC = new Double[10];
                adc_in.Text = ADC_IN.ToString();
            //校准电压并输入
            Voltage_cali:
                if (!Voltage_calibration.Vol_Cali(ADC_IN.ToString(), "2", "4", "0.0001", ref ref_volt))
                {
                    Play_LOG("重新校准！");
                    goto Voltage_cali;
                }
                //暂停
                while (zanting)
                {
                    Delay(100);
                }
                //测试控制
                if (!this_testing || !all_testing)
                {
                    if (!this_testing)
                    {
                        Play_LOG("跳出本条测试！");
                    }
                    break;
                }
            getonce:
                try
                {
                    if (!Config_file_USE || Test_number == 3)
                    {
                        if (Test_number == 3)
                        {
                            dac_get[index] = (float)GET_ADC(text8.Text, 100, false);
                        }
                        else
                        {
                            dac_get[index] = (float)GET_ADC("AdcSingle:Adc1Channel00", 100, false);
                        }
                    }
                    else
                    {
                        dac_get[index] = (float)GET_ADC(100, false);   
                    }

                    if (dac_get[index] == -1)
                    {
                        Play_LOG("重新获取Code");
                        goto getonce;
                    }
                }
                catch (Exception e)
                {
                    Play_LOG("测试异常中断！");
                    goto end1;
                }
                vol_get[index] = (float)(dac_get[index] * (Auto_One.volt != "NULL" ? Convert.ToDouble(Auto_One.volt) : 3.3) / 4095);
                dac_shoud[index] = (float)((ADC_IN * 4096) / (Auto_One.volt != "NULL" ? Convert.ToDouble(Auto_One.volt) : 3.3));
                textBox1.Text = dac_get[index].ToString();
                index++;
            }
            if (Test_number == 1)
            {
                Excel.ADD_ONE_EXCEL_ADC(file_name, Auto_One.ADC + "_" + Auto_One.wendu + "°C" + "(线性度测试)",
               (Auto_One.channel) + "_VOLT:" + Auto_One.volt + "V" + "_采样率:" + Auto_One.samp + "_分频:" + Auto_One.div,
                dac_set, vol_get, dac_get);
            }
            else if (Test_number == 2)
            {
                Excel.ADD_ONE_EXCEL_ADC(file_name, Auto_One.ADC + "_" + Auto_One.wendu + "°C" + "(单点测试)",
                (Auto_One.channel) + "_VOLT:" + Auto_One.volt + "V" + "_采样率:" + Auto_One.samp + "_分频:" + Auto_One.div,
                dac_set, vol_get, dac_get);
            }
            else
            {
                int nums = 0;
                foreach (float dac_get_1 in dac_get)
                {
                    Play_LOG(dac_set[nums].ToString()+"V:" +dac_get_1.ToString()+"\r\n");
                    nums++;
                }
            }
        end1: { }
        }

        public void one_test_freedom_USART()
        {
            this_testing = true;
            //温度
            if (Auto_One.wendu != "NULL")
            {
                string get_wendu_str;
                string set_wendu_str = ((int)(Convert.ToDouble(Auto_One.wendu) * 10)).ToString();
                if (wenxiang.set_wendu(set_wendu_str) == true)
                {
                    while (true)
                    {
                        Delay(500);
                        get_wendu_str = wenxiang.get_wendu();
                        if (get_wendu_str != "NULL")
                        {
                            wendu.Text = get_wendu_str;
                            if (Math.Abs(Convert.ToInt32(get_wendu_str, 10) - Convert.ToInt32(set_wendu_str, 10)) <= 10)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            //电压
            if (Auto_One.volt != "NULL")
            {
                //复位操作
                power_reset();
                dianya_now.Text = Auto_One.volt;
                foreach (Voltage_calibration.VOL_struct vol in VOL_LIST)
                {
                    if (Convert.ToDecimal(vol.VOLT_USE) == Convert.ToDecimal(Auto_One.volt))
                    {
                        power.set_volt(vol.VOLT_OUTPUT, "200", "1");
                        break;
                    }
                }
            }
            //生成测试列表
            List<string> DAC_config = new List<string>();
            DAC_config = freedom_usart_getstring().ToList();
            Play_LOG("正在配置MCU参数（根据自由串口)");
            foreach (string configs in DAC_config)
            {
                Play_LOG(configs);
                Megahunt_USART.Send(configs, Enabled);
                Delay(500);
            }
            Play_LOG("配置完成");
            //采集数据
            float dac_shoud;
            decimal[] current_DC = new decimal[20];
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 20;
            Delay(500);
            //计算平均值
            for (int i = 0; i <= 20; i++)
            {
                progressBar1.Value = i;
                progressBar1.Refresh();
                //暂停
                while (zanting)
                {
                    Delay(100);
                }
                //测试控制
                if (!this_testing || !all_testing)
                {
                    if (!this_testing)
                    {
                        Play_LOG("跳出本条测试！");
                    }
                    break;
                }
                wanyongbiao.get_screen(ref current_DC[i]);
                Play_LOG("Volt:" + current_DC[i].ToString());
                adc_in.Text = current_DC[i].ToString();
                Task.Delay(10);
            }
            decimal current_DC_avg = get_avg_NO_MINMAX(current_DC);
            Play_LOG("----------------------------------------");
            Play_LOG("Volt AVG:" + current_DC_avg.ToString());
        }


        //结束此条测试
        public bool this_testing = true;
        private void button1_Click(object sender, EventArgs e)
        {
            this_testing = false;
        }
        //结束全部测试
        public bool all_testing = true;
        private void button3_Click(object sender, EventArgs e)
        {
            all_testing = false;
        }
        //暂停测试
        public bool zanting = false;
        private void button4_Click(object sender, EventArgs e)
        {
            zanting = !zanting;
        }
        //打开文件
        public static void OPEN_FILE(string path)
        {
            if (File.Exists(path))
            {
                Process myProcess = new Process();
                myProcess.StartInfo.FileName = path;
                myProcess.StartInfo.Verb = "Open";
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.Start();
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

        //清空测试向量
        private void button6_Click(object sender, EventArgs e)
        {
            TestItems.Clear();
            listView1.Items.Clear();
            panel1.VerticalScroll.Value = panel1.VerticalScroll.Maximum;
            //是否存在测试向量
            listView1_ItemChecked(null, null);
        }

        //生成自由串口数据
        public string[] freedom_usart_getstring()
        {

            List<string> list = new List<string>();
            list.Clear();
            if (text1_check.Checked)
            {
                list.Add(text1.Text);
            }
            if (text2_check.Checked)
            {
                list.Add(text2.Text);
            }
            if (text3_check.Checked)
            {
                list.Add(text3.Text);
            }
            if (text4_check.Checked)
            {
                list.Add(text4.Text);
            }
            if (text5_check.Checked)
            {
                list.Add(text5.Text);
            }
            if (text6_check.Checked)
            {
                list.Add(text6.Text);
            }
            if (text7_check.Checked)
            {
                list.Add(text7.Text);
            }
            return list.ToArray();
        }
        //1 DAC通道1开启BUFF输出0~VREF/VDDA
        private void test1_check_CheckedChanged(object sender, EventArgs e)
        {

            test2_check.Checked = false;
            test3_check.Checked = false;
            test4_check.Checked = false;
            test5_check.Checked = false;
            test6_check.Checked = false;
            if (test1_check.Checked)
            {
                comboBox2.SelectedIndex = 0;
                //温度 电压
                wendu_text.Text = "-40;25;85;105";
                wendu_check.Checked = wenxiang_link;
                dianya_text.Text = "1.8;2.1;3.3;3.6";
                vol_check.Checked = wenyayuan_link;
                comboBox1.SelectedIndex = 0;
                textBox2.Text = "3";
                adc_min.Text = "0";
                adc_max.Text = "3.3";
                adc_step.Text = "0.5";
            }
            wenxiang_wenyayuan_LINKCHECK();
        }
        //2 DAC通道2开启BUFF输出0~VREF/VDDA
        private void test2_check_CheckedChanged(object sender, EventArgs e)
        {
            test1_check.Checked = false;
            test3_check.Checked = false;
            test4_check.Checked = false;
            test5_check.Checked = false;
            test6_check.Checked = false;
            if (test2_check.Checked)
            {
                comboBox2.SelectedIndex = 1;
                //温度 电压
                wendu_text.Text = "-40;25;85;105";
                wendu_check.Checked = wenxiang_link;
                dianya_text.Text = "1.8;2.1;3.3;3.6";
                vol_check.Checked = wenyayuan_link;
                comboBox1.SelectedIndex = 0;
                textBox2.Text = "3";
                adc_min.Text = "0";
                adc_max.Text = "3.3";
                adc_step.Text = "0.5";
            }
            wenxiang_wenyayuan_LINKCHECK();
        }
        //3 DAC通道1关闭BUFF输出0 ~VREF/VDDA
        private void test3_check_CheckedChanged(object sender, EventArgs e)
        {
            test1_check.Checked = false;
            test2_check.Checked = false;
            test4_check.Checked = false;
            test5_check.Checked = false;
            test6_check.Checked = false;
            if (test3_check.Checked)
            {
                comboBox2.SelectedIndex = 2;
                //温度 电压
                wendu_text.Text = "-40;25;85;105";
                wendu_check.Checked = wenxiang_link;
                dianya_text.Text = "1.8;2.1;3.3;3.6";
                vol_check.Checked = wenyayuan_link;
                comboBox1.SelectedIndex = 0;
                textBox2.Text = "3";
                adc_min.Text = "0";
                adc_max.Text = "3.3";
                adc_step.Text = "0.5";
            }
            wenxiang_wenyayuan_LINKCHECK();
        }
        //4 DAC通道2关闭BUFF输出0~VREF/VDDA
        private void test4_check_CheckedChanged(object sender, EventArgs e)
        {
            test1_check.Checked = false;
            test2_check.Checked = false;
            test3_check.Checked = false;
            test5_check.Checked = false;
            test6_check.Checked = false;
            if (test4_check.Checked)
            {
                comboBox2.SelectedIndex = 0;
                //温度 电压
                wendu_text.Text = "-40;25;85;105";
                wendu_check.Checked = wenxiang_link;
                dianya_text.Text = "1.8;2.1;3.3;3.6";
                vol_check.Checked = wenyayuan_link;
                comboBox1.SelectedIndex = 0;
                textBox2.Text = "3";
                adc_min.Text = "3.3";
                adc_max.Text = "3.3";
                adc_step.Text = "0.5";
            }
            wenxiang_wenyayuan_LINKCHECK();
        }
        //5 DAC通道1开启BUFF输出固定电压
        private void test5_check_CheckedChanged(object sender, EventArgs e)
        {
            test1_check.Checked = false;
            test2_check.Checked = false;
            test3_check.Checked = false;
            test4_check.Checked = false;
            test6_check.Checked = false;
            if (test5_check.Checked)
            {
                comboBox2.SelectedIndex = 1;
                //温度 电压
                wendu_text.Text = "-40;25;85;105";
                wendu_check.Checked = wenxiang_link;
                dianya_text.Text = "1.8;2.1;3.3;3.6";
                vol_check.Checked = wenyayuan_link;
                comboBox1.SelectedIndex = 0;
                textBox2.Text = "3";
                adc_min.Text = "3.3";
                adc_max.Text = "3.3";
                adc_step.Text = "0.5";
            }
            wenxiang_wenyayuan_LINKCHECK();
        }
        //6 DAC通道2开启BUFF输出固定电压
        private void test6_check_CheckedChanged(object sender, EventArgs e)
        {
            test1_check.Checked = false;
            test2_check.Checked = false;
            test3_check.Checked = false;
            test4_check.Checked = false;
            test5_check.Checked = false;
            if (test6_check.Checked)
            {
                comboBox2.SelectedIndex = 2;
                //温度 电压
                wendu_text.Text = "-40;25;85;105";
                wendu_check.Checked = wenxiang_link;
                dianya_text.Text = "1.8;2.1;3.3;3.6";
                vol_check.Checked = wenyayuan_link;
                comboBox1.SelectedIndex = 0;
                textBox2.Text = "3";
                adc_min.Text = "3.3";
                adc_max.Text = "3.3";
                adc_step.Text = "0.5";
            }
            wenxiang_wenyayuan_LINKCHECK();

        }

        //存储配置文件的结构体
        public struct ONE_Config
        {
            public string NAME;
            public int ADC_NUM;
            public List<string> Config_string;
            public List<string> Get_string;
            public List<string> Close_string;
        }
        //解析出命令
        public string Get_Command(string IN_string, string contains)
        {
            string command = "";
            if (IN_string.Contains(contains))
            {
                command = IN_string.Replace(contains, "");
            }
            else
            {
                command = "NULL";
            }
            return command;
        }
        //从TXT中读出配置文件
        public static List<ONE_Config> ADC_Config_LIST = new List<ONE_Config>();
        public static int command_flag;
        public static int config_num = 0;
        public bool Read_Config_File()
        {
            ADC_Config_LIST.Clear();
            string[] Read_out;
            string command="";
            List<string> CONFIG_COMMAND_list = new List<string>();
            List<string> GET_COMMAND_list = new List<string>();
            List<string> CLOSE_COMMAND_list = new List<string>();
            // int Config_NUM=-1; 
            Read_out = File.ReadLines(Directory.GetCurrentDirectory() + "\\Configs\\ADC_Config.txt").ToArray();
            ONE_Config ONE_Config_temp = new ONE_Config();
            try
            {
                foreach (string line in Read_out)
                {
                    if (line != "")
                    {
                        if (line.Contains("NAME:"))
                        {
                            ONE_Config_temp = new ONE_Config();
                            ONE_Config_temp.Close_string = new List<string>();
                            ONE_Config_temp.Get_string = new List<string>();
                            ONE_Config_temp.Config_string = new List<string>();
                            ONE_Config_temp.NAME = line.Replace("NAME:", "");
                        }
                        else if (line.Contains("ADC_NUM:"))
                        {
                            ONE_Config_temp.ADC_NUM = Convert.ToInt32(line.Replace("ADC_NUM:", ""), 10);
                        }

                        else if (line.Contains("CONFIG_COMMAND"))
                        {
                            CONFIG_COMMAND_list.Clear();
                            command = "CONFIG_COMMAND";
                        }
                        else if (line.Contains("GET_COMMAND"))
                        {
                            GET_COMMAND_list.Clear();
                            command = "GET_COMMAND";
                        }
                        else if (line.Contains("CLOSE_COMMAND"))
                        {
                            CLOSE_COMMAND_list.Clear();
                            command = "CLOSE_COMMAND";
                        }
                        else if (line.Contains("#END THIS"))
                        {
                            ONE_Config_temp.Close_string.AddRange(CLOSE_COMMAND_list.ToArray());
                            ONE_Config_temp.Config_string.AddRange(CONFIG_COMMAND_list.ToArray());
                            ONE_Config_temp.Get_string.AddRange(GET_COMMAND_list.ToArray());
                            ADC_Config_LIST.Add(ONE_Config_temp);
                        }
                        else
                        {
                            if (command == "CONFIG_COMMAND")
                            {
                                CONFIG_COMMAND_list.Add(line);

                            }
                            else if (command == "GET_COMMAND")
                            {
                                GET_COMMAND_list.Add(line);
                            }
                            else if (command == "CLOSE_COMMAND")
                            {
                                CLOSE_COMMAND_list.Add(line);
                            }
                        }

                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("配置文件不符合规范！");
                return false;
            }
            return true;
        }


        //将配置文件加载到输入框
        bool Config_file_USE = false;
        public void Config_to_select()
        {
            config_comboBox.Items.Clear();
            if (Read_Config_File())
            {
                foreach (ONE_Config ONE_Config_temp in ADC_Config_LIST)
                {
                    Config_file_USE = true;
                    config_comboBox.Items.Add(ONE_Config_temp.NAME);
                }
                config_comboBox.SelectedIndex = 0;
                config_num = config_comboBox.SelectedIndex;
                Play_LOG("已加载配置文件");
            }
            else
            {
                Play_LOG("未发现配置文件");
            }
        }

        //温箱测试程序
        public void wenxiang_test()
        {
            string get_wendu_str;
            string set_wendu_str = ((int)(Convert.ToDouble(wendu_text.Text) * 10)).ToString();
            Play_LOG("Set:" + wendu_text.Text);
            Play_LOG("正在调整温度！");
            if (wenxiang.set_wendu(set_wendu_str) == true)
            {
                while (true)
                {
                    Delay(500);
                    get_wendu_str = wenxiang.get_wendu();
                    if (get_wendu_str != "NULL")
                    {
                        wendu_now.Text = (Convert.ToDouble(get_wendu_str) / 10).ToString();
                        if (Math.Abs(Convert.ToInt32(get_wendu_str, 10) - Convert.ToInt32(set_wendu_str, 10)) <= 2)
                        {
                            Play_LOG("已到达预期温度");
                            break;
                        }
                    }
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            wenxiang_test();
        }

        private void listView2_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!listView2.Items[e.Index].Checked)//如果点击的CheckBoxes没有选中
            {
                foreach (ListViewItem lv in listView2.Items)
                {
                    if (lv.Checked)//取消所有已选中的CheckBoxes
                    {
                        lv.Checked = false;
                        lv.Selected = false;
                        // lv.BackColor = Color.White;
                    }
                }
                listView2.Items[e.Index].Selected = true;
            }
        }



        //ADC引脚结构体
        //某一个型号
        public struct ADC_Serise
        {
            public string Serise;
            public int NUM;
            public List<string> ADCX;
            public List<int> Channel;
            public List<string> PIN;

        }
        public static List<ADC_Serise>  ModelList = new List<ADC_Serise>();
        //初始化ADC的引脚图
        public  void ADC_MODEL_LIST()
        {
            ModelList.Clear();
            ADC_Serise adc_serise = new ADC_Serise();
            //F0
            adc_serise=new ADC_Serise();
            adc_serise.ADCX = new List<string>();   
            adc_serise.Channel=new List<int>();
            adc_serise.PIN=new List<string>();  

            adc_serise.Serise = "F030";
            adc_serise.NUM = 1;
            for (int i = 0; i < 16; i++)
            {
                adc_serise.ADCX.Add("ADC1");
                adc_serise.Channel.Add(i);
            }
            adc_serise.PIN=new List<string>() {"PA0","PA1", "PA2", "PA3", "PA4", "PA5", "PA6", "PA7", "PB0", "PB1", "PC0", "PC1", "PC2", "PC3", "PC4","PC5" };
            ModelList.Add(adc_serise);

            //F1
            adc_serise = new ADC_Serise();
            adc_serise.ADCX = new List<string>();
            adc_serise.Channel = new List<int>();
            adc_serise.PIN = new List<string>();
            adc_serise.Serise = "F103";
            adc_serise.NUM = 3;
            for (int i = 0; i < 16; i++)
            {
                adc_serise.ADCX.Add("ADC1");
                adc_serise.Channel.Add(i);
            }
            for (int i = 0; i < 16; i++)
            {
                adc_serise.ADCX.Add("ADC2");
                adc_serise.Channel.Add(i);
            }
            for (int i = 0; i < 16; i++)
            {
                adc_serise.ADCX.Add("ADC3");
                adc_serise.Channel.Add(i);
            }
            adc_serise.PIN = new List<string>()
            { 
            "PA0", "PA1", "PA2", "PA3", "PA4", "PA5", "PA6", "PA7", "PB0", "PB1", "PC0", "PC1", "PC2", "PC3", "PC4", "PC5",
            "PA0", "PA1", "PA2", "PA3", "PA4", "PA5", "PA6", "PA7", "PB0", "PB1", "PC0", "PC1", "PC2", "PC3", "PC4", "PC5",
            "PA0", "PA1", "PA2", "PA3", "PF6", "PF7", "PF8", "PF9", "PF10","VSS", "PC0", "PC1", "PC2", "PF3", "VSS", "VSS",
            };
            ModelList.Add(adc_serise);
            //F4
            adc_serise = new ADC_Serise();
            adc_serise.ADCX = new List<string>();
            adc_serise.Channel = new List<int>();
            adc_serise.PIN = new List<string>();
            adc_serise.Serise = "F407";
            adc_serise.NUM = 3;
            for (int i = 0; i < 16; i++)
            {
                adc_serise.ADCX.Add("ADC1");
                adc_serise.Channel.Add(i);
            }
            for (int i = 0; i < 16; i++)
            {
                adc_serise.ADCX.Add("ADC2");
                adc_serise.Channel.Add(i);
            }
            for (int i = 0; i < 16; i++)
            {
                adc_serise.ADCX.Add("ADC3");
                adc_serise.Channel.Add(i);
            }
            adc_serise.PIN = new List<string>()
            {
            "PA0", "PA1", "PA2", "PA3", "PA4", "PA5", "PA6", "PA7", "PB0", "PB1", "PC0", "PC1", "PC2", "PC13", "PC4", "PC5",
            "PA0", "PA1", "PA2", "PA3", "PA4", "PA5", "PA6", "PA7", "PB0", "PB1", "PC0", "PC1", "PC2", "PC13", "PC4", "PC5",
            "PA0", "PA1", "PA2", "PA3", "PF6", "PF7", "PF8", "PF9", "PF10","PF3", "PC0", "PC1", "PC2", "PF13", "PF4", "PF5",
            };
            ModelList.Add(adc_serise);


            //更新系列下拉
            comboBox3.Items.Clear();
            foreach (ADC_Serise this_model in ModelList)
            {
                comboBox3.Items.Add(this_model.Serise);
            }
             comboBox3.SelectedIndex = 0;
        }

        //切换ADC
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            listView2.Items.Clear();
            foreach (ADC_Serise this_model in ModelList)
            {
                if (comboBox3.Text == this_model.Serise)
                {
                    for (int i = 0; i < this_model.ADCX.Count; i++)
                    {
                        if (this_model.ADCX[i] == comboBox2.Text)
                        {
                            listView2.Items.Add(new ListViewItem(new String[] { "通道" + this_model.Channel[i].ToString(), this_model.PIN[i]}));
                        }

                    }
                    break;
                }
            }
        }
        //切换型号
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox2.Items.Clear();
            test1_check.Enabled = false;
            test2_check.Enabled = false;
            test3_check.Enabled = false;
            test4_check.Enabled = false;
            test5_check.Enabled = false;
            test6_check.Enabled = false;
            foreach (ADC_Serise this_model in ModelList)
            {
                if (comboBox3.Text == this_model.Serise)
                {              
                    for (int i = 0; i < this_model.NUM; i++)
                    {
                        comboBox2.Items.Add("ADC"+(i+1).ToString());
                        if (i == 0)
                        {
                            test1_check.Enabled = true;
                            test4_check.Enabled = true;
                        }
                        if (i == 1)
                        {
                            test2_check.Enabled = true;
                            test5_check.Enabled = true;
                        }
                        if (i == 2)
                        {
                            test3_check.Enabled = true;
                            test6_check.Enabled = true;
                        }
                  
                    }
                    break;
                }
            }
            comboBox2.SelectedIndex = 0;
        }
        //是否有选中未选中返回-1
        private int listView2_Checked()
        {
            int Count_all = listView2.Items.Count;
            int num = 0;
            int checked_num=-1;
            for (int i = 0; i < Count_all; i++)
            {
                if (listView2.Items[i].Checked)
                {
                    num++;
                    checked_num = i;
                }
            }
            if (num == 0)
            {
               return -1;
            }
            else
            {
                return checked_num;
            }
        }
        //发送配置文件
        public void Config_send(string ADC, string CHANNEL, string DIV, string SAMP,int delay_ms,bool playlog)
        {
            if (Config_file_USE)
            {
                foreach (var item in ADC_Config_LIST[config_comboBox.SelectedIndex].Config_string)
                {
                    string temp = item;
                    if (item.Contains("[ADC]"))
                    {
                        temp=temp.Replace("[ADC]",ADC);
                    }
                    if (item.Contains("[CHANNEL_NUM]"))
                    {
                        temp = temp.Replace("[CHANNEL_NUM]", CHANNEL);
                    }
                    if (item.Contains("[DIV_NUM]"))
                    {
                        temp = temp.Replace("[DIV_NUM]", DIV);
                    }
                    if (item.Contains("[SAMP_NUM]"))
                    {
                        temp = temp.Replace("[SAMP_NUM]", SAMP);
                    }
                    Megahunt_USART.Send(temp,true);
                    if (playlog)
                    {
                        Play_LOG(temp);
                    }
                    Delay(delay_ms);
                } 
            }      
        }
        //关闭设备 
        public void Close_send(string ADC, string CHANNEL, string DIV, string SAMP, int delay_ms)
        {
            if (Config_file_USE)
            {
                foreach (var item in ADC_Config_LIST[config_comboBox.SelectedIndex].Close_string)
                {
                    string temp = item;
                    if (item.Contains("[ADC]"))
                    {
                        temp = temp.Replace("[ADC]", ADC);
                    }
                    if (item.Contains("[CHANNEL_NUM]"))
                    {
                        temp = temp.Replace("[CHANNEL_NUM]", CHANNEL);
                    }
                    if (item.Contains("[DIV_NUM]"))
                    {
                        temp = temp.Replace("[DIV_NUM]", DIV);
                    }
                    if (item.Contains("[SAMP_NUM]"))
                    {
                        temp = temp.Replace("[SAMP_NUM]", SAMP);
                    }
                    Megahunt_USART.Send(temp, true);
                    Play_LOG(temp);
                    Delay(delay_ms);
                }
            }
        }
        //读取 
        public void Get_send(string ADC, string CHANNEL, string DIV, string SAMP, int delay_ms,bool playlog)
        {
            if (Config_file_USE)
            {
                foreach (var item in ADC_Config_LIST[config_comboBox.SelectedIndex].Get_string)
                {
                    string temp = item;
                    if (item.Contains("[ADC]"))
                    {
                        temp = temp.Replace("[ADC]", ADC);
                    }
                    if (item.Contains("[CHANNEL_NUM]"))
                    {
                        temp = temp.Replace("[CHANNEL_NUM]", CHANNEL);
                    }
                    if (item.Contains("[DIV_NUM]"))
                    {
                        temp = temp.Replace("[DIV_NUM]", DIV);
                    }
                    if (item.Contains("[SAMP_NUM]"))
                    {
                        temp = temp.Replace("[SAMP_NUM]", SAMP);
                    }
                    Megahunt_USART.Send(temp, true);
                    if (playlog)
                    {
                        Play_LOG(temp);
                    }
                    Delay(delay_ms);
                }
            }
        }
        //config FIle Test
        public double GET_ADC_Test(int delay, bool avg_ON)
        {
            try
            {
                Megahunt_USART.Data_Recved = "";
                Megahunt_USART.Data_Recved_ON = true;
                int temp_num = 0;
                Get_send("0", "0", "0", "1",delay, true);
            TEMP:
                temp_num++;
                double value;
                List<double> values = new List<double>();
                if (Megahunt_USART.Data_Recved == "" || !Megahunt_USART.Data_Recved.Contains("\r\n"))
                {
                    Delay(100);
                    goto TEMP;
                }
                string[] recvs = Megahunt_USART.Data_Recved.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                foreach (string recv in recvs)
                {
                    if (isPureNum(recv))
                    {
                        values.Add(Convert.ToDouble(recv));
                    }
                }
                if (values.Count == 10)
                {
                    if (avg_ON)
                    {
                        value = values.ToArray().Average();
                        return value;
                    }
                    else
                    {
                        return values[0];
                    }

                }
                else
                {
                    if (temp_num == 20)
                    {
                        return -1;
                    }
                    Delay(100);
                    goto TEMP;
                }
 
            }
            catch
            {

                return -1;
            }
        }
        public void Config_Test()
        {
            Config_send("1", "0", "0", "1", 500, true);
            while (true)
            {
                //Delay(1000);
                double get = GET_ADC_Test(100, false);
                if (get != -1)
                { 
                      Play_LOG(get.ToString());   
                }       
            }
           
        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            Config_Test();
        }

 
    }
}
