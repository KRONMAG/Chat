/**
 * \file
 * \brief Реализация ввода-вывода для чат-клиента и чат-сервера
*/
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading;

/**
 * \brief Система отправки и получения данных из входных и выходных потоков, вывод в файл информации об ошибах, возникших при работе программы
 * \author Макеев Владимир
 * \date 15.06.2017
*/
    public static class IO
    {
        public static string path = null;//!< Путь к файлу с описанием возникших ошибок
        public static List<Exception> error = new List<Exception>();//!< Список исключений, возникших в программе
        
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