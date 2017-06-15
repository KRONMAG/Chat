using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace Chat_Client
{
    public class Options : Form
    {
        private string path;
        private Button Accept;
        private Button Cancel;
        private Label L_Port;
        private Label L_IP;
        private TextBox T_IP;
        private NumericUpDown N_Port;
        private IContainer components = null;

        public Options(IPAddress ip, int port,string path)
        {
            InitializeComponent();
            T_IP.Text = ip.ToString();
            N_Port.Value = port;
            this.path = path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void Accept_Click(object sender, EventArgs e)
        {
            IPAddress test;
            if (IPAddress.TryParse(T_IP.Text, out test))
            {
                File.WriteAllText(path, T_IP.Text + Environment.NewLine + N_Port.Value);
                this.Close();
            }
            else MessageBox.Show("Invalid IP address specified");
        }

        private void InitializeComponent()
        {
        	this.Accept = new System.Windows.Forms.Button();
        	this.Cancel = new System.Windows.Forms.Button();
        	this.L_Port = new System.Windows.Forms.Label();
        	this.L_IP = new System.Windows.Forms.Label();
        	this.T_IP = new System.Windows.Forms.TextBox();
        	this.N_Port = new System.Windows.Forms.NumericUpDown();
        	((System.ComponentModel.ISupportInitialize)(this.N_Port)).BeginInit();
        	this.SuspendLayout();
        	// 
        	// Accept
        	// 
        	this.Accept.Location = new System.Drawing.Point(183, 114);
        	this.Accept.Name = "Accept";
        	this.Accept.Size = new System.Drawing.Size(80, 30);
        	this.Accept.TabIndex = 0;
        	this.Accept.Text = "Accept";
        	this.Accept.UseVisualStyleBackColor = true;
        	this.Accept.Click += new System.EventHandler(this.Accept_Click);
        	// 
        	// Cancel
        	// 
        	this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        	this.Cancel.Location = new System.Drawing.Point(24, 114);
        	this.Cancel.Name = "Cancel";
        	this.Cancel.Size = new System.Drawing.Size(80, 30);
        	this.Cancel.TabIndex = 1;
        	this.Cancel.Text = "Cancel";
        	this.Cancel.UseVisualStyleBackColor = true;
        	this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
        	// 
        	// L_Port
        	// 
        	this.L_Port.AutoSize = true;
        	this.L_Port.Location = new System.Drawing.Point(72, 70);
        	this.L_Port.Name = "L_Port";
        	this.L_Port.Size = new System.Drawing.Size(32, 15);
        	this.L_Port.TabIndex = 2;
        	this.L_Port.Text = "Port:";
        	// 
        	// L_IP
        	// 
        	this.L_IP.AutoSize = true;
        	this.L_IP.Location = new System.Drawing.Point(72, 26);
        	this.L_IP.Name = "L_IP";
        	this.L_IP.Size = new System.Drawing.Size(21, 15);
        	this.L_IP.TabIndex = 3;
        	this.L_IP.Text = "IP:";
        	// 
        	// T_IP
        	// 
        	this.T_IP.Location = new System.Drawing.Point(107, 23);
        	this.T_IP.Name = "T_IP";
        	this.T_IP.Size = new System.Drawing.Size(100, 21);
        	this.T_IP.TabIndex = 4;
        	// 
        	// N_Port
        	// 
        	this.N_Port.Location = new System.Drawing.Point(110, 70);
        	this.N_Port.Maximum = new decimal(new int[] {
			65535,
			0,
			0,
			0});
        	this.N_Port.Name = "N_Port";
        	this.N_Port.Size = new System.Drawing.Size(65, 21);
        	this.N_Port.TabIndex = 5;
        	// 
        	// Options
        	// 
        	this.AcceptButton = this.Accept;
        	this.CancelButton = this.Cancel;
        	this.ClientSize = new System.Drawing.Size(284, 161);
        	this.Controls.Add(this.N_Port);
        	this.Controls.Add(this.T_IP);
        	this.Controls.Add(this.L_IP);
        	this.Controls.Add(this.L_Port);
        	this.Controls.Add(this.Cancel);
        	this.Controls.Add(this.Accept);
        	this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
        	this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
        	this.MaximizeBox = false;
        	this.MinimizeBox = false;
        	this.Name = "Options";
        	this.Text = "Options";
        	((System.ComponentModel.ISupportInitialize)(this.N_Port)).EndInit();
        	this.ResumeLayout(false);
        	this.PerformLayout();

        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}