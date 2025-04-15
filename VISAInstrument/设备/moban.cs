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

    public partial class moban : MaterialForm
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

        public moban()
        {

            InitializeComponent();
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


    }
}
