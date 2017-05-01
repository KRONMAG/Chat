using System;
using System.ComponentModel;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Chat_Client
{
    public class Authorization : Form
    {
        public string cmd = null;
        private IContainer components = null;
        public string data;
        private Button Sign_In;
        private Button Sign_Up;
        private TextBox T_Password;
        private TextBox T_Login;
        private Label L_Login;
        private Label L_Password;

        public Authorization()
        {
            InitializeComponent();
        }

        private void Send(string c)
        {
            while (cmd != null)
                Thread.Sleep(0);
            string hash = Encoding.Unicode.GetString(new MD5CryptoServiceProvider().ComputeHash(Encoding.Unicode.GetBytes(T_Password.Text))), newhash = "";
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
            data = T_Login.Text + ":" + newhash;
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

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            Sign_In = new Button();
            Sign_Up = new Button();
            T_Password = new TextBox();
            T_Login = new TextBox();
            L_Login = new Label();
            L_Password = new Label();
            SuspendLayout();
            Sign_In.Location = new Point(41, 100);
            Sign_In.Name = "Sign_In";
            Sign_In.Size = new Size(75, 25);
            Sign_In.TabIndex = 0;
            Sign_In.Text = "Sign In";
            Sign_In.UseVisualStyleBackColor = true;
            Sign_In.Click += new EventHandler(Sign_In_Click);
            Sign_Up.Location = new Point(175, 100);
            Sign_Up.Name = "Sign_Up";
            Sign_Up.Size = new Size(75, 25);
            Sign_Up.TabIndex = 1;
            Sign_Up.Text = "Sign Up";
            Sign_Up.UseVisualStyleBackColor = true;
            Sign_Up.Click += new EventHandler(Sign_Up_Click);
            T_Password.Location = new Point(100, 54);
            T_Password.Name = "T_Password";
            T_Password.Size = new Size(150, 20);
            T_Password.TabIndex = 2;
            T_Password.UseSystemPasswordChar = true;
            T_Login.Location = new Point(100, 16);
            T_Login.Name = "T_Login";
            T_Login.Size = new Size(150, 20);
            T_Login.TabIndex = 3;
            L_Login.AutoSize = true;
            L_Login.Location = new Point(38, 19);
            L_Login.Name = "L_Login";
            L_Login.Size = new Size(36, 13);
            L_Login.TabIndex = 4;
            L_Login.Text = "Login:";
            L_Password.AutoSize = true;
            L_Password.Location = new Point(38, 57);
            L_Password.Name = "L_Password";
            L_Password.Size = new Size(56, 13);
            L_Password.TabIndex = 5;
            L_Password.Text = "Password:";
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(284, 161);
            Controls.Add(L_Password);
            Controls.Add(L_Login);
            Controls.Add(T_Login);
            Controls.Add(T_Password);
            Controls.Add(Sign_Up);
            Controls.Add(Sign_In);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Authorization";
            Text = "Autorization";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}