using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using Chat_Functions;

static class Server
{
    struct Post
    {
        public string msg;
        public DateTime date;

        public Post(string text)
        {
            msg = text;
            date = DateTime.Now;
        }
    }

    static TcpListener server;
    static IPAddress ip;
    static int port;
    static string path = Environment.CurrentDirectory;
    static List<Thread> work = new List<Thread>();
    static Thread error = new Thread(IO.ErrorControl);
    static Post[] post = new Post[16];
    
    static bool Check(string s)
    {
        bool res = true;
        int ctg;
        if (s.Length >= 3 && s.Length <= 15)
        {
            for (int i = 0; i < s.Length; i++)
            {
                ctg = (int)char.GetUnicodeCategory(s[i]);
                if (ctg < 2 || ctg == 8) continue;
                else
                {
                    res = false;
                    break;
                }
            }
        }
        else res = false;
        return res;
    }

    static bool Sign(Stream n, out string l, bool f)
    {
        try
        {
            bool res = true;
            l = "";
            string msg = IO.Read(n, 256);
            int pos = msg.IndexOf(':');
            if (pos >= 0)
            {
                for (int i = pos + 1; i < msg.Length; i++)
                    if (msg[i] == ':')
                    {
                        msg.Remove(i, 1);
                        msg.Insert(i, "a");
                    }
                string[] data = msg.Split(':');
                if (Check(data[0]))
                {
                    string file = path + @"/users/" + data[0] + ".usr";
                    if (File.Exists(file))
                    {
                        if (f && data[1] == File.ReadAllText(file)) l = data[0];
                        else res = false;
                    }
                    else
                    {
                        if (!f)
                        {
                            File.WriteAllText(file, data[1]);
                            l = data[0];
                        }
                        else res = false;
                    }
                }
                else res = false;
            }
            else res = false;
            return res;
        }
        catch (Exception error)
        {
            IO.error.Add(error);
            l = "";
            return false;
        }
    }

    static void Connection(object o)
    {
        try
        {
            TcpClient client = (TcpClient)o;
            string cmd = " ", nickname = "", msg, ip = client.Client.RemoteEndPoint.ToString();
            bool auth = false, first = true;
            NetworkStream stream = client.GetStream();
            Stopwatch time1 = new Stopwatch(), time2 = new Stopwatch();
            DateTime last = DateTime.MaxValue;
            time2.Start();
            int n;
            while (cmd != "out")
            {
                if (client.Available > 0)
                {
                    if (time1.IsRunning) time1.Reset();
                    cmd = IO.Read(stream, 3);
                    switch (cmd)
                    {
                        case "reg":
                        case "ath":
                            if (Sign(stream, out nickname, cmd == "ath" ? true : false))
                            {
                                IO.Write(stream, "Y", 1);
                                auth = true;
                                Console.WriteLine("{0} ({1}) has joined to the chat", nickname, ip);
                            }
                            else IO.Write(stream, "N", 1);
                            break;
                        case "msg":
                            if (auth && time2.ElapsedMilliseconds > 1000)
                            {
                                IO.Write(stream, "Y", 1);
                                msg = nickname + ": " + IO.Read(stream, 256);
                                for (int i = 0; i < 15; i++)
                                    post[i] = post[i + 1];
                                post[15] = new Post(msg);
                                time2.Restart();
                            }
                            else IO.Write(stream, "N", 1);
                            break;
                        case "all":
                            if (auth && (first || post[15].date != last))
                            {
                                n = 16;
                                IO.Write(stream, "Y", 1);
                                if (!first)
                                {
                                    for (int i = 15; i >= 0; i--)
                                        if (post[i].date == last)
                                        {
                                            n = 15 - i;
                                            break;
                                        }
                                }
                                else first = false;
                                IO.Write(stream, ((char)n).ToString(), 1);
                                for (int i = 16 - n; i < 16; i++)
                                    IO.Write(stream, post[i].msg, 256);
                                last = post[15].date;
                            }
                            else IO.Write(stream, "N", 1);
                            break;
                    }
                }
                else
                {
                    if (!time1.IsRunning) time1.Start();
                    else if (time1.Elapsed.Minutes == 1) break;
                    Thread.Sleep(0);
                }
            }
            client.Close();
            if (auth) Console.WriteLine("{0} ({1}) has left the chat", nickname, ip);
        }
        catch (Exception error)
        {
            IO.error.Add(error);
        }
    }

    static void Main()
    {
        Directory.CreateDirectory(path + @"/users");
        Directory.CreateDirectory(path + @"/messages");
        IO.path = path+@"/messages/errors.txt";
        error.Start();
        try
        {
            for (int i = 0; i < 16; i++)
            {
                post[i] = new Post(" ");
                Thread.Sleep(1);
            }
            Console.Write("IP: ");
            ip = IPAddress.Parse(Console.ReadLine());
            Console.Write("Port: ");
            port = int.Parse(Console.ReadLine());
            server = new TcpListener(ip, port);
            server.Start();
            while (true)
            {
                if (server.Pending())
                {
                    work.Add(new Thread(Connection));
                    work[work.Count - 1].Start(server.AcceptTcpClient());
                    for (int i = 0; i < work.Count; i++)
                        if (!work[i].IsAlive) work.RemoveAt(i);
                    Console.Title = "Connections: " + work.Count.ToString();
                }
                else Thread.Sleep(0);
            }
        }
        catch (Exception e)
        {
            IO.error.Add(e);
        }
        finally
        {
            while (IO.error.Count != 0) Thread.Sleep(0);
            error.Abort();
            work.Clear();
            if (server != null) server.Stop();
        }
    }
}