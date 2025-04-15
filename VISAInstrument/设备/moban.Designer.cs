namespace VISAInstrument
{
    partial class moban
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(moban));
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.Test_tab = new System.Windows.Forms.TabPage();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.INPUT_TAB = new System.Windows.Forms.TabPage();
            this.tabcontrl = new MaterialSkin.Controls.MaterialTabControl();
            this.tabcontrl.SuspendLayout();
            this.SuspendLayout();
            // 
            // Test_tab
            // 
            this.Test_tab.Location = new System.Drawing.Point(4, 25);
            this.Test_tab.Margin = new System.Windows.Forms.Padding(4);
            this.Test_tab.Name = "Test_tab";
            this.Test_tab.Padding = new System.Windows.Forms.Padding(4);
            this.Test_tab.Size = new System.Drawing.Size(1375, 819);
            this.Test_tab.TabIndex = 3;
            this.Test_tab.Text = "自动化测试";
            this.Test_tab.UseVisualStyleBackColor = true;
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage1.Size = new System.Drawing.Size(1375, 819);
            this.tabPage1.TabIndex = 2;
            this.tabPage1.Text = "标准波形";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // INPUT_TAB
            // 
            this.INPUT_TAB.ImageKey = "波形图.png";
            this.INPUT_TAB.Location = new System.Drawing.Point(4, 25);
            this.INPUT_TAB.Margin = new System.Windows.Forms.Padding(0);
            this.INPUT_TAB.Name = "INPUT_TAB";
            this.INPUT_TAB.Padding = new System.Windows.Forms.Padding(4);
            this.INPUT_TAB.Size = new System.Drawing.Size(585, 422);
            this.INPUT_TAB.TabIndex = 1;
            this.INPUT_TAB.Text = "导入波形";
            this.INPUT_TAB.UseVisualStyleBackColor = true;
            // 
            // tabcontrl
            // 
            this.tabcontrl.Controls.Add(this.INPUT_TAB);
            this.tabcontrl.Controls.Add(this.tabPage1);
            this.tabcontrl.Controls.Add(this.Test_tab);
            this.tabcontrl.Depth = 0;
            this.tabcontrl.Location = new System.Drawing.Point(4, 69);
            this.tabcontrl.Margin = new System.Windows.Forms.Padding(0);
            this.tabcontrl.MouseState = MaterialSkin.MouseState.HOVER;
            this.tabcontrl.Multiline = true;
            this.tabcontrl.Name = "tabcontrl";
            this.tabcontrl.SelectedIndex = 0;
            this.tabcontrl.Size = new System.Drawing.Size(593, 451);
            this.tabcontrl.TabIndex = 50;
            // 
            // moban
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(602, 530);
            this.Controls.Add(this.tabcontrl);
            this.DrawerTabControl = this.tabcontrl;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "moban";
            this.Padding = new System.Windows.Forms.Padding(4, 80, 4, 4);
            this.Sizable = false;
            this.Text = "ARB波形编辑器";
            this.tabcontrl.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.TabPage Test_tab;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage INPUT_TAB;
        private MaterialSkin.Controls.MaterialTabControl tabcontrl;
    }
}