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

namespace VISAInstrument
{
    public interface IView
    {
        void SetController(IController controller);
        //Open serial port event
        void OpenComEvent(Object sender, SerialPortEventArgs e);
        //Close serial port event
        void CloseComEvent(Object sender, SerialPortEventArgs e);
        //Serial port receive data event
        void ComReceiveDataEvent(Object sender, SerialPortEventArgs e);
    }
 
    public partial class Wave : MaterialForm, IView
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
        public static int[] data_set=new int[0];
        // 定义两个全局变量
        public bool isMouseDown = false;
        public int lastMove = 0; // 用于记录鼠标上次移动的点，用于判断是左移还是右移
                                 // 初始化ScaleView，可根据首次出现在chart中的数据点数修改合适的值
        public static int chart_length = 0;

        public Wave()
        {
          
            InitializeComponent();
            //Com口
            InitializeCOMCombox();
            auto_com_fresh_Click(null, null);
            //设置Chart线
            Chart1.Series["standard"].Points.AddXY(0, 0);
            Chart1.Series["standard"].Points.AddXY(60, 0);
            Chart1.ChartAreas[0].AxisX.ScaleView.Size = 5;
            // 设置不显示chart自带的滚动条
            Chart1.ChartAreas[0].AxisX.ScrollBar.Enabled = false;
            Chart1.ChartAreas[0].AxisY.ScrollBar.Enabled = false;
            // 注意不要开启X轴游标，默认不开启，如下设置false或者不设置下列参数
            Chart1.ChartAreas[0].CursorX.IsUserEnabled = false;
            Chart1.ChartAreas[0].CursorX.AutoScroll = false;
            Chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = false;
            
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
            materialButton4_Click_1(null,null);
            auto_sample_TextChanged(null, null);
            //arb_to_list();

            ////测试函数
            //int[] datas = new int[1];
            //readarbfile("BuiltIn\\SINC.arb", datas);
        }
        //将ARB文件添加到下拉窗口
        //public void arb_to_list()
        //{

        //    getFile("BuiltIn", ".arb", arbs);
        //    comboBox1.Items.Clear();
        //    foreach (FileInfo s in arbs)
        //    {
        //        comboBox1.Items.Add(s.Name);
        //    }


        //}
        private void InitializeCOMCombox()
        {

            //BaudRate
            baudRateCbx.Items.Add(4800);
            baudRateCbx.Items.Add(9600);
            baudRateCbx.Items.Add(19200);
            baudRateCbx.Items.Add(38400);
            baudRateCbx.Items.Add(57600);
            baudRateCbx.Items.Add(115200);
            baudRateCbx.Items.ToString();
            //get 9600 print in text
            baudRateCbx.Text = baudRateCbx.Items[5].ToString();

            //Data bits
            dataBitsCbx.Items.Add(7);
            dataBitsCbx.Items.Add(8);
            //get the 8bit item print it in the text 
            dataBitsCbx.Text = dataBitsCbx.Items[1].ToString();

            //Stop bits
            stopBitsCbx.Items.Add("One");
            stopBitsCbx.Items.Add("OnePointFive");
            stopBitsCbx.Items.Add("Two");
            //get the One item print in the text
            stopBitsCbx.Text = stopBitsCbx.Items[0].ToString();

            //Parity
            parityCbx.Items.Add("None");
            parityCbx.Items.Add("Even");
            parityCbx.Items.Add("Mark");
            parityCbx.Items.Add("Odd");
            parityCbx.Items.Add("Space");
            //get the first item print in the text
            parityCbx.Text = parityCbx.Items[0].ToString();

            //Handshaking
            handshakingcbx.Items.Add("None");
            handshakingcbx.Items.Add("XOnXOff");
            handshakingcbx.Items.Add("RequestToSend");
            handshakingcbx.Items.Add("RequestToSendXOnXOff");
            handshakingcbx.Text = handshakingcbx.Items[0].ToString();

            ////Com Ports
            //string[] ArrayComPortsNames = SerialPort.GetPortNames();
            //if (ArrayComPortsNames.Length == 0)
            //{
            //    statuslabel.Text = "No COM found !";
            //    openCloseSpbtn.Enabled = false;
            //}
            //else
            //{
            //    Array.Sort(ArrayComPortsNames);
            //    for (int i = 0; i < ArrayComPortsNames.Length; i++)
            //    {
            //        comListCbx.Items.Add(ArrayComPortsNames[i]);
            //    }
            //    comListCbx.Text = ArrayComPortsNames[0];
            //    openCloseSpbtn.Enabled = true;
            //}
            //w_length = this.Size.Width;
            //h_length = this.Size.Height;
        }
        /// <summary>
        /// Set controller
        /// </summary>
        /// <param name="controller"></param>
        public void SetController(IController controller)
        {
            this.controller = controller;
        }
        /// <summary>
        /// update status bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OpenComEvent(Object sender, SerialPortEventArgs e)
        {
            if (this.InvokeRequired)
            {
                Invoke(new Action<Object, SerialPortEventArgs>(OpenComEvent), sender, e);
                return;
            }

            if (e.isOpend)  //Open successfully
            {
                comListCbx.Enabled = false;
                baudRateCbx.Enabled = false;
                dataBitsCbx.Enabled = false;
                stopBitsCbx.Enabled = false;
                parityCbx.Enabled = false;
                handshakingcbx.Enabled = false;
                auto_com_fresh.Enabled = false;
                auto_com_connect.Text = "断开";
            }
            else    //Open failed
            {
     
            }
        }

        /// <summary>
        /// update status bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CloseComEvent(Object sender, SerialPortEventArgs e)
        {
            if (this.InvokeRequired)
            {
                Invoke(new Action<Object, SerialPortEventArgs>(CloseComEvent), sender, e);
                return;
            }

            if (!e.isOpend) //close successfully
            {


                comListCbx.Enabled = true;
                baudRateCbx.Enabled = true;
                dataBitsCbx.Enabled = true;
                stopBitsCbx.Enabled = true;
                parityCbx.Enabled = true;
                handshakingcbx.Enabled = true;
                auto_com_fresh.Enabled = true;
                auto_com_connect.Text = "连接";
            }
        }
        static int test_bkp_step = 0;
        static int all_error_num = 0;
        static int all_pass_num = 0;
        static int all_test_num = 0;
        static int error_buff = 2;
        static bool Start_caiji=false;
        /// <summary>
        /// Display received data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ComReceiveDataEvent(Object sender, SerialPortEventArgs e)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    Invoke(new Action<Object, SerialPortEventArgs>(ComReceiveDataEvent), sender, e);
                }
                catch (System.Exception)
                {
                    //disable form destroy exception
                }
                return;
            }

            //auto_log.AppendText(Encoding.GetEncoding("GBK").GetString(e.receivedBytes));
           
            string recv = Encoding.GetEncoding("GBK").GetString(e.receivedBytes);
  
            if (Start_caiji == true)
            {
                
                usart_text.Text += recv;
                //将光标设置到末尾位置
                usart_text.Select(point_text_box.Text.Length, 0);
                //将文本滚动到光标处,多行的时候用，单行可不用
                usart_text.ScrollToCaret();
                if (recv.Contains(start_text.Text))
                {
                    test_bkp_step = 0;
                    if(Regex.Matches(usart_text.Text, "00000001 00000002 00000003 00000004").Count==2)
                    {
                        all_pass_num++;
                        auto_error_num_textbox.Text = all_error_num.ToString() + "/" + all_pass_num.ToString();
                    }
                    else if (Regex.Matches(usart_text.Text, "read end").Count == 2)
                    {
                        error_buff--;
                        //忽略前两个错误
                        if (error_buff <= 0)
                        {
                            all_error_num++;
                            auto_error_num_textbox.Text = all_error_num.ToString() + "/" + all_pass_num.ToString();
                            string result_string = "\r\n\r\n";
                            result_string += "Time:" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff:ffffff") + "\tFailed\r\n";
                            result_string += "Test:" + all_test_num.ToString() + "\t";
                            result_string += "Pass:" + all_pass_num.ToString() + "\t";
                            result_string += "Fail:" + all_error_num.ToString() + "\r\n";
                            result_string += "上升时间：" + get_jiange(auto_sample.Text, auto_t_uptime.Text) + "\t";
                            result_string += "下降时间：" + get_jiange(auto_sample.Text, auto_t_downtime.Text) + "\r\n";
                            result_string += "最小值：" + auto_t_min.Text + "\t";
                            result_string += "最大值：" + auto_t_max.Text + "\r\n";
                            result_string += "保持时间：" + get_jiange(auto_sample.Text, auto_t_baochi.Text) + "\t";
                            result_string += "周期间隔：" + get_jiange(auto_sample.Text, auto_t_boxingjiange.Text) + "\t";
                            result_string += usart_text.Text;
                            SaveFile(result_string, @"Test result\问题记录.txt", true);
                        }
                    }
                    usart_text.Text = "";
                }
                if (recv.Contains("TS"))
                {
                    switch (test_bkp_step)
                    {
                        case 0:

                            test_bkp_step++;
                            controller.SendDataToCom_huanhang_ANSI("bkp read");
                            break;
                        case 1:
                            test_bkp_step++;
                            controller.SendDataToCom_huanhang_ANSI("bkp unprotect");
                            break;
                        case 2:

                            test_bkp_step++;
                            controller.SendDataToCom_huanhang_ANSI("bkp write");
                            break;
                        case 3:

                            test_bkp_step++;
                            controller.SendDataToCom_huanhang_ANSI("bkp read");
                            break;
                        default:
                            break;
                    }

                }
            }

        }
        /// <summary>
        /// 获得目录下所有文件或指定文件类型文件(包含所有子文件夹)
        /// </summary>
        /// <param name="path">文件夹路径</param>
        /// <param name="extName">扩展名可以多个 例如 .mp3.wma.rm</param>
        /// <returns>List<FileInfo></returns>
        public static List<FileInfo> getFile(string path, string extName, List<FileInfo> lst)
        {
            try
            {

                string[] dir = Directory.GetDirectories(path); //文件夹列表  
                DirectoryInfo fdir = new DirectoryInfo(path);
                FileInfo[] file = fdir.GetFiles();
                //FileInfo[] file = Directory.GetFiles(path); //文件列表  
                if (file.Length != 0 || dir.Length != 0) //当前目录文件或文件夹不为空          
                {
                    foreach (FileInfo f in file) //显示当前目录所有文件  
                    {
                        if (extName.ToLower().IndexOf(f.Extension.ToLower()) >= 0)
                        {
                            lst.Add(f);
                        }
                    }
                    foreach (string d in dir)
                    {
                        getFile(d, extName, lst);//递归  
                    }
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void readarbfile(string filename, ref int[] data)
        {
            string temp = "";
            List<string> StringList = new List<string>();
            string[] Data_string = new string[1];
            //已读的方式打开文件  并创建数据流
            StreamReader arbs = new StreamReader(filename);
            while (true)
            {
                temp = arbs.ReadLine();
                if (temp != null)
                {
                    StringList.Add(temp);
                }
                else
                {
                    break;
                }

            }
            //获取datac长度
            int data_nums = 0;//总长度
            int m = StringList.ToArray().Length;
            foreach (string s in StringList)
            {
                if (s.Contains("Data Points:"))
                {
                    data_nums = Convert.ToInt32(s.Replace("Data Points:", ""));
                    break;
                }
            }
            data = new int[data_nums];
            bool data_flag = false;
            int data_num = 0;
            foreach (string s in StringList)
            {
                if (data_flag == true && data_num < data_nums)
                {
                    data[data_num] = (int)Convert.ToInt32(s);
                    data_num++;
                }
                if (s.Contains("Data:"))
                {
                    data_flag = true;
                }
            }
            arbs.Close();
        }
        //导入ARB文件
        private void materialButton8_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.CheckPathExists = true;
  
            openFileDialog1.FileName = "选择ARB文件";
            openFileDialog1.Filter = "波形文件(*.arb)|*.arb";
            openFileDialog1.Title = "打开升级文件";
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                string filePath = openFileDialog1.FileName;
                file_path_see.Text = filePath;
               
            }
         
        }
        Thread th;
        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            { 
                readarbfile(file_path_see.Text, ref run_data);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:"+ex.Message);
                goto ERROR1;
            }
            if (th == null || !th.IsAlive)
            {
                Run();
                //th = new Thread(Run);
                //th.IsBackground = true;
                //th.Start();
                //pinlv_see.Enabled = false;
            }
        ERROR1:
            { }
            
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (th != null && th.IsAlive)
            {
                th.Abort();
                //pinlv_see.Enabled = true;
            }
        }
        List<double> listData = new List<double>();
        double autoMove = 0, interval = 0, move = 0;


        public void Run()
        {


            listData.Clear();
            int m = 0;
            DisplayChart(run_data);
            //while (m<run_data.Length-1)
            //{
            //    m ++;
            //    Thread.Sleep(Convert.ToInt32(pinlv_see.Text));
            //    //Random random = new Random();
            //    //int temp = random.Next(10, 1000);
            //    //double tempD = 10 + (temp / (double)1000.0) - temp % (double)1000.0;
            //    //listData.Add(tempD);
            //    listData.Add(run_data[m]);
            //    try
            //    {
            //        DisplayChart(listData, chart1.Series["Err"], materialLabel1, ref autoMove, move, ref interval, false);
            //    }
            //    catch (Exception ex)
            //    {
            //        string s = ex.Message;
            //    }
            //}
        }
        //求出一个数组中的拐点  length是数组的长度
        void getDot(int[] data, ref int[] guai)
        {
            List<int> DataList = new List<int>();
            DataList.Clear();
            int i = 0;
            int k = 0;
            while (i != data.Length-1)
            {
             
                    if(k != data[i+1]-data[i])
                    { 
                        DataList.Add(i);
                        k = data[i + 1] - data[i];
                    }
                    i++;    
            }
            guai = new int[DataList.Count];
            int num = 0;
            foreach (int s in DataList)
            {
                guai[num] = s;
                num++;
            }
        }
        //数据处理采样
        public void caiyangxianshi(int[] data_input, ref int[] data_output)
        {
            List<int> DataList = new List<int>();
            DataList.Clear();
            //获取采样数
            int caiyan_num = Convert.ToInt32(caiyangshu.Text);
            if (data_input.Length <= caiyan_num)
            {
                data_output = data_input;
            }
            else//需要处理数据
            {
                int[] guai = null;
                getDot(data_input, ref guai);
                //几个点采一次
                int temp_num=data_input.Length/caiyan_num; 
                DataList.Add((int)data_input[0]);
                int next_data_num = 0;
                for (int this_num = 1; this_num < data_input.Length-1;this_num++)
                {
                    if (guai.Contains(this_num))
                    {
                        DataList.Add((int)data_input[this_num]);
                        next_data_num = this_num + temp_num;
                    }
                    else if (this_num == next_data_num)
                    {
                        DataList.Add((int)data_input[this_num]);
                        next_data_num = this_num + temp_num;
                    }
                    
                }
                DataList.Add((int)data_input[data_input.Length-1]);
                data_output = new int[DataList.Count];
                int temp_nums = 0;
                foreach (int s in DataList)
                {
                    data_output[temp_nums] =s ;
                    temp_nums++;
                }



            }
        }
        private void DisplayChart(int[] data_input)
        {
            if (Display_check.Checked == false)
                goto end1;
            //进度条显示
            chart_process_bar2.Minimum = 0;
            chart_process_bar2.Maximum = data_input.Length;




            int[] data = null;
            if (caiyangxianshi_check.Checked == false)
            { 
                data = data_input;
            }
            else {
                caiyangxianshi(data_input, ref data);


            }
           
            int temp = 0;

            for (int i = 0; i < data.Length ; i++)
            {
                if (Math.Abs(data[i]) > temp)   //最后一个和谁去比？所以比较的时候不能比较到数组最后一个元素，到倒数第二个元素就停止
                {
                    temp = Math.Abs(data[i]);
                }
            }
            if (materialCheckbox1.Checked == true)
            {
                interval = temp / 2.8;
                textBox5.Text = interval.ToString();

            }
            else
            { 
             interval =    Convert.ToInt32(textBox5.Text);
            }
            //文本框显示,是否取样显示
            StringBuilder Tex_box = new StringBuilder();
            //StringBuilder Tex_temp = new StringBuilder();
            if (quyang_text.Checked == false)
            {
                point_text_box.Text = "";
                for (int i = 0; i < data_input.Length; i++)
                {
                    chart_process_bar2.Value = i;
                    chart_process_bar2.Refresh();   
                    Tex_box.Append(i + ": " + data_input[i] + "\r\n");
                }
            }
            else
            {
                point_text_box.Text = "";
                for (int i = 0; i < data.Length; i++)
                {
                    chart_process_bar2.Value = i;
                    chart_process_bar2.Refresh();
                    Tex_box.Append(i + ": " + data[i] + "\r\n");
                }


            }
            point_text_box.Text = Tex_box.ToString();
            //将光标设置到末尾位置
            point_text_box.Select(point_text_box.Text.Length, 0);
            //将文本滚动到光标处,多行的时候用，单行可不用
            point_text_box.ScrollToCaret();

            double tempY = 0;
            double tempInderval = interval;
          
            Chart1.Series["Err"].Points.Clear();
            Chart1.Series["Err"].Points.AddXY(0, 0);
            Chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            //label1s.Text = "每格:" + tempInderval;
            chart_process_bar2.Minimum = 0;
            chart_process_bar2.Maximum = data.Length;
            for (int i = 0; i < data.Length; i++)
            {
            
                tempY = (data[i]) * (10 / tempInderval);
                double m = (i * 60) / data.Length;

                 Chart1.Series["Err"].Points.AddXY(i + 1, tempY);
                 Chart1.Series["Err"].ChartType = SeriesChartType.FastLine;//选快速扫描线条
           
               
                chart_process_bar2.Value = i;
                chart_process_bar2.Refresh();

            } 
            Chart1.Series["Err"].Points.AddXY(data.Length+1, 0);
         
         
            //宽度调整
            Chart1.ChartAreas[0].AxisX.ScaleView.Size = data.Length*1.1; 
            chart_length=data.Length;
         
            //达到百分百
            chart_process_bar2.Value = data.Length;
           
            chart_process_bar2.Refresh();
        end1: { }

        }
        /// <summary>
        /// Chart绘图
        /// </summary>
        /// <param name="listErr">误差集合</param>
        /// <param name="series"></param>
        /// <param name="controlInterval">每大格值控件</param>
        /// <param name="autoMove">自动移动大小</param>
        /// <param name="move">移动大小</param>
        /// <param name="interval">每大格间距</param>
        /// <param name="isAuto">是否自动适应</param>
        private void DisplayChart(List<double> listErr, Series series, Control controlInterval, ref double autoMove, double move, ref double interval, bool isAuto)
        {
            double max = int.MinValue;
            double min = int.MaxValue;

            double max_abs = int.MinValue;
            double min_abs = int.MaxValue;

            //while (listErr.Count > 61)
            //{
            //    listErr.RemoveAt(0);
            //}
            //if (series.Points != null && series.Points.Count > 0)
            //{
            //    this.BeginInvoke((EventHandler)(delegate { series.Points.Clear(); }));
            //}
            if (listErr != null && listErr.Count > 1)
            {
                //if (listErr.Count < 10 || isAuto)
                //{
                 
                //    foreach (double f in listErr)
                //    {
                //        max = max> f ? max : f;
                //        min = min< f ? min : f;

                //        max_abs = Math.Abs(max_abs) > Math.Abs(f) ? Math.Abs(max_abs) : Math.Abs(f);
                //        min_abs = Math.Abs(min_abs) < Math.Abs(f) ? Math.Abs(max_abs) : Math.Abs(f);
                //    }

                //    //interval = GetInterval(Math.Abs(max - min) / 2);
                //    interval = Math.Abs(max)/4;

                //    if (interval == 0)
                //    {
                //        interval = 1;
                //    }

                //    autoMove = ((max - min) / 2) + min;
                //}
                foreach (double f in listErr)
                {
                    max = max > f ? max : f;
                    min = min < f ? min : f;

                    max_abs = Math.Abs(max_abs) > Math.Abs(f) ? Math.Abs(max_abs) : Math.Abs(f);
                    min_abs = Math.Abs(min_abs) < Math.Abs(f) ? Math.Abs(max_abs) : Math.Abs(f);
                }

                //interval = Math.Abs(max_abs) / 4;
                interval = 30000/ 3;
                double tempY = 0;   
                double tempInderval = interval;
                controlInterval.Text = "每格:" + tempInderval;
                for (int i = 0; i < listErr.Count; i++)
                {
                    tempY = (listErr[i]) * (10 / tempInderval) + move;
                    double m = (i * 60) / listErr.Count;
                    series.Points.AddXY((i / listErr.Count) * 60, tempY);
                }
                //this.BeginInvoke((EventHandler)(delegate
                //{
              
                //}
                //));

            }
        }

       

        // 鼠标按下事件
        private void chart1_MouseDown(object sender, MouseEventArgs e)
        {
            lastMove = 0;
            isMouseDown = true;
        }
        // 鼠标弹起事件
        private void chart1_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
        }
        // 鼠标移动事件
        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                try
                {
                    int m = (int)Chart1.ChartAreas[0].AxisX.ScaleView.Size / 50;
                  
                        // 可更改（交换）如下加减1或if条件来设置鼠标移动时曲线移动方向
                        if (lastMove != 0 && e.X - lastMove > 0)
                            Chart1.ChartAreas[0].AxisX.ScaleView.Position += 1*m;  // 每次移动1
                        else if (lastMove != 0 && e.X - lastMove < 0)
                            Chart1.ChartAreas[0].AxisX.ScaleView.Position -= 1*m; // 每次移动1
                        lastMove = e.X;
                    
                }
                catch 
                {
                    goto ERROR1;
                }
            }
            ERROR1:
            var area = Chart1.ChartAreas[0];
            double xValue = 0;
            double yValue = 0;
            try
            {
                 xValue = area.AxisX.PixelPositionToValue(e.X);
                 yValue = area.AxisY.PixelPositionToValue(e.Y);
             }
            catch 
            {
                    goto ERROR2;
            }
            label39.Text = string.Format("X: {0:f}  Y: {1:f}", xValue, (yValue/10)* Convert.ToDouble(textBox5.Text));
        ERROR2: { }
        }
   

        //鼠标悬停显示曲线点的值
        System.Windows.Forms.DataVisualization.Charting.ToolTipEventArgs toolTipEventArgs;
        private void chart1_GetToolTipText(object sender, ToolTipEventArgs e)
        {
            toolTipEventArgs = e;

            if (e.HitTestResult.ChartElementType == System.Windows.Forms.DataVisualization.Charting.ChartElementType.DataPoint)
            {
                int i = e.HitTestResult.PointIndex;
                System.Windows.Forms.DataVisualization.Charting.DataPoint dp = e.HitTestResult.Series.Points[i];
                e.Text = string.Format("X: {0:f}  Y: {1:f}", dp.XValue.ToString(), (dp.YValues[0] / 10) * Convert.ToDouble(textBox5.Text));
            }
        }

        //鼠标滚轮放大缩小
        private void chart1_MouseWheel(object sender, MouseEventArgs e)
        {

            int m = (int)Chart1.ChartAreas[0].AxisX.ScaleView.Size / 20;
            for (int i = 0; i <= m; i++)
            { // 实验发现鼠标滚轮滚动一圈时e.Delta = 120，正反转对应正负120
                if (Chart1.ChartAreas[0].AxisX.ScaleView.Size > 0) // 防止越过左边界
                {
                    Chart1.ChartAreas[0].AxisX.ScaleView.Size += (e.Delta / 120); // 每次缩放1
                }
                else if (e.Delta > 0)
                {

                    Chart1.ChartAreas[0].AxisX.ScaleView.Size += (e.Delta / 10); // 每次缩放1
                }
            }
        }
        private double GetInterval(double source)
        {
            double temp = source;
            if (source < 1)
            {
                string s = source.ToString("f10");
                string temps = s.Trim(new char[] { '0', '.' }).Length == 0 ? "0" : s.Trim(new char[] { '0', '.' });
                int index = s.IndexOf(temps);
                if (temps.Length > 1)
                {
                    temps = temps.Substring(0, 1);
                    int result = int.Parse(temps);
                    if (result < 5)
                    {
                        result = 5;
                        temps = s.Substring(0, index) + (result);
                        temp = double.Parse(temps);
                    }
                    else
                    {
                        result = 1;
                        temps = s.Substring(0, index) + (result);
                        temp = double.Parse(temps) * 10;
                    }

                }
                else
                {
                    int result = int.Parse(temps);
                    if (result > 5)
                    {
                        result = 1;
                        temps = s.Substring(0, index) + (result);
                        temp = double.Parse(temps) * 10;
                    }
                    else
                    {
                        result = 5;
                        temps = s.Substring(0, index) + (result);
                        temp = double.Parse(temps);
                    }
                }
            }
            else if (source < 10)
            {
                temp = Math.Ceiling(source);
            }
            else
            {
                temp = Math.Ceiling(temp / 10) * 10;
            }
            return temp;
        }


  

        //将data转成ARB
        public void data_to_arb(string filename, int[] data)
        {
            arbs_set.Clear();
            arbs_set.Add("Copyright: Agilent Technologies, 2010");
            arbs_set.Add("File Format:"+file_format.Text);
            arbs_set.Add("Checksum:" + check_sum_combo.FindString(check_sum_combo.Text).ToString());
            arbs_set.Add("Channel Count:"+channel_count_combo.Text);
            arbs_set.Add("Sample Rate:" + sample_rate.Text);
            arbs_set.Add("High Level:" + high_level.Text);
            arbs_set.Add("Low Level:" + low_level.Text);
           // arbs_set.Add("#Data Type:攕hort?");
            arbs_set.Add("Filter:" + @""""+fliter_combo.Text+@"""");
            arbs_set.Add("Data Points:"+string.Format("{0:D}",data.Length));
            arbs_set.Add("Data:");
            for (int i = 0; i < data.Length; i++)
            {
                arbs_set.Add(string.Format("{0:D}", data[i]));
            }
            //保存到txt
            //创建一个文件流，用以写入或者创建一个StreamWriter 
            //System.IO.FileStream fs = new System.IO.FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
            //StreamWriter sw = new StreamWriter(fs);
            StreamWriter sw = new StreamWriter(filename, false, Encoding.GetEncoding("GB2312"));
            sw.Flush();
            // 使用StreamWriter来往文件中写入内容 
            sw.BaseStream.Seek(0, SeekOrigin.Begin);
            for (int i = 0; i < arbs_set.Count; i++) sw.WriteLine(arbs_set[i]);
            //关闭此文件 
            sw.Flush();
            sw.Close();
            //fs.Close();
        }
        //增加一个点
        private void add_point_Click(object sender, EventArgs e)
        {
            int range_nums;//循环次数
            if (Point_range_time.Text == "" || Convert.ToInt32(Point_range_time.Text) < 0)
            {
                Point_range_time.Text = "1";
                range_nums = 1;
            }
            else
            {
                range_nums = Convert.ToInt32(Point_range_time.Text);
            }
            //获取新点位值
            if (point_textbox.Text == "")
            {
                MessageBox.Show("Point NULL");
                goto end1;
            } 
            
            int point = Convert.ToInt32(point_textbox.Text);
            if (point > 32767 || point < -32767)
            {
                MessageBox.Show("输入值在32767和-32767之间");
                goto end1;
            }
            for (int i = 0; i < range_nums; i++)
            {
              
                int point_new_nums = data_set.Length + 1;
                int[] point_new_data_temp = new int[point_new_nums];
                data_set.CopyTo(point_new_data_temp, 0);
                point_new_data_temp[data_set.Length] = point;
                data_set = point_new_data_temp;
            }
            //填充点位数量
            screen_point();
            //显示新波形图
           if(shishihuizhi.Checked==true)
            DisplayChart(data_set);
            end1: { }
        }
        //保存ARB文件
        private void save_arb_Click(object sender, EventArgs e)
        {
            if (data_set.Length <= 1)
            {
                MessageBox.Show("波形太短！");
                goto end1;
            }
            string m = "";
            SaveFileDialog saveImageDialog = new SaveFileDialog();
            //saveImageDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "User Files\\";
            saveImageDialog.FileName = "ARB_Out";
            saveImageDialog.Title = "保存ARB";
            saveImageDialog.Filter = @"ARB文件|*.arb";
            if (saveImageDialog.ShowDialog(this) == DialogResult.OK)
            {
                string Path = saveImageDialog.FileName;
                data_to_arb(Path, data_set);
                file_path_see.Text=Path;
            }
        end1: { };
          

        }

        //将文件全部加载到字符串
        string  LoadData(string path)                          //加载path文件，将文件中所有内容形成字符串并返回
        {
            string temp;                                        //创建字符串str，用来存放文件的一行内容
            StringBuilder strcommand =new StringBuilder();
            List<string> StringList = new List<string>();
            //已读的方式打开文件  并创建数据流
            StreamReader arbs = new StreamReader(path, Encoding.GetEncoding("GB2312"));
            while (true)
            {
                temp = arbs.ReadLine();
                if (temp != null)
                {
                    //每行字符串末尾加上换行
                    strcommand.Append(temp + "\n");
                }
                else
                {
                    break;
                }

            }
            arbs.Close();
            return strcommand.ToString();
        }
        //生成发送的Cmmand字符串
        string  SendArbWaveform( string strCommand, string file_name)    //给仪器发送数据
        {
            string str, Count, Length;                                     //由于仪器指令需要，创建字符串str存放仪器指令，Length存放字符串的字符个数，Count存放Length的位数
            int count = 1;                                                      //位数默认值为1
            int length = strCommand.Length;                                //获取字符串字符数
            while (length / 10 >= 1)                                                //计算位数
            {
                length = length / 10;
                count++;
            }
            str = "";
            str+= @"MMEM:DOWN:FNAM ""INT:\User Files\" + file_name + @"""" + "\r\n";                       //将指令合成存放在str中，这条指令的功能是在信号发生器中INT路径下创建同名arb文件。其中path存放的是计算机中arb文件的名字（包括后缀）
            Count =count.ToString();
            Length= strCommand.Length.ToString();
            str+= "MMEM:DOWN:DATA #" + Count + Length + strCommand + "\r\n";
            return str;
        }


        //发送到信号发生器
        private void send_ARB_Device_Click(object sender, EventArgs e)
        {
            //固定按钮
            send_ARB_Device.Enabled = false;
            string start_Text = send_ARB_Device.Text;
            send_ARB_Device.Text = "Seading......";
            try
            { 
                //开始处理
                string file_name = Path.GetFileName(file_path_see.Text);
                string path_name = file_path_see.Text;
                //合成将要发送的数据String
                string data_string = LoadData(path_name);
                //合成发送指令
                string Command = SendArbWaveform(data_string,file_name);
                //执行发送
                FrmMain.form_st.Write(Command, false); 
            }
           
            catch(Exception ex)
            {
                MessageBox.Show("ERROR:"+ex.Message);
                goto end1;
            } 
            MessageBox.Show("发送完成！");
            end1:
            //释放按钮
            send_ARB_Device.Text = start_Text;
            send_ARB_Device.Enabled = true;
        }
        //执行此文件
        private void button1_Click(object sender, EventArgs e)
        {
            string Command="";
            string sign;
            int command_num = 0;
         
            if(rst_checked.Checked==true)
            {
                Command += "*RST\r\n";
                command_num++;
            }
            if (memory_check.Checked == true)
            {
                Command += @"MMEM:LOAD:DATA ""INT:\User Files\" + Path.GetFileName(file_path_see.Text) + @"""" + "\r\n";
                command_num++;
            }
            if (run_check.Checked == true)
            {
                command_num++;
                Command += "FUNCtion ARB \r\n";
                //改变符号
                if (Convert.ToInt32(voltage_shuchu.Value.ToString()) > 0)
                    sign = "+";
                else
                    sign = "";
                Command += "VOLTage " + sign + voltage_shuchu.Value.ToString() + "\r\n";


                //改变符号
                if (Convert.ToInt32(voltage_offset_shuchu.Value.ToString()) > 0)
                    sign = "+";
                else
                    sign = "";
                Command += "VOLTage:OFFSet " + sign + voltage_offset_shuchu.Value.ToString() + "\r\n";


                Command += "FUNC:ARB:SRAT " + arb_srat_shuchu.Text + "\r\n";
                Command += @"FUNCtion:ARBitrary ""INT:\User Files\" + Path.GetFileName(file_path_see.Text) + @"""" + "\r\n";
                Command += "OUTPut " + Output_shuchu.Text + "\r\n";
            }
            if (command_num == 0)
            {
                MessageBox.Show("无勾选项！");
            }
            else
            { 
                FrmMain.form_st.Write(Command, true);
                MessageBox.Show("OK");
            }

        }
        //导入ARB文件（生成文件）
        private void materialButton7_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.CheckPathExists = true;
            openFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "BuiltIn\\";
            openFileDialog1.FileName = "选择ARB文件";
            openFileDialog1.Filter = "波形文件(*.arb)|*.arb";
            openFileDialog1.Title = "打开升级文件";
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                string filePath = openFileDialog1.FileName;
                textBox4.Text = filePath;
            }
        }
        //添加此波形
        private void materialButton5_Click(object sender, EventArgs e)
        {
            int[] this_arb_data = new int[1];
            int range_nums;//循环次数
            if (ARB_RANGE_NUMS.Text == "" || Convert.ToInt32(ARB_RANGE_NUMS.Text) < 0)
            {
                ARB_RANGE_NUMS.Text = "1";
                range_nums = 1;
            }
            else
            {
                range_nums = Convert.ToInt32(ARB_RANGE_NUMS.Text);
            }

            //读出错误
            try
            { 
               
                readarbfile(textBox4.Text, ref run_data);
            }
            catch (Exception ex)
            {
                MessageBox.Show("输入不合法："+ex.Message);
                goto ERROR1;
            }
            finally
            {
                //有无异常都会从这里进行处理 
            }


            for (int i = 0; i < range_nums; i++)
            {
                int point_new_nums = data_set.Length + run_data.Length;
                int[] point_new_data_temp = new int[point_new_nums];
                data_set.CopyTo(point_new_data_temp, 0);
                run_data.CopyTo(point_new_data_temp,data_set.Length);
                data_set = point_new_data_temp;
            }
            
            //填充点位数量
            screen_point();
            //显示新波形图
            if (shishihuizhi.Checked == true)
                DisplayChart(data_set);
        ERROR1: { };
        }
        //自动调整Check按钮
        private void materialCheckbox1_CheckedChanged(object sender, EventArgs e)
        {
            if(materialCheckbox1.Checked==true)
            {
                textBox5.Enabled = false;
            }
            else
            { 
             textBox5.Enabled = true;
            }
        }



        //获取点位数量并打印在
        public void screen_point()
        {
            point_nums.Text = String.Format("{0:d}", data_set.Length);
            
        }
        //清除所有Point
        private void clear_all_points_Click(object sender, EventArgs e)
        {
            data_set=new int[0];
            DisplayChart(data_set);
            screen_point();
        }
        //添加T波形按钮
        private void materialButton3_Click(object sender, EventArgs e)
        {
            int[] this_arb_data = new int[1];
            int range_nums;//循环次数
            if (T_round.Text == "" || Convert.ToInt32(T_round.Text) < 0)
            {

                T_round.Text = "1";
                range_nums = 1;
            }
            else
            {
                range_nums = Convert.ToInt32(T_round.Text);
            }


            try
            {
                if (Convert.ToInt32(T_min.Text) > 32767 || Convert.ToInt32(T_min.Text) < -32767|| Convert.ToInt32(T_max.Text) > 32767 || Convert.ToInt32(T_max.Text) < -32767)
                {
                    MessageBox.Show("输入值在32767和-32767之间");
                    goto ERROR1;
                }
                T_Wave(Convert.ToInt32(T_min.Text), Convert.ToInt32(T_max.Text), Convert.ToInt32(T_up.Text), Convert.ToInt32(T_down.Text), Convert.ToInt32(T_baochi.Text), Convert.ToInt32(T_jiange.Text));
            }
            catch(Exception ex)
            { 
                MessageBox.Show("输入不合法：" + ex.Message);
                goto ERROR1;
            }

            for (int i = 0; i < range_nums; i++)
            {
                int point_new_nums = data_set.Length + +T_wave_data.Length;
                int[] point_new_data_temp = new int[point_new_nums];
                data_set.CopyTo(point_new_data_temp, 0);
                T_wave_data.CopyTo(point_new_data_temp, data_set.Length);
                // point_new_data_temp[data_set.Length] = point;
                data_set = point_new_data_temp;
            }

            //填充点位数量
            screen_point();
            //显示新波形图
            DisplayChart(data_set);
        ERROR1: { };
        }

        //删除一个点
        private void materialButton2_Click(object sender, EventArgs e)
        {
            int range_nums=1;//循环次数
            //if (Point_range_time.Text == "" || Convert.ToInt32(Point_range_time.Text) < 0)
            //{
            //    Point_range_time.Text = "1";
            //    range_nums = 1;
            //}
            //else
            //{
            //    range_nums = Convert.ToInt32(Point_range_time.Text);
            //}
            //获取新点位值
            //if (point_textbox.Text == "")
            //{
            //    MessageBox.Show("Point NULL");
            //    goto end1;
            //}

            //int point = Convert.ToInt32(point_textbox.Text);
            for (int i = 0; i < range_nums; i++)
            { 
                int point_new_nums;
                //看是否为最后一个点
                if (data_set.Length >= 1)
                    point_new_nums = data_set.Length - 1;
                else
                    goto end1;

                int[] point_new_data_temp = new int[point_new_nums];
                Array.Copy(data_set, 0, point_new_data_temp, 0,point_new_data_temp.Length); 
                data_set = point_new_data_temp;
            }
            //填充点位数量
            screen_point();
            //显示新波形图
            if (shishihuizhi.Checked == true)
                DisplayChart(data_set);
        end1: { }
        }
        //删除多个点
        private void materialButton1_Click(object sender, EventArgs e)
        {
            int range_nums = 1;//循环次数
            if (Point_range_time.Text == "" || Convert.ToInt32(Point_range_time.Text) < 0)
            {
                Point_range_time.Text = "1";
                range_nums = 1;
            }
            else
            {
                range_nums = Convert.ToInt32(Point_range_time.Text);
            }
 

            //int point = Convert.ToInt32(point_textbox.Text);
            for (int i = 0; i < range_nums; i++)
            {
                int point_new_nums;
                //看是否为最后一个点
                if (data_set.Length >= 1)
                    point_new_nums = data_set.Length - 1;
                else
                    goto end1;

                int[] point_new_data_temp = new int[point_new_nums];
                Array.Copy(data_set, 0, point_new_data_temp, 0, point_new_data_temp.Length);
                data_set = point_new_data_temp;
            }
            //填充点位数量
            screen_point();
            //显示新波形图
            if (shishihuizhi.Checked == true)
                DisplayChart(data_set);
        end1: { }
        }

        //添加SIN波形
        private void materialButton4_Click(object sender, EventArgs e)
        {
            string Command = "";
            string sign;
            Command += "*RST\r\n";
            Command += "FUNCtion SIN\r\n";
            Command += "FREQuency "+ sin_freq.Text+"\r\n";
            Command += "VOLTage:HIGH " + sin_volt_high.Text + "\r\n";
            Command += "VOLTage:LOW " + sin_volt_low.Text + "\r\n";
            Command += "OUTPut ON\r\n";
            Command += "PHASe " + sin_phase.Text + "\r\n";
            FrmMain.form_st.Write(Command, true);
        }
        //添加方波
        private void fang_Button_Click(object sender, EventArgs e)
        {
            string Command = "";
            string sign;
            Command += "*RST\r\n";
            Command += "FUNCtion SQU\r\n";
            Command += "FUNC:SQU:DCYC " +fang_dcyc.Text+ "\r\n";
            Command += "FREQ " + fang_freq.Text + "\r\n";
            Command += "VOLT:HIGH " + fang_volt_high.Text + "\r\n";   
            Command += "VOLT:LOW " + fang_volt_low.Text + "\r\n";
            Command += "OUTPut 1\r\n";
            FrmMain.form_st.Write(Command, true);
        }
        //添加锯齿波
        private void juchi_Button_Click(object sender, EventArgs e)
        {
            string Command = "";
            string sign;
            Command += "*RST\r\n";
            Command += "FUNCtion RAMP\r\n";
            Command += "FUNCtion:RAMP:SYMMetry " + juchi_symm.Text + "\r\n";
            Command += "FREQ " + juchi_freq.Text + "\r\n";
            Command += "VOLTage " + juchi_volt.Text + "\r\n";
            Command += "VOLTage:OFFSet " + juchi_volt_offset.Text + "\r\n";
            Command += "OUTPut 1\r\n";
            FrmMain.form_st.Write(Command, true);
        }
        //添加脉冲波
        private void maichong_Button_Click(object sender, EventArgs e)
        {
            string Command = "";
            string sign;
            Command += "*RST\r\n";
            Command += "FUNCtion PULS\r\n";
            Command += "FUNC:PULS:TRAN:LEAD " + maichong_lead.Text + "\r\n";
            Command += "FUNC:PULS:TRAN:TRA " + maichong_trai.Text + "\r\n";
            Command += "FUNC:PULS:WIDT " + maichong_widt.Text + "\r\n";
            Command += "FREQ " + maichong_freq.Text + "\r\n";
            Command += "VOLT " + maichong_volt.Text + "\r\n";
            Command += "OUTPut ON\r\n";
            FrmMain.form_st.Write(Command, true);
        }
        //添加频率波
        private void frelist_Button_Click(object sender, EventArgs e)
        {
            string Command = "";
            string sign;
            Command += "*RST\r\n";
            Command += "FUNCtion SQU\r\n";
            Command += "TRIGger:SOURce IMMediate\r\n";
            Command += "FREQuency:MODE LIST\r\n";
            Command += "LIST:DWELl " + frelist_dwel.Text + "\r\n";
            Command += "LIST:FREQuency " + frelist_freq.Text + "\r\n";
            Command += "VOLTage " + frelist_volt.Text + "\r\n";
            Command += "OUTPut 1\r\n";
            FrmMain.form_st.Write(Command, true);

        }

        private void materialButton4_Click_1(object sender, EventArgs e)
        {
            double get_time = 0;
            //秒级
            try
            { 
                 get_time = 1 / Convert.ToDouble(sample_rate.Text); 
            }
            catch(Exception ex)
            {
                goto end1;
            }

            if (get_time == 0)
            {
                goto end1;
            }
         
            if (get_time >= 0.1)
            {
                point_time.Text = String.Format("{0:f}", get_time) + "s";
            }
            else if ((get_time * 1000) >= 0.1)
            {
                point_time.Text = String.Format("{0:f}", get_time*1000) + "ms";
            }
            else if ((get_time * 1000000) >= 0.1)
            {
                point_time.Text = String.Format("{0:f}", get_time * 1000000) + "us";
            }
            else
            {
                point_time.Text = String.Format("{0:f}", get_time * 1000000000) + "ns";
            }
            end1: { };
        }

        private void sample_rate_TextChanged(object sender, EventArgs e)
        {
            materialButton4_Click_1(null,null);
        }
        //实时绘制check
        private void shishihuizhi_CheckedChanged(object sender, EventArgs e)
        {
            if(shishihuizhi.Checked==true)
              {
                DisplayChart(data_set);
            }
        }
        //刷新
        private void huizhi_shuaxin_Click(object sender, EventArgs e)
        {
            if(tabcontrl.SelectedIndex==1)
                DisplayChart(run_data);
            else
                DisplayChart(data_set);
        }
       //采样使能
        private void caiyangxianshi_check_CheckedChanged(object sender, EventArgs e)
        {
            caiyangshu.Enabled = !caiyangxianshi_check.Checked;
        }

    

        public static int [] T_wave_data=new int[0];
        public void T_Wave(int T_mins,int T_maxs,int T_ups,int T_downs,int T_baochis,int T_jianges)
        {
            T_wave_data = new int[0];
            int point_nums_all=T_ups+T_downs+T_baochis+T_jianges;
            T_wave_data=new int [point_nums_all];
            //记录到第几个点
            int num_this = 0;
            //上升时
            for (int i = 0; i <= T_ups; i++)
            { 
            T_wave_data[num_this]=((T_maxs-T_mins)/T_ups)*i+T_mins;
                num_this++;
            }
            //保持时
            for (int i = 0; i < T_baochis-1; i++)
            { 
                T_wave_data[num_this] = T_maxs;
                num_this++;
            
            }
            //下降时
            for (int i = 0; i <= T_downs; i++)
            {

                T_wave_data[num_this] = ((T_mins-T_maxs)/ T_downs) * i + T_maxs ;
                num_this++;

            }
            //周期间隔
            for (int i = 0; i < T_jianges-1; i++)
            {

                T_wave_data[num_this] = T_mins;
                num_this++;

            }



        }

        public static int[] T_Rise_data = new int[0];
        //上升直线
        public void T_Rise(int T_mins_int, int T_maxs_int, int T_ups_int,int flag)
        {
            float T_mins = (float)T_mins_int;
            float T_maxs = (float)T_maxs_int;
            float T_ups=(float)T_ups_int;
            T_Rise_data = new int[0];
            int point_nums_all =(int) (T_ups+1);
            int point_num_temp = (int)(31 - T_ups);
            if (point_nums_all > 32)
            {
                T_Rise_data = new int[point_nums_all];
                //记录到第几个点
                int num_this = 0;
                //上升时
                for (int i = 0; i <= T_ups; i++)
                {
                    T_Rise_data[num_this] = (int)(((T_maxs - T_mins) / T_ups) * i + T_mins);
                    num_this++;
                }
            }
            else if (flag==0)
            {
                T_Rise_data = new int[32];
                int num_this = 0;
                for (int i = 0; i < point_nums_all; i++)
                {
                    T_Rise_data[num_this] =(int)(((T_maxs - T_mins) / T_ups) * i + T_mins);
                    num_this++;
                }
           
                for (int i = 0; i < point_num_temp; i++)
                {
                    T_Rise_data[num_this] = (int)T_maxs;
                    num_this++;
                }

            }
            else {
                T_Rise_data = new int[32];
                int num_this = 0;

                for (int i = 0; i < point_num_temp; i++)
                {
                    T_Rise_data[num_this] = (int)T_mins;
                    num_this++;
                }

                for (int i = 0; i < point_nums_all; i++)
                {
                    T_Rise_data[num_this] =(int)( ((T_maxs - T_mins) / T_ups) * i + T_mins);
                    num_this++;
                }

       


            }
        }
        public static int[] T_Line_data = new int[0];
        //直线
        public void T_Line(int T_mins, int T_ups)
        {
            T_Line_data = new int[0];
            int point_nums_all = T_ups;
            T_Line_data = new int[point_nums_all];
            //记录到第几个点
            int num_this = 0;
            //上升时
            for (int i = 0; i <T_ups; i++)
            {
                T_Line_data[num_this] = T_mins;
                num_this++;
            }
        }
        //---------------------------------AUTO TEST--------------------------------
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

        [System.Runtime.InteropServices.DllImport("user32.dll ")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int wndproc);
        [System.Runtime.InteropServices.DllImport("user32.dll ")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        public const int GWL_STYLE = -16;
        public const int WS_DISABLED = 0x8000000;
        //disable后颜色没有变化
        public static void SetControlEnabled(Control c, bool enabled)
        {
            if (enabled)
            { SetWindowLong(c.Handle, GWL_STYLE, (~WS_DISABLED) & GetWindowLong(c.Handle, GWL_STYLE)); }
            else
            { SetWindowLong(c.Handle, GWL_STYLE, WS_DISABLED + GetWindowLong(c.Handle, GWL_STYLE)); }
        }
        public static bool groupBox13_enable = true;
        //介入停止测试
        public static bool runing = false;

   

        //停止自动测试
        public void auto_end_test_Click(object sender, EventArgs e)
        {
            if (groupBox13_enable == false)
            {
                groupBox13_enable = true;
                SetControlEnabled(groupBox13, true); 
            }
            runing = false;
            auto_start_test.Enabled = true;
            auto_end_test.Enabled = false;
            label70.Text = "本测试进度:";
            Start_caiji = false;
            usart_text.Visible = false;
        }

        private void auto_com_connect_Click(object sender, EventArgs e)
        {
            if (auto_com_connect.Text == "连接")
            {
                controller.OpenSerialPort(comListCbx.Text, baudRateCbx.Text,
                    dataBitsCbx.Text, stopBitsCbx.Text, parityCbx.Text,
                    handshakingcbx.Text);
            }
            else
            {
                controller.CloseSerialPort();
            }

        }

        private void auto_com_fresh_Click(object sender, EventArgs e)
        {
            comListCbx.Items.Clear();
            //Com Ports
            string[] ArrayComPortsNames = SerialPort.GetPortNames();
            if (ArrayComPortsNames.Length == 0)
            {
                auto_com_connect.Enabled = false;
            }
            else
            {
                Array.Sort(ArrayComPortsNames);
                for (int i = 0; i < ArrayComPortsNames.Length; i++)
                {
                    comListCbx.Items.Add(ArrayComPortsNames[i]);
                }
                comListCbx.Text = ArrayComPortsNames[0];
                auto_com_connect.Enabled = true;       
            }
        }

        //执行此文件
        public void auto_start_wave()
        {
            string Command = "";
            string sign;
            Command += "*RST\r\n";
            Command += @"MMEM:LOAD:DATA ""INT:\User Files\" + "test.arb" + @"""" + "\r\n";
            Command += "FUNCtion ARB \r\n";
            //改变符号
            if (Convert.ToInt32(voltage_shuchu.Value.ToString()) > 0)
                sign = "+";
            else
                sign = "";
            Command += "VOLTage " + sign + voltage_shuchu.Value.ToString() + "\r\n";
            //改变符号
            if (Convert.ToInt32(voltage_offset_shuchu.Value.ToString()) > 0)
                sign = "+";
            else
                sign = "";
            Command += "VOLTage:OFFSet " + sign + voltage_offset_shuchu.Value.ToString() + "\r\n";
            Command += "FUNC:ARB:SRAT " + arb_srat_shuchu.Text + "\r\n";
            Command += @"FUNCtion:ARBitrary ""INT:\User Files\" + "test.arb" + @"""" + "\r\n";
            Command += "OUTPut " + Output_shuchu.Text + "\r\n";
            FrmMain.form_st.Write(Command, true);
        }
        //清理文件
        public void ClearTxt(String txtPath)
        {
            if (System.IO.File.Exists(txtPath))
            {
                String appDir = System.AppDomain.CurrentDomain.BaseDirectory  + txtPath;
                FileStream stream = File.Open(appDir, FileMode.OpenOrCreate, FileAccess.Write);
                stream.Seek(0, SeekOrigin.Begin);
                stream.SetLength(0);
                stream.Close();
            }
        }
        //查看log
        private void materialButton4_Click_2(object sender, EventArgs e)
        {
            string v_OpenFolderPath = @"Test result";
            System.Diagnostics.Process.Start("explorer.exe", v_OpenFolderPath);
        }
        //auto 采样率->ms
        private void auto_sample_TextChanged(object sender, EventArgs e)
        {
            double get_time = 0;
            //秒级
            try
            {
                get_time = 1 / Convert.ToDouble(auto_sample.Text);
            }
            catch (Exception ex)
            {
                goto end1;
            }

            if (get_time == 0)
            {
                goto end1;
            }

            if (get_time >= 0.1)
            {
                textBox1.Text = String.Format("{0:f}", get_time) + "s";
            }
            else if ((get_time * 1000) >= 0.1)
            {
                textBox1.Text = String.Format("{0:f}", get_time * 1000) + "ms";
            }
            else if ((get_time * 1000000) >= 0.1)
            {
                textBox1.Text = String.Format("{0:f}", get_time * 1000000) + "us";
            }
            else
            {
                textBox1.Text = String.Format("{0:f}", get_time * 1000000000) + "ns";
            }
        end1: { };
        }


        //取时间
        private string get_jiange(string sample,int num)
        {
            double get_time = 0;
            //秒级
            try
            {
                get_time = 1 / Convert.ToDouble(sample);
            }
            catch (Exception ex)
            {
                goto end1;
            }

            if (get_time == 0)
            {
                goto end1;
            }

            if (get_time >= 0.1)
            {
                return String.Format("{0:f}", get_time*num) + "s";
            }
            else if ((get_time * 1000) >= 0.1)
            {
                return String.Format("{0:f}", get_time * 1000*num) + "ms";
            }
            else if ((get_time * 1000000) >= 0.1)
            {
                return String.Format("{0:f}", get_time * 1000000*num) + "us";
            }
            else
            {
                return String.Format("{0:f}", get_time * 1000000000*num) + "ns";
            }
        end1: { };
            return null;
        }
        //取时间
        private string get_jiange(string sample, string num_string)
        {
            double get_time = 0;
            int num= Convert.ToInt32(num_string); 
            //秒级
            try
            {
                get_time = 1 / Convert.ToDouble(sample);
            }
            catch (Exception ex)
            {
                goto end1;
            }

            if (get_time == 0)
            {
                goto end1;
            }

            if (get_time >= 0.1)
            {
                return String.Format("{0:f}", get_time * num) + "s";
            }
            else if ((get_time * 1000) >= 0.1)
            {
                return String.Format("{0:f}", get_time * 1000 * num) + "ms";
            }
            else if ((get_time * 1000000) >= 0.1)
            {
                return String.Format("{0:f}", get_time * 1000000 * num) + "us";
            }
            else
            {
                return String.Format("{0:f}", get_time * 1000000000 * num) + "ns";
            }
        end1: { };
            return null;
        }

        private void auto_clear_Click(object sender, EventArgs e)
        {
            all_pass_num = 0;
            all_test_num = 0;
            all_error_num = 0;
            auto_error_num_textbox.Text = all_error_num.ToString() + "/" + all_pass_num.ToString();
        }
        //切换tab
        private void tabcontrl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabcontrl.SelectedIndex != 3)
            {
                usart_text.Visible = false;
                Display_check.Checked = true;
            }
            else
            {
                Display_check.Checked = false;
            }
        }

        private void auto_log_VisibleChanged(object sender, EventArgs e)
        {
            if (usart_text.Visible)
            {
                materialButton6.Text = "关闭串口显示";
            }
            else
            {
                materialButton6.Text = "打开串口显示";
            }

        }

        private void materialButton6_Click(object sender, EventArgs e)
        {
            usart_text.Visible = !usart_text.Visible;
        }
        //增加或减少
        public class arb_struct
        {
            public string Arb_file;
            public int  repeat; 
        }
        public static List<arb_struct> ARB_list = new List<arb_struct>();

        private void Remove_Click(object sender, EventArgs e)
        {
            if (ARB_list.FindIndex(t => t.Arb_file == seq_arb_filename.Text) != -1)
            {
                ARB_list.RemoveAt(ARB_list.FindIndex(t => t.Arb_file == seq_arb_filename.Text));
            }
            list_text();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            ARB_list.Clear();
            list_text();
        }

        private void Add_Click(object sender, EventArgs e)
        {
            try
            {
                if (Convert.ToInt32(seq_repeat.Text) >= 1)
                {
                    ARB_list.Add(new arb_struct { Arb_file = seq_arb_filename.Text, repeat = Convert.ToInt32(seq_repeat.Text) });
                }
            }
            catch {
                MessageBox.Show("input error");
            }
            list_text();
        }

        public void list_text()
        {
            string temp = "";

            foreach (arb_struct i in ARB_list)
            {
                temp += "文件名：" + i.Arb_file+ "\r\n";
                temp += "循环次数：" + i.repeat.ToString() + "\r\n";
                temp += "---------------------------------------------\r\n";
            }
            textBox3.Text = temp;

        }
        //打开ARB文件，seq
        private void materialButton9_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.CheckPathExists = true;

            openFileDialog1.FileName = "选择ARB文件";
            openFileDialog1.Filter = "波形文件(*.arb)|*.arb";
            openFileDialog1.Title = "打开升级文件";
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                string filePath = openFileDialog1.FileName;
                seq_arb_filename.Text = filePath;

            }
        }

        //预览序列
        private void button3_Click(object sender, EventArgs e)
        {
           List<int> data=new List<int>();
            int[] data_temp =new int[0];
            foreach (arb_struct i in ARB_list)
            {
                readarbfile(i.Arb_file, ref data_temp);
                for (int arbs = 0; arbs < i.repeat; arbs++)
                {
                    data.AddRange(data_temp);
                }
            }
            DisplayChart(data.ToArray());
        }

        //将文件全部加载到字符串
        string LoadData_seq()                          //加载path文件，将文件中所有内容形成字符串并返回
        {                                     
            string strcommand = "";
            //加入testseq
            strcommand += @""""+ @"testSeq" + @"""";
            foreach (arb_struct i in ARB_list)
            {
                string file_name = Path.GetFileName(i.Arb_file);
               strcommand += @",""INT:\User Files\" + file_name + @""","+ i.repeat.ToString()+ ",repeat,maintain,10";
            }

            return strcommand;
        }
        string LoadData_seq(List<arb_struct> arb_list2)                          //加载path文件，将文件中所有内容形成字符串并返回
        {
            string strcommand = "";
            //加入testseq
            strcommand += @"""" + @"testSeq" + @"""";
            foreach (arb_struct i in arb_list2)
            {
                string file_name = Path.GetFileName(i.Arb_file);
                strcommand += @",""INT:\User Files\" + file_name + @"""," + i.repeat.ToString() + ",repeat,maintain,10";
            }

            return strcommand;
        }

        //生成发送的Cmmand字符串
        string SendArbWaveform_seq(string Command)    //给仪器发送数据
        {
            string strcommand = "";
            int count = 1;                                                      //位数默认值为1
            int length = Command.Length;                                //获取字符串字符数
            while (length / 10 >= 1)                                                //计算位数
            {
                length = length / 10;
                count++;
            }
            string Count = count.ToString();
            string Length = Command.Length.ToString();
             strcommand += "DATA:SEQ #" + Count + Length+Command;

        
            return strcommand;
        }

        //seq start wave
        List<string> send_list = new List<string>();
        private void button4_Click(object sender, EventArgs e)
        {
            string Command = "";
            try
            {
                //去掉重复文件
                send_list.Clear();
                foreach (arb_struct i in ARB_list)
                {
                    if (send_list.FindIndex(t => t == i.Arb_file) == -1)
                    {
                        send_list.Add(i.Arb_file);
                    }
                }
                //发送所有有效文件
                foreach (string i in send_list)
                {//开始处理
                    string file_name = Path.GetFileName(i);
                    string path_name = i;
                    //合成将要发送的数据String
                    string data_string = LoadData(path_name);
                    //合成发送指令
                    Command = SendArbWaveform(data_string, file_name);
                    //执行发送
                    FrmMain.form_st.Write(Command, false);
                }
                //复位并将文件加载到存储
                FrmMain.form_st.Write("*CLS", false);
                FrmMain.form_st.Write("*RST", false);
                //FrmMain.form_st.Write("DATA: VOL: CLE", false);

                foreach (string i in send_list)
                {
                    string file_name = Path.GetFileName(i);
                    Command = @"MMEM:LOAD:DATA ""INT:\User Files\" + file_name + @"""";
                    FrmMain.form_st.Write(Command, false);
                }
                Command = SendArbWaveform_seq(LoadData_seq());
                FrmMain.form_st.Write(Command, false);
               // 运行此序列
                Command = "FUNCtion ARB \r\n";
                Command += @"FUNCtion:ARBitrary ""testSeq" + @"""" + "\r\n";    
                Command += "VOLT " + seq_volt_text.Text + "\r\n";
                Command += "VOLTage:OFFSet " + seq_offset_text.Text + "\r\n";
                Command += "OUTPut " + Output_shuchu.Text + "\r\n";
                //MessageBox.Show(strcommand);
                FrmMain.form_st.Write(Command, false);
            }

            catch (Exception ex)
            {
                MessageBox.Show("ERROR:" + ex.Message);
                goto end1;
            }


        end1: { }
        }

        //保存文件true为追加
        public void SaveFile(string str, String txtPath, bool saOrAp)
        {

            String appDir = System.AppDomain.CurrentDomain.BaseDirectory  + txtPath;
            StreamWriter sw = new StreamWriter(appDir, saOrAp);//saOrAp表示覆盖或者是追加  
            sw.WriteLine(str);
            sw.Close();
        }


        //单个ARB解决测试
        public void short_test()
        {

            if (groupBox13_enable == true)
            {
                groupBox13_enable = false;
                SetControlEnabled(groupBox13, false);
            }
            usart_text.Visible = true;
            int zhouqi_num = 0;
            all_pass_num = 0;
            all_test_num = 0;
            all_error_num = 0;

            auto_start_test.Enabled = false;
            auto_end_test.Enabled = true;
            //正在运行标志位使能
            runing = true;
            if (System.IO.File.Exists(@"Test result\测试结果.txt"))
            {
                DialogResult dr = MessageBox.Show("存在已有的“测试结果.txt”\n是：清楚已有测试结果\n否：在已有测试上追加", "警告", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)//调用messagebox 弹出是、否 按钮对话选择框
                {
                    ClearTxt(@"Test result\测试结果.txt");
                    ClearTxt(@"Test result\问题记录.txt");

                }
                else if (dr == DialogResult.No)
                {
                    //如果选择“否”，则执行以下代码
                }
                else
                {
                    goto end2;
                }

            }
            //判断是加还是减
            int time_temp = Convert.ToInt32(auto_t_time_end.Text) > Convert.ToInt32(auto_t_time_start.Text) ? Math.Abs(Convert.ToInt32(auto_t_fudu.Text)) : (0 - Math.Abs(Convert.ToInt32(auto_t_fudu.Text)));
            for (int time = Convert.ToInt32(auto_t_time_start.Text); time <= Convert.ToInt32(auto_t_time_end.Text); time = time + time_temp)
            {

                if (time > Convert.ToInt32(auto_t_time_end.Text))
                {
                    time = Convert.ToInt32(auto_t_time_end.Text);
                }
                //合成所需波形
                int[] this_arb_data = new int[0];
                int range_nums;//循环次数
                if (T_round.Text == "" || Convert.ToInt32(auto_t_range.Text) < 0)
                {

                    T_round.Text = "1";
                    range_nums = 1;
                }
                else
                {
                    range_nums = Convert.ToInt32(auto_t_range.Text);
                }


                try
                {
                    if (Convert.ToInt32(auto_t_min.Text) > 32767 || Convert.ToInt32(auto_t_min.Text) < -32767 || Convert.ToInt32(auto_t_max.Text) > 32767 || Convert.ToInt32(auto_t_max.Text) < -32767)
                    {
                        MessageBox.Show("输入值在32767和-32767之间");
                        goto ERROR1;
                    }
                    if (auto_t_diaodian_check.Checked == true)
                        auto_t_downtime.Text = time.ToString();
                    else if (auto_t_shangdian_check.Checked == true)
                        auto_t_uptime.Text = time.ToString();
                    else if (auto_t_didianping_check.Checked == true)
                        auto_t_boxingjiange.Text = time.ToString();
                    else
                    {
                        MessageBox.Show("请选择测试项目！");
                        goto end1;
                    }
                    T_Wave(Convert.ToInt32(auto_t_min.Text), Convert.ToInt32(auto_t_max.Text), Convert.ToInt32(auto_t_uptime.Text), Convert.ToInt32(auto_t_downtime.Text), Convert.ToInt32(auto_t_baochi.Text), Convert.ToInt32(auto_t_boxingjiange.Text));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("输入不合法：" + ex.Message);
                    goto ERROR1;
                }

                for (int i = 0; i < range_nums; i++)
                {
                    int point_new_nums = this_arb_data.Length + +T_wave_data.Length;
                    int[] point_new_data_temp = new int[point_new_nums];
                    this_arb_data.CopyTo(point_new_data_temp, 0);
                    T_wave_data.CopyTo(point_new_data_temp, data_set.Length);
                    // point_new_data_temp[data_set.Length] = point;
                    this_arb_data = point_new_data_temp;
                }


                //显示新波形图
                DisplayChart(this_arb_data);
                //将data转成ARB

                arbs_set.Clear();
                arbs_set.Add("Copyright: Agilent Technologies, 2010");
                arbs_set.Add("File Format:" + "1.0");
                arbs_set.Add("Channel Count:" + "1");
                arbs_set.Add("Sample Rate:" + auto_sample.Text);
                arbs_set.Add("High Level:" + auto_high_level.Text);
                arbs_set.Add("Low Level:" + auto_low_level.Text);
                arbs_set.Add("Filter:" + @"""" + auto_fliter.Text + @"""");
                arbs_set.Add("Data Points:" + string.Format("{0:D}", this_arb_data.Length));
                arbs_set.Add("Data:");
                for (int i = 0; i < this_arb_data.Length; i++)
                {
                    arbs_set.Add(string.Format("{0:D}", this_arb_data[i]));
                }
                //停止采样
                Start_caiji = false;



                //写入到信号发生器 
                string temp;                                        //创建字符串str，用来存放文件的一行内容
                string strcommand = "";
                //已读的方式打开文件  并创建数据流
                for (int i = 0; i < arbs_set.Count; i++)
                {
                    strcommand += (arbs_set[i] + "\n");
                }
                //合成发送指令
                string Command = SendArbWaveform(strcommand, "test.arb");
                //执行发送
                FrmMain.form_st.Write(Command, false);
                Delay(1000);
                //执行
                auto_start_wave();
                int delay_time = 0;
                if (auto_t_qiehuanjiange_danwei.FindString(auto_t_qiehuanjiange_danwei.Text) == 1)
                {
                    delay_time = Convert.ToInt32(auto_t_qiehuanjiange.Text) * 60;
                }
                else
                {
                    delay_time = Convert.ToInt32(auto_t_qiehuanjiange.Text);
                }
                label70.Text = "测试" + zhouqi_num.ToString() + "进度:";
                auto_progressBar1.Maximum = delay_time - 1;
                auto_progressBar1.Minimum = 0;
                for (int i = 0; i < delay_time; i++)
                {

                    Delay(1000);
                    if (i == 10)
                    {
                        Start_caiji = true;
                        error_buff = 2;
                    }
                    if (runing == false)
                    {
                        goto end1;
                    }
                    auto_progressBar1.Value = i;
                    auto_progressBar1.Refresh();

                }

                string result_string = "\r\n\r\n周期:" + zhouqi_num.ToString() + "测试结果：\r\n";
                result_string += "Time:" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff:ffffff") + "\r\n";
                result_string += "Test:" + all_test_num.ToString() + "\t";
                result_string += "Pass:" + all_pass_num.ToString() + "\t";
                result_string += "Fail:" + all_error_num.ToString() + "\r\n";
                result_string += "上升时间：" + get_jiange(auto_sample.Text, auto_t_uptime.Text) + "\t";
                result_string += "下降时间：" + get_jiange(auto_sample.Text, auto_t_downtime.Text) + "\r\n";
                result_string += "最小值：" + auto_t_min.Text + "\t";
                result_string += "最大值：" + auto_t_max.Text + "\r\n";
                result_string += "保持时间：" + get_jiange(auto_sample.Text, auto_t_baochi.Text) + "\t";
                result_string += "周期间隔：" + get_jiange(auto_sample.Text, auto_t_boxingjiange.Text) + "\t";
                SaveFile(result_string, @"Test result\测试结果.txt", true);
                auto_clear_Click(null, null);
                zhouqi_num++;
            }

        //结束测试

        ERROR1: { };
        end1:
            //执行停止
            FrmMain.form_st.Write("OUTP OFF", false);
        end2:
            auto_end_test_Click(null, null);


            MessageBox.Show("测试结束");
            //查看log
            materialButton4_Click_2(null, null);
        }
        //获取变量名
        public static string GetVarName(System.Linq.Expressions.Expression exp )
        {
            return ((System.Linq.Expressions.MemberExpression)exp).Member.Name;
        }

        //需要seq解决测试
        public void long_test()
        {
            if (groupBox13_enable == true)
            {
                groupBox13_enable = false;
                SetControlEnabled(groupBox13, false);
            }
            usart_text.Visible = true;
            int zhouqi_num = 0;
            all_pass_num = 0;
            all_test_num = 0;
            all_error_num = 0;

            auto_start_test.Enabled = false;
            auto_end_test.Enabled = true;
            //正在运行标志位使能
            runing = true;

            if (System.IO.File.Exists(@"Test result\测试结果.txt"))
            {
                DialogResult dr = MessageBox.Show("存在已有的“测试结果.txt”\n是：清楚已有测试结果\n否：在已有测试上追加", "警告", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)//调用messagebox 弹出是、否 按钮对话选择框
                {
                    ClearTxt(@"Test result\测试结果.txt");
                    ClearTxt(@"Test result\问题记录.txt");

                }
                else if (dr == DialogResult.No)
                {
                    //如果选择“否”，则执行以下代码
                }
                else
                {
                    goto end2;
                }

            }
            //判断是加还是减
            int time_temp = Convert.ToInt32(auto_t_time_end.Text) > Convert.ToInt32(auto_t_time_start.Text) ? Math.Abs(Convert.ToInt32(auto_t_fudu.Text)) : (0 - Math.Abs(Convert.ToInt32(auto_t_fudu.Text)));
            for (int time = Convert.ToInt32(auto_t_time_start.Text); time <= Convert.ToInt32(auto_t_time_end.Text); time = time + time_temp)
            {

                if (time > Convert.ToInt32(auto_t_time_end.Text))
                {
                    time = Convert.ToInt32(auto_t_time_end.Text);
                }
                //合成所需波形

                //上升和下降波形
                int[] up_arb = new int[0];
                int[] down_arb = new int[0];
                int[] jiange_arb=new int[0];
                //保持一个周期波形
                int[] baochi_arb=new int[0];
                int baochi_arb_range = 0;
                List<int> baochi_arb_list = new List<int>();

                try
                {
                    if (Convert.ToInt32(auto_t_min.Text) > 32767 || Convert.ToInt32(auto_t_min.Text) < -32767 || Convert.ToInt32(auto_t_max.Text) > 32767 || Convert.ToInt32(auto_t_max.Text) < -32767)
                    {
                        MessageBox.Show("输入值在32767和-32767之间");
                        goto ERROR1;
                    }
                    if (auto_t_diaodian_check.Checked == true)
                        auto_t_downtime.Text = time.ToString();
                    else if (auto_t_shangdian_check.Checked == true)
                        auto_t_uptime.Text = time.ToString();
                    else if (auto_t_didianping_check.Checked == true)
                        auto_t_boxingjiange.Text = time.ToString();
                    else
                    {
                        MessageBox.Show("请选择测试项目！");
                        goto end1;
                    }
                    //合成上升下降和间隔ARB
                    T_Rise(Convert.ToInt32(auto_t_min.Text), Convert.ToInt32(auto_t_max.Text), Convert.ToInt32(auto_t_uptime.Text),0);
                    up_arb = T_Rise_data;
                    data_to_arb_auto(@"User Files\" + "up_arb.arb", up_arb);
                    T_Rise(Convert.ToInt32(auto_t_max.Text),Convert.ToInt32(auto_t_min.Text) , Convert.ToInt32(auto_t_downtime.Text),1);
                    down_arb = T_Rise_data;
                    data_to_arb_auto(@"User Files\" + "down_arb.arb", down_arb);
                    T_Line(Convert.ToInt32(auto_t_min.Text), Convert.ToInt32(auto_t_boxingjiange.Text));
                    jiange_arb = T_Line_data;
                    data_to_arb_auto(@"User Files\" + "jiange_arb.arb", jiange_arb);


                    //合成保持ARB
                    int arb_size = Convert.ToInt32(auto_t_baochi.Text) / 50;
                    textBox2.Text = arb_size.ToString();
                    T_Line(Convert.ToInt32(auto_t_max.Text), arb_size);
                    baochi_arb = T_Line_data;
                    data_to_arb_auto(@"User Files\" + "baochi_arb.arb", baochi_arb);
                    baochi_arb_range = (int)(Convert.ToInt32(auto_t_baochi.Text) / arb_size) + 1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("输入不合法：" + ex.Message);
                    goto ERROR1;
                }

                //停止采样
                Start_caiji = false;
                //写入到信号发生器 
                List<arb_struct> this_list=new List<arb_struct>();
                arb_struct arb_struct_temp=new arb_struct();
                arb_struct arb_struct_temp1 = new arb_struct();
                arb_struct arb_struct_temp2 = new arb_struct();
                arb_struct arb_struct_temp3 = new arb_struct();

                //上升时间
                arb_struct_temp.Arb_file= @"User Files\up_arb.arb";
                arb_struct_temp.repeat = 1;
                this_list.Add(arb_struct_temp);
                //保持时间
                arb_struct_temp1.Arb_file = @"User Files\baochi_arb.arb";
                arb_struct_temp1.repeat = baochi_arb_range;
                this_list.Add(arb_struct_temp1);
                //下降时间
                arb_struct_temp2.Arb_file = @"User Files\down_arb.arb";
                arb_struct_temp2.repeat = 1;
                this_list.Add(arb_struct_temp2);
                //周期间隔
                arb_struct_temp3.Arb_file = @"User Files\jiange_arb.arb";
                arb_struct_temp3.repeat = 1;
                this_list.Add(arb_struct_temp3);

                display_seq(this_list);
                //执行
                start_seq(this_list, auto_sample.Text, auto_high_level.Text, auto_low_level.Text, "0");


                int delay_time = 0;
                if (auto_t_qiehuanjiange_danwei.FindString(auto_t_qiehuanjiange_danwei.Text) == 1)
                {
                    delay_time = Convert.ToInt32(auto_t_qiehuanjiange.Text) * 60;
                }
                else
                {
                    delay_time = Convert.ToInt32(auto_t_qiehuanjiange.Text);
                }
                label70.Text = "测试" + zhouqi_num.ToString() + "进度:";
                auto_progressBar1.Maximum = delay_time - 1;
                auto_progressBar1.Minimum = 0;
                for (int i = 0; i < delay_time; i++)
                {

                    Delay(1000);
                    if (i == 10)
                    {
                        Start_caiji = true;
                        error_buff = 2;
                    }
                    if (runing == false)
                    {
                        goto end1;
                    }
                    auto_progressBar1.Value = i;
                    auto_progressBar1.Refresh();

                }

                string result_string = "\r\n\r\n周期:" + zhouqi_num.ToString() + "测试结果：\r\n";
                result_string += "Time:" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff:ffffff") + "\r\n";
                result_string += "Test:" + all_test_num.ToString() + "\t";
                result_string += "Pass:" + all_pass_num.ToString() + "\t";
                result_string += "Fail:" + all_error_num.ToString() + "\r\n";
                result_string += "上升时间：" + get_jiange(auto_sample.Text, auto_t_uptime.Text) + "\t";
                result_string += "下降时间：" + get_jiange(auto_sample.Text, auto_t_downtime.Text) + "\r\n";
                result_string += "最小值：" + auto_t_min.Text + "\t";
                result_string += "最大值：" + auto_t_max.Text + "\r\n";
                result_string += "保持时间：" + get_jiange(auto_sample.Text, auto_t_baochi.Text) + "\t";
                result_string += "周期间隔：" + get_jiange(auto_sample.Text, auto_t_boxingjiange.Text) + "\t";
                SaveFile(result_string, @"Test result\测试结果.txt", true);
                auto_clear_Click(null, null);
                zhouqi_num++;
            }

        //结束测试

        ERROR1: { };
        end1:
            //执行停止
            FrmMain.form_st.Write("OUTP OFF", false);
        end2:
            auto_end_test_Click(null, null);


            MessageBox.Show("测试结束");
            //查看log
            materialButton4_Click_2(null, null);



        }

        //开始自动测试
        private void auto_start_test_Click(object sender, EventArgs e)
        {
            try
            {
                int point_nums_all = Convert.ToInt32(auto_t_uptime.Text) + Convert.ToInt32(auto_t_downtime.Text) + Convert.ToInt32(auto_t_baochi.Text) + Convert.ToInt32(auto_t_boxingjiange.Text);
                if (point_nums_all < 80000)
                {
                    short_test();
                }
                else
                {
                    long_test();
                }
  
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:" + ex.Message);
                goto end1;
            }



        end1: { };
        }

        //将data转成ARB auto test
        public void data_to_arb_auto(string filename, int[] data)
        {
            arbs_set.Clear();
            arbs_set.Add("Copyright: Agilent Technologies, 2010");
            arbs_set.Add("File Format:" + "1.0");
            arbs_set.Add("Channel Count:" + "1");
            arbs_set.Add("Sample Rate:" + auto_sample.Text);
            arbs_set.Add("High Level:" + auto_high_level.Text);
            arbs_set.Add("Low Level:" + auto_low_level.Text);
            arbs_set.Add("Filter:" + @"""" + auto_fliter.Text + @"""");
            arbs_set.Add("Data Points:" + string.Format("{0:D}", data.Length));
            arbs_set.Add("Data:");

            for (int i = 0; i < data.Length; i++)
            {
                arbs_set.Add(string.Format("{0:D}", data[i]));
            }

            StreamWriter sw = new StreamWriter( filename, false, Encoding.GetEncoding("GB2312"));
            sw.Flush();
            // 使用StreamWriter来往文件中写入内容 
            sw.BaseStream.Seek(0, SeekOrigin.Begin);
            for (int i = 0; i < arbs_set.Count; i++) sw.WriteLine(arbs_set[i]);
            //关闭此文件 
            sw.Flush();
            sw.Close();
        }

        //将ARB发送到
        public void send_arb(string arb_Path)
        {
            try
            {
                //开始处理
                string file_name = Path.GetFileName(arb_Path);
                string path_name = arb_Path;
                //合成将要发送的数据String
                string data_string = LoadData(path_name);
                //合成发送指令
                string Command = SendArbWaveform(data_string, file_name);
                //执行发送
                FrmMain.form_st.Write(Command, false);
            }

            catch (Exception ex)
            {
                MessageBox.Show("ERROR:" + ex.Message);
                goto end1;
            }
        end1:
            { }
        }
        //执行SEQ
        public void start_seq(List<arb_struct> ARB_list2,string caiyanglv,string volt, string volt_low, string offset)
        {
            List<string> send_list2 = new List<string>();
            string Command = "";
            try
            {
                //去掉重复文件
                send_list2.Clear();
                foreach (arb_struct i in ARB_list2)
                {
                    if (send_list2.FindIndex(t => t == i.Arb_file) == -1)
                    {
                        send_list2.Add(i.Arb_file);
                    }
                }
                //发送所有有效文件
                foreach (string i in send_list2)
                {
                    send_arb(i);     
                }
                //复位并将文件加载到存储
                FrmMain.form_st.Write("*CLS", false);
                FrmMain.form_st.Write("*RST", false);

                foreach (string i in send_list2)
                {
                    string file_name = Path.GetFileName(i);
                    Command = @"MMEM:LOAD:DATA ""INT:\User Files\" + file_name + @"""";
                    FrmMain.form_st.Write(Command, false);
                }
                Command = SendArbWaveform_seq(LoadData_seq(ARB_list2));
                FrmMain.form_st.Write(Command, false);
                // 运行此序列
                Command = "FUNCtion ARB \r\n";



                Command += @"FUNCtion:ARBitrary ""testSeq" + @"""" + "\r\n";
                Command += "FUNC:ARB:SRAT " + caiyanglv+ "\r\n";
                Command += "VOLTage:HIGH " + volt + "\r\n";
                Command += "VOLTage:LOW " + volt_low + "\r\n";
               //Command += "VOLTage:OFFSet " + offset + "\r\n";

                Command += "OUTPut " + Output_shuchu.Text + "\r\n";
                FrmMain.form_st.Write(Command, false);
            }

            catch (Exception ex)
            {
                MessageBox.Show("ERROR:" + ex.Message);
                goto end1;
            }


        end1: { }


        }

        private void materialButton10_Click(object sender, EventArgs e)
        {
            PWM(PWM_Freq.Text,PWM_Dcycle.Text,PWM_Channel.Text);
        }

        //display seq
        public void display_seq(List<arb_struct> ARB_list1)
        {
            List<int> data = new List<int>();
            int[] data_temp = new int[0];
            foreach (arb_struct i in ARB_list1)
            {
                readarbfile(i.Arb_file, ref data_temp);
                for (int arbs = 0; arbs < i.repeat; arbs++)
                {
                    data.AddRange(data_temp);
                }
            }
            DisplayChart(data.ToArray());
        }

      //生成PWM波形
      public static bool PWM(string freq, string cycle,string channel)
      {
            string Command = "";
            string sign;
           // Command += "*RST\r\n";
            Command += "FUNCtion SQU\r\n";
            Command += "FUNC:SQU:DCYC "+cycle+"\r\n";
            Command += "FREQ "+freq+"\r\n";
            Command += "VOLT:HIGH +1.65\r\n";
            Command += "VOLT:LOW +0.0\r\n";
            Command += "OUTPut "+channel+"\r\n";
            return FrmMain.form_st.Write(Command, true);
      }



    }
}
