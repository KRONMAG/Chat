/**
 * \file
 * \brief Реализация ввода-вывода для чат-клиента и чат-сервера
*/
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.Security.Cryptography;
using System.Numerics;

/**
 * \brief Система отправки и получения данных из входных и выходных потоков, вывод в файл информации об ошибах, возникших при работе программы
 * \author Макеев Владимир
 * \date 15.06.2017
*/
public static class IO
{
    public static string path = null;//!< Путь к файлу с описанием возникших ошибок
    public static List<Exception> error = new List<Exception>();//!< Список исключений, возникших в программе

    public static Dictionary<int, AESCrypt> list = new Dictionary<int, IO.AESCrypt>();

    public struct AESCrypt
    {
        public ICryptoTransform encrypt, decrypt;
    }

    public static void SetKey(Stream s, bool g)
    {
        RijndaelManaged crypt = new RijndaelManaged() { KeySize = 256, BlockSize = 256, Padding = PaddingMode.None, Mode = CipherMode.CBC };
        AESCrypt EDcrypt;
        BigInteger p, q, a, b, vector = new BigInteger();
        HashSet<BigInteger> set;
        if (g)
        {
            b = GetNumber(128);
            if (Read(s, 4) == "swap")
            {
                p = BigInteger.Parse(Read(s, 256));
                q = BigInteger.Parse(Read(s, 256));
                crypt.Key = KeyToByte(BigInteger.ModPow(BigInteger.Parse(Read(s, 256)), b, p));
                Write(s, BigInteger.ModPow(q, b, p).ToString(), 256);
                vector = BigInteger.Parse(Read(s, 256));
            }
        }
        else
        {
            vector = GetNumber(256);
            p = GetNumber(256);
            a = GetNumber(128);
            if (p % 2 == 0) p++;
            while (!TestKey(p)) p += 2;
            p = GetPKey(p, out set);
            q = GetQKey(p, set);
            Write(s, "swap", 4);
            Write(s, p.ToString(), 256);
            Write(s, q.ToString(), 256);
            Write(s, BigInteger.ModPow(q, a, p).ToString(), 256);
            crypt.Key = KeyToByte(BigInteger.ModPow(BigInteger.Parse(Read(s, 256)), a, p));
            Write(s, vector.ToString(), 256);
        }
        crypt.IV = KeyToByte(vector);
        EDcrypt.encrypt = crypt.CreateEncryptor();
        EDcrypt.decrypt = crypt.CreateDecryptor();
        list.Add(s.GetHashCode(), EDcrypt);
    }

    public static byte[] KeyToByte(BigInteger k)
    {
        byte[] res = new byte[32];
        for (int i = 0; i < 32; i++)
        {
            res[i] = (byte)(k % 256);
            k >>= 8;
        }
        return res;
    }

    public static int GetSize(int x)
    {
        return (int)Math.Ceiling(x / 32.0) * 32;
    }

    public static BigInteger GetNumber(int a)
    {
        BigInteger num = 1;
        int low = 65536, high = 2097153;
        Random rand = new Random();
        while (BigInteger.Log(num, 2) <= a)
            num *= rand.Next(low, high);
        return num;
    }

    public static bool TestKey(BigInteger n)
    {
        int k = (int)Math.Floor(BigInteger.Log(n, 2)), s = 0;
        BigInteger t = n - 1, a;
        Random rand = new Random();
        while (t % 2 == 0)
        {
            t /= 2;
            s++;
        }
        for (int i = 0; i < k; i++)
        {
            a = GetNumber(rand.Next(1, k + 1));
            BigInteger x = BigInteger.ModPow(a, t, n);
            if (x == 1 || x == n - 1) continue;
            for (int j = 0; j < s - 1; j++)
            {
                x = (x * x) % n;
                if (x == 1) return false;
                if (x == n - 1) break;
            }
            if (x == n - 1) continue;
            return false;
        }
        return true;
    }

    public static BigInteger GetQKey(BigInteger n, HashSet<BigInteger> s)
    {
        BigInteger q;
        BigInteger[] multi = new BigInteger[s.Count];
        bool state;
        s.CopyTo(multi);
        while (true)
        {
            state = true;
            q = GetNumber(32);
            for (int i = 0; i < multi.Length; i++)
                if (BigInteger.ModPow(q, (n - 1) / multi[i], n) == 1)
                {
                    state = false;
                    break;
                }
            if (state) break;
        }
        return q;
    }

    public static BigInteger GetPKey(BigInteger q, out HashSet<BigInteger> s)
    {
        int k = (int)Math.Floor(BigInteger.Log(4 * (q + 1), 2)) / 25, i;
        int[] p = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97 };
        BigInteger r, a, n;
        Random rand = new Random();
        HashSet<BigInteger> e = new HashSet<BigInteger>();
        while (true)
        {
            r = 2;
            e.Add(2);
            while (BigInteger.Log(r, 2) < k)
            {
                i = p[rand.Next(0, 25)];
                r *= i;
                e.Add(i);
            }
            a = GetNumber(32);
            n = q * r + 1;
            if (BigInteger.ModPow(a, n - 1, n) == 1 && BigInteger.ModPow(a, (n - 1) / q, n) != 1)
            {
                e.Add(q);
                s = e;
                return q * r + 1;
            }
            e.Clear();
        }
    }

    public static byte[] Crypt(byte[] b, ICryptoTransform c)
    {
        int size = GetSize(b.Length), index;
        Array.Resize(ref b, size);
        byte[] res = new byte[size];
        for (int i = 0; i < size / 32; i++)
        {
            index = i * 32;
            Array.Copy(c.TransformFinalBlock(b, index, 32), 0, res, index, 32);
        }
        return res;
    }

    //! Вывод информации о ошибках в файл в формате: дата, место возникновения (номер строки в коде), описание ошибки
    public static void ErrorControl()
    {
        while (true)
        {
            if (error.Count != 0)
            {
                if (path != null) File.AppendAllText(path, "Date: " + DateTime.Now.ToString() + Environment.NewLine + "Point: " + error[0].StackTrace.ToString() + Environment.NewLine + "Error: " + error[0].Message + Environment.NewLine);
                error.RemoveAt(0);
            }
            else Thread.Sleep(0);
        }
    }

    /**
     * \brief Чтение последовательности байтов из входного потока и её конвертирование в строку в кодировке Unicode
     * \param[in] n Входной поток
     * \param[in] s Число символов, которое необходимо прочитать
     * \return Строку, полученную из массива байтов
*/
    public static string Read(Stream n, int s)
    {
        try
        {
            bool state;
            AESCrypt crypt;
            s *= 2;
            byte[] buf = new byte[s];
            if ((state = list.TryGetValue(n.GetHashCode(), out crypt)) == true)
            {
                s = GetSize(s);
                Array.Resize(ref buf, s);
            }
            n.Read(buf, 0, s);
            return Encoding.Unicode.GetString(state ? Crypt(buf, crypt.decrypt) : buf).TrimEnd((char)0);
        }
        catch (Exception e)
        {
            error.Add(e);
            return "null";
        }
    }

    /**
     * \brief Запись строки в выходной поток в виде массивва байтов
     * \param[in] n Выходной поток
     * \param[in] m Строка, которую необходимо записать в поток
     * \param[in] s Количество символов в строке
*/
    public static void Write(Stream n, string m, int s)
    {
        try
        {
            AESCrypt crypt;
            s *= 2;
            byte[] buf = Encoding.Unicode.GetBytes(m);
            Array.Resize<byte>(ref buf, s);
            if (list.TryGetValue(n.GetHashCode(), out crypt))
            {
                buf = Crypt(buf, crypt.encrypt);
                s = buf.Length;
            }
            n.Write(buf, 0, s);
        }
        catch (Exception e)
        {
            error.Add(e);
        }
    }
}