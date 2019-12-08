namespace StBleCommTest
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lb_version = new System.Windows.Forms.Label();
            this.bt_scan = new System.Windows.Forms.Button();
            this.bt_connect = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.t_midi_rx = new System.Windows.Forms.TextBox();
            this.t_midi_tx = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cb_devices = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.t_uart_rx = new System.Windows.Forms.TextBox();
            this.t_uart_tx = new System.Windows.Forms.TextBox();
            this.t_log = new System.Windows.Forms.RichTextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.web = new System.Windows.Forms.WebBrowser();
            this.bt_menu = new System.Windows.Forms.Button();
            this.b_home = new System.Windows.Forms.Button();
            this.t_addr = new System.Windows.Forms.TextBox();
            this.b_go = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // lb_version
            // 
            this.lb_version.AutoSize = true;
            this.lb_version.Location = new System.Drawing.Point(72, 668);
            this.lb_version.Name = "lb_version";
            this.lb_version.Size = new System.Drawing.Size(55, 13);
            this.lb_version.TabIndex = 0;
            this.lb_version.Text = "lb_version";
            // 
            // bt_scan
            // 
            this.bt_scan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bt_scan.AutoSize = true;
            this.bt_scan.Location = new System.Drawing.Point(552, 12);
            this.bt_scan.Name = "bt_scan";
            this.bt_scan.Size = new System.Drawing.Size(42, 23);
            this.bt_scan.TabIndex = 1;
            this.bt_scan.Text = "Scan";
            this.bt_scan.UseVisualStyleBackColor = true;
            this.bt_scan.Click += new System.EventHandler(this.bt_scan_Click);
            // 
            // bt_connect
            // 
            this.bt_connect.Location = new System.Drawing.Point(409, 12);
            this.bt_connect.Name = "bt_connect";
            this.bt_connect.Size = new System.Drawing.Size(88, 23);
            this.bt_connect.TabIndex = 3;
            this.bt_connect.Text = "Connect";
            this.bt_connect.UseVisualStyleBackColor = true;
            this.bt_connect.Click += new System.EventHandler(this.bt_connect_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 668);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Lib version:";
            // 
            // t_midi_rx
            // 
            this.t_midi_rx.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.t_midi_rx.BackColor = System.Drawing.SystemColors.WindowText;
            this.t_midi_rx.ForeColor = System.Drawing.SystemColors.Window;
            this.t_midi_rx.Location = new System.Drawing.Point(8, 19);
            this.t_midi_rx.Multiline = true;
            this.t_midi_rx.Name = "t_midi_rx";
            this.t_midi_rx.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.t_midi_rx.Size = new System.Drawing.Size(569, 163);
            this.t_midi_rx.TabIndex = 10;
            // 
            // t_midi_tx
            // 
            this.t_midi_tx.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.t_midi_tx.BackColor = System.Drawing.SystemColors.WindowText;
            this.t_midi_tx.ForeColor = System.Drawing.SystemColors.Window;
            this.t_midi_tx.Location = new System.Drawing.Point(8, 188);
            this.t_midi_tx.Name = "t_midi_tx";
            this.t_midi_tx.Size = new System.Drawing.Size(569, 20);
            this.t_midi_tx.TabIndex = 11;
            this.t_midi_tx.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.onMidiKeyPressed);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(0, 13);
            this.label3.TabIndex = 12;
            // 
            // cb_devices
            // 
            this.cb_devices.FormattingEnabled = true;
            this.cb_devices.Location = new System.Drawing.Point(75, 13);
            this.cb_devices.Name = "cb_devices";
            this.cb_devices.Size = new System.Drawing.Size(310, 21);
            this.cb_devices.TabIndex = 13;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(21, 17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Devices:";
            // 
            // t_uart_rx
            // 
            this.t_uart_rx.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.t_uart_rx.BackColor = System.Drawing.SystemColors.WindowText;
            this.t_uart_rx.ForeColor = System.Drawing.SystemColors.Window;
            this.t_uart_rx.Location = new System.Drawing.Point(6, 19);
            this.t_uart_rx.Multiline = true;
            this.t_uart_rx.Name = "t_uart_rx";
            this.t_uart_rx.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.t_uart_rx.Size = new System.Drawing.Size(574, 156);
            this.t_uart_rx.TabIndex = 17;
            // 
            // t_uart_tx
            // 
            this.t_uart_tx.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.t_uart_tx.BackColor = System.Drawing.SystemColors.WindowText;
            this.t_uart_tx.ForeColor = System.Drawing.SystemColors.Window;
            this.t_uart_tx.Location = new System.Drawing.Point(6, 181);
            this.t_uart_tx.Name = "t_uart_tx";
            this.t_uart_tx.Size = new System.Drawing.Size(574, 20);
            this.t_uart_tx.TabIndex = 18;
            // 
            // t_log
            // 
            this.t_log.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.t_log.Location = new System.Drawing.Point(11, 20);
            this.t_log.Name = "t_log";
            this.t_log.Size = new System.Drawing.Size(566, 162);
            this.t_log.TabIndex = 19;
            this.t_log.Text = "";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.AutoSize = true;
            this.groupBox1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.groupBox1.Controls.Add(this.t_midi_tx);
            this.groupBox1.Controls.Add(this.t_midi_rx);
            this.groupBox1.Location = new System.Drawing.Point(7, 49);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(587, 227);
            this.groupBox1.TabIndex = 20;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "MIDI";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.AutoSize = true;
            this.groupBox2.BackColor = System.Drawing.SystemColors.ControlDark;
            this.groupBox2.Controls.Add(this.t_uart_tx);
            this.groupBox2.Controls.Add(this.t_uart_rx);
            this.groupBox2.Location = new System.Drawing.Point(7, 263);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(587, 220);
            this.groupBox2.TabIndex = 21;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Shell";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.AutoSize = true;
            this.groupBox3.BackColor = System.Drawing.SystemColors.Desktop;
            this.groupBox3.Controls.Add(this.t_log);
            this.groupBox3.Location = new System.Drawing.Point(7, 464);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(586, 201);
            this.groupBox3.TabIndex = 22;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Log";
            // 
            // web
            // 
            this.web.Location = new System.Drawing.Point(616, 53);
            this.web.MinimumSize = new System.Drawing.Size(20, 20);
            this.web.Name = "web";
            this.web.Size = new System.Drawing.Size(363, 577);
            this.web.TabIndex = 23;
            // 
            // bt_menu
            // 
            this.bt_menu.Location = new System.Drawing.Point(616, 636);
            this.bt_menu.Name = "bt_menu";
            this.bt_menu.Size = new System.Drawing.Size(46, 23);
            this.bt_menu.TabIndex = 24;
            this.bt_menu.Text = "Menu";
            this.bt_menu.UseVisualStyleBackColor = true;
            this.bt_menu.Click += new System.EventHandler(this.bt_menu_Click);
            // 
            // b_home
            // 
            this.b_home.Location = new System.Drawing.Point(668, 636);
            this.b_home.Name = "b_home";
            this.b_home.Size = new System.Drawing.Size(49, 23);
            this.b_home.TabIndex = 25;
            this.b_home.Text = "Home";
            this.b_home.UseVisualStyleBackColor = true;
            this.b_home.Click += new System.EventHandler(this.b_home_Click);
            // 
            // t_addr
            // 
            this.t_addr.Location = new System.Drawing.Point(723, 636);
            this.t_addr.Name = "t_addr";
            this.t_addr.Size = new System.Drawing.Size(208, 20);
            this.t_addr.TabIndex = 26;
            // 
            // b_go
            // 
            this.b_go.Location = new System.Drawing.Point(937, 636);
            this.b_go.Name = "b_go";
            this.b_go.Size = new System.Drawing.Size(41, 22);
            this.b_go.TabIndex = 27;
            this.b_go.Text = "Go";
            this.b_go.UseVisualStyleBackColor = true;
            this.b_go.Click += new System.EventHandler(this.b_go_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1019, 690);
            this.Controls.Add(this.b_go);
            this.Controls.Add(this.t_addr);
            this.Controls.Add(this.b_home);
            this.Controls.Add(this.bt_menu);
            this.Controls.Add(this.web);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cb_devices);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.bt_connect);
            this.Controls.Add(this.bt_scan);
            this.Controls.Add(this.lb_version);
            this.Controls.Add(this.label3);
            this.Name = "FormMain";
            this.Text = "Form1";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lb_version;
        private System.Windows.Forms.Button bt_scan;
        private System.Windows.Forms.Button bt_connect;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox t_midi_rx;
        private System.Windows.Forms.TextBox t_midi_tx;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cb_devices;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox t_uart_rx;
        private System.Windows.Forms.TextBox t_uart_tx;
        private System.Windows.Forms.RichTextBox t_log;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.WebBrowser web;
        private System.Windows.Forms.Button bt_menu;
        private System.Windows.Forms.Button b_home;
        private System.Windows.Forms.TextBox t_addr;
        private System.Windows.Forms.Button b_go;
    }
}

