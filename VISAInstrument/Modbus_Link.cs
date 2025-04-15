using IoTClient.Tool;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IoTClient.Tool
{
    public partial class Modbus_Link : Form
    {
        public static bool Wave_close_flag=false;
        public ModbusTcpControl modbusTcp = new ModbusTcpControl(false);
        public Modbus_Link()
        {
            InitializeComponent();
  
            modbusTcp.Dock = DockStyle.Fill;
            tabPage1 .Controls.Add(modbusTcp);
           // CheckForIllegalCrossThreadCalls = false;
        }

        private void Modbus_Link_FormClosing(object sender, FormClosingEventArgs e)
        {

            VISAInstrument.MainForm.wave_frmMain4.Visible = false;
            e.Cancel = true;
        }
    }
}
