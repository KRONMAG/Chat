using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Data.OleDb;

static class Server
{
    static readonly string[] symbols = { ";", "'", "--", "{*", "*}" };

    struct Post
    {
        public string msg, nickname;
        public DateTime date;

        public Post(DateTime time, string nick, string text)
        {
            date = time;
            nickname = nick;
            msg = text;
        }
    }

    static TcpListener server;
    static IPAddress ip;
    static int port;
    static string path = Environment.CurrentDirectory;
    static List<Thread> work = new List<Thread>();
    static Thread error = new Thread(IO.ErrorControl);
    static Post[] post = new Post[16];
    static OleDbConnection db;
    static OleDbCommand cmd;
    static HashSet<string> online = new HashSet<string>();
    static bool busy = false;

    static bool IsNull(object o)
    {
        return (o == DBNull.Value || o == null);
    }

    static object SQLCommand(string c, params object[] d)
    {
        while (busy)
            Thread.Sleep(0);
        busy = true;
        object query, res = true;
        OleDbDataReader stream;
        if (c == "reg" || c == "ath")
        {
            if (c == "reg")
            {
                cmd.CommandText = "SELECT id FROM Users WHERE login='" + d[0] + "'";
                if (IsNull(cmd.ExecuteScalar()))
                {
                    cmd.CommandText = "SELECT MAX(id) FROM Users";
                    query = cmd.ExecuteScalar();
                    int id = !IsNull(query) ? (int)query + 1 : 1;
                    cmd.CommandText = "INSERT INTO Users VALUES (" + id + ",'" + d[0] + "','" + d[1] + "','" + d[2] + "')";
                    cmd.ExecuteNonQuery();
                    res = true;
                }
                else
                    res = false;
            }
            else
            {
                cmd.CommandText = "SELECT id FROM Users WHERE login='" + d[0] + "' AND password='" + d[1] + "'";
                if (!IsNull(cmd.ExecuteScalar()))
                    res = true;
                else
                    res = false;
            }
        }
        else if (c == "all" || c == "msg")
        {
            cmd.CommandText = "SELECT MAX(id) FROM Posts";
            query = cmd.ExecuteScalar();
            int nums = !IsNull(query) ? (int)query : 0;
            if (c == "all")
            {
                int k = 15;
                cmd.CommandText = "SELECT date, login, text FROM Posts WHERE id>=" + nums.ToString() + "-15";
                stream = cmd.ExecuteReader();
                while (stream.Read())
                {
                    post[k] = new Post(DateTime.Parse(stream[0].ToString()), stream[1].ToString(), stream[2].ToString());
                    k--;
                }
                for (int i = k; i >= 0; i--)
                    post[i] = new Post(DateTime.MinValue, " ", " ");
                Array.Sort<Post>(post, (x, y) => x.date.CompareTo(y.date));
                stream.Close();
            }
            else
            {
                for (int i = 0; i < 5; i++)
                    d[2] = ((string)d[2]).Replace(symbols[i], "");
                cmd.CommandText = "INSERT INTO Posts VALUES (" + (nums + 1).ToString() + ",'" + d[0] + "','" + d[1] + "','" + d[2] + "')";
                cmd.ExecuteNonQuery();
            }
        }
        else if (c == "ban")
        {
            cmd.CommandText = "SELECT date, reason FROM Ban_list WHERE pchash='" + d[0] + "'";
            stream = cmd.ExecuteReader();
            res = "N";
            while (stream.Read())
            {
                res = DateTime.Compare(DateTime.Parse(stream[0].ToString()), DateTime.Now) > 0 ? stream[0].ToString() + '/' + stream[1].ToString() : "N";
                if (res != "N") break;
            }
            stream.Close();
        }
        busy = false;
        return res;
    }

    static bool Check(string s)
    {
        bool res = true;
        int ctg;
        if (s.Length >= 3 && s.Length <= 15)
        {
            for (int i = 0; i < s.Length; i++)
            {
                ctg = (int)char.GetUnicodeCategory(s[i]);
                if (ctg < 2 || ctg == 8)
                    continue;
                else
                {
                    res = false;
                    break;
                }
            }
        }
        else
            res = false;
        return res;
    }

    static bool Sign(Stream n, out string l, out string h, string c)
    {
        try
        {
            bool res = true;
            l = "";
            h = "";
            string msg = IO.Read(n, 256);
            int pos1 = msg.IndexOf(':'), pos2 = msg.Length - 17;
            if (pos1 >= 0 && pos2 > 0 ? (msg[pos2] == ':' ? true : false) : false)
            {
                for (int i = pos1 + 1; i < pos2; i++)
                {
                    if (msg[i] == ':')
                    {
                        msg = msg.Remove(i, 1);
                        msg = msg.Insert(i, "a");
                    }
                }
                string[] data = msg.Split(':');
                if (Check(data[0]))
                {
                    if ((bool)SQLCommand(c, data[0], data[1], data[2]))
                    {
                        l = data[0];
                        h = data[2];
                        res = true;
                    }
                    else
                        res = false;
                }
                else
                    res = false;
            }
            else
                res = false;
            return res;
        }
        catch (Exception error)
        {
            IO.error.Add(error);
            l = "";
            h = "";
            return false;
        }
    }

    static void Connection(object o)
    {
        TcpClient client = (TcpClient)o;
        string cmd = " ", nickname = "", ip = client.Client.RemoteEndPoint.ToString(), pchash = "", state = "/";
        bool auth = false, first = true;
        NetworkStream stream = client.GetStream();
        Stopwatch time1 = new Stopwatch(), time2 = new Stopwatch();
        DateTime last = DateTime.MaxValue;
        try
        {
            time2.Start();
            int n;
            while (cmd != "out")
            {
                if (client.Available > 0)
                {
                    if (time1.IsRunning)
                        time1.Reset();
                    cmd = IO.Read(stream, 3);
                    switch (cmd)
                    {
                        case "reg":
                        case "ath":
                            if (Sign(stream, out nickname, out pchash, cmd) && !online.Contains(nickname))
                            {
                                online.Add(nickname);
                                IO.Write(stream, "Y", 1);
                                auth = true;
                                Console.WriteLine("{0} ({1}) has joined to the chat", nickname, ip);
                                state = (string)SQLCommand("ban", pchash);
                                if (state == "N")
                                    IO.Write(stream, "N", 1);
                                else
                                {
                                    IO.Write(stream, "Y", 1);
                                    goto case "ban";
                                }
                            }
                            else
                                IO.Write(stream, "N", 1);
                            break;
                        case "msg":
                            if (auth && time2.ElapsedMilliseconds > 1000)
                            {
                                state = (string)SQLCommand("ban", pchash);
                                if (state == "N")
                                {
                                    IO.Write(stream, "YN", 2);
                                    string text = IO.Read(stream, 256);
                                    for (int i = 0; i < 15; i++)
                                        post[i] = post[i + 1];
                                    post[15] = new Post(DateTime.Now, nickname, text);
                                    SQLCommand("msg", post[15].date.ToString(), nickname, text);
                                    time2.Restart();
                                }
                                else
                                {
                                    IO.Write(stream, "YY", 2);
                                    goto case "ban";
                                }
                            }
                            else
                                IO.Write(stream, "NN", 2);
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
                                else
                                    first = false;
                                IO.Write(stream, ((char)n).ToString(), 1);
                                for (int i = 16 - n; i < 16; i++)
                                    IO.Write(stream, post[i].nickname + (post[i].nickname != " " ? ':' : ' ') + post[i].msg, 272);
                                last = post[15].date;
                            }
                            else
                                IO.Write(stream, "N", 1);
                            break;
                        case "ban":
                            if (auth)
                            {
                                IO.Write(stream, state.Split('/')[0], 19);
                                IO.Write(stream, state.Split('/')[1], 256);
                                cmd = "out";
                            }
                            break;
                    }
                }
                else
                {
                    if (!time1.IsRunning)
                        time1.Start();
                    else if (time1.Elapsed.Minutes == 1)
                        break;
                    Thread.Sleep(0);
                }
            }
        }
        catch (Exception error)
        {
            IO.error.Add(error);
        }
        finally
        {
            client.Close();
            if (auth)
            {
                online.Remove(nickname);
                Console.WriteLine("{0} ({1}) has left the chat", nickname, ip);
            }
        }
    }

    static void Main()
    {
        Directory.CreateDirectory(path + @"/messages");
        IO.path = path + @"/messages/errors.txt";
        error.Start();
        try
        {
            db = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Users\Владимир\Documents\Visual Studio 2017\Projects\Chat\Chat_Server\Chat.mdb");
            db.Open();
            cmd = db.CreateCommand();
            SQLCommand("all");
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
                        if (!work[i].IsAlive)
                            work.RemoveAt(i);
                    Console.Title = "Connections: " + work.Count.ToString();
                }
                else
                    Thread.Sleep(0);
            }
        }
        catch (Exception e)
        {
            IO.error.Add(e);
        }
        finally
        {
            while (IO.error.Count != 0)
                Thread.Sleep(0);
            error.Abort();
            work.Clear();
            if (server != null)
                server.Stop();
            if (db != null)
            {
                db.Close();
                db.Dispose();
            }
        }
    }
}