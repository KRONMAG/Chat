using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using Chat_Functions;

namespace Chat_Client
{
    class Client
    {
        static bool ath = false;
        static TcpClient client = new TcpClient();
        static bool busy = false;
        static Authorization auth = new Authorization();
        static Chat chat = null;
        const string E_ATH = "Unable to login: invalid username or password";
        const string E_REG = "Unable to create an account: invalid username and password";
        const string E_SRV = "Can not connect to server";
        const string E_MSG = "The message was not delivered";
        static NetworkStream stream;

        static void Error(string e)
        {
            MessageBox.Show(e, "Error");
        }

        static void Run(object o)
        {
            if (ath)
                Application.Run((Form)o);
            else
                Application.Run((Form)o);
        }

        static void GetMsg()
        {
            busy = true;
            IO.Write(stream, "all", 3);
            if (IO.Read(stream, 1) == "Y")
            {
                int num = 16 - IO.Read(stream, 1)[0];
                for (int i = 0; i < 16; i++)
                    chat.post[i] = i >= num ? IO.Read(stream, 256) : chat.post[i + 1];
                chat.update = true;
            }
            busy = false;
        }

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

        static void Main()
        {
            Thread thread = new Thread(Run);
            auth.FormClosed += Exit;
            IPAddress address = IPAddress.Parse("192.168.1.3");
            int port = 49001;
            thread.Start(auth);
            client.Connect(address, port);
            Stopwatch load = new Stopwatch();
            if (client.Connected)
            {
                stream = client.GetStream();
                while (!ath)
                {
                    if (auth.cmd != null)
                    {
                        busy = true;
                        IO.Write(stream, auth.cmd, 3);
                        IO.Write(stream, auth.data, 256);
                        if (IO.Read(stream, 1) == "Y")
                        {
                            auth.FormClosed -= Exit;
                            ath = true;
                            Application.Exit();
                            auth = null;
                            chat = new Chat();
                            chat.FormClosed += Exit;
                            thread = new Thread(Run);
                            thread.Start(chat);
                        }
                        else
                        {
                            Error(auth.cmd == "ath" ? "Unable to login: invalid username or password" : "Unable to create an account: invalid username and password");
                            auth.cmd = null;
                        }
                        busy = false;
                    }
                    else
                        Thread.Sleep(0);
                }
                load.Start();
                while (true)
                {
                    if (chat.data != null)
                    {
                        busy = true;
                        IO.Write(stream, "msg", 3);
                        if (IO.Read(stream, 1) == "Y")
                            IO.Write(stream, chat.data, 256);
                        else
                            Error("The message was not delivered");
                        chat.data = null;
                        busy = false;
                    }
                    else if (load.Elapsed.Milliseconds >= 500)
                    {
                        GetMsg();
                        load.Restart();
                    }
                    else
                        Thread.Sleep(0);
                }
            }
            else
            {
                Error("Can not connect to server");
                Environment.Exit(0);
            }
        }
    }
}