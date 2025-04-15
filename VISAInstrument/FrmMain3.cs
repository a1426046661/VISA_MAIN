﻿using Ivi.Visa;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VISAInstrument.Configuration;
using VISAInstrument.Ports;
using VISAInstrument.Properties;
using VISAInstrument.Utility;
using VISAInstrument.Utility.Extension;
using VISAInstrument.Utility.Extension.UI;
using System.Net.Sockets;
using MaterialSkin.Controls;
using MaterialSkin;


using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Text.RegularExpressions;
namespace VISAInstrument
{
    
    public partial class FrmMain3 : MaterialForm
    {
        //端口开关委托
        public delegate void MyDelegate(string sender, bool isopened);
        public event MyDelegate open_change_event=null;



        public static MaterialSkinManager materialSkinManager;
        public static bool Wave_close_flag = false;
        public static FrmMain3 form_st=null;
        public static string Model_name;
        public FrmMain3()
        {
            InitializeComponent();
            form_st = this;
  
        }

        private int Main(object sender, bool isopened)
        {
            throw new NotImplementedException();
        }

        private bool _cancelDisplayForm;
        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
             if(rbtRS232 == sender as RadioButton)
            {
                rdoSpecifiedCount.Text = @"读取指定字节数量";
                this.tableLayoutPanel.RowStyles[2].Height = 35F;
                this.tableLayoutPanel.RowStyles[3].Height = 0F; 
                
                return;
            }
            rdoSpecifiedCount.Text = @"读取指定字节数量或收到结束符";
            if (rbtLAN == sender as RadioButton)
            {
                this.tableLayoutPanel.RowStyles[2].Height = 0F;
                this.tableLayoutPanel.RowStyles[3].Height = 35F;
                return;
            }
            this.tableLayoutPanel.RowStyles[2].Height = 0F;
            this.tableLayoutPanel.RowStyles[3].Height = 0F;
        }

        private void DoSomethingForRadioButton(out string message,params Func<string>[] actionOfRbt)
        {
            message = string.Empty;
            if (actionOfRbt.Length != 4) throw new ArgumentException();
            if (rbtRS232.Checked) message = actionOfRbt[0]();
            if (rbtUSB.Checked)  message = actionOfRbt[1]();
            if (rbtGPIB.Checked)  message = actionOfRbt[2]();
            if (rbtLAN.Checked)  message = actionOfRbt[3]();
        }

        private readonly int[] _baudRate = { 256000, 128000, 115200, 57600, 56000, 43000, 38400, 28800, 19200, 9600, 4800, 2400, 1200, 600, 300, 110 };
        private readonly int[] _dataBits = { 8, 7, 6 };
        private bool _isAsciiCommand = true;
        private void FrmMain_Load(object sender, EventArgs e)
        {
            _cts = new CancellationTokenSource();
            ShowTime();
            关于ToolStripMenuItem.Text = $@"{Application.ProductName}({Application.ProductVersion})";
            rbtUSB.Checked = true;
            btnRefresh.PerformClick();
            btnOpen.Text = Resources.OpenString;
            cboBaudRate.DataSource = _baudRate;
            cboBaudRate.SelectedIndex = 9;
            cboParity.DataSource = Enum.GetValues(typeof(SerialParity));
            cboStopBits.DataSource = Enum.GetValues(typeof(SerialStopBitsMode));
            cboStopBits.SelectedIndex = 0;
            cboDataBits.DataSource = _dataBits;
            cboFlowControl.DataSource = Enum.GetValues(typeof(SerialFlowControlModes));
            EnableControl(true);
            //设置开始风格
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.EnforceBackcolorOnAllComponents = true;
           // materialSkinManager.AddFormToManage(this);

            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(
                        Primary.BlueGrey700,
                        Primary.BlueGrey900,
                        Primary.BlueGrey400,
                        Accent.LightBlue200,
                        TextShade.WHITE);
            //获取本地IP
            local_ip_Click(null,null);
            if (_cancelDisplayForm) Close();
        }

        private CancellationTokenSource _cts;
        private void ShowTime()
        {
            Task.Factory.StartNew(() => 
            {
                while(!_cts.IsCancellationRequested)
                {
                    string now = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                    InvokeToForm(() => 时间ToolStripMenuItem.Text = now);
                    Thread.Sleep(500);
                }
            });
        }

        private PortOperatorBase _portOperatorBase;
        private bool _isWritingError;
        /// <summary>
        /// 获取本地IPv4地址
        /// </summary>
        /// <returns></returns>
        public IPAddress GetLocalIPv4Address()
        {
            IPAddress localIpv4 = null;
            //获取本机所有的IP地址列表
            IPAddress[] IpList = Dns.GetHostAddresses(Dns.GetHostName());
            //循环遍历所有IP地址
            foreach (IPAddress IP in IpList)
            {
                //判断是否是IPv4地址
                if (IP.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIpv4 = IP;
                }
                else
                {
                    continue;
                }
            }
            return localIpv4;
        }

        public bool Write(string command,bool getback)
        {
            _isWritingError = false;
            if (string.IsNullOrEmpty(command))
            {
                Invoke(new Action(() => MessageBox.Show(this, Resources.CommandNotEmpty)));
                return false;
            }
            string asciiString = string.Empty;
            byte[] byteArray = null;
            if (_isAsciiCommand)
            {
                asciiString = command;
            }
            else
            {
                if (StringEx.TryParseByteStringToByte(command, out byte[] bytes))
                {
                    byteArray = bytes;
                }
                else
                {
                    _isWritingError = true;
                    
                    Invoke(new Action(() => MessageBox.Show(this, @"转换字节失败，请按照“XX XX XX”格式输入内容")));
                    return false;
                }
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                if (_isAsciiCommand)
                {
                    if (chkAppendNewLine.Checked)
                    {
                        _portOperatorBase.WriteLine(asciiString);
                    }
                    else
                    {
                        _portOperatorBase.Write(asciiString);
                    }
                }
                else
                {
                    if (chkAppendNewLine.Checked)
                    {
                        _portOperatorBase.WriteLine(byteArray);
                    }
                    else
                    {
                        _portOperatorBase.Write(byteArray);
                    }
                }
            }
            catch (Exception ex)
            {
                Invoke(new Action(() => MessageBox.Show(this, $@"写入命令“{command}”失败！{Environment.NewLine}{ex.Message}")));
                return false;
            }
            if (getback == true)
                Invoke(new Action(() => DisplayToTextBox($"[Time:{stopwatch.ElapsedMilliseconds}ms] Write: {command}")));
            return true;
        }

        private bool Write()
        {
            _isWritingError = false;
            if (string.IsNullOrEmpty(txtCommand.Text))
            {
                Invoke(new Action(() => MessageBox.Show(this,Resources.CommandNotEmpty)));
                return false;
            }
            string asciiString = string.Empty;
            byte[] byteArray = null;
            if (_isAsciiCommand)
            {
                asciiString = txtCommand.Text;
            }
            else
            {
                if (StringEx.TryParseByteStringToByte(txtCommand.Text, out byte[] bytes))
                {
                    byteArray = bytes;
                }
                else
                {
                    _isWritingError = true;
                    Invoke(new Action(() => MessageBox.Show(this,@"转换字节失败，请按照“XX XX XX”格式输入内容")));
                    return false;
                }
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                if (_isAsciiCommand)
                {
                    if (chkAppendNewLine.Checked)
                    {
                        _portOperatorBase.WriteLine(asciiString);
                    }
                    else
                    {
                        _portOperatorBase.Write(asciiString);
                    }
                }
                else
                {
                    if (chkAppendNewLine.Checked)
                    {
                        _portOperatorBase.WriteLine(byteArray);
                    }
                    else
                    {
                        _portOperatorBase.Write(byteArray);
                    }
                }
            }
            catch(Exception ex)
            {
                Invoke(new Action(() => MessageBox.Show(this,$@"写入命令“{txtCommand.Text}”失败！{Environment.NewLine}{ex.Message}")));
                return false;
            }
            Invoke(new Action(() => DisplayToTextBox($"[Time:{stopwatch.ElapsedMilliseconds}ms] Write: {txtCommand.Text}")));
            return true;
        }

        private void btnWrite_Click(object sender, EventArgs e)
        {
            Write();
        }

        private void Read()
        {
            Read(rdoUntilNewLine.Checked, (int)nudSpecifiedCount.Value);
        }
        public void Read(bool isUntilNewLine, int specifiedCount,ref string get_string)
        {
            ClearIfTextBoxOverFlow();
            string result;
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                if (_isAsciiCommand)
                {
                    result = isUntilNewLine ? _portOperatorBase.ReadLine() : _portOperatorBase.Read(specifiedCount);
                    get_string = result;
                }
                else
                {
                    byte[] bytes = isUntilNewLine ? _portOperatorBase.ReadToBytes() : _portOperatorBase.ReadToBytes(specifiedCount);
                    if (ByteEx.TryParseByteToByteString(bytes, out string byteString))
                    {
                        result = byteString;
                        get_string = result;
                    }
                    else
                    {
                        throw new InvalidCastException("无法转换从接收缓冲区接收回来的数据");
                        get_string = "";
                    }
                }
            }
            catch (IOTimeoutException)
            {
                result = Resources.ReadTimeout;
                get_string = "";
            }
            catch (Exception ex)
            {
                result = ex.Message;
                get_string = "";
            }

           // Invoke(new Action(() => DisplayToTextBox($"[Time:{stopwatch.ElapsedMilliseconds}ms] Read:  {result}")));
        }
        private void Read(bool isUntilNewLine,int specifiedCount)
        {
            ClearIfTextBoxOverFlow();
            string result;
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                if (_isAsciiCommand)
                {
                    result = isUntilNewLine ? _portOperatorBase.ReadLine() : _portOperatorBase.Read(specifiedCount);
                }
                else
                {
                    byte[] bytes = isUntilNewLine ? _portOperatorBase.ReadToBytes() : _portOperatorBase.ReadToBytes(specifiedCount);
                    if (ByteEx.TryParseByteToByteString(bytes, out string byteString))
                    {
                        result = byteString;
                    }
                    else
                    {
                        throw new InvalidCastException("无法转换从接收缓冲区接收回来的数据");
                    }
                }
            }
            catch (IOTimeoutException)
            {
                result = Resources.ReadTimeout;
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            Invoke(new Action(() => DisplayToTextBox($"[Time:{stopwatch.ElapsedMilliseconds}ms] Read:  {result}")));
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            Read();
        }

        private bool Query(string command, bool getback, bool isUntilNewLine, int specifiedCount, ref string get_string)
        {
            bool isSuccessful = Write(command,getback);
            if (isSuccessful && !_isWritingError) Read(isUntilNewLine, specifiedCount,ref get_string);
            return isSuccessful;
        }
        public bool Query()
        {
            bool isSuccessful = Write();
            if (isSuccessful && !_isWritingError) Read();
            return isSuccessful;
        }
        private void btnQuery_Click(object sender, EventArgs e)
        {
            Query();
        }

        private bool NewPortInstance(out string message)
        {
            bool hasAddress = false;
            bool hasException = false;
            DoSomethingForRadioButton(out message,
                () =>
                {
                    string message1 = string.Empty;
                    if (cboRS232.SelectedIndex == -1) return "没有串口选中";
                    try
                    {
                        bool successful = int.TryParse(cboBaudRate.Text.ToString(), out int baudRate);
                        if (!successful)
                        {
                            throw new InvalidOperationException("指定的波特率无效");
                        } 
                        _portOperatorBase = new Rs232PortOperator(((Pair<string, string>)cboRS232.SelectedItem).Value,
                                               baudRate, (SerialParity)cboParity.SelectedItem,
                                               (SerialStopBitsMode)cboStopBits.SelectedItem, (int)cboDataBits.SelectedItem);
                        hasAddress = true;
                    }
                    catch(Exception e1)
                    {
                        
                        hasException = true;
                        message1 =  e1.ToString();
                    }

                    return message1;
                },
                () =>
                {
                    string message2 = string.Empty;
                    if (cboUSB.SelectedIndex == -1) return "没有USB选中";
                    try
                    {
                        _portOperatorBase = new UsbPortOperator(cboUSB.SelectedItem.ToString());
                        hasAddress = true;
                    }
                    catch(Exception e1)
                    {
                        hasException = true;
                        message2 =  e1.ToString();
                    }
                    return message2;
                },
                () =>
                {
                    string message3 = string.Empty;
                    if (cboGPIB.SelectedIndex == -1) return "没有GPIB选中";
                    try
                    {
                        _portOperatorBase = new GpibPortOperator(cboGPIB.SelectedItem.ToString());
                        hasAddress = true;
                    }
                    catch(Exception e1)
                    {
                        hasException = true;
                        message3 =  e1.ToString();
                    }
                    return message3;
                },
                () =>
                {
                    string message4 = string.Empty;
                    if (cboLAN.SelectedIndex == -1) return "没有LAN选中";
                    try
                    {
                        _portOperatorBase = new LanPortOperator(cboLAN.SelectedItem.ToString());
                        hasAddress = true;
                    }
                    catch(Exception e1)
                    {
                        hasException = true;
                        message4 =  e1.ToString();
                    }
                    return message4;
                });
            if(!hasException && hasAddress) _portOperatorBase.Timeout = (int)nudTimeout.Value;
            return hasAddress;
        }

        private void DisplayToTextBox(string content)
        {
            txtDisplay.Text += $@"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {content}{Environment.NewLine}";
            txtDisplay.SelectionStart = txtDisplay.Text.Length - 1;
            txtDisplay.ScrollToCaret();
        }

        private void ClearIfTextBoxOverFlow()
        {
            if (txtDisplay.Text.Length > 204800) txtDisplay.Clear();
        }

        private Task _task;

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            string title = Text;
            _task = Task.Factory.StartNew(() =>
            {
                InvokeToForm(() => { btnRefresh.Enabled = false; btnOpen.Enabled = false; Text = title + Resources.LoadingRS232; });
                string[] content1 = PortHelper.FindAddresses(PortType.Rs232);
                string[] content2 = PortHelper.FindRs232Type(content1);
                List<string> list1 = new List<string>();
                List<string> list2 = new List<string>();
                for (int i = 0; i < content2.Length; i++)
                {
                    if (content2[i].Contains("LPT")) continue;
                    list1.Add(content1[i]);
                    list2.Add(content2[i]);
                }
                content1 = list1.ToArray();
                content2 = list2.ToArray();
                var content3 = content1;
                InvokeToForm(() => cboRS232.ShowAndDisplay(content3, content2));
                InvokeToForm(() => { Text = title + Resources.LoadingUSB; });
                content1 = PortHelper.FindAddresses(PortType.Usb);
                var content4 = content1;
                InvokeToForm(() => cboUSB.ShowAndDisplay(content4));
                InvokeToForm(() => { Text = title + Resources.LoadingGPIB; });
                content1 = PortHelper.FindAddresses(PortType.Gpib);
                var content5 = content1;
                InvokeToForm(() => cboGPIB.ShowAndDisplay(content5));
                InvokeToForm(() => { Text = title + Resources.LoadingLAN; });
                content1 = PortHelper.FindAddresses(PortType.Lan);
                InvokeToForm(() => cboLAN.ShowAndDisplay(content1));
                InvokeToForm(() => { btnRefresh.Enabled = true; btnOpen.Enabled = true; Text = title; });
            }).ContinueWith(x=> 
            {
                if(x.IsFaulted)
                {
                    _cancelDisplayForm = true;
                    InvokeToForm(() => { tableLayoutPanel.Enabled = false; this.Text = Resources.RuntimeError; });
                    if (x.Exception?.InnerException != null) MessageBox.Show(this, x.Exception.InnerException.Message);
                }
            });
        }


        private void InvokeToForm(Action action)
        {
            try
            {
                this.Invoke(action);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }




        public static bool isopen = false;
        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (btnOpen.Text == Resources.OpenString)
            {
                try
                {
                    if (!NewPortInstance(out string message))
                    {
                        MessageBox.Show(this, message);
                        return;
                    }
                    _portOperatorBase.Open();
                    btnOpen.Text = Resources.CloseString;
                    if (_portOperatorBase is Rs232PortOperator)
                    {
                        chkRealTimeReceive.Enabled = true;

                       
                        BindOrRemoveDataReceivedEvent();
                   
                    }
                    else
                    {
                        chkRealTimeReceive.Enabled = false;
                    }

                    EnableControl(false);
                    chkStartCycle_CheckedChanged(null, null);
                    //询问设备型号
                    string Device_ID="";
                    Query("*IDN? ",false,rdoUntilNewLine.Checked,(int)nudSpecifiedCount.Value, ref Device_ID);
                    DisplayToTextBox("Connected："+Device_ID);
                    Model_name = Device_ID;
                    //判断是否为信号发生器
                    if (Device_ID.Contains("B290") || Device_ID.Contains("E363"))
                    {
                        isopen = true;
                    }
                    else
                    {
                        MessageBox.Show("貌似不是稳压源！");
                        btnOpen_Click(null, null);

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.ToString());
                }
            }
            else
            {
                if(CheckCycleEnable("关闭"))
                {
                    return;
                }

                try
                {
                    _portOperatorBase.Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                btnOpen.Text = Resources.OpenString;
                EnableControl(true);
                if (true)
                {
                    isopen = false;
                }
            }
            open_change_event.Invoke("可调电源",isopen);
        }

        private void EnableControl(bool enable)
        {
            flowLayoutPanel1.Enabled = enable;
            flowLayoutPanel2.Enabled = enable;
            btnRefresh.Enabled = enable;
            flowLayoutPanel5.Enabled = enable;
            flowLayoutPanel7.Enabled = !enable;
            txtCommand.Enabled = !enable;
            //ARB_OPEN.Enabled = !enable;
            //RST.Enabled= !enable;
            if(enable)
            {
                btnWrite.Enabled = false;
                btnRead.Enabled = false;
                btnQuery.Enabled = false;
                btnCycle.Enabled = false;
            }
            
            lblOverTime.Enabled = enable;
            lblTimeout.Enabled = enable;
            nudTimeout.Enabled = enable;
        }

        private void 清除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtDisplay.Clear();
        }

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            EnableContextMenuStrip(txtDisplay.Text.Length != 0);
        }

        private void EnableContextMenuStrip(bool enable)
        {
            contextMenuStrip.Enabled = enable;
        }

        private void 全选ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtDisplay.Focus();
            txtDisplay.SelectAll();
        }

        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtDisplay.SelectedText);
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //// 显示关闭系统确认对话框
            //AboutBox dlg = new AboutBox();

            //// 如果用户点击了取消，则不关闭。
            //if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            //    e.Cancel = true;
            if (Wave_close_flag == true)
            {
                e.Cancel = true;
                Wave_close_flag = false;
                return;

            }
            if (CheckCycleEnable("关闭"))
            {
                e.Cancel = true;
                return;
            }

            if (_task != null && !_task.IsCompleted)
            {
                MessageBox.Show(this, Resources.LoadingInstrumentResource);
                e.Cancel = true;
                return;
            }

            MainForm.wave_frmMain3.Visible = false;
            e.Cancel = true;
            return;

            _cts.Cancel();
            try
            {
                _portOperatorBase?.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private const string IpRegex = @"^((25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(25[0-5]|2[0-4]\d|[01]?\d\d?)$";
        private void btnCheckIP_Click(object sender, EventArgs e)
        {
            if (!txtIPAddress.Text.IsMatch(IpRegex))
            {
                MessageBox.Show(this,Resources.NotCorrectIP);
                txtIPAddress.SetSelect();
                return;
            }
            if (cboLAN.Items.Cast<object>().Any(item => ((string)item).Contains(txtIPAddress.Text)))
            {
                MessageBox.Show(this,Resources.LANContainIP);
                txtIPAddress.SetSelect();
                return;
            }
            if (!PortHelper.OpenIpAddress(txtIPAddress.Text, out string fullAddress))
            {
                MessageBox.Show(this,Resources.NotDetectIP);
                txtIPAddress.SetSelect();
                return;
            }
            cboLAN.Items.Add(fullAddress);
            cboLAN.Text = cboLAN.Items[cboLAN.Items.Count-1].ToString();
            //自动连接
            btnOpen_Click(null,null);
            MessageBox.Show("连接成功");
            
            //MessageBox.Show(this,Resources.DetectOK);
        }

        private void githubToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process.Start(Resources.GithubURL);
        }

        private void blogToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process.Start(Resources.BlogURL);
        }

        private void byCNXYToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process.Start("mailto:zhao_yh@megahuntmicro.com");
        }

        private void 时间ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string system32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
                Process.Start($@"{system32}\rundll32.exe", "shell32.dll,Control_RunDLL timedate.cpl,,0");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void 全选ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            txtCommand.SelectAll();
        }

        private void 复制ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtCommand.SelectedText);
        }

        private void 粘贴ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtCommand.Text = Clipboard.GetText();
            Clipboard.Clear();
        }

        private void cmsCommand_Opening(object sender, CancelEventArgs e)
        {
            if(string.IsNullOrEmpty(txtCommand.Text.Trim()))
            {
                全选ToolStripMenuItem1.Enabled = false;
                复制ToolStripMenuItem1.Enabled = false;
            }
            else
            {
                全选ToolStripMenuItem1.Enabled = true;
                复制ToolStripMenuItem1.Enabled = !string.IsNullOrEmpty(txtCommand.SelectedText);
            }

            粘贴ToolStripMenuItem.Enabled = !string.IsNullOrEmpty(Clipboard.GetText());
        }

        private void rdoAsciiByte_CheckedChanged(object sender, EventArgs e)
        {
            _isAsciiCommand = rdoAscii.Checked;
            if (sender == rdoAscii && !rdoAscii.Checked) return;
            if(sender == rdoByte && !rdoByte.Checked) return;
            if (string.IsNullOrEmpty(txtCommand.Text)) return;
            bool isSuccessful = false;
            if (rdoAscii.Checked)
            {
                if (StringEx.TryParseByteStringToAsciiString(txtCommand.Text, out string asciiString))
                {
                    txtCommand.Text = asciiString;
                    isSuccessful = true;
                }
            }
            else
            {

                if (StringEx.TryParseAsciiStringToByteString(txtCommand.Text, out string byteString))
                {
                    txtCommand.Text = byteString;
                    isSuccessful = true;
                }
            }
            if(!isSuccessful) txtCommand.Clear();
        }

        private void rdoUntilNewLineSpecifiedCount_CheckedChanged(object sender, EventArgs e)
        {
            if (_portOperatorBase is Rs232PortOperator portOperator)
            {
                if (rdoUntilNewLine.Checked)
                {
                    portOperator.SetReadTerminationCharacterEnabled(true);
                }
                else
                {
                    portOperator.SetReadTerminationCharacterEnabled(false);
                }
            }
            nudSpecifiedCount.Enabled = !rdoUntilNewLine.Checked;
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            string message = VisaInformation.VisaSharedComponent.Concat(VisaInformation.NiVisaRuntime).Aggregate((x, y) => $"{x}{Environment.NewLine}{y}").TrimEnd('\r', '\n');
            MessageBox.Show(this,message);
        }

        private void chkStartCycle_CheckedChanged(object sender, EventArgs e)
        {
            if(!chkRealTimeReceive.Checked)
            {
                flowLayoutPanel10.Enabled = chkStartCycle.Checked;
                btnWrite.Enabled = !chkStartCycle.Checked;
                btnRead.Enabled = !chkStartCycle.Checked;
                btnQuery.Enabled = !chkStartCycle.Checked;
                btnCycle.Enabled = chkStartCycle.Checked;
                rdoUntilNewLine.Enabled = true;
                rdoSpecifiedCount.Enabled = true;
                rdoSendRead.Enabled = true;
            }
            else
            {
                btnRead.Enabled = false;
                btnQuery.Enabled = false;

                rdoUntilNewLine.Enabled = false;
                rdoSendRead.Enabled = false;
                rdoSpecifiedCount.Enabled = false;

                if (chkStartCycle.Checked)
                {
                    rdoSend.Checked = true;
                    rdoUntilNewLine.Checked = true;
                    
                    flowLayoutPanel10.Enabled = true;
                    btnWrite.Enabled = false;
                    btnCycle.Enabled = true;
                }
                else
                {
                    flowLayoutPanel10.Enabled = false;
                    btnWrite.Enabled = true;
                    btnCycle.Enabled = false;
                }
            }
        }

        private void rdoSendRead_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoSendRead.Checked)  btnCycle.Text = @"循环发送读取";
        }

        private void rdoSend_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoSend.Checked) btnCycle.Text = @"循环发送";
        }

        private void EnableCycle(bool enabled)
        {
            Invoke(new Action(() =>
            {
                flowLayoutPanel9.Enabled = !enabled;
                groupBox1.Enabled = !enabled;
                groupBox2.Enabled = !enabled;
                txtCommand.Enabled = !enabled;
            }));
            _cycleEnabled = enabled;
        }

        private bool _cycleEnabled;
        private string _originalCycleText;
        private const string StopCycleText = "停止"; 
        private void btnCycle_Click(object sender, EventArgs e)
        {
            if(btnCycle.Text == StopCycleText)
            {
                EnableCycle(false);
                btnCycle.Enabled = false;
                Task.Factory.StartNew(() => 
                {
                    while(btnCycle.Text == StopCycleText)
                    {
                        Thread.Sleep(100);
                    }
                    Invoke(new Action(() => btnCycle.Enabled = true));
                });
            }
            else
            {
                EnableCycle(true);
                _originalCycleText = btnCycle.Text;
                btnCycle.Text = StopCycleText;
                Task.Factory.StartNew(() =>
                {
                    int count = 0;
                    int cycleCount = (int)nudCycleCount.Value;
                    int intervalTime = (int)nudInterval.Value;
                    while (_cycleEnabled)
                    {
                        if (cycleCount != 0 && count >= cycleCount)
                        {
                            EnableCycle(false);
                            break;
                        }
                        bool isSuccessful = rdoSend.Checked ? Write() : Query();
                        if (!isSuccessful)
                        {
                            EnableCycle(false);
                            break;
                        }
                        Thread.Sleep(intervalTime);
                        if(cycleCount != 0) count++;
                    }
                    Invoke(new Action(() => btnCycle.Text = _originalCycleText));
                });
            }
        }

        private bool CheckCycleEnable(string operationName)
        {
            bool cycleEnabledTemp = _cycleEnabled;
            if (cycleEnabledTemp)
            {
                MessageBox.Show(this,$@"请停止循环操作后再执行{operationName}操作");
            }
            return cycleEnabledTemp;
        }

        private void BindOrRemoveDataReceivedEvent()
        {
            if (_portOperatorBase is Rs232PortOperator portOperator)
            {
                if (chkRealTimeReceive.Checked)
                {
                    portOperator.DataReceived += PortOperator_DataReceived;
                }
                else
                {
                    portOperator.DataReceived -= PortOperator_DataReceived;
                }
            }
        }

        private void chkRealTimeReceive_CheckedChanged(object sender, EventArgs e)
        {
            chkStartCycle_CheckedChanged(null, null);
            BindOrRemoveDataReceivedEvent();
        }

        private void PortOperator_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if(e.BytesToRead > 0) Read(false, e.BytesToRead);
        }
        //波形发生窗口
        private void button1_Click(object sender, EventArgs e)
        {
            //Wave wave = new Wave();
            //wave.StartPosition = FormStartPosition.CenterScreen;
            Wave_close_flag = true;
            //IController controller = new IController(wave);
            ////this.Enabled = false;
            //wave.Show();
        }
        //放入本地IP
        private void local_ip_Click(object sender, EventArgs e)
        {
            //接收IPv4的地址
            IPAddress localIP = GetLocalIPv4Address();
            //赋值给文本框
            txtIPAddress.Text = localIP.ToString();
        }

        private void RST_Click(object sender, EventArgs e)
        {
            Write("*RST", true);
        }
        //清空显示框
        private void button2_Click(object sender, EventArgs e)
        {
            txtDisplay.Text = "";
        }
        //保存TXT
        private void button3_Click(object sender, EventArgs e)
        {
            //ARB_OPEN.Enabled = true;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.Filter = "txt file|*.txt";
            saveFileDialog.DefaultExt = ".txt";
            saveFileDialog.FileName = "received.txt";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                String fName = saveFileDialog.FileName;
                System.IO.File.WriteAllText(fName, txtDisplay.Text);
            }
        }

        //软件帮助文档
        private void 软件帮助文档ToolStripMenuItem_Click(object sender, EventArgs e)
        {
             Process myProcess = new Process();
            myProcess.StartInfo.FileName = "help\\help.pdf";
            myProcess.StartInfo.Verb = "Open";
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.Start();
        }

        private void keysightSCPIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process myProcess = new Process();
            myProcess.StartInfo.FileName = "help\\操作手册.pdf";
            myProcess.StartInfo.Verb = "Open";
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.Start();
        }
    }
}