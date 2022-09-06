using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bill_Service
{
    /// <summary>
    /// Класс для поиска файлов по маскам, с проверкой на недопустимые символы и существование каталога
    /// </summary>
    class DirectorySearcher
    {
        private string Dir;
        private string Filter;
        private string[] FileList;
        private string[] ErrorMessage;
        private SearchOption SerOp;

        /// <summary>
        /// Каталог в котором необходимо произвести поиск
        /// </summary>
        public string Путь
        {
            get { return Dir; }
            set { Dir = value; }
        }

        /// <summary>
        /// Маски имен файлов по которым необходимо произвести поиск, разделенные "|"; Например: "*.txt|200?.doc".
        /// </summary>
        public string Маска
        {
            get { return Filter; }
            set { Filter = value; }
        }

        /// <summary>
        /// Список файлов полученый при выполнении SearchDirectory
        /// </summary>
        public string[] СписокФайлов
        { get { return FileList; } }

        /// <summary>
        /// Список сообщений об ошибках полученый при выполнении SearchDirectory
        /// </summary>
        public string[] Ошибка
        {
            get { return ErrorMessage; }
        }
        
        /// <summary>
        /// Производить поиск в верхней директории или в верхней директории и в поддиректориях.
        /// По умолчанию в верхней.
        /// </summary>
        public SearchOption КритерийПоиска
        {
            get { return SerOp; }
            set { SerOp = value; }
        }

        /// <summary>
        /// Производит поиск в каталоге и вносит найденные файлы в СписокФайлов
        /// </summary>
        /// <param name="Путь">Каталог в котором необходимо произвести поиск.</param>
        /// <param name="Маска">Маски имен файлов по которым необходимо произвести поиск, разделенные "|"; Например: "*.txt|200?.doc".</param>
        /// <param name="КритерийПоиска">Производить поиск в верхней директории или в верхней директории и в поддиректориях.</param>
        public void SearchDirectory(string Путь, string Маска, SearchOption КритерийПоиска)
        {
            char[] invalidChars;
            string[] Filters;

            Dir = Путь;
            Filter = Маска;
            SerOp = КритерийПоиска;
            ErrorMessage = null;
            FileList = null;
            invalidChars = Path.GetInvalidPathChars();
            
            foreach (char sign in Путь.ToCharArray())
            {
                for (int i = 0; i <= invalidChars.Count()-1; i++)
                {
                    if (sign == invalidChars[i]) 
                    {
                        if (ErrorMessage == null)
                        {
                            ErrorMessage = new string[1];
                            ErrorMessage[0] = "В строке \"Путь\" недопустимый символ: " + sign;
                        }
                        else
                        {
                            ErrorMessage = ErrorMessage.Union(("В строке \"Путь\" недопустимый символ: " + sign).Split(("|").ToCharArray(), StringSplitOptions.RemoveEmptyEntries)).ToArray();
                        }
                    }
                }
            }

            Filters = Маска.Split(("|").ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            
            foreach (string line in Filters)
            {
                foreach (char sign in line.ToCharArray())
                {
                    for (int i = 0; i <= invalidChars.Count() - 1; i++)
                    {
                        if (sign == invalidChars[i])
                        {
                            if (ErrorMessage == null)
                            {
                                ErrorMessage = new string[1];
                                ErrorMessage[0] = "В строке \"Маска\" недопустимый символ: " + sign;
                            }
                            else
                            {
                                ErrorMessage = ErrorMessage.Union(("В строке \"Маска\" недопустимый символ: " + sign).Split(("|").ToCharArray(), StringSplitOptions.RemoveEmptyEntries)).ToArray();
                            }
                        }
                    }
                }
            }

            if (ErrorMessage != null)
            {
                return;
            }

            if (!Directory.Exists(Путь))
            {
                ErrorMessage = new string[1];
                ErrorMessage[0] = "Данного пути не существует: " + Путь;
                return; 
            }

            
            ErrorMessage = null;
            foreach (string line in Filters) 
            {
                if (FileList == null)
                {
                    try
                    {
                        FileList = Directory.GetFiles(Путь, line, SerOp);
                    }
                    catch (Exception e)
                    {
                        ErrorMessage = new string[2];
                        ErrorMessage[0] = Путь;
                        ErrorMessage[1] = e.Message;
                        return;
                    }
                }
                else
                {
                    try
                    {
                        FileList = FileList.Union(FileList = Directory.GetFiles(Путь, line, SerOp)).ToArray();
                    }
                    catch (Exception e)
                    {
                        ErrorMessage = new string[2];
                        ErrorMessage[0] = Путь;
                        ErrorMessage[1] = e.Message;
                        FileList = null;
                        return;
                    }
                }
            }
            if (FileList != null && FileList.Count() == 0)
            {
                FileList = null;
            }
            
        }

        /// <summary>
        /// Производит поиск в каталоге и вносит найденные файлы в СписокФайлов
        /// </summary>
        /// <param name="Путь">Каталог в котором необходимо произвести поиск.</param>
        /// <param name="Маска">Маски имен файлов по которым необходимо произвести поиск, разделенные "|"; Например: "*.txt|200?.doc".</param>
        public void SearchDirectory(string Путь, string Маска)
        {
            char[] invalidChars;
            string[] Filters;

            Dir = Путь;
            Filter = Маска;
            ErrorMessage = null;
            FileList = null;
            invalidChars = Path.GetInvalidPathChars();

            foreach (char sign in Путь.ToCharArray())
            {
                for (int i = 0; i <= invalidChars.Count() - 1; i++)
                {
                    if (sign == invalidChars[i])
                    {
                        if (ErrorMessage == null)
                        {
                            ErrorMessage = new string[1];
                            ErrorMessage[0] = "В строке \"Путь\" недопустимый символ: " + sign;
                        }
                        else
                        {
                            ErrorMessage = ErrorMessage.Union(("В строке \"Путь\" недопустимый символ: " + sign).Split(("|").ToCharArray(), StringSplitOptions.RemoveEmptyEntries)).ToArray();
                        }
                    }
                }
            }

            Filters = Маска.Split(("|").ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in Filters)
            {
                foreach (char sign in line.ToCharArray())
                {
                    for (int i = 0; i <= invalidChars.Count() - 1; i++)
                    {
                        if (sign == invalidChars[i])
                        {
                            if (ErrorMessage == null)
                            {
                                ErrorMessage = new string[1];
                                ErrorMessage[0] = "В строке \"Маска\" недопустимый символ: " + sign;
                            }
                            else
                            {
                                ErrorMessage = ErrorMessage.Union(("В строке \"Маска\" недопустимый символ: " + sign).Split(("|").ToCharArray(), StringSplitOptions.RemoveEmptyEntries)).ToArray();
                            }
                        }
                    }
                }
            }

            if (ErrorMessage != null)
            {
                return;
            }

            if (!Directory.Exists(Путь))
            {
                ErrorMessage = new string[1];
                ErrorMessage[0] = "Данного пути не существует: " + Путь;
                return;
            }


            ErrorMessage = null;
            foreach (string line in Filters)
            {
                if (FileList == null)
                {
                    try
                    {
                        FileList = Directory.GetFiles(Путь, line, SerOp);
                    }
                    catch (Exception e)
                    {
                        ErrorMessage = new string[2];
                        ErrorMessage[0] = Путь;
                        ErrorMessage[1] = e.Message;
                        return;
                    }
                }
                else
                {
                    try
                    {
                        FileList = FileList.Union(FileList = Directory.GetFiles(Путь, line, SerOp)).ToArray();
                    }
                    catch (Exception e)
                    {
                        ErrorMessage = new string[2];
                        ErrorMessage[0] = Путь;
                        ErrorMessage[1] = e.Message;
                        FileList = null;
                        return;
                    }
                }
            }
            if (FileList != null && FileList.Count() == 0)
            {
                FileList = null;
            }
        }
        /// <summary>
        /// Производит поиск в каталоге и вносит найденные файлы в СписокФайлов
        /// </summary>
        public void SearchDirectory()
        {
            char[] invalidChars;
            string[] Filters;
            ErrorMessage = null;
            FileList = null;

            if (Путь == null | Маска == null)
            {
                if (Путь == null) 
                {
                    ErrorMessage = new string[1];
                    ErrorMessage[0] = "Параметр \"Путь\" не заполнен";
                }
                if (Маска == null)
                {
                    if (ErrorMessage == null)
                    {
                        ErrorMessage = new string[1];
                        ErrorMessage[0] = "Параметр \"Маска\" не заполнен";
                    }
                    else
                    {
                        ErrorMessage = ErrorMessage.Union(("Параметр \"Маска\" не заполнен").Split(("|").ToCharArray())).ToArray();
                    }
                }
                return; 
            }
            
            invalidChars = Path.GetInvalidPathChars();

            foreach (char sign in Путь.ToCharArray())
            {
                for (int i = 0; i <= invalidChars.Count() - 1; i++)
                {
                    if (sign == invalidChars[i])
                    {
                        if (ErrorMessage == null)
                        {
                            ErrorMessage = new string[1];
                            ErrorMessage[0] = "В строке \"Путь\" недопустимый символ: " + sign;
                        }
                        else
                        {
                            ErrorMessage = ErrorMessage.Union(("В строке \"Путь\" недопустимый символ: " + sign).Split(("|").ToCharArray(), StringSplitOptions.RemoveEmptyEntries)).ToArray();
                        }
                    }
                }
            }

            Filters = Маска.Split(("|").ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in Filters)
            {
                foreach (char sign in line.ToCharArray())
                {
                    for (int i = 0; i <= invalidChars.Count() - 1; i++)
                    {
                        if (sign == invalidChars[i])
                        {
                            if (ErrorMessage == null)
                            {
                                ErrorMessage = new string[1];
                                ErrorMessage[0] = "В строке \"Маска\" недопустимый символ: " + sign;
                            }
                            else
                            {
                                ErrorMessage = ErrorMessage.Union(("В строке \"Маска\" недопустимый символ: " + sign).Split(("|").ToCharArray(), StringSplitOptions.RemoveEmptyEntries)).ToArray();
                            }
                        }
                    }
                }
            }

            if (ErrorMessage != null)
            {
                return;
            }

            if (!Directory.Exists(Путь))
            {
                ErrorMessage = new string[1];
                ErrorMessage[0] = "Данного пути не существует: " + Путь;
                return;
            }

            
            ErrorMessage = null;
            foreach (string line in Filters)
            {
                if (FileList == null)
                {
                    try
                    {
                        FileList = Directory.GetFiles(Путь, line, SerOp);
                    }
                    catch (Exception e)
                    {
                        ErrorMessage = new string[2];
                        ErrorMessage[0] = Путь;
                        ErrorMessage[1] = e.Message;
                        return;
                    }
                }
                else
                {
                    try
                    {
                    FileList = FileList.Union(FileList = Directory.GetFiles(Путь, line, SerOp)).ToArray();
                    }
                    catch (Exception e)
                    {
                        ErrorMessage = new string[2];
                        ErrorMessage[0] = Путь;
                        ErrorMessage[1] = e.Message;
                        FileList = null;
                        return;
                    }
                }
            }
            if (FileList != null && FileList.Count() == 0)
            {
                FileList = null;
            }

        }
    }
}
