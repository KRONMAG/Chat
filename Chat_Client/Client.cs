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
        const string E_REG = "Unable to create an account: invalid username and password or username already exists";
        const string E_SRV = "Can not connect to server or connection was interrupted";
        const string E_MSG = "Do not send more than one message per second";
        static readonly string[] E_BAN = { "You were banned before ", ". Cause: " };
        static NetworkStream stream;

        static void Error(string e, bool b = false)
        {
            MessageBox.Show(e, "Error");
            if (b) throw new SocketException();
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
                    chat.post[i] = i >= num ? IO.Read(stream, 272) : chat.post[i + 1];
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
            try
            {
                Thread thread = new Thread(Run);
                auth.FormClosed += Exit;
                IPAddress address = IPAddress.Parse("127.0.0.1");
                int port = 49001;
                thread.Start(auth);
                client.Connect(address, port);
                Stopwatch load = new Stopwatch();
                stream = client.GetStream();
                while (!ath)
                {
                    if (client.Connected)
                    {
                        if (auth.cmd != null)
                        {
                            busy = true;
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
                                else  Error(E_BAN[0] + IO.Read(stream, 19) + E_BAN[1] + IO.Read(stream, 256), true);
                            }
                            else
                            {
                                Error(auth.cmd == "ath" ? E_ATH : E_REG);
                                auth.cmd = null;
                            }
                            busy = false;
                        }
                        else Thread.Sleep(0);
                    }
                    else throw new SocketException();
                }
                load.Start();
                while (true)
                {
                    if (client.Connected)
                    {
                        if (chat.data != null)
                        {
                            busy = true;
                            IO.Write(stream, "msg", 3);
                            switch (IO.Read(stream, 2))
                            {
                                case "YN":IO.Write(stream, chat.data, 256);
                                    break;
                                case "NN":Error(E_MSG);
                                    break;
                                case "YY":Error(E_BAN[0] + IO.Read(stream, 19) + E_BAN[1] + IO.Read(stream, 256), true);
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
                        else
                            Thread.Sleep(0);
                    }
                    else throw new SocketException();
                }
            }
            catch (SocketException e)
            {
                Error(E_SRV);
                Environment.Exit(0);
            }
        }
    }
}