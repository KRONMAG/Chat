using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Chat_Client
{
    public class Chat : Form
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
            Messages = new TextBox();
            T_Message = new TextBox();
            L_Message = new Label();
            SuspendLayout();
            Messages.Cursor = Cursors.Arrow;
            Messages.Location = new Point(0, 0);
            Messages.Multiline = true;
            Messages.Name = "Messages";
            Messages.ReadOnly = true;
            Messages.ScrollBars = ScrollBars.Vertical;
            Messages.Size = new Size(284, 220);
            Messages.TabIndex = 0;
            T_Message.Location = new Point(0, 239);
            T_Message.MaxLength = 256;
            T_Message.Name = "T_Message";
            T_Message.Size = new Size(284, 20);
            T_Message.TabIndex = 1;
            T_Message.KeyDown += new KeyEventHandler(T_Message_KeyDown);
            L_Message.AutoSize = true;
            L_Message.Location = new Point(-3, 223);
            L_Message.Name = "L_Message";
            L_Message.Size = new Size(53, 13);
            L_Message.TabIndex = 3;
            L_Message.Text = "Message:";
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(284, 261);
            Controls.Add(L_Message);
            Controls.Add(T_Message);
            Controls.Add(Messages);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Chat";
            Text = "Chat";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
