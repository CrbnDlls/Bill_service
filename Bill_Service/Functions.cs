using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OracleClient;
using System.Data;
using System.Threading;


namespace Bill_Service
{
    class Functions
    {
        
        /// <summary>
        /// Добавляет ноль к строке, спереди, если длинна строки = 1
        /// </summary>
        /// <param name="input">Строка для проверки</param>
        /// <returns>string</returns>
        public static string AddZero(string input)
        {
            if (input.Length == 1)
            {
                input = "0" + input;
            }
            return input;
        }

        /// <summary>
        /// Добавляет ноль к строке, спереди, если длинна строки = 1
        /// </summary>
        /// <param name="input">Строка для проверки</param>
        /// <returns>string</returns>
        public static string AddZero(int input)
        {
            if (input.ToString().Length == 1)
            {
                return "0" + input.ToString();
            }
            else
            {
                return input.ToString();
            }

        }

        /// <summary>
        /// Возвращает OracleDataReader, при вводе OracleCommand; выводит окно с возможностью повторить попытку или отменить, при отмене возвращает null
        /// </summary>
        /// <param name="inputCmd">Команда Оракле для которой создать ридер</param>
        /// <returns>OracleDataReader</returns>
        public static OracleDataReader ExecuteReader(OracleCommand inputCmd, bool Silent)
        {
            OracleDataReader reader = null;
       
            try
            {
                if (JustLogin.OpenConnection(inputCmd.Connection, Silent))
                {
                    reader = inputCmd.ExecuteReader();
                }
            }
            catch 
            {
               
            }
            return reader;
        }



        /// <summary>
        /// Возвращает DataTable, с данными выборки косанды Oracle;
        /// </summary>
        /// <param name="cmd">OracleDataReader которому перевести строку</param>
        /// <returns>DataTable</returns>
        public static DataTable GetData(OracleCommand cmd, bool Silent)
        {
            
            OracleDataReader reader = ExecuteReader(cmd, Silent);
            if (reader != null)
            {
                DataTable result = new DataTable();
                try
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        result.Columns.Add();
                    }
                }
                catch 
                {
                    return null;
                }

                try
                {
                    if (JustLogin.OpenConnection(cmd.Connection, Silent))
                    {
                                         
                        while (reader.Read())
                        {
                            DataRow row = result.NewRow();

                            for (int x = 0; x < reader.FieldCount; x++)
                            {
                                switch (reader.GetDataTypeName(x))
                                {
                                    case "VARCHAR2":
                                        try
                                        {
                                            row[x] = reader.GetString(x);
                                        }
                                        catch
                                        {

                                        }
                                        break;
                                    case "NUMBER":
                                        try
                                        {
                                            OracleNumber num = reader.GetOracleNumber(x);
                                            if (!num.IsNull)
                                            {
                                                row[x] = num.Value;
                                            }
                                        }
                                        catch 
                                        {
                                            
                                        }
                                        break;
                                    case "DATE":
                                        try
                                        {

                                            row[x] = reader.GetString(x);
                                        }
                                        catch
                                        {

                                        }
                                        break;
                                }
                            }
                            result.Rows.Add(row);
                        }
                        reader.Close();
                        cmd.Connection.Close();
                    }
                    else
                    {
                        return null;
                    }
                }
                catch 
                {
                    return null;
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Возвращает true если команда исполнена; выводит окно с возможностью повторить попытку или отменить, при отмене возвращает false
        /// </summary>
        /// <param name="cmd">Команда Оракле которую необходимо выполнить</param>
        /// <returns>bool</returns>
        public static bool ExecuteNonQuery(OracleCommand cmd, bool Silent)
        {
            
            try
            {
                if (JustLogin.OpenConnection(cmd.Connection, Silent))
                {
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch 
            {
                return false;
            }
        }
    }
}
