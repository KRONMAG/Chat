/**
 * \file
 * \brief Реализация клиентской части чата
*/
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

//! Пространство имен чата-клиента, объединяющее четыре класса: класс соединения с сервером и три класса форм: авторизация, сообщения и опции
namespace Chat_Client
{
	/**
     * \brief Система взаимодействия с серверной стороной чата, состоящая из двух частей: регистрация (авторизация) пользователя и отправка/прием сообщений
     * \author Макеев Владимир
     * \date 15.06.2017
     */
    static class Client
    {
        static bool ath = false;//!< Переменная, содержащая значение истины, в случае удачной регистрации (авторизации) пользователя
        static TcpClient client = new TcpClient();//!< Клиентский сокет, необходимый для установления соединения с сервером
        static bool busy = false;//!< Переменная, использующаяся для последовательной отправки запросов на сервер, равна истине, если в данный момент данных для отправки нет
        static Authorization auth = new Authorization();//!< Форма регистрации (авторизации) пользователя
        static Chat chat = null;//!< Форма, позволяющая отправлять сообщения и отображать полученные с сервера
        static NetworkStream stream;//!< Поток ввода-вывода данных для клиентского сокета
        
        /**
         * \defgroup ClientErrors Ошибки чат-клиента
		 * \brief Описание ошибок, возникающих при присоединении к серверу и дальнейшей работой с ним
		 * \{
		*/
        const string E_ATH = "Unable to login: invalid username or password";//!< Неудачная авторизация пользователя - неправильно введен логин или пароль
        const string E_REG = "Unable to create an account: invalid username or username already exists";//!< Создание аккаунта невозможно - некорректный логин или указанный пользователь уже существует
        const string E_SRV = "Can not connect to server or connection was interrupted";//!< Соединение с сервером не установлено или потеряно
        const string E_MSG = "Do not send more than one message per second";//!< Отправка больше одного сообщений в секунду
        static readonly string[] E_BAN = { "You were banned before ", ". Cause: " };//!< На компьютер, с которого пытаются зарегистрироваться (авторизироваться), наложен временный запрет входа в чат
        /**
         * \}
        */

       /**
        * \brief Вывод сообщения о возникшей ошибке в диалоговое окно
        * \param[in] e Строка, содержащая описание ошибки
       */
        static void Error(string e)
        {
            MessageBox.Show(e, "Error");
        }

        /**
         * \brief Запуск формы (используется для создания нового потока)
         * \param[in] o Форма, которую необходимо отобразить
        */
        static void Run(object o)
        {
                Application.Run((System.Windows.Forms.Form)o);
        }

        //! Получение последних шестнадцати сообщений с сервера и отображение их на экране
        static void GetMsg()
        {
            busy = true;
            IO.Write(stream, "all", 3);
            if (IO.Read(stream, 1) == "Y")
            {
                int num = 16 - IO.Read(stream, 1)[0];
                for (int i = 0; i < 16; i++)
                    chat.post[i] = i >= num ? IO.Read(stream, 272) : chat.post[i + 1];
                chat.update = true;
            }
            busy = false;
        }

        /**
 		 * \brief Разрыв соединения с сервером и завершение работы программы, метод вызывается при закрытии формы
 		 * \param[in] sender, e Параметры, передаваемые методу при возникновении события
 		*/
        static void Exit(object sender, FormClosedEventArgs e)
        {
            if (client.Connected)
            {
                while (busy)
                    Thread.Sleep(0);
                IO.Write(stream, "out", 3);
            }
            Environment.Exit(0);
        }
        
        //! Точка входа в программу, отображение и закрытие форм, соединение и обмен данными с сервером
        static void Main()
        {
                Thread thread = new Thread(Run);
                auth.FormClosed += Exit;
                thread.Start(auth);
                Stopwatch load = new Stopwatch();
                while (!ath)
                {
                    if (auth.cmd != null)
                {
                    try
                    {
                        busy = true;
                        client.Connect(auth.ip, auth.port);
                            stream = client.GetStream();
                            IO.SetKey(stream, false);
                            IO.Write(stream, auth.cmd, 3);
                            IO.Write(stream, auth.data, 256);
                        if (IO.Read(stream, 1) == "Y")
                        {
                            ath = true;
                            if (IO.Read(stream, 1) == "N")
                            {
                                auth.FormClosed -= Exit;
                                Application.Exit();
                                auth = null;
                                chat = new Chat();
                                chat.FormClosed += Exit;
                                thread = new Thread(Run);
                                thread.Start(chat);
                            }
                            else
                            {
                                Error(E_BAN[0] + IO.Read(stream, 19) + E_BAN[1] + IO.Read(stream, 256));
                                Environment.Exit(0);
                            }
                        }
                        else Error(auth.cmd == "ath" ? E_ATH : E_REG);
                    }
                    catch (SocketException e)
                    {
                        Error(E_SRV);
                    }
                    if (!ath)
                        {
                        auth.cmd = null;
                        IO.Write(stream, "out", 3);
                        client.Close();
                        client = new TcpClient();
                        }
                        busy = false;
                    }
                    else Thread.Sleep(0);
                }
                load.Start();
            while (true)
            {
                try
                {
                    if (client.Connected)
                    {
                        if (chat.data != null)
                        {
                            busy = true;
                            IO.Write(stream, "msg", 3);
                            switch (IO.Read(stream, 2))
                            {
                                case "YN":
                                    IO.Write(stream, chat.data, 256);
                                    break;
                                case "NN":
                                    Error(E_MSG);
                                    break;
                                case "YY":
                                    throw new Exception(E_BAN[0] + IO.Read(stream, 19) + E_BAN[1] + IO.Read(stream, 256));
                                    break;
                            }
                            chat.data = null;
                            busy = false;
                        }
                        else if (load.Elapsed.Milliseconds >= 500)
                        {
                            GetMsg();
                            load.Restart();
                        }
                        else Thread.Sleep(0);
                    }
                    else throw new SocketException();
                }
                catch (Exception e)
                {
                    if (e is SocketException) Error(E_SRV);
                    else Error(e.Message);
                    Environment.Exit(0);
                }
                }
        }
    }
}