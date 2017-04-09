using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace Chat_Functions
{
    public class IO
    {
        public static string path = null;
        public static List<Exception> error = new List<Exception>();
        
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

        public static string Read(Stream n, int s)
        {
            try
            {
                s *= 2;
                byte[] buf = new byte[s];
                n.Read(buf, 0, s);
                return Encoding.Unicode.GetString(buf).TrimEnd((char)0);
            }
            catch (Exception e)
            {
                error.Add(e);
                return "null";
            }
        }

        public static void Write(Stream n, string m, int s)
        {
            try
            {
                s *= 2;
                byte[] buf = Encoding.Unicode.GetBytes(m);
                Array.Resize<byte>(ref buf, s);
                n.Write(buf, 0, s);
            }
            catch (Exception e)
            {
                error.Add(e);
            }
        }
    }
}