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
using VISAInstrument.AutoTest;
using VISAInstrument.设备;
using Sunny.UI;
//using MaterialSkin.Controls;
//using MaterialSkin;

namespace VISAInstrument
{
    //public interface MainView
    //{
    //    void openchanged(object sender,bool isopen);
    //}

    public partial class MainForm : UIForm
    {

        private MaterialSkinManager materialSkinManager2;
        public MainForm()
        {
            InitializeComponent();
            //materialSkinManager_Config();
        }
        public static FrmMain wave_frmMain=null;
        //设置开始风格
        //public void materialSkinManager_Config()
        //{           

        //    materialSkinManager2 = MaterialSkinManager.Instance;             
        //    materialSkinManager2.EnforceBackcolorOnAllComponents = true;            
        //    materialSkinManager2.AddFormToManage(this); 
        //    materialSkinManager2.Theme = MaterialSkinManager.Themes.DARK;
        //    materialSkinManager2.ColorScheme = new ColorScheme(
        //            Primary.BlueGrey700,
        //            Primary.BlueGrey900,
        //            Primary.BlueGrey400,
        //            Accent.LightBlue200,
        //            TextShade.WHITE);

        //}
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
        //打开信号发生器
        private void xinhaofashneg_open_Click(object sender, EventArgs e)
        {
            if (wave_frmMain == null)
            {
                FrmMain wave1 = new FrmMain();
                wave1.StartPosition = FormStartPosition.CenterScreen;
                wave1.Show();
                wave_frmMain = wave1;
                wave1.open_change_event += new FrmMain.MyDelegate(openchanged);
            }
            else {
                wave_frmMain.Visible = true;

            }

        }
        //打开台式万用表
        public static FrmMain2 wave_frmMain2 = null;
        private void wendu_open_Click(object sender, EventArgs e)
        {
            if (wave_frmMain2== null)
            {
                FrmMain2 wave2 = new FrmMain2();
                wave2.StartPosition = FormStartPosition.CenterScreen;
                wave2.Show();
                wave_frmMain2 = wave2;
                wave2.open_change_event += new FrmMain2.MyDelegate(openchanged);
            }
            else
            {
                wave_frmMain2.Visible = true;

            }
        }
        //可调电源连接按钮
        public static FrmMain3 wave_frmMain3 = null;
        private void button8_Click(object sender, EventArgs e)
        {
            if (wave_frmMain3== null)
            {
                FrmMain3 wave3 = new FrmMain3();
                wave3.StartPosition = FormStartPosition.CenterScreen;
                wave3.Show();
                wave_frmMain3 = wave3;
                wave3.open_change_event += new FrmMain3.MyDelegate(openchanged);
            }
            else
            {
                wave_frmMain3.Visible = true;

            }

        }
        //温箱连接按钮
        public static IoTClient.Tool.Modbus_Link wave_frmMain4 = null;
        private void button1_Click_1(object sender, EventArgs e)
        {
            if (wave_frmMain4 == null)
            {
                IoTClient.Tool.Modbus_Link wave4 = new IoTClient.Tool.Modbus_Link();
                wave4.StartPosition = FormStartPosition.CenterScreen;
                wave4.Show();
                wave_frmMain4 = wave4;
                wave4.modbusTcp.open_change_event += new IoTClient.Tool.ModbusTcpControl.MyDelegate(openchanged);
            }
            else
            {
                wave_frmMain4.Visible = true;

            }
        }
        //关闭信号发生器
        private void button1_Click(object sender, EventArgs e)
        {
        
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                wave_frmMain2.Close();
                wave_frmMain2 = null;
            }
            catch
            {


            }
        }



        //阻止随意关闭
        public static int Wave_close_flag = 0;
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Wave_close_flag>0)
            {
                e.Cancel = true;
                Wave_close_flag --;
                return;

            }
        }
        public void openchanged(string sender, bool isopen)
        {
           // materialSwitch1.Checked = true;
            if (sender == "信号发生器")
            {
                //信号发生器自动化
               // ARB_OPEN.Enabled = isopen;
                //信号发生器功能项
                button3.Enabled= isopen;
                materialSwitch1.Checked = isopen; 
            }
            else if (sender == "数字万用表")
            {
                //数字万用表功夫功能
                button4.Enabled = isopen;
                //ARB_OPEN.Enabled = isopen;
                materialSwitch2.Checked = isopen;
            }
            else if (sender == "可调电源")
            {
                //数字万用表自动化
                button7.Enabled = isopen;
                //ARB_OPEN.Enabled = isopen;
                materialSwitch5.Checked = isopen;
             
            }
            else if (sender == "温箱")
            {
                //数字万用表自动化
                button5.Enabled = isopen;
                //ARB_OPEN.Enabled = isopen;
                materialSwitch4.Checked = isopen;

            }
            else if (sender == "usart")
            {
                //ARB_OPEN.Enabled = isopen;
                materialSwitch3.Checked = isopen;
            }
            else if (sender == "usart2")
            {
                //ARB_OPEN.Enabled = isopen;
                Usart2_check.Checked = isopen;
            }
        }
        //public void materialSwitch4_check(bool isopen)
        //{
        //    materialSwitch4.Checked = isopen;
        //}
        //信号发生器开关按钮
        private void materialSwitch1_CheckedChanged(object sender, EventArgs e)
        {

            materialSwitch1.Enabled = materialSwitch1.Checked;
            if (materialSwitch1.Checked == false)
            {
                try
                {
                    wave_frmMain.Close();
                    wave_frmMain = null;
                    openchanged("信号发生器", false);
                }
                catch
                {


                }
            }
        }
        //数字万用表开关
        private void materialSwitch2_CheckedChanged(object sender, EventArgs e)
        {

            materialSwitch2.Enabled = materialSwitch2.Checked;
            if (materialSwitch2.Checked == false)
            {
                try
                {
                    
                    wave_frmMain2.Close();
                    wave_frmMain2 = null;
                    openchanged("数字万用表", false);
                }
                catch
                {


                }
            }
        }
        //温箱控制开关
        private void materialSwitch4_CheckedChanged(object sender, EventArgs e)
        {
            materialSwitch4.Enabled = materialSwitch4.Checked;
            if (materialSwitch4.Checked == false)
            {
                try
                {
                    wave_frmMain4.modbusTcp.CloseAll();
                    wave_frmMain4.Close();
                    wave_frmMain4 = null;
                    openchanged("温箱", false);
                }
                catch
                {


                }
            }
        }
        //可调电源控制开关
        private void materialSwitch5_CheckedChanged(object sender, EventArgs e)
        {
            materialSwitch5.Enabled = materialSwitch5.Checked;
            if (materialSwitch5.Checked == false)
            {
                try
                {

                    wave_frmMain3.Close();
                    wave_frmMain3 = null;
                    openchanged("可调电源", false);
                }
                catch
                {


                }
            }

        }
        //串口采集开关
        private void materialSwitch3_CheckedChanged(object sender, EventArgs e)
        {
            materialSwitch3.Enabled = materialSwitch3.Checked;
            if (materialSwitch3.Checked == false)
            {
                try
                {
                    usart_static.controller.CloseSerialPort();
                    usart_static.Close();
                    usart_static = null;
                    openchanged("usart", false);
                }
                catch
                {


                }
            }
        }


        //打开串口连接器
        public static Megahunt_USART usart_static = null;
        private void button2_Click_1(object sender, EventArgs e)
        {
            if(usart_static == null)
            {
                Megahunt_USART usart = new Megahunt_USART();
                IController controller = new IController(usart);
                usart.StartPosition = FormStartPosition.CenterScreen;
                usart.Show();
                usart_static = usart;
                usart.open_change_event += new Megahunt_USART.MyDelegate(openchanged);
            }
            else
            {
                usart_static.Visible = true;
            }
        }

        //打开串口连接器
        public static Megahunt_USART2 usart2_static = null;
        private void Usart2_button_Click(object sender, EventArgs e)
        {
            if (usart2_static == null)
            {
                Megahunt_USART2 usart = new Megahunt_USART2();
                IController controller = new IController(usart);
                usart.StartPosition = FormStartPosition.CenterScreen;
                usart.Show();
                usart2_static = usart;
                usart.open_change_event += new Megahunt_USART2.MyDelegate(openchanged);
            }
            else
            {
                usart2_static.Visible = true;
            }
        }
        //打开信号发生器功能选项
        private void button3_Click(object sender, EventArgs e)
        {
            Wave wave = new Wave();
            wave.StartPosition = FormStartPosition.CenterScreen;
            Wave_close_flag++;
            IController controller = new IController(wave);
            wave.Show();
        }
        //台式万用表功能选项
        private void button4_Click(object sender, EventArgs e)
        {
            wanyongbiao Wave = new wanyongbiao();
            Wave.StartPosition = FormStartPosition.CenterScreen;
            Wave_close_flag++;
            Wave.Show();
        }
        //可调电源功能选项
        private void button7_Click(object sender, EventArgs e)
        {
            power Wave = new power();
            Wave.StartPosition = FormStartPosition.CenterScreen;
            Wave_close_flag++;
            Wave.Show();
        }
        //温箱功能选项
        private void button5_Click(object sender, EventArgs e)
        {
           wenxiang Wave = new wenxiang();
            Wave.StartPosition = FormStartPosition.CenterScreen;
            Wave_close_flag++;
            Wave.Show();
        }
        //上下电测试按钮
        private void ARB_OPEN_Click(object sender, EventArgs e)
        {
            if (materialSwitch1.Checked)
            {
                Wave wave = new Wave();
                wave.StartPosition = FormStartPosition.CenterScreen;
                Wave_close_flag++;
                IController controller = new IController(wave);
                //this.Enabled = false;
                wave.Show();
            }
            else
            {
                xinhaofashneg_open.BackColor = Color.Salmon;
                Delay(1000);
                xinhaofashneg_open.BackColor = Color.White;

            }
        }
        //功耗测试按钮
        private void button9_Click(object sender, EventArgs e)
        {
            PowerTest wave = new PowerTest();
            wave.StartPosition = FormStartPosition.CenterScreen;
            wave.Show();
            if (materialSwitch2.Checked&& materialSwitch3.Checked&& materialSwitch4.Checked&& materialSwitch5.Checked)
            {
                //PowerTest wave = new PowerTest();
                //wave.StartPosition = FormStartPosition.CenterScreen;
                //wave.Show();
            }
            else
            {

                wendu_open.BackColor = Color.Salmon;
                button8.BackColor = Color.Salmon;
                button1.BackColor = Color.Salmon;
                button2.BackColor = Color.Salmon;

                Delay(1000);

                wendu_open.BackColor = Color.White;
                button8.BackColor = Color.White;
                button1.BackColor = Color.White;
                button2.BackColor = Color.White;

            }
        }
        //电压校准功能
        private void button10_Click(object sender, EventArgs e)
        {

            if (materialSwitch2.Checked && materialSwitch5.Checked)
                //if (true)
                {
               Voltage_calibration wave = new Voltage_calibration();
                wave.StartPosition = FormStartPosition.CenterScreen;
                wave.Show();
            }
            else
            {
                wendu_open.BackColor = Color.Salmon;
                button8.BackColor = Color.Salmon;
                Delay(1000);
                wendu_open.BackColor = Color.White;
                button8.BackColor = Color.White;

            }
        }

        //DAC测试按钮
        private void button11_Click(object sender, EventArgs e)
        {

            DAC_Test1 wave = new DAC_Test1(materialSwitch4.Checked, materialSwitch5.Checked);
            wave.StartPosition = FormStartPosition.CenterScreen;
            wave.Show();
            if (materialSwitch3.Checked&& materialSwitch2.Checked)
            {
                //DAC_Test1 wave = new DAC_Test1(materialSwitch4.Checked, materialSwitch5.Checked);
                //wave.StartPosition = FormStartPosition.CenterScreen;
                //wave.Show();
            }
            else
            {
                wendu_open.BackColor = Color.Salmon;
                button2.BackColor = Color.Salmon;
                Delay(1000);
                button2.BackColor = Color.White;
                wendu_open.BackColor = Color.White;

            }
        }
        //ADC测试按钮
        private void button12_Click(object sender, EventArgs e)
        {
            ADC_Test1 wave = new ADC_Test1(materialSwitch4.Checked, materialSwitch5.Checked);
            wave.StartPosition = FormStartPosition.CenterScreen;
            wave.Show();
            if (materialSwitch3.Checked && materialSwitch2.Checked)
            {

            }
            else
            {
                wendu_open.BackColor = Color.Salmon;
                button2.BackColor = Color.Salmon;
                Delay(1000);
                button2.BackColor = Color.White;
                wendu_open.BackColor = Color.White;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {

        }

        private void Usart2_check_CheckedChanged(object sender, EventArgs e)
        {
            Usart2_check.Enabled = Usart2_check.Checked;
            if (Usart2_check.Checked == false)
            {
                try
                {
                    usart2_static.controller.CloseSerialPort();
                    usart2_static.Close();
                    usart2_static = null;
                    openchanged("usart2", false);
                }
                catch
                {


                }
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {

        }

        private void button14_Click(object sender, EventArgs e)
        {
            MCU_Calib wave = new MCU_Calib();
            wave.StartPosition = FormStartPosition.CenterScreen;
            wave.Show();
        }

    }
}
