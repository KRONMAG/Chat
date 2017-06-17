/**
 * \file
 * \brief Реализация окна настроек
*/
using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace Chat_Client
{
	/**
	 * \brief Система настройки чата (ввод порта и IP-адреса сервера)
	 * \author Макеев Владимир
	 * \date 15.06.2017
	*/
    public class Options : Form
    {
        private string path;//!< Путь к файлу с настройками
        private Button Accept;//!< Кнопка принятия изменений
        private Button Cancel;//!< Кнопка отмены изменений
        private Label L_Port;//!< Метка, указывающая на счетчик для ввода порта
        private Label L_IP;//!< Метка, указывающая на поле для ввода IP-адреса
        private TextBox T_IP;//!< Тектовое поле для ввода IP-адреса сервера
        private NumericUpDown N_Port;//!< Счетчик для указания порта сервера
        private IContainer components = null;//!< Переменная, содержащая компоненты формы

        /**
         * \brief Конструктор класса: вызов метода для инициализации компонентов формы, присваивание значений входных параметров членам класса
         * \param[in] ip, port Текущий IP-адрес и порт сервера
         * \param[in] path Путь к файлу с настройками
        */
        public Options(IPAddress ip, int port, string path)
        {
            InitializeComponent();
            T_IP.Text = ip.ToString();
            N_Port.Value = port;
            this.path = path;
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

        /**
 		 * \brief Внесение изменений в файл с настройками, закрытие формы, метод вызывается при нажатиии на клавишу "Accept"
 		 * \param[in] sender, e Параметры, передаваемые методу при возникновении события
 		*/
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

        /**
 		 * \brief Отмена редактирования настроек чата, метод вызывается при нажатиии на клавишу "Cancel"
 		 * \param[in] sender, e Параметры, передаваемые методу при возникновении события
 		*/
        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        //! Инициализация компонентов формы
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
    }
}