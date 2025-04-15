namespace VISAInstrument
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.xinhaofashneg_open = new System.Windows.Forms.Button();
            this.wendu_open = new System.Windows.Forms.Button();
            this.materialSwitch1 = new MaterialSkin.Controls.MaterialSwitch();
            this.materialSwitch2 = new MaterialSkin.Controls.MaterialSwitch();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button13 = new System.Windows.Forms.Button();
            this.Usart2_check = new MaterialSkin.Controls.MaterialSwitch();
            this.Usart2_button = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.materialSwitch5 = new MaterialSkin.Controls.MaterialSwitch();
            this.button8 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.materialSwitch3 = new MaterialSkin.Controls.MaterialSwitch();
            this.materialSwitch4 = new MaterialSkin.Controls.MaterialSwitch();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.ARB_OPEN = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button14 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // xinhaofashneg_open
            // 
            this.xinhaofashneg_open.BackColor = System.Drawing.Color.White;
            this.xinhaofashneg_open.Cursor = System.Windows.Forms.Cursors.Hand;
            this.xinhaofashneg_open.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.xinhaofashneg_open.ForeColor = System.Drawing.SystemColors.ControlText;
            this.xinhaofashneg_open.Location = new System.Drawing.Point(17, 35);
            this.xinhaofashneg_open.Margin = new System.Windows.Forms.Padding(4);
            this.xinhaofashneg_open.Name = "xinhaofashneg_open";
            this.xinhaofashneg_open.Size = new System.Drawing.Size(217, 45);
            this.xinhaofashneg_open.TabIndex = 0;
            this.xinhaofashneg_open.Text = "信号发生器(Link)";
            this.xinhaofashneg_open.UseVisualStyleBackColor = false;
            this.xinhaofashneg_open.Click += new System.EventHandler(this.xinhaofashneg_open_Click);
            // 
            // wendu_open
            // 
            this.wendu_open.BackColor = System.Drawing.Color.White;
            this.wendu_open.Cursor = System.Windows.Forms.Cursors.Hand;
            this.wendu_open.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.wendu_open.Location = new System.Drawing.Point(17, 88);
            this.wendu_open.Margin = new System.Windows.Forms.Padding(4);
            this.wendu_open.Name = "wendu_open";
            this.wendu_open.Size = new System.Drawing.Size(217, 42);
            this.wendu_open.TabIndex = 1;
            this.wendu_open.Text = "台式万用表(Link)";
            this.wendu_open.UseVisualStyleBackColor = false;
            this.wendu_open.Click += new System.EventHandler(this.wendu_open_Click);
            // 
            // materialSwitch1
            // 
            this.materialSwitch1.AutoSize = true;
            this.materialSwitch1.Depth = 0;
            this.materialSwitch1.Enabled = false;
            this.materialSwitch1.Location = new System.Drawing.Point(457, 35);
            this.materialSwitch1.Margin = new System.Windows.Forms.Padding(0);
            this.materialSwitch1.MouseLocation = new System.Drawing.Point(-1, -1);
            this.materialSwitch1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialSwitch1.Name = "materialSwitch1";
            this.materialSwitch1.Ripple = true;
            this.materialSwitch1.Size = new System.Drawing.Size(58, 37);
            this.materialSwitch1.TabIndex = 6;
            this.materialSwitch1.UseVisualStyleBackColor = true;
            this.materialSwitch1.CheckedChanged += new System.EventHandler(this.materialSwitch1_CheckedChanged);
            // 
            // materialSwitch2
            // 
            this.materialSwitch2.AutoSize = true;
            this.materialSwitch2.Depth = 0;
            this.materialSwitch2.Enabled = false;
            this.materialSwitch2.Location = new System.Drawing.Point(457, 81);
            this.materialSwitch2.Margin = new System.Windows.Forms.Padding(0);
            this.materialSwitch2.MouseLocation = new System.Drawing.Point(-1, -1);
            this.materialSwitch2.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialSwitch2.Name = "materialSwitch2";
            this.materialSwitch2.Ripple = true;
            this.materialSwitch2.Size = new System.Drawing.Size(58, 37);
            this.materialSwitch2.TabIndex = 7;
            this.materialSwitch2.UseVisualStyleBackColor = true;
            this.materialSwitch2.CheckedChanged += new System.EventHandler(this.materialSwitch2_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button13);
            this.groupBox2.Controls.Add(this.Usart2_check);
            this.groupBox2.Controls.Add(this.Usart2_button);
            this.groupBox2.Controls.Add(this.button7);
            this.groupBox2.Controls.Add(this.materialSwitch5);
            this.groupBox2.Controls.Add(this.button8);
            this.groupBox2.Controls.Add(this.button6);
            this.groupBox2.Controls.Add(this.button5);
            this.groupBox2.Controls.Add(this.button4);
            this.groupBox2.Controls.Add(this.button3);
            this.groupBox2.Controls.Add(this.materialSwitch3);
            this.groupBox2.Controls.Add(this.materialSwitch4);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.xinhaofashneg_open);
            this.groupBox2.Controls.Add(this.materialSwitch2);
            this.groupBox2.Controls.Add(this.wendu_open);
            this.groupBox2.Controls.Add(this.materialSwitch1);
            this.groupBox2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox2.Location = new System.Drawing.Point(16, 39);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(518, 341);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "设备连接";
            // 
            // button13
            // 
            this.button13.BackColor = System.Drawing.Color.White;
            this.button13.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button13.Enabled = false;
            this.button13.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button13.Location = new System.Drawing.Point(242, 286);
            this.button13.Margin = new System.Windows.Forms.Padding(4);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(211, 42);
            this.button13.TabIndex = 21;
            this.button13.Text = "串口2拓展功能";
            this.button13.UseVisualStyleBackColor = false;
            this.button13.Click += new System.EventHandler(this.button13_Click);
            // 
            // Usart2_check
            // 
            this.Usart2_check.AutoSize = true;
            this.Usart2_check.Depth = 0;
            this.Usart2_check.Enabled = false;
            this.Usart2_check.Location = new System.Drawing.Point(457, 290);
            this.Usart2_check.Margin = new System.Windows.Forms.Padding(0);
            this.Usart2_check.MouseLocation = new System.Drawing.Point(-1, -1);
            this.Usart2_check.MouseState = MaterialSkin.MouseState.HOVER;
            this.Usart2_check.Name = "Usart2_check";
            this.Usart2_check.Ripple = true;
            this.Usart2_check.Size = new System.Drawing.Size(58, 37);
            this.Usart2_check.TabIndex = 20;
            this.Usart2_check.UseVisualStyleBackColor = true;
            this.Usart2_check.CheckedChanged += new System.EventHandler(this.Usart2_check_CheckedChanged);
            // 
            // Usart2_button
            // 
            this.Usart2_button.BackColor = System.Drawing.Color.White;
            this.Usart2_button.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Usart2_button.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Usart2_button.Location = new System.Drawing.Point(17, 285);
            this.Usart2_button.Margin = new System.Windows.Forms.Padding(4);
            this.Usart2_button.Name = "Usart2_button";
            this.Usart2_button.Size = new System.Drawing.Size(217, 42);
            this.Usart2_button.TabIndex = 19;
            this.Usart2_button.Text = "USART2";
            this.Usart2_button.UseVisualStyleBackColor = false;
            this.Usart2_button.Click += new System.EventHandler(this.Usart2_button_Click);
            // 
            // button7
            // 
            this.button7.BackColor = System.Drawing.Color.White;
            this.button7.Enabled = false;
            this.button7.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button7.Location = new System.Drawing.Point(242, 134);
            this.button7.Margin = new System.Windows.Forms.Padding(4);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(211, 42);
            this.button7.TabIndex = 18;
            this.button7.Text = "稳压电源功能菜单";
            this.button7.UseVisualStyleBackColor = false;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // materialSwitch5
            // 
            this.materialSwitch5.AutoSize = true;
            this.materialSwitch5.Depth = 0;
            this.materialSwitch5.Enabled = false;
            this.materialSwitch5.Location = new System.Drawing.Point(457, 128);
            this.materialSwitch5.Margin = new System.Windows.Forms.Padding(0);
            this.materialSwitch5.MouseLocation = new System.Drawing.Point(-1, -1);
            this.materialSwitch5.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialSwitch5.Name = "materialSwitch5";
            this.materialSwitch5.Ripple = true;
            this.materialSwitch5.Size = new System.Drawing.Size(58, 37);
            this.materialSwitch5.TabIndex = 17;
            this.materialSwitch5.UseVisualStyleBackColor = true;
            this.materialSwitch5.CheckedChanged += new System.EventHandler(this.materialSwitch5_CheckedChanged);
            // 
            // button8
            // 
            this.button8.BackColor = System.Drawing.Color.White;
            this.button8.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button8.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button8.Location = new System.Drawing.Point(17, 134);
            this.button8.Margin = new System.Windows.Forms.Padding(4);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(217, 42);
            this.button8.TabIndex = 16;
            this.button8.Text = "稳压电源(Link)";
            this.button8.UseVisualStyleBackColor = false;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button6
            // 
            this.button6.BackColor = System.Drawing.Color.White;
            this.button6.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button6.Enabled = false;
            this.button6.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button6.Location = new System.Drawing.Point(242, 236);
            this.button6.Margin = new System.Windows.Forms.Padding(4);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(211, 42);
            this.button6.TabIndex = 15;
            this.button6.Text = "串口1拓展功能";
            this.button6.UseVisualStyleBackColor = false;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button5
            // 
            this.button5.BackColor = System.Drawing.Color.White;
            this.button5.Enabled = false;
            this.button5.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button5.Location = new System.Drawing.Point(242, 184);
            this.button5.Margin = new System.Windows.Forms.Padding(4);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(211, 45);
            this.button5.TabIndex = 14;
            this.button5.Text = "温箱功能菜单";
            this.button5.UseVisualStyleBackColor = false;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.White;
            this.button4.Enabled = false;
            this.button4.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button4.Location = new System.Drawing.Point(242, 88);
            this.button4.Margin = new System.Windows.Forms.Padding(4);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(211, 42);
            this.button4.TabIndex = 13;
            this.button4.Text = "数字万用表功能菜单";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.White;
            this.button3.Enabled = false;
            this.button3.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button3.Location = new System.Drawing.Point(242, 35);
            this.button3.Margin = new System.Windows.Forms.Padding(4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(211, 42);
            this.button3.TabIndex = 12;
            this.button3.Text = "信号发生器功能菜单";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // materialSwitch3
            // 
            this.materialSwitch3.AutoSize = true;
            this.materialSwitch3.Depth = 0;
            this.materialSwitch3.Enabled = false;
            this.materialSwitch3.Location = new System.Drawing.Point(457, 236);
            this.materialSwitch3.Margin = new System.Windows.Forms.Padding(0);
            this.materialSwitch3.MouseLocation = new System.Drawing.Point(-1, -1);
            this.materialSwitch3.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialSwitch3.Name = "materialSwitch3";
            this.materialSwitch3.Ripple = true;
            this.materialSwitch3.Size = new System.Drawing.Size(58, 37);
            this.materialSwitch3.TabIndex = 11;
            this.materialSwitch3.UseVisualStyleBackColor = true;
            this.materialSwitch3.CheckedChanged += new System.EventHandler(this.materialSwitch3_CheckedChanged);
            // 
            // materialSwitch4
            // 
            this.materialSwitch4.AutoSize = true;
            this.materialSwitch4.Depth = 0;
            this.materialSwitch4.Enabled = false;
            this.materialSwitch4.Location = new System.Drawing.Point(457, 181);
            this.materialSwitch4.Margin = new System.Windows.Forms.Padding(0);
            this.materialSwitch4.MouseLocation = new System.Drawing.Point(-1, -1);
            this.materialSwitch4.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialSwitch4.Name = "materialSwitch4";
            this.materialSwitch4.Ripple = true;
            this.materialSwitch4.Size = new System.Drawing.Size(58, 37);
            this.materialSwitch4.TabIndex = 10;
            this.materialSwitch4.UseVisualStyleBackColor = true;
            this.materialSwitch4.CheckedChanged += new System.EventHandler(this.materialSwitch4_CheckedChanged);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.White;
            this.button1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.button1.Location = new System.Drawing.Point(17, 184);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(217, 45);
            this.button1.TabIndex = 8;
            this.button1.Text = "温箱控制(Link)";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.White;
            this.button2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button2.Location = new System.Drawing.Point(17, 236);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(217, 42);
            this.button2.TabIndex = 9;
            this.button2.Text = "USART1";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click_1);
            // 
            // ARB_OPEN
            // 
            this.ARB_OPEN.BackColor = System.Drawing.Color.White;
            this.ARB_OPEN.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ARB_OPEN.Location = new System.Drawing.Point(17, 35);
            this.ARB_OPEN.Margin = new System.Windows.Forms.Padding(4);
            this.ARB_OPEN.Name = "ARB_OPEN";
            this.ARB_OPEN.Size = new System.Drawing.Size(237, 38);
            this.ARB_OPEN.TabIndex = 12;
            this.ARB_OPEN.Text = "上下电测试(ARB)";
            this.ARB_OPEN.UseVisualStyleBackColor = false;
            this.ARB_OPEN.Click += new System.EventHandler(this.ARB_OPEN_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button14);
            this.groupBox1.Controls.Add(this.button12);
            this.groupBox1.Controls.Add(this.button11);
            this.groupBox1.Controls.Add(this.button10);
            this.groupBox1.Controls.Add(this.button9);
            this.groupBox1.Controls.Add(this.ARB_OPEN);
            this.groupBox1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox1.Location = new System.Drawing.Point(542, 39);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(271, 341);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "自动化测试";
            // 
            // button14
            // 
            this.button14.BackColor = System.Drawing.Color.White;
            this.button14.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button14.Location = new System.Drawing.Point(17, 172);
            this.button14.Margin = new System.Windows.Forms.Padding(4);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(237, 38);
            this.button14.TabIndex = 17;
            this.button14.Text = "MCU校准";
            this.button14.UseVisualStyleBackColor = false;
            this.button14.Click += new System.EventHandler(this.button14_Click);
            // 
            // button12
            // 
            this.button12.BackColor = System.Drawing.Color.White;
            this.button12.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button12.Location = new System.Drawing.Point(17, 264);
            this.button12.Margin = new System.Windows.Forms.Padding(4);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(237, 38);
            this.button12.TabIndex = 16;
            this.button12.Text = "ADC测试";
            this.button12.UseVisualStyleBackColor = false;
            this.button12.Click += new System.EventHandler(this.button12_Click);
            // 
            // button11
            // 
            this.button11.BackColor = System.Drawing.Color.White;
            this.button11.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button11.Location = new System.Drawing.Point(17, 218);
            this.button11.Margin = new System.Windows.Forms.Padding(4);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(237, 38);
            this.button11.TabIndex = 15;
            this.button11.Text = "DAC测试";
            this.button11.UseVisualStyleBackColor = false;
            this.button11.Click += new System.EventHandler(this.button11_Click);
            // 
            // button10
            // 
            this.button10.BackColor = System.Drawing.Color.White;
            this.button10.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button10.Location = new System.Drawing.Point(17, 128);
            this.button10.Margin = new System.Windows.Forms.Padding(4);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(237, 38);
            this.button10.TabIndex = 14;
            this.button10.Text = "电压校准";
            this.button10.UseVisualStyleBackColor = false;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // button9
            // 
            this.button9.BackColor = System.Drawing.Color.White;
            this.button9.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button9.Location = new System.Drawing.Point(17, 80);
            this.button9.Margin = new System.Windows.Forms.Padding(4);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(237, 38);
            this.button9.TabIndex = 13;
            this.button9.Text = "功耗测试(D16)";
            this.button9.UseVisualStyleBackColor = false;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(824, 390);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Opacity = 0.96D;
            this.RectColor = System.Drawing.Color.Black;
            this.Style = Sunny.UI.UIStyle.Custom;
            this.Text = "MH VISA设备控制器";
            this.TitleColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.TitleFont = new System.Drawing.Font("微软雅黑", 9F);
            this.ZoomScaleRect = new System.Drawing.Rectangle(19, 19, 937, 691);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button xinhaofashneg_open;
        private System.Windows.Forms.Button wendu_open;
        private MaterialSkin.Controls.MaterialSwitch materialSwitch1;
        private MaterialSkin.Controls.MaterialSwitch materialSwitch2;
        private System.Windows.Forms.GroupBox groupBox2;
        private MaterialSkin.Controls.MaterialSwitch materialSwitch3;
        private MaterialSkin.Controls.MaterialSwitch materialSwitch4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button7;
        private MaterialSkin.Controls.MaterialSwitch materialSwitch5;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button ARB_OPEN;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.Button button13;
        private MaterialSkin.Controls.MaterialSwitch Usart2_check;
        private System.Windows.Forms.Button Usart2_button;
        private System.Windows.Forms.Button button14;
    }
}