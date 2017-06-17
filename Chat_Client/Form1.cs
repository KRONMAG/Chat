/**
 * \file
 * \brief Реализация окна чата
*/
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Chat_Client
{
	/**
	 * \brief Система ввода сообщений и отображения в форме полученных
	 * \author Макеев Владимир
	 * \date 15.06.2017
	*/
    public class Chat : System.Windows.Forms.Form
    {
        public delegate void Work(string t);//!< Делегат, используемый для создания метода, обновляющего элемент формы с сообщениями

        public bool update = false;//!< Переменная, равная истине в случае, если с сервера были получены новые сообщения 
        public string data = null;//!< Переменная, содеражащая данные для отправки на сервер (команду, пароль, логин и т.д.)
        public string[] post = new string[16];//!< Последние шестнадцать сообщений, полученных с сервера
        public Thread msg;//!< Поток для обновления сообщений на экране
        private IContainer components = null;//!< Переменная, содержащая компоненты формы
        private TextBox Messages;//!< Текстовое поле для ввода сообщений
        private TextBox T_Message;//!< Текстовое поле для отображения полученных сообщений
        private Label L_Message;//!< Метка, указывающая пользователю на поле для ввода сообщений

        //! Конструктор класа: вызов метода для инициализации компонентов формы, создание потока отображения сообщений
        public Chat()
        {
            InitializeComponent();
            msg = new Thread(Check);
            msg.Start();
        }

        //! Обновление текстового поля с сообщениями
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

        //! Проверка на необходимость загрузки новых сообщений в поле
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

        /**
 		 * \brief Добавление запроса на отправку сообщения, метод вызывается при нажатии на клавишу "Send"
 		 * \param[in] sender, e Параметры, передаваемые методу при возникновении события
 		*/
        private void T_Message_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                e.SuppressKeyPress = true;
                data = T_Message.Text;
                T_Message.Text = "";
            }
        }

        /**
         * \brief Уничтожение компонентов формы, используемых классом
         * \param[in] disposing Если указано истина, то освобождает неуправляемые и неуправляемые ресурсы, иначе - только неуправляемые
         */
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        //! Инициализация компонентов формы
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