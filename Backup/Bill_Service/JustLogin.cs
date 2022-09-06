using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OracleClient;
using System.Security;
using System.Runtime.InteropServices;
using System.Data;


namespace Bill_Service
{
    class JustLogin
    {
        
        public JustLogin(string Database, string UserID, SecureString Password, bool Silent)
        {
            
        }

       public static bool OpenConnection(OracleConnection DBConn, bool Silent)
        {
            
            try
            {
                if (DBConn.State == ConnectionState.Closed | DBConn.State == ConnectionState.Broken)
                {
                    DBConn.Open();
                }
                return true;
            }
            catch 
            {
                return false;
            }

        }

       public static OracleConnection SetConnection(string UserName, string OraBase, SecureString PassWord)
       {
           OracleConnection connection = new OracleConnection();
           OracleConnectionStringBuilder connString = new OracleConnectionStringBuilder();
           connString.DataSource = OraBase;
           connString.UserID = UserName;
           IntPtr hWndPass = Marshal.SecureStringToCoTaskMemAnsi(PassWord);
           connString.Password = Marshal.PtrToStringAnsi(hWndPass, PassWord.Length);
           Marshal.ZeroFreeCoTaskMemAnsi(hWndPass);
           connection.ConnectionString = connString.ConnectionString;
           connString.Password = "";
           return connection;
       }
    }
}

