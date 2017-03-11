using System;
using System.IO;
using System.Text;

namespace Chat_Functions
{
    public class IO
    {
        public static StreamWriter errors = null;

        public static void ErrorToFile(Exception e)
        {
            if (errors != null)
                lock (errors) errors.WriteLine("Date: " + DateTime.Now.ToString() + Environment.NewLine + "Point: " + e.TargetSite.ToString() + Environment.NewLine + "Error: " + e.Message + Environment.NewLine);
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
                ErrorToFile(e);
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
                ErrorToFile(e);
            }
        }
    }
}