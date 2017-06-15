using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

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

        static void Error(string e)
        {
            MessageBox.Show(e, "Error");
        }

        static void Run(object o)
        {
            if (ath)
                Application.Run((System.Windows.Forms.Form)o);
            else
                Application.Run((System.Windows.Forms.Form)o);
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