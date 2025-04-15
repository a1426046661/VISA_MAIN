﻿/**
 
 * Copyright (c) 2014-2015, Wenhuix, All rights reserved.

 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:

 * Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.

 * Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.

 * Neither the name of COMDBG nor the names of its contributors may 
 * be used to endorse or promote products derived from this software without 
 * specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
 * THE POSSIBILITY OF SUCH DAMAGE.
 */


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Windows;
using System.Windows.Forms;
using Sunny.UI;

namespace VISAInstrument
{
    //public interface IView
    //{
    //    void SetController(IController controller);
    //    //Open serial port event
    //    void OpenComEvent(Object sender, SerialPortEventArgs e);
    //    //Close serial port event
    //    void CloseComEvent(Object sender, SerialPortEventArgs e);
    //    //Serial port receive data event
    //    void ComReceiveDataEvent(Object sender, SerialPortEventArgs e);
    //}


public partial class Megahunt_USART : UIForm, IView
    {
        public IController controller;
        private int sendBytesCount = 0;
        private int receiveBytesCount = 0;
        //端口开关委托
        public delegate void MyDelegate(string sender, bool isopened);
        public event MyDelegate open_change_event = null;
        public static Megahunt_USART form_st = null;
        public Megahunt_USART()
        {
            InitializeComponent();
            InitializeCOMCombox();
            form_st = this;
            this.statusTimeLabel.Text = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            this.toolStripStatusTx.Text = "Sent: 0";
            this.toolStripStatusRx.Text = "Received: 0";
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            Encode_box.SelectedIndex = 1;
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
        /// Initialize serial port information
        /// </summary>
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
            baudRateCbx.Text = baudRateCbx.Items[1].ToString();

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

            //Com Ports
            string[] ArrayComPortsNames = SerialPort.GetPortNames();
            if (ArrayComPortsNames.Length == 0)
            {
                statuslabel.Text = "No COM found !";
                openCloseSpbtn.Enabled = false;
            }
            else
            {
                Array.Sort(ArrayComPortsNames);
                for (int i = 0; i < ArrayComPortsNames.Length; i++)
                {
                    comListCbx.Items.Add(ArrayComPortsNames[i]);
                }
                comListCbx.Text = ArrayComPortsNames[0];
                openCloseSpbtn.Enabled = true;
            }
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
                open_change_event.Invoke("usart",true);
                statuslabel.Text = comListCbx.Text + " Opend";
                openCloseSpbtn.Text = "Close";
                sendbtn.Enabled = true;
                autoSendcbx.Enabled = true;
                autoReplyCbx.Enabled = true;

                comListCbx.Enabled = false;
                baudRateCbx.Enabled = false;
                dataBitsCbx.Enabled = false;
                stopBitsCbx.Enabled = false;
                parityCbx.Enabled = false;
                handshakingcbx.Enabled = false;
                refreshbtn.Enabled = false;

                if (autoSendcbx.Checked)
                {
                    autoSendtimer.Start();
                    sendtbx.ReadOnly = true;
                }
            }
            else    //Open failed
            {
                open_change_event.Invoke("usart", false);
                statuslabel.Text = "Open failed !";
                sendbtn.Enabled = false;
                autoSendcbx.Enabled = false;
                autoReplyCbx.Enabled = false;
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
                open_change_event.Invoke("usart", false);
                statuslabel.Text = comListCbx.Text + " Closed";
                openCloseSpbtn.Text = "Open";

                sendbtn.Enabled = false;
                sendtbx.ReadOnly = false;
                autoSendcbx.Enabled = false;
                autoSendtimer.Stop();

                comListCbx.Enabled = true;
                baudRateCbx.Enabled = true;
                dataBitsCbx.Enabled = true;
                stopBitsCbx.Enabled = true;
                parityCbx.Enabled = true;
                handshakingcbx.Enabled = true;
                refreshbtn.Enabled = true;
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

        /// <summary>
        /// Display received data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static string Data_Recved = "";
        public static bool Data_Recved_ON=false;
        public void ComReceiveDataEvent(Object sender, SerialPortEventArgs e)
        {
            try
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
                if (receivetbx.Lines.Length >= 2000)
                {
                    clearText(receivetbx, 500);
                }
                receivetbx.AppendText(Megahunt_USART.Encodes.GetString(e.receivedBytes));

                if (Data_Recved_ON)
                {
                    Data_Recved += Megahunt_USART.Encodes.GetString(e.receivedBytes);
                }
                if (recStrRadiobtn.Checked) //display as string
                {

                }
                else //display as hex
                {
                    if (receivetbx.Text.Length > 0)
                    {
                        receivetbx.AppendText("-");
                    }
                    receivetbx.AppendText(IController.Bytes2Hex(e.receivedBytes));
                }
                //update status bar
                receiveBytesCount += e.receivedBytes.Length;
                toolStripStatusRx.Text = "Received: " + receiveBytesCount.ToString();

                //auto reply
                if (autoReplyCbx.Checked)
                {

                    foreach (contains_struct i in Contains_list)
                    {
                        if (Megahunt_USART.Encodes.GetString(e.receivedBytes).Contains(i.Contains_value))
                        {
                            i.num++;
                            list_text();
                            if (i.Reply_value != "")
                            {
                                if (huanhang_check.Checked)
                                    controller.SendDataToCom_huanhang(i.Reply_value);
                                else
                                    controller.SendDataToCom(i.Reply_value);
                                break;
                            }
                        }
                    }

                }
            }
            catch {
                System.Windows.MessageBox.Show("find error");
            }
        }

        /// <summary>
        /// Auto scroll in receive textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void receivetbx_TextChanged(object sender, EventArgs e)
        {
            receivetbx.SelectionStart = receivetbx.Text.Length;
            receivetbx.ScrollToCaret();
        }

        /// <summary>
        /// update time in status bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void statustimer_Tick(object sender, EventArgs e)
        {
            this.statusTimeLabel.Text = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        }

        /// <summary>
        /// open or close serial port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openCloseSpbtn_Click(object sender, EventArgs e)
        {
            if (openCloseSpbtn.Text == "Open")
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

        /// <summary>
        /// Refresh soft to find Serial port device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshbtn_Click(object sender, EventArgs e)
        {
            comListCbx.Items.Clear();
            //Com Ports
            string[] ArrayComPortsNames = SerialPort.GetPortNames();
            if (ArrayComPortsNames.Length == 0)
            {
                statuslabel.Text = "No COM found !";
                openCloseSpbtn.Enabled = false;
            }
            else
            {
                Array.Sort(ArrayComPortsNames);
                for (int i = 0; i < ArrayComPortsNames.Length; i++)
                {
                    comListCbx.Items.Add(ArrayComPortsNames[i]);
                }
                comListCbx.Text = ArrayComPortsNames[0];
                openCloseSpbtn.Enabled = true;
                statuslabel.Text = "OK !";
            }

        }

        /// <summary>
        /// Send data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendbtn_Click(object sender, EventArgs e)
        {
            String sendText = sendtbx.Text;
            bool flag = false;
            if (sendText == null)
            {
                return;
            }
            //set select index to the end
            sendtbx.SelectionStart = sendtbx.TextLength;

            if (sendHexRadiobtn.Checked)
            {
                //If hex radio checked
                //send bytes to serial port
                Byte[] bytes = IController.Hex2Bytes(sendText);
                sendbtn.Enabled = false;//wait return
                flag = controller.SendDataToCom(bytes);
                sendbtn.Enabled = true;
                sendBytesCount += bytes.Length;
            }
            else
            {
                //send String to serial port
                sendbtn.Enabled = false;//wait return
                flag = controller.SendDataToCom(sendText);
                sendbtn.Enabled = true;
                sendBytesCount += sendText.Length;
            }
            if (flag)
            {
                statuslabel.Text = "Send OK !";
            }
            else
            {
                statuslabel.Text = "Send failed !";
            }
            //update status bar
            toolStripStatusTx.Text = "Sent: " + sendBytesCount.ToString();
        }

        /// <summary>
        /// clear text in send area
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearSendbtn_Click(object sender, EventArgs e)
        {
            sendtbx.Text = "";
            toolStripStatusTx.Text = "Sent: 0";
            sendBytesCount = 0;
            addCRCcbx.Checked = false;
        }

        /// <summary>
        /// clear receive text in receive area
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearReceivebtn_Click(object sender, EventArgs e)
        {
            receivetbx.Text = "";
            toolStripStatusRx.Text = "Received: 0";
            receiveBytesCount = 0;
        }

        /// <summary>
        /// String to hex
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void recHexRadiobtn_CheckedChanged(object sender, EventArgs e)
        {
            if (recHexRadiobtn.Checked)
            {
                if (receivetbx.Text == null)
                {
                    return;
                }
                receivetbx.Text = IController.String2Hex(receivetbx.Text);
            }
        }

        /// <summary>
        /// Hex to string
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void recStrRadiobtn_CheckedChanged(object sender, EventArgs e)
        {
            if (recStrRadiobtn.Checked)
            {
                if (receivetbx.Text == null)
                {
                    return;
                }
                receivetbx.Text = IController.Hex2String(receivetbx.Text);
            }
        }

        /// <summary>
        /// String to Hex
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendHexRadiobtn_CheckedChanged(object sender, EventArgs e)
        {
            if (sendHexRadiobtn.Checked)
            {
                if (sendtbx.Text == null)
                {
                    return;
                }
                sendtbx.Text = IController.String2Hex(sendtbx.Text);
                addCRCcbx.Enabled = true;
            }
        }

        /// <summary>
        /// Hex to string
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendStrRadiobtn_CheckedChanged(object sender, EventArgs e)
        {
            if (sendStrRadiobtn.Checked)
            {
                if (sendtbx.Text == null)
                {
                    return;
                }
                sendtbx.Text = IController.Hex2String(sendtbx.Text);
                addCRCcbx.Enabled = false;
            }
        }

        /// <summary>
        /// Filter illegal input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendtbx_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Input Hex, should like: AF-1B-09
            if (sendHexRadiobtn.Checked)
            {
                e.Handled = true;
                int length = sendtbx.SelectionStart;
                switch (length % 3)
                {
                    case 0:
                    case 1:
                        if ((e.KeyChar >= 'a' && e.KeyChar <= 'f')
                            || (e.KeyChar >= 'A' && e.KeyChar <= 'F')
                            || char.IsDigit(e.KeyChar)
                            || (char.IsControl(e.KeyChar) && e.KeyChar != (char)13))
                        {
                            e.Handled = false;
                        }
                        break;
                    case 2:
                        if (e.KeyChar == '-'
                            || (char.IsControl(e.KeyChar) && e.KeyChar != (char)13))
                        {
                            e.Handled = false;
                        }
                        break;
                }
            }
        }


        /// <summary>
        /// Auto send data to serial port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void autoSendcbx_CheckedChanged(object sender, EventArgs e)
        {
            if (autoSendcbx.Checked)
            {
                autoSendtimer.Enabled = true;
                autoSendtimer.Interval = int.Parse(sendIntervalTimetbx.Text);
                autoSendtimer.Start();

                //disable send botton and textbox
                sendIntervalTimetbx.Enabled = false;
                sendtbx.ReadOnly = true;
                sendbtn.Enabled = false;
            }
            else
            {
                autoSendtimer.Enabled = false;
                autoSendtimer.Stop();

                //enable send botton and textbox
                sendIntervalTimetbx.Enabled = true;
                sendtbx.ReadOnly = false;
                sendbtn.Enabled = true;
            }
        }

        private void autoSendtimer_Tick(object sender, EventArgs e)
        {
            sendbtn_Click(sender, e);
        }

        /// <summary>
        /// filter illegal input of auto send interval time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendIntervalTimetbx_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar) || e.KeyChar == '\b')
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Add CRC checkbox changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addCRCcbx_CheckedChanged(object sender, EventArgs e)
        {
            String sendText = sendtbx.Text;
            if (sendText == null || sendText == "")
            {
                addCRCcbx.Checked = false;
                return;
            }
            if (addCRCcbx.Checked)
            {
                //Add 2 bytes CRC to the end of the data
                Byte[] senddata = IController.Hex2Bytes(sendText);
                Byte[] crcbytes = BitConverter.GetBytes(CRC16.Compute(senddata));
                sendText += "-" + BitConverter.ToString(crcbytes, 1, 1);
                sendText += "-" + BitConverter.ToString(crcbytes, 0, 1);
            }
            else
            {
                //Delete 2 bytes CRC to the end of the data
                if (sendText.Length >= 6)
                {
                    sendText = sendText.Substring(0, sendText.Length - 6);
                }
            }
            sendtbx.Text = sendText;
        }

        /// <summary>
        /// save received data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void receivedDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.Filter = "txt file|*.txt";
            saveFileDialog.DefaultExt = ".txt";
            saveFileDialog.FileName = "received.txt";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                String fName = saveFileDialog.FileName;
                System.IO.File.WriteAllText(fName, receivetbx.Text);
            }
        }

        /// <summary>
        /// save send data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.Filter = "txt file|*.txt";
            saveFileDialog.DefaultExt = ".txt";
            saveFileDialog.FileName = "send.txt";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                String fName = saveFileDialog.FileName;
                System.IO.File.WriteAllText(fName, sendtbx.Text);
            }
        }

        /// <summary>
        /// Quit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// about me
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //AboutForm about = new AboutForm();
            //about.StartPosition = FormStartPosition.CenterParent;
            //about.Show();

            //if (about.StartPosition == FormStartPosition.CenterParent)
            //{
            //    var x = Location.X + (Width - about.Width) / 2;
            //    var y = Location.Y + (Height - about.Height) / 2;
            //    about.Location = new Point(Math.Max(x, 0), Math.Max(y, 0));
            //}
        }

        /// <summary>
        /// Help
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //HelpForm help = new HelpForm();
            //help.StartPosition = FormStartPosition.CenterParent;
            //help.Show();

            //if (help.StartPosition == FormStartPosition.CenterParent)
            //{
            //    var x = Location.X + (Width - help.Width) / 2;
            //    var y = Location.Y + (Height - help.Height) / 2;
            //    help.Location = new Point(Math.Max(x, 0), Math.Max(y, 0));
            //}
        }



        //字符集
        public static Encoding Encodes = Encoding.Unicode;
        //更换字符集
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            sendtbx.Text = "";
            receivetbx.Text = "";

            switch (Encode_box.SelectedIndex)
            {
                case 0:
                    Encodes = Encoding.Default;
                    break;
                case 1:
                    Encodes = Encoding.ASCII;
                    break;
                case 2:
                    Encodes = Encoding.Unicode;
                    break;
                case 3:
                    Encodes = Encoding.BigEndianUnicode;
                    break;
                case 4:
                    Encodes = Encoding.UTF7;
                    break;
                case 5:
                    Encodes = Encoding.UTF8;
                    break;
                case 6:
                    Encodes = Encoding.UTF32;
                    break;
                case 7:
                    Encodes = Encoding.GetEncoding("gb2312");
                    break;
                case 8:
                    Encodes = Encoding.GetEncoding("GBK");
                    break;

            }
        }
        //增加或减少
        public class contains_struct
        {
            public string Contains_value;//{ set; get; }

            public string Reply_value; //{ set; get; }
            public int num;
        }
        public static List<contains_struct> Contains_list = new List<contains_struct>();
        private void Add_Click(object sender, EventArgs e)
        {
            if (Contains_list.FindIndex(t=>t.Contains_value==Contains.Text)!=-1)
            {
                Contains_list.RemoveAt(Contains_list.FindIndex(t => t.Contains_value == Contains.Text));
                Contains_list.Add(new contains_struct { Contains_value = Contains.Text, Reply_value = Reply.Text,num=0 });
            }
            else
            {
                Contains_list.Add(new contains_struct { Contains_value=Contains.Text, Reply_value= Reply.Text ,num=0});
            }
            list_text();
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            if (Contains_list.FindIndex(t => t.Contains_value == Contains.Text) != -1)
            {
                Contains_list.RemoveAt(Contains_list.FindIndex(t => t.Contains_value == Contains.Text));
            }
            list_text();

        }
        public void list_text()
        {
            string temp = "";

            foreach (contains_struct i in Contains_list)
            {
                temp += "Contains\t" + i.Contains_value+"\r\n";
                temp +=  "Reply\t" + i.Reply_value + "\r\n";
                temp += "nums\t" + i.num.ToString() + "\r\n";
                temp += "---------------------------------------------\r\n";
            }
            textBox3.Text = temp;



        }

        private void button2_Click(object sender, EventArgs e)
        {
            list_text();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Contains_list.Clear();
            list_text();
        }

        private void huanhang_check_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void Reply_TextChanged(object sender, EventArgs e)
        {

        }

        private void Contains_TextChanged(object sender, EventArgs e)
        {

        }

        private void autoReplyCbx_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Megahunt_USART_FormClosing(object sender, FormClosingEventArgs e)
        {
            MainForm.usart_static.Visible = false;
            e.Cancel = true;
            return;
        }
        //串口发送函数
        public static bool Send(string strings,bool huanhang)
        {
            if (huanhang)
            {
                if (form_st.controller.SendDataToCom_huanhang(strings))
                {
                    return true;
                }
            }
            else
            {
                if (form_st.controller.SendDataToCom(strings))
                {

                    return true;
                }
            }
            return false;
        }

    }
}

