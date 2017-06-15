using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Chat_Client
{
    public class Chat : System.Windows.Forms.Form
    {
        public delegate void Work(string t);

        public bool update = false;
        public string data = null;
        public string[] post = new string[16];
        private IContainer components = null;
        public Thread msg;
        private TextBox Messages;
        private TextBox T_Message;
        private Label L_Message;

        public Chat()
        {
            InitializeComponent();
            msg = new Thread(Check);
            msg.Start();
        }

        private void SetText(string t)
        {
            if (Messages.InvokeRequired) Invoke(new Work(SetText), t);
            else
            {
                Messages.Text = t;
                Messages.SelectionStart = t.Length;
                Messages.ScrollToCaret();
            }
        }

        private void Check()
        {
            while (true)
            {
                if (update)
                {
                    string t = "";
                    for (int i = 0; i < 16; i++)
                        t = t + post[i] + (i != 15 ? Environment.NewLine : "");
                    SetText(t);
                    update = false;
                }
            }
        }

        private void T_Message_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                e.SuppressKeyPress = true;
                data = T_Message.Text;
                T_Message.Text = "";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Messages = new System.Windows.Forms.TextBox();
            this.T_Message = new System.Windows.Forms.TextBox();
            this.L_Message = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Messages
            // 
            this.Messages.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.Messages.Location = new System.Drawing.Point(0, 0);
            this.Messages.Multiline = true;
            this.Messages.Name = "Messages";
            this.Messages.ReadOnly = true;
            this.Messages.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Messages.Size = new System.Drawing.Size(331, 253);
            this.Messages.TabIndex = 0;
            // 
            // T_Message
            // 
            this.T_Message.Location = new System.Drawing.Point(0, 276);
            this.T_Message.MaxLength = 256;
            this.T_Message.Name = "T_Message";
            this.T_Message.Size = new System.Drawing.Size(331, 21);
            this.T_Message.TabIndex = 1;
            this.T_Message.KeyDown += new System.Windows.Forms.KeyEventHandler(this.T_Message_KeyDown);
            // 
            // L_Message
            // 
            this.L_Message.AutoSize = true;
            this.L_Message.Location = new System.Drawing.Point(-3, 257);
            this.L_Message.Name = "L_Message";
            this.L_Message.Size = new System.Drawing.Size(61, 15);
            this.L_Message.TabIndex = 3;
            this.L_Message.Text = "Message:";
            // 
            // Chat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(331, 301);
            this.Controls.Add(this.L_Message);
            this.Controls.Add(this.T_Message);
            this.Controls.Add(this.Messages);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Chat";
            this.Text = "Chat";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
