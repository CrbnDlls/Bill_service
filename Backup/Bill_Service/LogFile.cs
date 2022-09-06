using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Bill_Service
{
    /// <summary>
    /// Класс реализут запись сообщений в файл журнала
    /// </summary>
    class LogFile
    {
        private string FileName;
        private string ErrorMessage;
        private string Message;

        /// <summary>
        /// Путь и Имя файла в который необходимо записать сообщение. 
        /// Например: C:\TEMP\AlarmDog.log  для записи в каталог C:\TEMP
        /// или AlarmDog.log для записи в каталог расположения программы.
        /// </summary>
        public string ПутьиИмяФайла
        {
            get { return FileName; }
            set { FileName = value; }
        }
        /// <summary>
        /// Сообщение ошибки возникшей при выполнении WriteToLogFile
        /// </summary>
        public string Ошибка
        { get { return ErrorMessage; } }

        
        private void CheckFileName()
        {
            string PathF, Name;
            char[] invalidChars;

            if (FileName.LastIndexOf("\\") != -1)
            {
                PathF = FileName.Remove(FileName.LastIndexOf("\\"));
                Name = FileName.Substring(FileName.LastIndexOf("\\") + 1);

                if (Name == "")
                {
                    ErrorMessage = "Строка \"Имя файла\" не заполнена.";
                    return;
                }

                invalidChars = Path.GetInvalidPathChars();

                foreach (char sign in PathF.ToCharArray())
                {
                    for (int i = 0; i <= invalidChars.Count() - 1; i++)
                    {
                        if (sign == invalidChars[i])
                        {
                            ErrorMessage = "В строке \"Путь\" недопустимый символ: " + sign;

                            return;
                        }
                    }
                }

                if (!Directory.Exists(PathF))
                {
                    ErrorMessage = "Данного пути не существует: " + PathF;

                    return;
                }

                invalidChars = Path.GetInvalidFileNameChars();

                foreach (char sign in Name.ToCharArray())
                {
                    for (int i = 0; i <= invalidChars.Count() - 1; i++)
                    {
                        if (sign == invalidChars[i])
                        {
                            ErrorMessage = "В строке \"Имя файла\" недопустимый символ: " + sign;
                            return;
                        }
                    }
                }
            }
            else
            {
                invalidChars = Path.GetInvalidFileNameChars();

                foreach (char sign in FileName.ToCharArray())
                {
                    for (int i = 0; i <= invalidChars.Count() - 1; i++)
                    {
                        if (sign == invalidChars[i])
                        {
                            ErrorMessage = "В строке \"Имя файла\" недопустимый символ: " + sign;
                            return;
                        }
                    }
                }
            }

            ErrorMessage = null;
        }

        /// <summary>
        /// Записывает сообщение в файл указанный в ПутьиИмяФайла
        /// </summary>
        /// <param name="ПутьиИмяФайла">Путь и Имя файла в который необходимо записать сообщение. 
        /// Например: C:\TEMP\AlarmDog.log  для записи в каталог C:\TEMP
        /// или AlarmDog.log для записи в каталог расположения программы.</param>
        /// <param name="Сообщение">Текст сообщения которое необходимо записать.</param>
        public void WriteToLogFile(string ПутьиИмяФайла, string Сообщение) 
        {
            FileName = ПутьиИмяФайла;
            Message = Сообщение;
            if (Message.IndexOf("\r") != -1)
            {
                Message = Message.Substring(0, Message.IndexOf("\r"));
            }
            CheckFileName();
            if (ErrorMessage != null)
            {
                return;
            }

            ErrorMessage = null;
            using (StreamWriter fileWriter = new StreamWriter(FileName, true, Encoding.Default))
            {
                fileWriter.WriteLine(DateTime.Now.TimeOfDay.ToString().Substring(0, 8) + " - " + Message);
            } 

        }

        /// <summary>
        /// Записывает сообщение в файл указанный в ПутьиИмяФайла
        /// </summary>
        /// <param name="Сообщение">Текст сообщения которое необходимо записать.</param>
        public void WriteToLogFile(string Сообщение)
        {

            if (FileName == null)
            {
                ErrorMessage = "Строка \"ПутьиИмяФайла\" не заполнена.";
                return;
            }

            Message = Сообщение;
            if (Message.IndexOf("\r") != -1)
            {
                Message = Message.Substring(0, Message.IndexOf("\r"));
            }
            CheckFileName();
            if (ErrorMessage != null)
            {
                return;
            }
            ErrorMessage = null;
            using (StreamWriter fileWriter = new StreamWriter(FileName, true, Encoding.Default))
            {
                fileWriter.WriteLine(DateTime.Now.TimeOfDay.ToString().Substring(0, 8) + " - " + Message);
            }
        }

        public string GetLogFileName(string LogFileNameFormat)
        {
            string Date, Month;

            if (DateTime.Now.Day.ToString().Length == 1)
            {
                Date = "0" + DateTime.Now.Day.ToString();
            }
            else
            {
                Date = DateTime.Now.Day.ToString();
            }
            if (DateTime.Now.Month.ToString().Length == 1)
            {
                Month = "0" + DateTime.Now.Month.ToString();
            }
            else
            {
                Month = DateTime.Now.Month.ToString();
            }

            LogFileNameFormat = LogFileNameFormat.Replace("dd", Date);
            LogFileNameFormat = LogFileNameFormat.Replace("mm", Month);
            LogFileNameFormat = LogFileNameFormat.Replace("yyyy", DateTime.Now.Year.ToString());
            LogFileNameFormat = LogFileNameFormat.Replace("yy", DateTime.Now.Year.ToString().Substring(2));
            FileName = LogFileNameFormat;
            return LogFileNameFormat;
        }

    }
}
