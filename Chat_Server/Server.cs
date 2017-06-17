/**
 * \file
 * \brief Реализация серверной стороны чата
*/
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Data.OleDb;

/**
 * \brief Система взаимодействия с клиентским приложением и СУБД: для каждого пользователя, зашедшего в чат, создается поток, обращение к базе данных последовательное
 * \author Макеев Владимир
 * \date 15.06.2017
*/
static class Server
{
    static readonly string[] symbols = { ";", "'", "--", "{*", "*}" };//!< Набор символов для удаление их из строки SQL-запроса

    //! Структура, описывающая сообщение чата
    struct Post
    {
    	public string nickname;//!< Имя пользователя, отправившего сообщение
        public string msg;//!< Текст сообщения
        public DateTime date;//!< Дата отправки сообщения

        //! Конструктор структуры, инициализирующий все её члены значениями соответсвующих параметров, указанных при его вызове
        public Post(DateTime time, string nick, string text)
        {
            date = time;
            nickname = nick;
            msg = text;
        }
    }

    static TcpListener server;//!< Сокет, использующийся для обработки подключений клиента к серверу
    static IPAddress ip;//!< IP-адрес сервера
    static int port;//!< Порт сервера
    static string path = Environment.CurrentDirectory;//!< Путь к директории, из которой была запущена программа
    static List<Thread> work = new List<Thread>();//!< Список потоков для работы с входящими содинениями
    static Thread error = new Thread(IO.ErrorControl);//!< Поток обработки ошибок
    static Post[] post = new Post[16];//!< Массив, содержащий последние 16 сообщений, которые были отправлены в чат или загружены из БД
    static OleDbConnection db;//!< Экземпляр класса для подключения к БД
    static OleDbCommand cmd;//!< Экземпляр класса для выполнения запросов к БД и получения соответствующих результатов
    static HashSet<string> online = new HashSet<string>();//!< Множество имен пользователей, находящихся в чате в данный момент
    static bool busy = false;//!< Переменная для оповещения других потоков, идет ли выполнение запроса к базе данных

    /**
     * \brief Проверка строки, полученной в ходе выполнения запроса к БД, на содержание null-значения
     * \param[in] o Строка, содержащая результат запроса
     * \return true, если ссылка на строку имеет нулевое значение (DBNull.Value или null), иначе - false
    */
    static bool IsNull(object o)
    {
        return (o == DBNull.Value || o == null);
    }

    /**
     * \brief Создание и выполнение запроса к БД, обработка полученных результатов
     * \param[in] c Строка, содержащая команду, определющую тип запроса к БД, например: регистрация и авторизация пользователей, получение и запись сообщений чата, выдача бана
     * \param[in] d Входные параметры, число которых зависит от вида команды
     * \return Объект, являющийся преобразованным результатом выполнения запроса
    */
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

    /**
     * \brief Проверка на корректность имени пользователя (оно может содержать только строчные и заглавные буквы, десятичные цифры)
     * \param[in] s Строка, содержащая имя пользователя
     * \return true, если имя удовлетворяет указанным требованим, иначе - false
    */
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

    /**
     * \brief Регистрация нового пользователя и авторизация существующего
     * \param[in] n Поток ввода-вывода, являющийся частью соответствующего клиентского сокета
     * \param[out] l Параметр, в который будет записано имя пользователя
     * \param[out] h Параметр, в который будет записано значение хэш-функции, аргументом которой служат данные о компьютере (информация о видеокарте, процессоре, жестких дисках)
     * \param[in] c Строка, сообщающая методу, авторизировать или зарегистрировать пользователя
     * \return true, если регистрация/авторизация прошла успешно, или false, если пароль и логин введены неверно или некорректно, если пользователь уже существует или ему выдан бан, если возникло исключение
    */
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

    /**
     * \brief Обмен данными с клиентом, обработка входящих команд (регистрация, отправка сообщений и т.д.), обращение к базе данных, вывод информации о посещении чата
     * \param[in] o Клиентский сокет, с которым было установлено соединение
    */
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

    //! Точка входа в программу, создание необходимых директорий и файлов, установка и разрыв соединения с БД, инициализация серверного сокета и принятие запросов на подключение, создание потока обработки ошибок, освобождение памяти
    static void Main()
    {
        Directory.CreateDirectory(path + @"/messages");
        IO.path = path + @"/messages/errors.txt";
        error.Start();
        try
        {
            db = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Users\ACER\Documents\GitHub\Chat\Chat_Server\Chat.mdb");
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