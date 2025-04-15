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
using static VISAInstrument.AutoTest.DAC_Test1;
using static ICSharpCode.SharpZipLib.Zip.ExtendedUnixData;

namespace VISAInstrument.AutoTest
{
    public partial class DAC_Test1 : MaterialForm
    {
        public static MaterialSkinManager materialSkinManager;
        public static bool wenxiang_link = false;
        public static bool wenyayuan_link = false;
        public DAC_Test1(bool wenxiang,bool wenyayuan)
        {
   
            InitializeComponent();
            //设置开始风格
            materialSkinManager_Config();
            //温箱和稳压源的连接状态

            wenxiang_link = wenxiang;
            wenyayuan_link = wenyayuan;            
            wenxiang_wenyayuan_LINKCHECK();
            Config_to_select();
        }
        ControlChange cc = new ControlChange();
        //自适应页面
        private void DAC_Test1_Load(object sender, EventArgs e)
        { //窗体自适应
            cc.x = this.Width;
            cc.y = this.Height;
            cc.setTag(this);
            //让控件在父控件中居中
            // cc.CenterCtr(panel1, true, true);
        }

        private void DAC_Test1_Resize(object sender, EventArgs e)
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
            dianya_text.Enabled= wenyayuan_link;
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
        //测试向量存储
        public struct TestItem
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
        public static List<TestItem> TestItems = new List<TestItem>();
        public static TestItem this_Item;
        //一次测试
        public static TestItem Auto_One;
        //----------------------------------------



        //自由串口输出
        private void checkBox12_CheckedChanged(object sender, EventArgs e)

        {
   
            button2.Enabled= !checkBox12.Checked;
            button5.Enabled = !checkBox12.Checked;
            button6.Enabled = !checkBox12.Checked;
            DAC1_GROUP.Enabled = !checkBox12.Checked;
            DAC2_GROUP.Enabled = !checkBox12.Checked;
            groupBox6.Enabled = !checkBox12.Checked;
        }
        public void wendu_dianya_LIST()
        {
            string volt;
            string wendu;
            string[] volts = dianya_text.Text.Split(new[] { ',', '，', ';' }, StringSplitOptions.RemoveEmptyEntries);
            string[] wendus = wendu_text.Text.Split(new[] { ',', '，', ';' }, StringSplitOptions.RemoveEmptyEntries);
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
        public void UI_LIST(bool clear,bool screen)
        {
            //清除已经存储的序列
            if (clear)
            {
                TestItems.Clear();
            }
            string volt;
            string wendu;
            string DAC;
            bool buff;
            string[] volts = dianya_text.Text.Split(new[] { ',', '，', ';' }, StringSplitOptions.RemoveEmptyEntries);
            string[] wendus = wendu_text.Text.Split(new[] { ',', '，', ';' }, StringSplitOptions.RemoveEmptyEntries);
            bool[] dacs;
            bool[] buff1s;
            bool[] buff2s;
            dacs = new bool[2] { DAC1_enable.Checked, DAC2_enable.Checked };
            buff1s = new bool[2] { BUFF1_ON.Checked, BUFF1_OFF.Checked };
            buff2s = new bool[2] { BUFF2_ON.Checked, BUFF2_OFF.Checked };

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
                //DAC1
                if (dacs[0])
                {
                    foreach (string vol_for in volts)
                    {
                        DAC = "DAC1";
                        if (buff1s[0])
                        {
                            buff = true;
                            this_Item.volt = vol_for;
                            this_Item.wendu = wendu_for;
                            this_Item.buff = buff;
                            this_Item.DAC = DAC;
                            this_Item.DAC_MIN = Convert.ToInt32(dac1_min.Text, 10);
                            this_Item.DAC_MAX = Convert.ToInt32(dac1_max.Text, 10);
                            this_Item.DAC_Step = Convert.ToInt32(dac1_step.Text, 10);
                            this_Item.MIX = DAC2_MIX.Enabled;
                            TestItems.Add(this_Item);


                        }
                        if (buff1s[1])
                        {
                            buff = false;
                            this_Item.volt = vol_for;
                            this_Item.wendu = wendu_for;
                            this_Item.buff = buff;
                            this_Item.DAC = DAC;
                            this_Item.DAC_MIN = Convert.ToInt32(dac1_min.Text, 10);
                            this_Item.DAC_MAX = Convert.ToInt32(dac1_max.Text, 10);
                            this_Item.DAC_Step = Convert.ToInt32(dac1_step.Text, 10);
                            this_Item.MIX = DAC2_MIX.Enabled;
                            TestItems.Add(this_Item);

                        }
                    }

                }
                //DAC2
                if (dacs[1])
                {
                    DAC = "DAC2";
                    foreach (string vol_for in volts)
                    {
                        if (buff2s[0])
                        {
                            buff = true;
                            this_Item.volt = vol_for;
                            this_Item.wendu = wendu_for;
                            this_Item.buff = buff;
                            this_Item.DAC = DAC;
                            this_Item.DAC_MIN = Convert.ToInt32(dac2_min.Text, 10);
                            this_Item.DAC_MAX = Convert.ToInt32(dac2_max.Text, 10);
                            this_Item.DAC_Step = Convert.ToInt32(dac2_step.Text, 10);
                            this_Item.MIX = DAC1_MIX.Enabled;
                            TestItems.Add(this_Item);


                        }
                        if (buff2s[1])
                        {
                            buff = false;
                            this_Item.volt = vol_for;
                            this_Item.wendu = wendu_for;
                            this_Item.buff = buff;
                            this_Item.DAC = DAC;
                            this_Item.DAC_MIN = Convert.ToInt32(dac2_min.Text, 10);
                            this_Item.DAC_MAX = Convert.ToInt32(dac2_max.Text, 10);
                            this_Item.DAC_Step = Convert.ToInt32(dac2_step.Text, 10);
                            this_Item.MIX = DAC1_MIX.Enabled;
                            TestItems.Add(this_Item);
                        }
                    }
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
                    listView1.Items.Add(new ListViewItem(new String[] {num.ToString("d2"),Item.wendu,Item.DAC,Item.buff?"True":"false",Item.volt,Item.MIX?"True":"false",Item.DAC_MIN.ToString()
                ,Item.DAC_MAX.ToString(),Item.DAC_Step.ToString()}));
                    listView1.Items[num].Checked = true;
                    num++;
                }
                panel1.VerticalScroll.Value = panel1.VerticalScroll.Maximum;
                //是否存在测试向量
                listView1_ItemChecked(null, null);
            }


        }
        //查看测试向量
        private void button2_Click(object sender, EventArgs e)
        {
            UI_LIST(true,true);
        }
        //追加自动化向量
        private void button5_Click(object sender, EventArgs e)
        {
            UI_LIST(false,true);
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
           power. Power_ON_OFF("1",false);
            Delay(1500);
            power.Power_ON_OFF("1", true);
            Delay(1500);
            Play_LOG("已复位MCU");
        }
  
        //自动化测试按键控制
        public void AUTO_Test_Button_contrl(bool testing)
        {        
            button4.Enabled = testing;
            button1.Enabled = testing;
            button3.Enabled = testing;
            auto_test_button.Enabled = !testing;    
            Auto_One.DAC = "NULL";
        }
        //单条测试按键控制
        public void ONCE_Test_Button_contrl(bool testing)
        {
            button4.Enabled = testing;
            button1.Enabled = testing;
            button3.Enabled = false;
            auto_test_button.Enabled =false;
            Auto_One.DAC = "NULL";
        }
        //开始自动化测试
        public static List< Voltage_calibration.VOL_struct> VOL_LIST = new List<Voltage_calibration.VOL_struct>(); 
        private void auto_test_button_Click(object sender, EventArgs e)
        {
       
            //暂停
             zanting = false;
            //结束所有
            all_testing = true;
            AUTO_Test_Button_contrl(true);
            file_name ="TEST DATA\\DAC_TEST_DATA\\DAC_"+ DateTime.Now.ToString("yy_MMdd_hhmm_ss")+".xlsx";
            Directory.CreateDirectory(Directory.GetCurrentDirectory()+ "\\TEST DATA\\DAC_TEST_DATA");

            int Count_all = listView1.Items.Count;

            if (vol_check.Checked == true|| re_jiaozhun_check.Checked==true)
            {
                MessageBox.Show("请完成连接!\r\n请将万用表与电源输出连接\r\n连接完成后，点击按钮继续");
                string[] dianyas = dianya_text.Text.Split(new[] { ',', '，', ';' }, StringSplitOptions.RemoveEmptyEntries);
                Play_LOG("正在执行电压校准");
                if (Voltage_calibration.Vol_list("1", dianyas, ref VOL_LIST))
                {
                    foreach (Voltage_calibration.VOL_struct vol in VOL_LIST)
                    {
                      Play_LOG(vol.VOLT_USE.ToString()+"<-->"+ vol.VOLT_OUTPUT.ToString());
                    }
                    re_jiaozhun_check.Enabled = true;
                    MessageBox.Show("电压校准成功!\r\n请将万用表与DAC输出PA4/PA5 连接\r\n连接完成后，点击按钮继续");
                }
                else
                {
                    MessageBox.Show("电压校准失败！");
                    goto end1;
                }
            }
            for (int i = 0; i < Count_all; i++)
            {
                if (listView1.Items[i].Checked)
                {
                    //测试控制/结束全部
                    if (TestItems[i].DAC != Auto_One.DAC)
                    {
                        if (TestItems[i].DAC == "DAC1")
                        {
                            MessageBox.Show("连接DAC通道!\r\n请将万用表与DAC输出PA4 连接\r\n连接完成后，点击按钮继续");
                        }
                        else
                        {
                            MessageBox.Show("连接DAC通道!\r\n请将万用表与DAC输出PA5 连接\r\n连接完成后，点击按钮继续");
                        }
                    }
                    Auto_One = TestItems[i];
                    
                    if (!all_testing)
                    {
                        Play_LOG("已手动退出测试！");
                        goto end2;
                    }
                    Play_LOG("向量："+(i+1).ToString()+" 开始测试");
                    Play_LOG("VOLT:" + Auto_One.volt + "V\r\nBUFF:" + (Auto_One.buff ? "ON" : "OFF") + "\r\nDAC " + (Auto_One.MIX ? "MIX" : "  NOMIX"));       
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
            testfinish();
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
        //开始单条测试
        private void one_test_button_Click(object sender, EventArgs e)
        {
         
            //未使用自由串口
          if (!checkBox12.Checked)
            {          
                UI_LIST(true,false);
                if (TestItems.Count != 1)
                {
                    MessageBox.Show("测试项数量 > 1,请重新编辑测试配置");
                    goto end1;
                }
                zanting = false;
                all_testing = true;
                ONCE_Test_Button_contrl(true);
                file_name = "TEST DATA\\DAC_TEST_DATA\\DACONCE_" + DateTime.Now.ToString("yy_MM_hh_mm_ss") + ".xlsx";
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\TEST DATA\\DAC_TEST_DATA");
                if (vol_check.Checked == true)
                {
                    MessageBox.Show("请完成连接!\r\n请将万用表与电源输出连接\r\n连接完成后，点击按钮继续");
                    string[] dianyas = dianya_text.Text.Split(new[] { ',', '，', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    Play_LOG("正在执行电压校准");
                    if (Voltage_calibration.Vol_list("1", dianyas, ref VOL_LIST))
                    {
                        foreach (Voltage_calibration.VOL_struct vol in VOL_LIST)
                        {
                            Play_LOG(vol.VOLT_USE.ToString() + "<-->" + vol.VOLT_OUTPUT.ToString());
                        }
                        MessageBox.Show("电压校准成功!\r\n请将万用表与DAC输出PA4/PA5 连接\r\n连接完成后，点击按钮继续");
                    }
                    else
                    {
                        MessageBox.Show("电压校准失败！");
                    }
                }
                //测试控制/结束全部
                if (TestItems[0].DAC == "DAC1")
                {
                    MessageBox.Show("连接DAC通道!\r\n请将万用表与DAC输出PA4 连接\r\n连接完成后，点击按钮继续");
                }
                else
                {
                    MessageBox.Show("连接DAC通道!\r\n请将万用表与DAC输出PA5 连接\r\n连接完成后，点击按钮继续");
                }               
                Auto_One = TestItems[0];
                Play_LOG("向量开始测试");
                Play_LOG("VOLT:" + Auto_One.volt + "V\r\nBUFF:" + (Auto_One.buff ? "ON" : "OFF") + "\r\nDAC " + (Auto_One.MIX ? "MIX" : "  NOMIX"));
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
                    if (Voltage_calibration.Vol_list("1", dianyas, ref VOL_LIST))
                    {
                        foreach (Voltage_calibration.VOL_struct vol in VOL_LIST)
                        {
                            Play_LOG(vol.VOLT_USE.ToString() + "<-->" + vol.VOLT_OUTPUT.ToString());
                        }
                        MessageBox.Show("电压校准成功!\r\n请将万用表与DAC输出PA4/PA5 连接\r\n连接完成后，点击按钮继续");
                    }
                    else
                    {
                        MessageBox.Show("电压校准失败！");
                    }
                }
                Play_LOG("向量开始测试");
                Play_LOG("VOLT:" + Auto_One.volt + "V\r\nBUFF:" + (Auto_One.buff ? "ON" : "OFF") + "\r\nDAC " + (Auto_One.MIX ? "MIX" : "  NOMIX"));
                one_test_freedom_USART();
            }
            end1: {

                testfinish();
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
        public float[] dac_set = new float[32];
        public float[] dac_get = new float[32];
        public float[] dac_shoud = new float[32];
        public List<float> dac_set_list = new List<float>();
        public string file_name="TEST1";
        public int delay = 100;
        public string last_wendu="NULL";
        public void one_test()
        {
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
                    while (count<=300)
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
                dianya_now.Text = Auto_One.volt;
                foreach (Voltage_calibration.VOL_struct vol in VOL_LIST)
                {
                    if (Convert.ToDecimal(vol.VOLT_USE) == Convert.ToDecimal(Auto_One.volt))
                    {
                        power.set_volt(vol.VOLT_OUTPUT, "200", "1");         
                        power_reset();
                        break;
                    }
                }
            }
            //线性度测试
            if (Auto_One.DAC_MIN != Auto_One.DAC_MAX)
            {
                //生成测试列表

                dac_set_list.Clear();
                for (int i = Auto_One.DAC_MIN; i <= Auto_One.DAC_MAX; i = i + Auto_One.DAC_Step)
                {
                    dac_set_list.Add(i);
                }
                dac_set = dac_set_list.ToArray();
                dac_get = new float[dac_set_list.Count];
                dac_shoud = new float[dac_set_list.Count];

                List<string> DAC_config = new List<string>(); 
                if (Auto_One.DAC == "DAC1")
                {
         
                    DAC_config.Clear();
                    if (!Config_file_USE)
                    {    //是否开关BUFF
                        DAC_config.Add(Auto_One.buff ? "dac config buffer on" : "dac config buffer off");
                        //是否为混合输出
                        DAC_config.Add(Auto_One.MIX ? "dac init all" : "dac init channel1");
                        DAC_config.Add(Auto_One.MIX ? "dac enable all" : "dac enable channel1");
                        //设置初始DAC
                        DAC_config.Add("dac out channel1 0");
                        DAC_config.Add("dac out channel2 4095");
                        textBox1.Text = (Auto_One.MIX ? "4095" : "NULL");
                    }
                    else
                    {
                        //是否开关BUFF
                        DAC_config.Add(Auto_One.buff ? DAC_Config_LIST[config_num].BUFFON : DAC_Config_LIST[config_num].BUFFOFF);
                        //是否为混合输出
                        DAC_config.Add(Auto_One.MIX ? DAC_Config_LIST[config_num].INIT : DAC_Config_LIST[config_num].DAC[0].INIT);
                        DAC_config.Add(Auto_One.MIX ? DAC_Config_LIST[config_num].ENABLE : DAC_Config_LIST[config_num].DAC[0].ENABLE);
                        //设置初始DAC
                        DAC_config.Add(DAC_Config_LIST[config_num].DAC[0].DAC_OUT + " 0");
                        DAC_config.Add(DAC_Config_LIST[config_num].DAC[1].DAC_OUT + " 4095");
                        textBox1.Text = (Auto_One.MIX ? "4095" : "NULL");
                    }

                    Play_LOG("正在配置MCU参数（根据UI配置)");
                    foreach (string configs in DAC_config)
                    {
                        Play_LOG(configs);
                        Megahunt_USART.Send(configs,Enabled);   
                        Delay(500);
                    }
                    //进度条控制
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum= dac_set.Length-1;       
                    //一个DAC测试
                    int index = 0;
                    foreach (int dac_now in dac_set)
                    {
                        progressBar1.Value = index;
                        progressBar1.Refresh();
                        decimal[] current_DC=new decimal[10];
                        if (!Config_file_USE)
                        {
                            Megahunt_USART.Send("dac out channel1 " + dac_now.ToString(), Enabled);
                        }
                        else
                        {
                            Megahunt_USART.Send(DAC_Config_LIST[config_num].DAC[0].DAC_OUT + " " + dac_now.ToString(), Enabled);
                        }
                        dac1_now.Text = dac_now.ToString();
                        Delay(delay);
                        //计算平均值
                        for (int i = 0; i <= 9; i++)
                        {
                            //暂停
                            while (zanting)
                            { 
                                Delay(100);
                            }
                            //测试控制
                            if (!this_testing|| !all_testing)
                            {
                                if (!this_testing)
                                {
                                    Play_LOG("跳出本条测试！");
                                }
                                break;
                            }
                            wanyongbiao.get_screen(ref current_DC[i]);
                            Task.Delay(10);
                        }
                        //测试控制
                        if (!this_testing || !all_testing)
                        {
                            break;
                        }
                        decimal current_DC_avg = get_avg_NO_MINMAX(current_DC);
                        dac_get[index] = (float)current_DC_avg;
                        dac_shoud[index] = (float)(dac_now * (Auto_One.volt == "NULL" ? 3.3 : Convert.ToDouble(Auto_One.volt)) / 4095);
                        index++;
                    }
                    Excel.ADD_ONE_EXCEL_DAC(file_name, Auto_One.DAC + "_" + Auto_One.wendu + "°C" + "(线性度测试)",
                        "VOLT:" +Auto_One.volt+"V  BUFF:"+(Auto_One.buff?"ON":"OFF")+ (Auto_One.MIX ? "  MIX" : "  NOMIX"),
                        dac_set, dac_shoud,dac_get);

                }
                else if (Auto_One.DAC == "DAC2")
                {
                    //生成测试列表
                    dac_set_list.Clear();
                    for (int i = Auto_One.DAC_MIN; i <= Auto_One.DAC_MAX; i = i + Auto_One.DAC_Step)
                    {
                        dac_set_list.Add(i);
                    }
                    dac_set = dac_set_list.ToArray();
                    dac_get = new float[dac_set_list.Count];
                    dac_shoud = new float[dac_set_list.Count];

                    DAC_config.Clear();
                    if (!Config_file_USE)
                    {    //是否开关BUFF
                        DAC_config.Add(Auto_One.buff ? "dac config buffer on" : "dac config buffer off");
                        //是否为混合输出
                        DAC_config.Add(Auto_One.MIX ? "dac init all" : "dac init channel1");
                        DAC_config.Add(Auto_One.MIX ? "dac enable all" : "dac enable channel1");
                        //设置初始DAC
                        DAC_config.Add("dac out channel1 0");
                        DAC_config.Add("dac out channel2 4095");
                        dac1_now.Text = (Auto_One.MIX ? "4095" : "NULL");
                    }
                    else
                    {
                        //是否开关BUFF
                        DAC_config.Add(Auto_One.buff ? DAC_Config_LIST[config_num].BUFFON : DAC_Config_LIST[config_num].BUFFOFF);
                        //是否为混合输出
                        DAC_config.Add(Auto_One.MIX ? DAC_Config_LIST[config_num].INIT : DAC_Config_LIST[config_num].DAC[0].INIT);
                        DAC_config.Add(Auto_One.MIX ? DAC_Config_LIST[config_num].ENABLE : DAC_Config_LIST[config_num].DAC[0].ENABLE);
                        //设置初始DAC
                        DAC_config.Add(DAC_Config_LIST[config_num].DAC[1].DAC_OUT + " 0");
                        DAC_config.Add(DAC_Config_LIST[config_num].DAC[0].DAC_OUT + " 4095");
                        dac1_now.Text = (Auto_One.MIX ? "4095" : "NULL");
                    }
                    Play_LOG("正在配置MCU参数（根据UI配置)");
                    foreach (string configs in DAC_config)
                    {
                        Play_LOG(configs);
                        Megahunt_USART.Send(configs,Enabled);   
                        Delay(500);
                    }
                    //进度条控制
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum= dac_set.Length-1;       
                    //一个DAC测试
                    int index = 0;
                    foreach (int dac_now in dac_set)
                    {
                        progressBar1.Value = index;
                        progressBar1.Refresh();
                        decimal[] current_DC=new decimal[10];
                        if (!Config_file_USE)
                        {
                            Megahunt_USART.Send("dac out channel2 " + dac_now.ToString(), Enabled);
                        }
                        else
                        {
                            Megahunt_USART.Send(DAC_Config_LIST[config_num].DAC[1].DAC_OUT + " " + dac_now.ToString(), Enabled);
                        }
                        dac1_now.Text = dac_now.ToString();
                        Delay(delay);
                        //计算平均值
                        for (int i = 0; i <= 9; i++)
                        {
                            //暂停
                            while (zanting)
                            { 
                                Delay(100);
                            }
                            //测试控制
                            if (!this_testing|| !all_testing)
                            {
                                if (!this_testing)
                                {
                                    Play_LOG("跳出本条测试！");
                                }
                                break;
                            }
                            wanyongbiao.get_screen(ref current_DC[i]);
                            Task.Delay(10);
                        }
                        //测试控制
                        if (!this_testing || !all_testing)
                        {
                            break;
                        }
                        decimal current_DC_avg = get_avg_NO_MINMAX(current_DC);
                        dac_get[index] = (float)current_DC_avg;
                        dac_shoud[index] = (float)(dac_now * (Auto_One.volt == "NULL" ? 3.3 : Convert.ToDouble(Auto_One.volt)) / 4095);
                        index++;
                    }
                    Excel.ADD_ONE_EXCEL_DAC(file_name, Auto_One.DAC + "_" + Auto_One.wendu + "°C" + "(线性度测试)",
                  "VOLT:" +Auto_One.volt+"V  BUFF:"+(Auto_One.buff?"ON":"OFF")+ (Auto_One.MIX ? "  MIX" : "  NOMIX"),
                        dac_set, dac_shoud,dac_get);               
                }
            }
            //点的测试
            else
            {
                dac_set_list.Clear();
                for (int i = 0; i <=Auto_One.DAC_Step ; i ++)
                {
                    dac_set_list.Add(Auto_One.DAC_MIN);
                }
                dac_set = dac_set_list.ToArray();
                dac_get = new float[dac_set_list.Count];
                dac_shoud = new float[dac_set_list.Count];

                List<string> DAC_config = new List<string>();

                if (Auto_One.DAC == "DAC1")
                {

                    DAC_config.Clear();
                    if (!Config_file_USE)
                    {    //是否开关BUFF
                        DAC_config.Add(Auto_One.buff ? "dac config buffer on" : "dac config buffer off");
                        //是否为混合输出
                        DAC_config.Add(Auto_One.MIX ? "dac init all" : "dac init channel1");
                        DAC_config.Add(Auto_One.MIX ? "dac enable all" : "dac enable channel1");
                        //设置初始DAC
                        DAC_config.Add("dac out channel1 0");
                        DAC_config.Add("dac out channel2 4095");
                        textBox1.Text = (Auto_One.MIX ? "4095" : "NULL");
                    }
                    else
                    {
                        //是否开关BUFF
                        DAC_config.Add(Auto_One.buff ? DAC_Config_LIST[config_num].BUFFON : DAC_Config_LIST[config_num].BUFFOFF);
                        //是否为混合输出
                        DAC_config.Add(Auto_One.MIX ? DAC_Config_LIST[config_num].INIT : DAC_Config_LIST[config_num].DAC[0].INIT);
                        DAC_config.Add(Auto_One.MIX ? DAC_Config_LIST[config_num].ENABLE : DAC_Config_LIST[config_num].DAC[0].ENABLE);
                        //设置初始DAC
                        DAC_config.Add(DAC_Config_LIST[config_num].DAC[0].DAC_OUT + " 0");
                        DAC_config.Add(DAC_Config_LIST[config_num].DAC[1].DAC_OUT + " 4095");
                        textBox1.Text = (Auto_One.MIX ? "4095" : "NULL");
                    }
                    Play_LOG("正在配置MCU参数（根据UI配置)");
                    foreach (string configs in DAC_config)
                    {
                        Play_LOG(configs);
                        Megahunt_USART.Send(configs, Enabled);
                        Delay(500);
                    }
                    //进度条控制
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = dac_set.Length - 1;
                    //一个DAC测试
                    int index = 0;
                    foreach (int dac_now in dac_set)
                    {
                        progressBar1.Value = index;
                        progressBar1.Refresh();
                        decimal[] current_DC = new decimal[10];
                        if (!Config_file_USE)
                        {
                            Megahunt_USART.Send("dac out channel1 " + dac_now.ToString(), Enabled);
                        }
                        else
                        {
                            Megahunt_USART.Send(DAC_Config_LIST[config_num].DAC[0].DAC_OUT + " " + dac_now.ToString(), Enabled);
                        }
                        dac1_now.Text = dac_now.ToString();
                        Delay(delay);
                        //计算平均值
                        for (int i = 0; i <= 9; i++)
                        {
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
                            Task.Delay(10);
                        }
                        //测试控制
                        if (!this_testing || !all_testing)
                        {
                            break;
                        }
                        decimal current_DC_avg = get_avg_NO_MINMAX(current_DC);
                        dac_get[index] = (float)current_DC_avg;
                        dac_shoud[index] = (float)(dac_now * (Auto_One.volt == "NULL" ? 3.3 : Convert.ToDouble(Auto_One.volt)) / 4095);
                        index++;
                    }
                    Excel.ADD_ONE_EXCEL_DAC(file_name, Auto_One.DAC + "_" + Auto_One.wendu + "°C"+"(单点测试)",
                        "VOLT:" + Auto_One.volt + "V  BUFF:" + (Auto_One.buff ? "ON" : "OFF") + (Auto_One.MIX ? "  MIX" : "  NOMIX"),
                        dac_set, dac_shoud, dac_get);

                }
                else if (Auto_One.DAC == "DAC2")
                {

                    DAC_config.Clear();
                    //是否使用配置文件
                    if (!Config_file_USE)
                    { //是否开关BUFF
                        DAC_config.Add(Auto_One.buff ? "dac config buffer on" : "dac config buffer off");
                        //是否为混合输出
                        DAC_config.Add(Auto_One.MIX ? "dac init all" : "dac init channel2");
                        DAC_config.Add(Auto_One.MIX ? "dac enable all" : "dac enable channel2");
                        //设置初始DAC
                        DAC_config.Add("dac out channel2 0");
                        DAC_config.Add("dac out channel1 4095");
                        dac1_now.Text = (Auto_One.MIX ? "4095" : "NULL");
                    }
                    else
                    {
                        //是否开关BUFF
                        DAC_config.Add(Auto_One.buff ? DAC_Config_LIST[config_num].BUFFON : DAC_Config_LIST[config_num].BUFFOFF);
                        //是否为混合输出
                        DAC_config.Add(Auto_One.MIX ? DAC_Config_LIST[config_num].INIT : DAC_Config_LIST[config_num].DAC[1].INIT);
                        DAC_config.Add(Auto_One.MIX ? DAC_Config_LIST[config_num].ENABLE : DAC_Config_LIST[config_num].DAC[1].ENABLE);
                        //设置初始DAC
                        DAC_config.Add(DAC_Config_LIST[config_num].DAC[1].DAC_OUT + " 0");
                        DAC_config.Add(DAC_Config_LIST[config_num].DAC[0].DAC_OUT + " 4095");
                        dac1_now.Text = (Auto_One.MIX ? "4095" : "NULL");
                    }

                    Play_LOG("正在配置MCU参数（根据UI配置)");
                    foreach (string configs in DAC_config)
                    {
                        Play_LOG(configs);
                        Megahunt_USART.Send(configs, Enabled);
                        Delay(500);
                    }
                    //进度条控制
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = dac_set.Length - 1;
                    //一个DAC测试
                    int index = 0;
                    foreach (int dac_now in dac_set)
                    {
                        progressBar1.Value = index;
                        progressBar1.Refresh();
                        decimal[] current_DC = new decimal[10];
                        if (!Config_file_USE)
                        {
                            Megahunt_USART.Send("dac out channel2 " + dac_now.ToString(), Enabled);
                        }
                        else
                        {
                            Megahunt_USART.Send(DAC_Config_LIST[config_num].DAC[1].DAC_OUT + " " + dac_now.ToString(), Enabled);
                        }
                        dac1_now.Text = dac_now.ToString();
                        Delay(delay);
                        //计算平均值
                        for (int i = 0; i <= 9; i++)
                        {
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
                            Task.Delay(10);
                        }
                        //测试控制
                        if (!this_testing || !all_testing)
                        {
                            break;
                        }
                        decimal current_DC_avg = get_avg_NO_MINMAX(current_DC);
                        dac_get[index] = (float)current_DC_avg;
                        dac_shoud[index] = (float)(dac_now * (Auto_One.volt == "NULL" ? 3.3 : Convert.ToDouble(Auto_One.volt)) / 4095);
                        index++;
                    }
                    Excel.ADD_ONE_EXCEL_DAC(file_name, Auto_One.DAC + "_" + Auto_One.wendu + "°C" + "(单点测试)",
                        "VOLT:" + Auto_One.volt + "V  BUFF:" + (Auto_One.buff ? "ON" : "OFF") + (Auto_One.MIX ? "  MIX" : "  NOMIX"),
                        dac_set, dac_shoud, dac_get);
                }
            }
        }
        public void one_test_freedom_USART()
        {
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
             
                }       
                Play_LOG("开始测试");
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
                decimal[] current_DC = new decimal[1000];
                progressBar1.Minimum = 0;
                progressBar1.Maximum = 999;
                Delay(500);
            //计算平均值
            for (int i = 0; i <= 999; i++)
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
                dac1_now.Text = current_DC[i].ToString();
                Task.Delay(10);
            }
                decimal current_DC_avg = get_avg_NO_MINMAX(current_DC);
            Play_LOG("----------------------------------------");
            Play_LOG("Volt AVG:" + current_DC_avg.ToString());
        }


        //结束此条测试
        public bool this_testing=true;
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
        //更新LOG
        public void Play_LOG(string text)
        {
           LogText.Text += text+"\r\n";
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
            int num = 0;
            listView1.Items.Clear();
            foreach (TestItem Item in TestItems)
            {
                listView1.Items.Add(new ListViewItem(new String[] {num.ToString("d2"),Item.wendu,Item.DAC,Item.buff?"True":"false",Item.volt,Item.MIX?"True":"false",Item.DAC_MIN.ToString()
                ,Item.DAC_MAX.ToString(),Item.DAC_Step.ToString()}));
                listView1.Items[num].Checked = true;
                num++;
            }
            panel1.VerticalScroll.Value = panel1.VerticalScroll.Maximum;
            //是否存在测试向量
            listView1_ItemChecked(null, null);
        }

        //生成自由串口数据
        public string [] freedom_usart_getstring()
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
            test7_check.Checked = false;
            test8_check.Checked = false;
            DAC1_GROUP.Enabled =!test1_check.Checked;
            DAC2_GROUP.Enabled =!test1_check.Checked;
            if (test1_check.Checked)
            {
                //温度 电压
                wendu_text.Text = "-40;25;85;105";
                wendu_check.Checked = false;
                dianya_text.Text = "1.8;2.1;3.3;3.6";
                vol_check.Checked = false;
                //DAC1
                DAC1_enable.Checked = true;
                BUFF1_ON.Checked = true;
                BUFF1_OFF.Checked = false;
                DAC2_MIX.Checked = true;
                dac1_min.Text = "0";
                dac1_max.Text = "4095";
                dac1_step.Text = "1";
                //DAC2
                DAC2_enable.Checked = false;
                BUFF2_ON.Checked = false;
                BUFF2_OFF.Checked = false;
                DAC1_MIX.Checked = false;
                dac2_min.Text = "";
                dac2_max.Text = "";
                dac2_step.Text = "";
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
            test7_check.Checked = false;
            test8_check.Checked = false;
            DAC1_GROUP.Enabled = !test2_check.Checked;
            DAC2_GROUP.Enabled = !test2_check.Checked;
            if (test2_check.Checked)
            {
                //温度 电压
                wendu_text.Text = "-40;25;85;105";
                wendu_check.Checked = false;
                dianya_text.Text = "1.8;2.1;3.3;3.6";
                vol_check.Checked = false;
                //DAC1
                DAC1_enable.Checked = false;
                BUFF1_ON.Checked = false;
                BUFF1_OFF.Checked = false;
                DAC2_MIX.Checked = false;
                dac1_min.Text = "";
                dac1_max.Text = "";
                dac1_step.Text = "";
                //DAC2
                DAC2_enable.Checked = true;
                BUFF2_ON.Checked = true;
                BUFF2_OFF.Checked = false;
                DAC1_MIX.Checked = true;
                dac2_min.Text = "0";
                dac2_max.Text = "4095";
                dac2_step.Text = "1";
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
            test7_check.Checked = false;
            test8_check.Checked = false;
            DAC1_GROUP.Enabled = !test3_check.Checked;
            DAC2_GROUP.Enabled = !test3_check.Checked;
            if (test3_check.Checked)
            {   //温度 电压
                wendu_text.Text = "-40;25;85;105";
                wendu_check.Checked = false;
                dianya_text.Text = "1.8;2.1;3.3;3.6";
                vol_check.Checked = false;
                //DAC1
                DAC1_enable.Checked = true;
                BUFF1_ON.Checked = false;
                BUFF1_OFF.Checked = true;
                DAC2_MIX.Checked = true;
                dac1_min.Text = "0";
                dac1_max.Text = "4095";
                dac1_step.Text = "1";
                //DAC2
                DAC2_enable.Checked = false;
                BUFF2_ON.Checked = false;
                BUFF2_OFF.Checked = false;
                DAC1_MIX.Checked = false;
                dac2_min.Text = "";
                dac2_max.Text = "";
                dac2_step.Text = "";
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
            test7_check.Checked = false;
            test8_check.Checked = false;
            DAC1_GROUP.Enabled = !test4_check.Checked;
            DAC2_GROUP.Enabled = !test4_check.Checked;
            if (test4_check.Checked)
            { //温度 电压
                wendu_text.Text = "-40;25;85;105";
                wendu_check.Checked = false;
                dianya_text.Text = "1.8;2.1;3.3;3.6";
                vol_check.Checked = false;
                //DAC1
                DAC1_enable.Checked = false;
                BUFF1_ON.Checked = false;
                BUFF1_OFF.Checked = false;
                DAC2_MIX.Checked = false;
                dac1_min.Text = "";
                dac1_max.Text = "";
                dac1_step.Text = "";
                //DAC2
                DAC2_enable.Checked = true;
                BUFF2_ON.Checked = false;
                BUFF2_OFF.Checked = true;
                DAC1_MIX.Checked = true;
                dac2_min.Text = "0";
                dac2_max.Text = "4095";
                dac2_step.Text = "1";
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
            test7_check.Checked = false;
            test8_check.Checked = false;
            DAC2_GROUP.Enabled = !test5_check.Checked;
            if (test5_check.Checked)
            { //温度 电压
                wendu_text.Text = "-40;25;85;105";
                wendu_check.Checked = false;
                dianya_text.Text = "1.8;2.1;3.3;3.6";
                vol_check.Checked = false;
                //DAC1
                DAC1_enable.Checked = true;
                BUFF1_ON.Checked = true;
                BUFF1_OFF.Checked = false;
                DAC2_MIX.Checked = true;
                dac1_min.Text = "0";
                dac1_max.Text = "0";
                dac1_step.Text = "4095";
                //DAC2
                DAC2_enable.Checked = false;
                BUFF2_ON.Checked = false;
                BUFF2_OFF.Checked = false;
                DAC1_MIX.Checked = false;
                dac2_min.Text = "";
                dac2_max.Text = "";
                dac2_step.Text = "";
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
            test7_check.Checked = false;
            test8_check.Checked = false;
            DAC1_GROUP.Enabled = !test6_check.Checked;
            if (test6_check.Checked)
            {//温度 电压
                wendu_text.Text = "-40;25;85;105";
                wendu_check.Checked = false;
                dianya_text.Text = "1.8;2.1;3.3;3.6";
                vol_check.Checked = false;
                //DAC1
                DAC1_enable.Checked = false;
                BUFF1_ON.Checked = false;
                BUFF1_OFF.Checked = false;
                DAC2_MIX.Checked = false;
                dac1_min.Text = "";
                dac1_max.Text = "";
                dac1_step.Text = "";
                //DAC2
                DAC2_enable.Checked = true;
                BUFF2_ON.Checked = true;
                BUFF2_OFF.Checked = false;
                DAC1_MIX.Checked = true;
                dac2_min.Text = "0";
                dac2_max.Text = "0";
                dac2_step.Text = "4095";
            }
            wenxiang_wenyayuan_LINKCHECK();

        }
        //7 DAC通道1关闭BUFF输出固定电压
        private void test7_check_CheckedChanged(object sender, EventArgs e)
        {
            test1_check.Checked = false;
            test2_check.Checked = false;
            test3_check.Checked = false;
            test4_check.Checked = false;
            test5_check.Checked = false;
            test6_check.Checked = false;
            test8_check.Checked = false;
            DAC2_GROUP.Enabled = !test7_check.Checked;
            if (test7_check.Checked)
            {//温度 电压
                wendu_text.Text = "-40;25;85;105";
                wendu_check.Checked = false;
                dianya_text.Text = "1.8;2.1;3.3;3.6";
                vol_check.Checked = false;
                //DAC1
                DAC1_enable.Checked = true;
                BUFF1_ON.Checked = false;
                BUFF1_OFF.Checked = true;
                DAC2_MIX.Checked = true;
                dac1_min.Text = "0";
                dac1_max.Text = "0";
                dac1_step.Text = "4095";
                //DAC2
                DAC2_enable.Checked = false;
                BUFF2_ON.Checked = false;
                BUFF2_OFF.Checked = false;
                DAC1_MIX.Checked = false;
                dac2_min.Text = "";
                dac2_max.Text = "";
                dac2_step.Text = "";
            }
            wenxiang_wenyayuan_LINKCHECK();
        }
        //8 DAC通道2关闭BUFF输出固定电压
        private void test8_check_CheckedChanged(object sender, EventArgs e)
        {
            test1_check.Checked = false;
            test2_check.Checked = false;
            test3_check.Checked = false;
            test4_check.Checked = false;
            test5_check.Checked = false;
            test6_check.Checked = false;
            test7_check.Checked = false;
            DAC1_GROUP.Enabled = !test8_check.Checked;
            if (test8_check.Checked)
            {
                //温度 电压
                wendu_text.Text = "-40;25;85;105";
                wendu_check.Checked = false;
                dianya_text.Text = "1.8;2.1;3.3;3.6";
                vol_check.Checked = false;
                //DAC1
                DAC1_enable.Checked = false;
                BUFF1_ON.Checked = false;
                BUFF1_OFF.Checked = false;
                DAC2_MIX.Checked = false;
                dac1_min.Text = "";
                dac1_max.Text = "";
                dac1_step.Text = "";
                //DAC2
                DAC2_enable.Checked = true;
                BUFF2_ON.Checked = false;
                BUFF2_OFF.Checked = true;
                DAC1_MIX.Checked = true;
                dac2_min.Text = "0";
                dac2_max.Text = "0";
                dac2_step.Text = "4095";
            }
            wenxiang_wenyayuan_LINKCHECK();
        }


        //存储配置文件的结构体
        public struct ONE_Config
        {
            public string NAME;
            public int DAC_NUM;
            public string BUFFON;
            public string BUFFOFF;
            public string INIT;
            public string ENABLE;
            public DAC_Config[] DAC;
        }
        public struct DAC_Config
        {
            public string INIT;
            public string ENABLE;
            public string DAC_OUT;
        }
        //解析出命令
        public string Get_Command(string IN_string,string contains)
        {
            string command="";
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
        public static List<ONE_Config> DAC_Config_LIST=new List<ONE_Config>();
        public static int command_flag;
        public static int  config_num=0;
        public bool Read_Config_File()
        {
            DAC_Config_LIST.Clear();
            string[] Read_out;
            string temp_command;
           // int Config_NUM=-1; 
            Read_out=File.ReadLines(Directory.GetCurrentDirectory() + "\\Configs\\DAC_Config.txt").ToArray();
            ONE_Config ONE_Config_temp = new ONE_Config();
            DAC_Config DAC_Config_temp = new DAC_Config();
            try
            {
                foreach (string line in Read_out)
                {
                    if (line != "")
                    {
                        if (line.Contains("NAME:"))
                        {
                            ONE_Config_temp = new ONE_Config();
                            ONE_Config_temp.NAME = line.Replace("NAME:", "");
                        }
                        else if (line.Contains("DAC_NUM:"))
                        {
                            ONE_Config_temp.DAC_NUM = Convert.ToInt32(line.Replace("DAC_NUM:", ""), 10);
                            ONE_Config_temp.DAC = new DAC_Config[ONE_Config_temp.DAC_NUM];
                        }
                        else if (line.Contains("#END THIS"))
                        {
                            DAC_Config_LIST.Add(ONE_Config_temp);
                        }
                        else if (line.Contains("ALL_COMMAND"))
                        {
                            command_flag = -1;
                            DAC_Config_temp = new DAC_Config();
                        }
                        else if (line.Contains("DAC1_COMMAND"))
                        {
                            command_flag = 0;
                            DAC_Config_temp = new DAC_Config();
                        }
                        else if (line.Contains("DAC2_COMMAND"))
                        {
                            command_flag = 1;
                        }
                        else if (line.Contains("BUFFON:"))
                        {
                            temp_command = Get_Command(line, "BUFFON:");
                            ONE_Config_temp.BUFFON = temp_command;

                        }
                        else if (line.Contains("BUFFOFF:"))
                        {
                            temp_command = Get_Command(line, "BUFFOFF:");
                            ONE_Config_temp.BUFFOFF = temp_command;
                        }
                        else if (line.Contains("INIT:"))
                        {
                            temp_command = Get_Command(line, "INIT:");
                            if (command_flag == -1)
                            {
                                ONE_Config_temp.INIT = temp_command;
                            }
                            else
                            {
                                ONE_Config_temp.DAC[command_flag].INIT = temp_command;
                            }

                        }
                        else if (line.Contains("ENABLE:"))
                        {
                            temp_command = Get_Command(line, "ENABLE:");
                            if (command_flag == -1)
                            {
                                ONE_Config_temp.ENABLE = temp_command;
                            }
                            else
                            {
                                ONE_Config_temp.DAC[command_flag].ENABLE = temp_command;
                            }
                        }
                        else if (line.Contains("DAC_OUT:"))
                        {
                            temp_command = Get_Command(line, "DAC_OUT:");
                            ONE_Config_temp.DAC[command_flag].DAC_OUT = temp_command;


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
                foreach (ONE_Config ONE_Config_temp in DAC_Config_LIST)
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
            string set_wendu_str = ((int)(Convert.ToDouble(wendu_text.Text)*10)).ToString();
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
                        wendu_now.Text =(Convert.ToDouble(get_wendu_str)/10).ToString();
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
        public void Config_file_Test()
        {
            Auto_One = TestItems[0]; 
            List<string> DAC_config = new List<string>();
            //是否开关BUFF
            DAC_config.Add(Auto_One.buff ? DAC_Config_LIST[config_num].BUFFON : DAC_Config_LIST[config_num].BUFFOFF);
            //是否为混合输出
            DAC_config.Add(Auto_One.MIX ? DAC_Config_LIST[config_num].INIT : DAC_Config_LIST[config_num].DAC[0].INIT);
            DAC_config.Add(Auto_One.MIX ? DAC_Config_LIST[config_num].ENABLE : DAC_Config_LIST[config_num].DAC[0].ENABLE);
            //设置初始DAC
            DAC_config.Add(DAC_Config_LIST[config_num].DAC[0].DAC_OUT + " 0");
            DAC_config.Add(DAC_Config_LIST[config_num].DAC[1].DAC_OUT + " 4095");
            foreach (var item in DAC_config)
            {
                Delay(1000);
                Play_LOG(item);
                Megahunt_USART.Send(item, Enabled);
            }
            textBox1.Text = (Auto_One.MIX ? "4095" : "NULL");
            int num = 1000;
            while (true) {
                num=num+30;
                Delay(1000);
                Play_LOG(DAC_Config_LIST[config_num].DAC[0].DAC_OUT + " " + num.ToString());
                Megahunt_USART.Send(DAC_Config_LIST[config_num].DAC[0].DAC_OUT + " " + num.ToString(), Enabled);
                if(num>=4095)
                    break;
            }
        }
        //测试配置文件
        private void button7_Click_1(object sender, EventArgs e)
        {
            UI_LIST(true, true);
            Config_file_Test();
        }
        //选择配置文件的check
        private void config_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            config_num=config_comboBox.SelectedIndex;
        }



    }
}
