using Sunny.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
//using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using VISAInstrument.设备;
using static ICSharpCode.SharpZipLib.Zip.ExtendedUnixData;
using static VISAInstrument.AutoTest.PowerTest_1;

namespace VISAInstrument.AutoTest
{

    public partial class PowerTest : UIForm
    {
        public List<decimal> current_data=new List<decimal>();
        public PowerTest()
        {
            InitializeComponent();
            listbox_view();
        }
        public void LogText_add(string logtext)
        {
            LogText.AppendText(logtext+"\r\n");
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


        //开始测试
        public bool start_flag = true;
        private void button1_Click(object sender, EventArgs e)
        {
            if (vol_check.Checked == true)
            {
                MessageBox.Show("请完成连接!\r\n请将万用表与电源输出通道1连接\r\n连接完成后，点击按钮继续");
                string[] dianyas = volt.Text.Split(new[] { ',', '，', ';' }, StringSplitOptions.RemoveEmptyEntries);
                LogText_add("正在执行电压校准");
                if (Voltage_calibration.Vol_list("1", dianyas, ref VOL_LIST))
                {
                    foreach (Voltage_calibration.VOL_struct vol in VOL_LIST)
                    {
                        LogText_add(vol.VOLT_USE.ToString() + "<-->" + vol.VOLT_OUTPUT.ToString());
                    }
                    MessageBox.Show("电压校准成功!");
                }
                else
                {
                    MessageBox.Show("电压校准失败！");
                    goto end1;
                }
            }
            start_flag = true;
           //初始化部分数据
           current_data.Clear();
            LogText.Clear();
            //电流表电流采集模式
            if (wanyongbiao.set_current("DC","1"))
            {
                LogText_add("万用表设置为电流采集模式");
            }
            //Power 供电
            if (vol_check.Checked == true)
            {
                //复位操作
                //dianya_now.Text = Auto_One.volt;
                foreach (Voltage_calibration.VOL_struct vol in VOL_LIST)
                {
                    if (Convert.ToDecimal(vol.VOLT_USE) == Convert.ToDecimal(volt.Text))
                    {
                        power.set_volt(vol.VOLT_OUTPUT, "200", "1");
                        break;
                    }
                }
            }
            //reset一下
            if (power.power_reset("1"))
            {
                LogText_add("已复位MCU");
            }
            Delay(100);
            //设置时钟源
            if (source.FindString(source.Text) == 0)
            {
                Megahunt_USART.form_st.controller.SendDataToCom_huanhang("frequency pll-source hse-2");
            }
            else if (source.FindString(source.Text) == 1)
            {
                Megahunt_USART.form_st.controller.SendDataToCom_huanhang("frequency pll-source hsi-2");
            }
            Delay(500);
            LogText_add("时钟源: " + source.Text + " 设置成功");
            //设置工作频率
            Megahunt_USART.form_st.controller.SendDataToCom_huanhang("frequency sysclk " + hclk.Text);
            Delay(500);
            LogText_add("HCLK: " + hclk.Text + "Mhz 设置成功");
            //使能外设
            if (waishe_combo.FindString(waishe_combo.Text) == 0)
            {
                Megahunt_USART.form_st.controller.SendDataToCom_huanhang("clk enable all");
            }
            else if (waishe_combo.FindString(waishe_combo.Text) == 1)
            {
                Megahunt_USART.form_st.controller.SendDataToCom_huanhang("clk disable all");
            }
            else//自定义使能
            {
                List<string> command_out= new List<string>();
                usart_command(ref command_out);
                foreach (var item in command_out)
                {
                    if (item != "")
                    {
                        Megahunt_USART.form_st.controller.SendDataToCom_huanhang(item);
                        Delay(300);
                    }
                }
            }
            Delay(500);
            LogText_add("外设使能: " + waishe_combo.Text + " 设置成功");
            //温箱
            if (wenxiang.temp_contrl(wendu.Text, "0"))
            {
                LogText_add("温箱已到达目标温度："+wendu.Text);
            }

            //温度
            if (wendu_check.Enabled==true)
            {
                string get_wendu_str;
                string set_wendu_str = ((int)(Convert.ToDouble(wendu.Text) * 10)).ToString();
                if (wenxiang.set_wendu(set_wendu_str) == true)
                {
                    while (true)
                    {
                        Delay(500);
                        get_wendu_str = wenxiang.get_wendu();
                        if (get_wendu_str != "NULL")
                        {
                            textBox6.Text = get_wendu_str;
                            if (Math.Abs(Convert.ToInt32(get_wendu_str, 10) - Convert.ToInt32(set_wendu_str, 10)) <= 10)
                            {
                                break;
                            }
                        }
                    }
                }
            }


            //记录数据 计算出经过多少个100ms
            Double time = Convert.ToDouble(textBox1.Text);
            Double time_range = time * 60 * 10;
            
            //采集频率
            int time_fre = Convert.ToInt32(fre_current.Text, 10);
            int fre_pre = time_fre / 100;
            if (fre_pre == 0)
            {
                fre_pre = 1;
            }

            int num = 0;
            progressBar1.Minimum = 0;
            progressBar1.Maximum =(int)time_range;
            while (start_flag)
            {
                num++;
                progressBar1.Value= num;       
                progressBar1.Refresh();
                //拿温箱数据
                current_data.Clear();
                if (num % fre_pre == 0)
                {
                    decimal current_now = 0;
                    if (wanyongbiao.get_screen(ref current_now))
                    {
                        current_data.Add(current_now);
                        now_current.Text=current_now.ToString();
                    }
                }
               
               if (num == time_range)
                {
                    break;
                }
                
                Delay(100);
            }
            LogText_add("测试结束\r\n" +"电流平均值: "+current_data.Average().ToString());
            end1:
            MessageBox.Show("测试结束");

        }
        //每一个外设的开关使能
        public string clk_enable(bool flag,string function)
        {
            string command = "";
            if (flag)
            {
                command= "clk enable " + function;
            }
            return command; 
        }
  
        //生成串口序列的数据
        public bool usart_command(ref List<string> command_out)
        {
            command_out.Clear();
            //dma
            command_out.Add(clk_enable(dma1.Enabled,"dma1"));
            command_out.Add(clk_enable(dma2.Enabled, "dma2"));
            //adc
            command_out.Add(clk_enable(adc1.Enabled, "adc1"));
            command_out.Add(clk_enable(adc2.Enabled, "adc2"));
            command_out.Add(clk_enable(adc3.Enabled, "adc3"));
            //timer
            command_out.Add(clk_enable(tim1.Enabled, "tim1"));
            command_out.Add(clk_enable(tim2.Enabled, "tim2"));
            command_out.Add(clk_enable(tim3.Enabled, "tim3"));
            command_out.Add(clk_enable(tim4.Enabled, "tim4"));
            command_out.Add(clk_enable(tim5.Enabled, "tim5"));
            command_out.Add(clk_enable(tim6.Enabled, "tim6"));
            command_out.Add(clk_enable(tim7.Enabled, "tim7"));
            command_out.Add(clk_enable(tim8.Enabled, "tim8"));
            command_out.Add(clk_enable(tim9.Enabled, "tim9"));
            command_out.Add(clk_enable(tim10.Enabled, "tim10"));
            command_out.Add(clk_enable(tim11.Enabled, "tim11"));
            command_out.Add(clk_enable(tim12.Enabled, "tim12"));
            command_out.Add(clk_enable(tim13.Enabled, "tim13"));
            //spi
            command_out.Add(clk_enable(spi1.Enabled, "spi1"));
            command_out.Add(clk_enable(spi2.Enabled, "spi2"));
            command_out.Add(clk_enable(spi3.Enabled, "spi3"));
            //gpio
            command_out.Add(clk_enable(gpio_a.Enabled, "gpio-a"));
            command_out.Add(clk_enable(gpio_b.Enabled, "gpio-b"));
            command_out.Add(clk_enable(gpio_c.Enabled, "gpio-c"));
            command_out.Add(clk_enable(gpio_d.Enabled, "gpio-d"));
            command_out.Add(clk_enable(gpio_e.Enabled, "gpio-e"));
            command_out.Add(clk_enable(gpio_f.Enabled, "gpio-f"));
            command_out.Add(clk_enable(gpio_g.Enabled, "gpio-g"));
            //usart
            command_out.Add(clk_enable(usart1.Enabled, "usart1"));
            command_out.Add(clk_enable(usart2.Enabled, "usart2"));
            command_out.Add(clk_enable(usart3.Enabled, "usart3"));
            command_out.Add(clk_enable(uart4.Enabled, "uart4"));
            command_out.Add(clk_enable(uart5.Enabled, "uart5"));
            //others
            command_out.Add(clk_enable(usb.Enabled, "usb"));
            command_out.Add(clk_enable(bkp.Enabled, "bkp"));
            command_out.Add(clk_enable(can.Enabled, "can"));
            command_out.Add(clk_enable(dac.Enabled, "dac"));
            command_out.Add(clk_enable(pwr.Enabled, "pwr"));
            command_out.Add(clk_enable(afio.Enabled, "afio"));
            command_out.Add(clk_enable(crc.Enabled, "crc"));
            command_out.Add(clk_enable(flitf.Enabled, "flitf"));
            command_out.Add(clk_enable(sram.Enabled, "sram"));
            command_out.Add(clk_enable(i2c1.Enabled, "i2c1"));
            command_out.Add(clk_enable(i2c2.Enabled, "i2c2"));
            command_out.Add(clk_enable(wwdg.Enabled, "i2c3"));
            return true;
        }
        //生成串口序列的数据
        public struct waishe_struct
        {
            public string waishe;
            public string command;
            public decimal current_all;
            public decimal current_waishe;
        }
        //每一个外设的开关使能
        public void  clk_enable_save(bool flag,string waishe)
        {
            waishe_struct temps=new waishe_struct();
            string command = "";
            if (flag)
            {
                command = "clk enable " + waishe;
            }
            else
            {
                command = "clk disable " + waishe;
                command = "";
            }
            temps.command=command;
            temps.waishe=waishe;
            temps.current_all = 0;
            temps.current_waishe = 0;
            waishe_Structs.Add(temps);
        }
        public List<waishe_struct> waishe_Structs = new List<waishe_struct>();

        public bool usart_command_save()
        {
            waishe_Structs.Clear();
            //dma
            clk_enable_save(true,"dma1");
            clk_enable_save(true, "dma2");
            //adc
            clk_enable_save(true, "adc1");
            clk_enable_save(true, "adc2");
            clk_enable_save(true, "adc3");
            //timer
            clk_enable_save(true, "tim1");
            clk_enable_save(true, "tim2");
            clk_enable_save(true, "tim3");
            clk_enable_save(true, "tim4");
            clk_enable_save(true, "tim5");
            clk_enable_save(true, "tim6");
            clk_enable_save(true, "tim7");
            clk_enable_save(true, "tim8");
            clk_enable_save(true, "tim9");
            clk_enable_save(true, "tim10");
            clk_enable_save(true, "tim11");
            clk_enable_save(true, "tim12");
            clk_enable_save(true, "tim13");
            //spi
            clk_enable_save(true, "spi1");
            clk_enable_save(true, "spi2");
            clk_enable_save(true, "spi3");
            //gpio
            clk_enable_save(true, "gpio-a");
            clk_enable_save(true, "gpio-b");
            clk_enable_save(true, "gpio-c");
            clk_enable_save(true, "gpio-d");
            clk_enable_save(true, "gpio-e");
            clk_enable_save(true, "gpio-f");
            clk_enable_save(true, "gpio-g");
            //usart
            clk_enable_save(true, "usart1");
            clk_enable_save(true, "usart2");
            clk_enable_save(true, "usart3");
            clk_enable_save(true, "uart4");
            clk_enable_save(true, "uart5");
            //others
            clk_enable_save(true, "usb");
            clk_enable_save(true, "bkp");
            clk_enable_save(true, "can");
            clk_enable_save(true, "dac");
            clk_enable_save(true, "pwr");
            clk_enable_save(true, "afio");
            clk_enable_save(true, "crc");
            clk_enable_save(true, "flitf");
            clk_enable_save(true, "sram");
            clk_enable_save(true, "i2c1");
            clk_enable_save(true, "i2c2");
            clk_enable_save(true, "i2c3");
            return true;
        }

        //发送串口序列的数据
        public bool send_usart_command(List<string> command,int delay_ms)
        {
            try
            {
                foreach (string command_one in command)
                {
                    Megahunt_USART.form_st.controller.SendDataToCom_huanhang(command_one);
                    Delay(delay_ms);
                }
            }
            catch (Exception e)
            { 
                LogText.AppendText("Error:"+e.ToString()+"\r\n");
                return false;
            }
            return true;
        }


        //停止
        private void button2_Click(object sender, EventArgs e)
        {
            start_flag=false;
        }
        //自定义串口使能
        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            //button4.Enabled=checkBox5.Checked;
        }
        //打开配置文件
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.FileName = "Test_List1";
            openFileDialog1.Title = "打开测试文件";
            openFileDialog1.Filter = @"PowerTest1文件|*.PowerTest1";
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                string Path = openFileDialog1.FileName;
                textBox2.Text = Path;
                button11_Click(null,null);
            }
        }
        //重载文件
        private void button11_Click(object sender, EventArgs e)
        {
            //读取到本地文件啊
            read_PowerTest1(textBox2.Text, ref PowerTest_FileRead);
            listView_screen(PowerTest_FileRead);
        }
        //listview
        public void listbox_view()
        {

            //获取本地图片信息显示到列表
            Dictionary<string, int> dicIndex = new Dictionary<string, int>();
            ImageList il = new ImageList();
            //列表定义分组
            ListViewGroup[] lvgs = { new ListViewGroup("内地", HorizontalAlignment.Center),
                                          new ListViewGroup("日韩", HorizontalAlignment.Center),
                                          new ListViewGroup("欧美", HorizontalAlignment.Center),
                                          new ListViewGroup("其他", HorizontalAlignment.Center)};
            listView1.Groups.AddRange(lvgs);

            for (int i = 0, counti = il.Images.Count; i < counti; i++)
            {
                string itemText = il.Images.Keys[i];
                //定义列表展示项
                ListViewItem lvi = new ListViewItem();
                lvi.Text = itemText;
                lvi.ImageIndex = dicIndex[itemText];
                lvi.StateImageIndex = dicIndex[itemText];

                foreach (ListViewGroup lvg in lvgs)
                {
                    if (itemText.Contains(lvg.Header))
                    {
                        //给展示项分组
                        lvi.Group = lvg;
                        lvi.ToolTipText = string.Format("【{0}】{1}", lvg.Header, itemText);
                        break;
                    }
                }
                if (string.IsNullOrWhiteSpace(lvi.ToolTipText))
                {
                    //文件名未按标准格式命名时统一分到一组展示
                    lvi.Group = lvgs[lvgs.Length - 1];
                    lvi.ToolTipText = string.Format("【{0}】{1}", lvgs[lvgs.Length - 1].Header, itemText);
                }

                //添加项
                listView1.Items.Add(lvi);

            }

        }
        public struct PowerTest_2_struct
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
        public decimal[] powers=new decimal[45];
        //显示合成配置使能:bool
        public bool enable_screen(bool[] flag )
        {
            dma1.Checked = flag[0];
            dma2.Checked = flag[1];
            adc1.Checked = flag[2];
            adc2.Checked = flag[3];
            adc3.Checked = flag[4];
            tim1.Checked = flag[5];
            tim2.Checked = flag[6];
            tim3.Checked = flag[7];
            tim4.Checked = flag[8];
            tim5.Checked = flag[9];
            tim6.Checked = flag[10];
            tim7.Checked = flag[11];
            tim8.Checked = flag[12];
            tim9.Checked = flag[13];
            tim10.Checked = flag[14];
            tim11.Checked = flag[15];
            tim12.Checked = flag[16];
            tim13.Checked = flag[17];
            spi1.Checked = flag[18];
            spi2.Checked = flag[19];
            spi3.Checked = flag[20];
            gpio_a.Checked = flag[21];
            gpio_b.Checked = flag[22];
            gpio_c.Checked = flag[23];
            gpio_d.Checked = flag[24];
            gpio_e.Checked = flag[25];
            gpio_f.Checked = flag[26];
            gpio_g.Checked = flag[27];
            usb.Checked = flag[28];
            usart1.Checked = flag[29];
            usart2.Checked = flag[30];
            usart3.Checked = flag[31];
            uart4.Checked = flag[32];
            uart5.Checked = flag[33];
            bkp.Checked = flag[34];
            can.Checked = flag[35];
            dac.Checked = flag[36];
            pwr.Checked = flag[37];
            afio.Checked = flag[38];
            crc.Checked = flag[39];
            flitf.Checked = flag[40];
            sram.Checked = flag[41];
            i2c1.Checked = flag[42];
            i2c2.Checked = flag[43];
            wwdg.Checked = flag[44];

            return true;
        }
        //保存配置使能:bool
        public bool enable_save(ref bool[] flag)
        {
            flag = new bool[45];
            flag[0]=dma1.Checked;
            flag[1]= dma2.Checked;
            flag[2]= adc1.Checked;
            flag[3]= adc2.Checked;
            flag[4]= adc3.Checked;
            flag[5]= tim1.Checked;
            flag[6]= tim2.Checked;
            flag[7]= tim3.Checked;
            flag[8]= tim4.Checked;
            flag[9]= tim5.Checked;
            flag[10]= tim6.Checked;
            flag[11]= tim7.Checked;
            flag[12]= tim8.Checked;
            flag[13]= tim9.Checked;
            flag[14]= tim10.Checked;
            flag[15]= tim11.Checked;
            flag[16]= tim12.Checked;
            flag[17]= tim13.Checked;
            flag[18]= spi1.Checked;
            flag[19]= spi2.Checked;
            flag[20]= spi3.Checked;
            flag[21]= gpio_a.Checked;
            flag[22]= gpio_b.Checked;
            flag[23]= gpio_c.Checked;
            flag[24]= gpio_d.Checked;
            flag[25]= gpio_e.Checked;
            flag[26]= gpio_f.Checked;
            flag[27]= gpio_g.Checked;
            flag[28]= usb.Checked;
            flag[29]= usart1.Checked;
            flag[30]= usart2.Checked;
            flag[31]= usart3.Checked;
            flag[32]= uart4.Checked;
            flag[33]= uart5.Checked;
            flag[34]= bkp.Checked;
            flag[35]= can.Checked;
            flag[36]= dac.Checked;
            flag[37]= pwr.Checked;
            flag[38]= afio.Checked;
            flag[39]= crc.Checked;
            flag[40]= flitf.Checked;
            flag[41]= sram.Checked;
            flag[42]= i2c1.Checked;
            flag[43]= i2c2.Checked;
            flag[44]= wwdg.Checked;
            return true;
        }

        //添加测试
        PowerTest_1 PowerTest_FileRead = new PowerTest_1();
        private void button7_Click(object sender, EventArgs e)
        {
             
            
            string[] volts = volt.Text.Split(new[] { ',','，'}, StringSplitOptions.RemoveEmptyEntries);
            string[] wendus = wendu.Text.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
            string[] sources= source.Text.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
            string[] hclks = hclk.Text.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
            string[] power_states = runing.Text.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
            string[] waisheshineng_whichs = waishe_combo.Text.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);

            if (DialogResult.Yes == MessageBox.Show("添加" + (volts.Length * wendus.Length * sources.Length
                * hclks.Length * power_states.Length * waisheshineng_whichs.Length).ToString() + "条测试", "提示", MessageBoxButtons.YesNo))
            {  
                foreach (var wendu_this in wendus)
                {
                    foreach (var volt_this in volts)
                    {
                        foreach (var sources_this in sources)
                        {
                            foreach (var hclk_this in hclks)
                            {
                                foreach (var power_state_this in power_states)
                                {
                                    foreach (var waisheshineng_whichs_this in waisheshineng_whichs)
                                    {
                                        //验证数据正确性
                                        if (source.Items.Contains(sources_this) && hclk.Items.Contains(hclk_this) && power_states.Contains(power_state_this) && waishe_combo.Items.Contains(waisheshineng_whichs_this))
                                        {
                                            PowerTest_1_struct PowerTest_add = new PowerTest_1_struct();
                                            enable_save(ref PowerTest_add.waisheshineng);
                                            PowerTest_add.name = test_name.Text;
                                            PowerTest_add.time = textBox1.Text;
                                            PowerTest_add.danwei = comboBox1.Text;
                                            PowerTest_add.index = PowerTest_FileRead.data.Count.ToString();
                                            PowerTest_add.volt = volt_this;
                                            PowerTest_add.wendu = wendu_this;   
                                            PowerTest_add.source = sources_this;
                                            PowerTest_add.hclk = hclk_this;
                                            PowerTest_add.power_state = power_state_this;
                                            PowerTest_add.waisheshineng_which = waisheshineng_whichs_this;
                                            PowerTest_add.user_string_enable = checkBox5.Checked;
                                            PowerTest_add.user_string = textBox3.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                            PowerTest_FileRead.data.Add(PowerTest_add);
                                            listView_screen(PowerTest_FileRead);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }//循环结尾
            }
            else {
                return;
            }




            //new_index(listView1);
            //listView1.Items.Add(listView1.Items.Count.ToString());
        }
        //类显示
        public void listView_screen(PowerTest_1 test_class)
        {
            listView1.Items.Clear();
         
            //遍历所有
            foreach (PowerTest_1_struct this_struct in test_class.data)
            {
                listView1.Items.Add(new ListViewItem(new String[] {this_struct.index,this_struct.volt, this_struct.wendu,this_struct.source,
                this_struct.hclk,this_struct.power_state,this_struct.waisheshineng_which,this_struct.user_string_enable.ToString(),this_struct.name,"null" }));
            }
            
        }


        //删除一行或多行
        private void button6_Click(object sender, EventArgs e)
        {
            
            foreach (ListViewItem item in this.listView1.SelectedItems)
            {
                if (item.Selected)
                {
                    PowerTest_FileRead.data.RemoveAt(item.Index);
                    break;
                }
            }
            this.listView1.Refresh();
            new_index_class(PowerTest_FileRead);
            listView_screen(PowerTest_FileRead);
        }
        //重新赋 ListView Index
        public void new_index(ListView listView_this)
        {
            int Count_all = listView_this.Items.Count;
            for (int i = 0; i < Count_all; i++)
            {
                listView_this.Items[i].SubItems[0].Text = i.ToString();    
            }
        
        
        }
        //重新赋 class Index
        public void new_index_class(PowerTest_1 PowerTest_1_this)
        {
            
            PowerTest_1_struct struct_temp_temp=new PowerTest_1_struct();

           for(int index = 0; index< PowerTest_1_this.data.Count; index++)
           {            
                struct_temp_temp = PowerTest_1_this.data[index];
                struct_temp_temp.index = index.ToString();
                PowerTest_1_this.data[index]= struct_temp_temp;
            }
           

        }
        //新建PowerTest1文件
        private void button5_Click(object sender, EventArgs e)
        {
            //新建类
            PowerTest_1 new_PowerTest_1=new PowerTest_1();
            SaveFileDialog saveImageDialog = new SaveFileDialog();
            saveImageDialog.FileName = "Test_List1";
            saveImageDialog.Title = "新建测试文件";
            saveImageDialog.Filter = @"PowerTest1文件|*.PowerTest1";
            if (saveImageDialog.ShowDialog(this) == DialogResult.OK)
            {
                string Path = saveImageDialog.FileName;
                //创建内存流  //序列化
                MemoryStream stream_temp = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream_temp, new_PowerTest_1);
                Byte[] byte_readout = null;
                byte_readout = stream_temp.ToArray();
                //保存数据
                FileStream stream = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                stream.Write(byte_readout,0,byte_readout.Length);
                stream.Close();
                textBox2.Text = Path;
                read_PowerTest1(Path, ref PowerTest_FileRead);
            }

        }

       
        //将类内容从文件解析出

        public void read_PowerTest1(string path, ref PowerTest_1 this_class)
        {
            //反序列化
            FileStream stream = new FileStream(path, FileMode.OpenOrCreate);
            //读出数据
            Byte[] byte_aes = new byte[stream.Length];
            stream.Read(byte_aes, 0, (int)stream.Length);
            BinaryFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            this_class = (PowerTest_1)formatter.Deserialize(stream);
            stream.Close();
        }
        //将类内容显示
        public void screen_PowerTest1(PowerTest_1_struct struct_this,string index)
        { 
            
        
        
        }
        //添加到串口输入
        string[] usart_send=new string[0];
        private void button4_Click(object sender, EventArgs e)
        {
            List<string> list = new List<string>(); 
            var lines = textBox3.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
               list.Add(line);
            }
            usart_send = list.ToArray();

        }


        //另存为
        private void button10_Click(object sender, EventArgs e)
        {
           
            SaveFileDialog saveImageDialog = new SaveFileDialog();
            saveImageDialog.FileName = "Test_List1";
            saveImageDialog.Title = "新建测试文件";
            saveImageDialog.Filter = @"PowerTest1文件|*.PowerTest1";
            if (saveImageDialog.ShowDialog(this) == DialogResult.OK)
            {
                string Path = saveImageDialog.FileName;
                //创建内存流  //序列化
                MemoryStream stream_temp = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream_temp,PowerTest_FileRead);
                Byte[] byte_readout = null;
                byte_readout = stream_temp.ToArray();
                //保存数据
                FileStream stream = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                stream.Write(byte_readout, 0, byte_readout.Length);
                stream.Close();
            }
        }
        //保存配置 
        private void button9_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show("是否保存正在编辑配置到文件，该操作会重载配置？", "警告", MessageBoxButtons.YesNo))
            {
                string Path = textBox2.Text;
                //创建内存流  //序列化
                MemoryStream stream_temp = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream_temp, PowerTest_FileRead);
                Byte[] byte_readout = null;
                byte_readout = stream_temp.ToArray();
                //保存数据
                FileStream stream = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                stream.Write(byte_readout, 0, byte_readout.Length);
                stream.Close();
                button11_Click(null, null);
            }
        }
        //点击
        public int mark = -1;
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (listView1.SelectedIndices.Count > 0)         //若有选中项 
            {
                int index = listView1.SelectedItems[0].Index;

            }
     
        }

        //设备全选
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            dma1.Checked=checkBox1.Checked;
            dma2.Checked = checkBox1.Checked;
            adc1.Checked= checkBox1.Checked;
            adc2.Checked = checkBox1.Checked;
            adc3.Checked = checkBox1.Checked;
            tim1.Checked=checkBox1.Checked;
            tim2.Checked = checkBox1.Checked;
            tim3.Checked = checkBox1.Checked;
            tim4.Checked = checkBox1.Checked;
            tim5.Checked = checkBox1.Checked;
            tim6.Checked = checkBox1.Checked;
            tim7.Checked = checkBox1.Checked;
            tim8.Checked = checkBox1.Checked;
            tim9.Checked = checkBox1.Checked;
            tim10.Checked = checkBox1.Checked;
            tim11.Checked = checkBox1.Checked;
            tim12.Checked = checkBox1.Checked;
            tim13.Checked = checkBox1.Checked;
            spi1.Checked = checkBox1.Checked;
            spi2.Checked = checkBox1.Checked;
            spi3.Checked = checkBox1.Checked;
            gpio_a.Checked = checkBox1.Checked;
            gpio_b.Checked = checkBox1.Checked;
            gpio_c.Checked = checkBox1.Checked;
            gpio_d.Checked = checkBox1.Checked;
            gpio_e.Checked = checkBox1.Checked;
            gpio_f.Checked = checkBox1.Checked;
            gpio_g.Checked = checkBox1.Checked;
            usart1.Checked = checkBox1.Checked;
            usart2.Checked = checkBox1.Checked;
            usart3.Checked = checkBox1.Checked;
            uart4.Checked = checkBox1.Checked;
            uart5.Checked = checkBox1.Checked;
            usb.Checked = checkBox1.Checked;
            bkp.Checked = checkBox1.Checked;
            can.Checked = checkBox1.Checked;
            dac.Checked = checkBox1.Checked;
            pwr.Checked = checkBox1.Checked;
            afio.Checked = checkBox1.Checked;
            crc.Checked = checkBox1.Checked;
            flitf.Checked = checkBox1.Checked;
            sram.Checked = checkBox1.Checked;
            i2c1.Checked = checkBox1.Checked;
            i2c2.Checked = checkBox1.Checked;
            wwdg.Checked = checkBox1.Checked;

        }
        //全选&取消全选
        private void checkbox2_CheckedChanged(object sender, EventArgs e)
        {
          
            int Count_all = listView1.Items.Count;
            for (int i = 0; i < Count_all; i++)
            {
                listView1.Items[i].Checked=checkbox2.Checked;
            }
        }
        //选中测试
        public string last_wendu = "NULL";
        string file_name = "";
        public static List<Voltage_calibration.VOL_struct> VOL_LIST = new List<Voltage_calibration.VOL_struct>();
        private void button8_Click(object sender, EventArgs e)
        {
            string file_name = "TEST DATA\\功耗测试\\逐步开外设_" + DateTime.Now.ToString("yy_MMdd_hhmm_ss") + ".xlsx";
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\TEST DATA\\功耗测试");

            PowerTest_1_struct this_struct;

            if (vol_check.Checked == true)
            {
                MessageBox.Show("请完成连接!\r\n请将万用表与电源输出通道1连接\r\n连接完成后，点击按钮继续");
                string[] dianyas = volt.Text.Split(new[] { ',', '，', ';' }, StringSplitOptions.RemoveEmptyEntries);
                LogText_add("正在执行电压校准");
                if (Voltage_calibration.Vol_list("1", dianyas, ref VOL_LIST))
                {
                    foreach (Voltage_calibration.VOL_struct vol in VOL_LIST)
                    {
                        LogText_add(vol.VOLT_USE.ToString() + "<-->"  + vol.VOLT_OUTPUT.ToString());
                    }
                    MessageBox.Show("电压校准成功!");
                    power.Power_ON_OFF("1",false);
                }
                else
                {
                    MessageBox.Show("电压校准失败！");
                    goto end1;
                }
            }

            foreach (ListViewItem item1 in this.listView1.Items)
            {
                if (item1.Checked)
                {
                    this_struct = PowerTest_FileRead.data[item1.Index];
                    start_flag = true;
                    //初始化部分数据
                    current_data.Clear();
                    LogText.Clear();
                    //电流表电流采集模式
                    if (wanyongbiao.set_current("DC", "1"))
                    {
                        LogText_add("万用表设置为电流采集模式");
                    }
                    //Power 供电
                    if (vol_check.Checked == true)
                    {
                        //复位操作
                        //dianya_now.Text = Auto_One.volt;
                        foreach (Voltage_calibration.VOL_struct vol in VOL_LIST)
                        {
                            if (Convert.ToDecimal(vol.VOLT_USE) == Convert.ToDecimal(this_struct.volt))
                            {
                                power.set_volt(vol.VOLT_OUTPUT, "200", "1");
                                break;
                            }
                        }
                    }
                    //温度
                    if (wendu_check.Checked == true)
                    {

                        int count = 0;
                        string get_wendu_str;
                        string set_wendu_str = ((int)(Convert.ToDouble(this_struct.wendu) * 10)).ToString();
                        LogText_add("Set:" + this_struct.wendu);
                        LogText_add("正在调整温度！");
                        if (wenxiang.set_wendu(set_wendu_str) == true)
                        {
                            while (true)
                            {
                                Delay(500);
                                get_wendu_str = wenxiang.get_wendu();
                                if (get_wendu_str != "NULL")
                                {
                                    textBox6.Text = (Convert.ToDouble(get_wendu_str) / 10).ToString();
                                    if (Math.Abs(Convert.ToInt32(get_wendu_str, 10) - Convert.ToInt32(set_wendu_str, 10)) <= 10)
                                    {
                                        LogText_add("已到达预期温度");
                                        break;
                                    }
                                }
                            }
                        }
                        if (last_wendu != set_wendu_str)
                        {

                            LogText_add("等待5 min 使温度稳定");
                            while (count <= 300)
                            {
                                count++;
                                get_wendu_str = wenxiang.get_wendu();
                                Delay(1000);

                            }
                            last_wendu = set_wendu_str;
                            LogText_add("开始测试");
                        }
                    }

                    //reset一下
                    if (power.power_reset("1"))
                    {
                        LogText_add("已复位MCU");
                    }
                    Delay(1000);
                    //设置时钟源
                    if (source.FindString(this_struct.source) == 0)
                    {
                        Megahunt_USART.form_st.controller.SendDataToCom_huanhang("frequency pll-source hse-1");
                    }
                    else if (source.FindString(this_struct.source) == 1)
                    {
                        Megahunt_USART.form_st.controller.SendDataToCom_huanhang("frequency pll-source hsi-1");
                    }
                    Delay(1000);
                    LogText_add("时钟源: " + this_struct.source + " 设置成功");
                    //设置工作频率
                    Megahunt_USART.form_st.controller.SendDataToCom_huanhang("frequency sysclk " + this_struct.hclk);
                    Delay(1000);
                    LogText_add("HCLK: " + this_struct.hclk + "Mhz 设置成功");
                    //空载电流
                    decimal last_current = wanyongbiao.get_screen_data(10,200);
                    LogText_add("空载电流: " +(last_current*1000).ToString()+"mA");
                    //使能外设
                    if (waishe_combo.FindString(this_struct.waisheshineng_which) == 0)
                    {
                        Megahunt_USART.form_st.controller.SendDataToCom_huanhang("clk enable all");
                        goto Test1;
                    }
                    else if (waishe_combo.FindString(this_struct.waisheshineng_which) == 1)
                    {
                        Megahunt_USART.form_st.controller.SendDataToCom_huanhang("clk disable all");
                        goto Test1; 
                    }
                    else if ((waishe_combo.FindString(this_struct.waisheshineng_which) == 2))
                    {
                        List<string> command_out = new List<string>();
                        usart_command(ref command_out);
                        foreach (var item in command_out)
                        {
                            if (item != "")
                            {
                                Megahunt_USART.form_st.controller.SendDataToCom_huanhang(item);
                                LogText_add(item);
                                //从界面取出延迟时间
                                Delay(Convert.ToInt32(wait_time.Text, 10));
                            }
                        }
                           goto Test1;
                    }
                    else
                    {
                        goto Test2;
                    }

                Test1:
                    {
                        Delay(1000);
                        LogText_add("外设使能: " + this_struct.waisheshineng_which + " 设置成功");

                        //记录数据 计算出经过多少个100ms
                        Double time = Convert.ToDouble(textBox1.Text);
                        Double time_range = time * 60 * 10;

                        //采集频率
                        int time_fre = Convert.ToInt32(fre_current.Text, 10);
                        int fre_pre = time_fre / 100;
                        if (fre_pre == 0)
                        {
                            fre_pre = 1;
                        }

                        int num = 0;
                        progressBar1.Minimum = 0;
                        progressBar1.Maximum = (int)time_range;
                        while (start_flag)
                        {
                            num++;
                            progressBar1.Value = num;
                            progressBar1.Refresh();
                            //拿温箱数据
                            current_data.Clear();
                            if (num % fre_pre == 0)
                            {
                                decimal current_now = 0;
                                if (wanyongbiao.get_screen(ref current_now))
                                {
                                    if (current_now >= 0)
                                    {
                                        current_data.Add((current_now-last_current) * 1000);
                                        now_current.Text = (current_now * 1000).ToString();
                                    }
                                }
                            }

                            if (num == time_range)
                            {
                                break;
                            }

                            Delay(100);
                        }

                        listView1.Items[item1.Index] = new ListViewItem(new String[] {this_struct.index,this_struct.volt, this_struct.wendu,this_struct.source,
                this_struct.hclk,this_struct.power_state,this_struct.waisheshineng_which,this_struct.user_string_enable.ToString(),this_struct.name, current_data.Average().ToString() });
                        item1.Checked = false;
                        LogText_add("测试结束\r\n" + "电流平均值: " + current_data.Average().ToString());
                    }
                Test2:
                    {
                        usart_command_save();
                        progressBar1.Minimum = 0;
                        progressBar1.Maximum = waishe_Structs.Count;
                        progressBar1.Value = 0;
                        for (int i = 0; i < waishe_Structs.Count; i++)
                        {
                            waishe_struct temp1 = waishe_Structs[i];
                            progressBar1.Value++;
                            progressBar1.Refresh();
                            Megahunt_USART.form_st.controller.SendDataToCom_huanhang(temp1.command);
                            Delay(Convert.ToInt32(wait_time.Text, 10));
                            temp1.current_all=wanyongbiao.get_screen_data(10,100);
                            now_current.Text = (temp1.current_all * 1000).ToString();
                            temp1.current_waishe = temp1.current_all - last_current;
                            LogText_add(temp1.waishe + "功耗: " + (temp1.current_waishe*1000).ToString()+"mA");
                            last_current = temp1.current_all;
                            waishe_Structs[i]=temp1;
                        }
                        Excel.Gonghao_Test(file_name,"Master Sheet","温度:"+ this_struct.wendu+" HCLK:"+this_struct.hclk+ " 电压:" + this_struct.volt, waishe_Structs);
                        LogText_add("已写入文件");
                    }


                }
            }

            end1:
            MessageBox.Show("测试结束");
        }

        private void wendu_check_CheckedChanged(object sender, EventArgs e)
        {
            wendu.Enabled=wendu_check.Checked;
        }

        private void vol_check_CheckedChanged(object sender, EventArgs e)
        {
            volt.Enabled = vol_check.Checked;
        }
    }
}
