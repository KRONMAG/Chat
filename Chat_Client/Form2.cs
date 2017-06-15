using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Management;
using System.Net;
using System.IO;

namespace Chat_Client
{
    public class Authorization : System.Windows.Forms.Form
    {
    	private readonly string[] query={"SELECT Name FROM Win32_Processor",
    									 "SELECT Caption FROM Win32_VideoController",
    									 "SELECT Caption FROM Win32_Volume"};
        readonly string path = Environment.CurrentDirectory + @"/settings";
        readonly string options = @"/options.cfg";

        public IPAddress ip = IPAddress.Parse("192.168.0.47");
        public int port = 49001;
        public string cmd = null;
        public string data;
        private IContainer components = null;
        private Button Sign_In;
        private Button Sign_Up;
        private TextBox T_Password;
        private TextBox T_Login;
        private Label L_Login;
        private Button B_Options;
        private Label L_Password;

        public Authorization()
        {
            InitializeComponent();
            SetNetData();
        }

        private void SetNetData(object sender=null, EventArgs e=null)
        {
            Directory.CreateDirectory(path);
            if (File.Exists(path + options))
            {
                string[] data = File.ReadAllLines(path + options);
                if (data.Length == 2)
                {
                    ip = IPAddress.Parse(data[0]);
                    port = int.Parse(data[1]);
                }
            }
            else File.WriteAllText(path + options, ip.ToString() + Environment.NewLine + port.ToString());
        }

        private string ComputeHash(string t)
        {
        	string hash=Encoding.Unicode.GetString(new MD5CryptoServiceProvider().ComputeHash(Encoding.Unicode.GetBytes(t))), newhash="";
        	int num, sym;
            for (int i = 0; i < hash.Length; i++)
            {
                num = hash[i];
                for (int j = 0; j < 2; j++)
                {
                    sym = num % 256 + 1;
                    if ((char)sym == '\'') sym++;
                    newhash += (char)sym;
                    num /= 256;
                }
            }
            return newhash;
        }
        
        private void Send(string c)
        {
            while (cmd != null)
                Thread.Sleep(0);
            string hash = ComputeHash(T_Password.Text), pchash="";
            for (int i=0;i<3;i++)
            {
            	ManagementObjectCollection pc = new ManagementObjectSearcher("root\\CIMV2", query[i]).Get();
            	foreach (ManagementObject o in pc)
            		if (i==0) pchash+=o["Name"];
            	else pchash+=o["Caption"];
            }
            pchash=ComputeHash(pchash);
            data = T_Login.Text + ':' + hash+':'+pchash;
            cmd = c;
        }

        private void Sign_Up_Click(object sender, EventArgs e)
        {
            Send("reg");
        }

        private void Sign_In_Click(object sender, EventArgs e)
        {
            Send("ath");
        }

        private void B_Options_Click(object sender, EventArgs e)
        {
            Options opt = new Options(ip, port, path+options);
            opt.Show();
            opt.FormClosed += new FormClosedEventHandler(SetNetData);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Sign_In = new System.Windows.Forms.Button();
            this.Sign_Up = new System.Windows.Forms.Button();
            this.T_Password = new System.Windows.Forms.TextBox();
            this.T_Login = new System.Windows.Forms.TextBox();
            this.L_Login = new System.Windows.Forms.Label();
            this.L_Password = new System.Windows.Forms.Label();
            this.B_Options = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Sign_In
            // 
            this.Sign_In.Location = new System.Drawing.Point(22, 115);
            this.Sign_In.Margin = new System.Windows.Forms.Padding(4);
            this.Sign_In.Name = "Sign_In";
            this.Sign_In.Size = new System.Drawing.Size(80, 30);
            this.Sign_In.TabIndex = 0;
            this.Sign_In.Text = "Sign In";
            this.Sign_In.UseVisualStyleBackColor = true;
            this.Sign_In.Click += new System.EventHandler(this.Sign_In_Click);
            // 
            // Sign_Up
            // 
            this.Sign_Up.Location = new System.Drawing.Point(230, 115);
            this.Sign_Up.Margin = new System.Windows.Forms.Padding(4);
            this.Sign_Up.Name = "Sign_Up";
            this.Sign_Up.Size = new System.Drawing.Size(80, 30);
            this.Sign_Up.TabIndex = 1;
            this.Sign_Up.Text = "Sign Up";
            this.Sign_Up.UseVisualStyleBackColor = true;
            this.Sign_Up.Click += new System.EventHandler(this.Sign_Up_Click);
            // 
            // T_Password
            // 
            this.T_Password.Location = new System.Drawing.Point(116, 62);
            this.T_Password.Margin = new System.Windows.Forms.Padding(4);
            this.T_Password.Name = "T_Password";
            this.T_Password.Size = new System.Drawing.Size(175, 21);
            this.T_Password.TabIndex = 2;
            this.T_Password.UseSystemPasswordChar = true;
            // 
            // T_Login
            // 
            this.T_Login.Location = new System.Drawing.Point(116, 19);
            this.T_Login.Margin = new System.Windows.Forms.Padding(4);
            this.T_Login.Name = "T_Login";
            this.T_Login.Size = new System.Drawing.Size(175, 21);
            this.T_Login.TabIndex = 3;
            // 
            // L_Login
            // 
            this.L_Login.AutoSize = true;
            this.L_Login.Location = new System.Drawing.Point(45, 22);
            this.L_Login.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.L_Login.Name = "L_Login";
            this.L_Login.Size = new System.Drawing.Size(41, 15);
            this.L_Login.TabIndex = 4;
            this.L_Login.Text = "Login:";
            // 
            // L_Password
            // 
            this.L_Password.AutoSize = true;
            this.L_Password.Location = new System.Drawing.Point(45, 66);
            this.L_Password.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.L_Password.Name = "L_Password";
            this.L_Password.Size = new System.Drawing.Size(64, 15);
            this.L_Password.TabIndex = 5;
            this.L_Password.Text = "Password:";
            // 
            // B_Options
            // 
            this.B_Options.Location = new System.Drawing.Point(129, 153);
            this.B_Options.Margin = new System.Windows.Forms.Padding(4);
            this.B_Options.Name = "B_Options";
            this.B_Options.Size = new System.Drawing.Size(80, 30);
            this.B_Options.TabIndex = 6;
            this.B_Options.Text = "Options";
            this.B_Options.UseVisualStyleBackColor = true;
            this.B_Options.Click += new System.EventHandler(this.B_Options_Click);
            // 
            // Authorization
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(332, 186);
            this.Controls.Add(this.B_Options);
            this.Controls.Add(this.L_Password);
            this.Controls.Add(this.L_Login);
            this.Controls.Add(this.T_Login);
            this.Controls.Add(this.T_Password);
            this.Controls.Add(this.Sign_Up);
            this.Controls.Add(this.Sign_In);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Authorization";
            this.Text = "Authorization";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}