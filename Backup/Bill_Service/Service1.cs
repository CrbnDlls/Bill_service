using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Xml;
using System.IO;
using System.Data.OracleClient;
using System.Security;
using System.Net.NetworkInformation;

namespace Bill_Service
{
    public partial class Bill_Service : ServiceBase
    {
        string path, serveraddr;
        XmlDocument dataXml;
        OracleConnection conn;
        OracleCommand cmdBill, cmdBillData;
        LogFile log;
        System.Timers.Timer timerSend, timerRecieve;


        public Bill_Service()
        {
            InitializeComponent();
            log = new LogFile();
            
            log.ПутьиИмяФайла = AppDomain.CurrentDomain.BaseDirectory + "bill_service_" + Functions.AddZero(DateTime.Now.Month) + Functions.AddZero(DateTime.Now.Day) + ".log";
            log.WriteToLogFile(AppDomain.CurrentDomain.BaseDirectory);
            log.WriteToLogFile("InitializeComponent");
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(AppDomain.CurrentDomain.BaseDirectory + "bill_service.xml");
                log.WriteToLogFile("doc.Load(" + AppDomain.CurrentDomain.BaseDirectory + "\"bill_service.xml\")");
                dataXml = new XmlDocument();
                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "data.xml"))
                {
                    
                    XmlElement element = dataXml.CreateElement("wrksh");
                    dataXml.AppendChild(element);
                    log.WriteToLogFile("Create data.xml");
                }
                else
                {
                    dataXml.Load(AppDomain.CurrentDomain.BaseDirectory + "data.xml");
                    log.WriteToLogFile("Load data.xml");
                }
                path = doc.SelectSingleNode("/settings/path").Attributes["path"].Value;
                log.WriteToLogFile(path);
                serveraddr = doc.SelectSingleNode("/settings/path").Attributes["serveraddr"].Value;
                log.WriteToLogFile("Path = " + path);
                char[] c1s = new char[] { 'd', 'w', 'q', 'e', '7', '7', '7', '7', 'r', 'e', 't', 'e', 'w', 'g', 'r', 'e', '3', '4', '1', '2', '3', '4', '1', 'e' };
                SecureString c1 = new SecureString();

                foreach (char c1sc in c1s)
                {
                    c1.AppendChar(c1sc);
                }
                conn = JustLogin.SetConnection("LBAGUD", "LONDA", c1);
                log.WriteToLogFile("Set connection");
                XmlDocument updatesXml = new XmlDocument();


                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "updates.xml"))
                {
                    XmlElement element = updatesXml.CreateElement("updates");
                    element.SetAttribute("NUM", "0");
                    element.SetAttribute("NUMB", "0");
                    element.SetAttribute("NUMA", "0");
                    updatesXml.AppendChild(element);
                    updatesXml.Save(AppDomain.CurrentDomain.BaseDirectory + "updates.xml");
                    log.WriteToLogFile("Create updates.xml");
                }
                else
                {
                    updatesXml.Load(AppDomain.CurrentDomain.BaseDirectory + "updates.xml");
                    log.WriteToLogFile("Load updates.xml");
                }

                XmlDocument staffXml = new XmlDocument();
                try
                {
                    staffXml.Load(path + "//staff.xml");
                }
                catch (Exception sex)
                {
                    log.WriteToLogFile("Failed to load staff.xml | " + sex.Message);
                }
                if (Int32.Parse(staffXml.DocumentElement["updates"].Attributes["ID"].Value) > Int32.Parse(updatesXml.DocumentElement.Attributes["NUMA"].Value))
                {
                    int cnt = updatesXml.DocumentElement.ChildNodes.Count;
                    for (int i = 0; i < cnt; i++)
                    {
                        updatesXml.DocumentElement.RemoveChild(updatesXml.DocumentElement.ChildNodes[0]);
                    }
                    updatesXml.DocumentElement.Attributes["NUMA"].Value = staffXml.DocumentElement["updates"].Attributes["ID"].Value;
                    if (Int32.Parse(updatesXml.DocumentElement.Attributes["NUMA"].Value) > Int32.Parse(updatesXml.DocumentElement.Attributes["NUM"].Value))
                    {
                        updatesXml.DocumentElement.Attributes["NUM"].Value = updatesXml.DocumentElement.Attributes["NUMA"].Value;
                    }
                    updatesXml.Save(AppDomain.CurrentDomain.BaseDirectory + "updates.xml");

                }

                
                #region cmdBill LONDA.SAVEBILLTOCHECK Комманда добавления заглавия счета
                cmdBill = new OracleCommand();
                cmdBill.CommandType = CommandType.StoredProcedure;
                cmdBill.CommandText = "LONDA.SAVEBILLTOCHECK";
                cmdBill.Connection = conn;
                OracleParameter par = cmdBill.CreateParameter();
                par.ParameterName = "IN_NUM";
                par.OracleType = OracleType.Number;
                par.Size = 0;
                par.Direction = ParameterDirection.Input;
                cmdBill.Parameters.Add(par);
                par = cmdBill.CreateParameter();
                par.ParameterName = "IN_DAT";
                par.OracleType = OracleType.VarChar;
                par.Size = 0;
                par.Direction = ParameterDirection.Input;
                cmdBill.Parameters.Add(par);
                par = cmdBill.CreateParameter();
                par.ParameterName = "IN_SALON";
                par.OracleType = OracleType.Number;
                par.Size = 0;
                par.Direction = ParameterDirection.Input;
                cmdBill.Parameters.Add(par);
                par = cmdBill.CreateParameter();
                par.ParameterName = "IN_WORKERNUM";
                par.OracleType = OracleType.Number;
                par.Size = 0;
                par.Direction = ParameterDirection.Input;
                cmdBill.Parameters.Add(par);
                par = cmdBill.CreateParameter();
                par.ParameterName = "IN_WORKER";
                par.OracleType = OracleType.VarChar;
                par.Size = 0;
                par.Direction = ParameterDirection.Input;
                cmdBill.Parameters.Add(par);
                par = cmdBill.CreateParameter();
                par.ParameterName = "IN_STIME";
                par.OracleType = OracleType.VarChar;
                par.Size = 0;
                par.Direction = ParameterDirection.Input;
                cmdBill.Parameters.Add(par);
                par = cmdBill.CreateParameter();
                par.ParameterName = "IN_FTIME";
                par.OracleType = OracleType.VarChar;
                par.Size = 0;
                par.Direction = ParameterDirection.Input;
                cmdBill.Parameters.Add(par);
                par = cmdBill.CreateParameter();

                par.ParameterName = "RESULT";
                par.OracleType = OracleType.NVarChar;
                par.Size = 4000;
                par.Direction = ParameterDirection.Output;
                cmdBill.Parameters.Add(par);
                #endregion

                #region cmdBillData LONDA.SAVEBILLDATATOCHECK Комманда добавления строк счета
                cmdBillData = new OracleCommand();
                cmdBillData.CommandType = CommandType.StoredProcedure;
                cmdBillData.CommandText = "LONDA.SAVEBILLDATATOCHECK";
                cmdBillData.Connection = conn;
                par = cmdBillData.CreateParameter();
                par.ParameterName = "IN_BILL";
                par.OracleType = OracleType.Number;
                par.Size = 0;
                par.Direction = ParameterDirection.Input;
                cmdBillData.Parameters.Add(par);
                par = cmdBillData.CreateParameter();
                par.ParameterName = "IN_SCODE";
                par.OracleType = OracleType.Number;
                par.Size = 0;
                par.Direction = ParameterDirection.Input;
                cmdBillData.Parameters.Add(par);
                par = cmdBillData.CreateParameter();
                par.ParameterName = "IN_SNAME";
                par.OracleType = OracleType.VarChar;
                par.Size = 0;
                par.Direction = ParameterDirection.Input;
                cmdBillData.Parameters.Add(par);
                par = cmdBillData.CreateParameter();
                par.ParameterName = "IN_PRICE";
                par.OracleType = OracleType.VarChar;
                par.Size = 0;
                par.Direction = ParameterDirection.Input;
                cmdBillData.Parameters.Add(par);
                par = cmdBillData.CreateParameter();
                par.ParameterName = "IN_QUANTITY";
                par.OracleType = OracleType.Number;
                par.Size = 0;
                par.Direction = ParameterDirection.Input;
                cmdBillData.Parameters.Add(par);
                par = cmdBillData.CreateParameter();
                par.ParameterName = "IN_DISC";
                par.OracleType = OracleType.VarChar;
                par.Size = 0;
                par.Direction = ParameterDirection.Input;
                cmdBillData.Parameters.Add(par);
                par = cmdBillData.CreateParameter();
                par.ParameterName = "IN_SELLPRICE";
                par.OracleType = OracleType.VarChar;
                par.Size = 0;
                par.Direction = ParameterDirection.Input;
                cmdBillData.Parameters.Add(par);
                par = cmdBillData.CreateParameter();
                par.ParameterName = "IN_BO";
                par.OracleType = OracleType.Number;
                par.Size = 0;
                par.Direction = ParameterDirection.Input;
                cmdBillData.Parameters.Add(par);

                par = cmdBillData.CreateParameter();
                par.ParameterName = "RESULT";
                par.OracleType = OracleType.NVarChar;
                par.Size = 4000;
                par.Direction = ParameterDirection.Output;
                cmdBillData.Parameters.Add(par);
                #endregion

                IPStatus status = IPStatus.Unknown;
                try
                {
                    status = new Ping().Send(serveraddr).Status;
                }
                catch (Exception e) 
                {
                    log.WriteToLogFile("ping error: " + e.Message);
                }

                if (status == IPStatus.Success)
                {
                    log.WriteToLogFile("Ping success");
                    log.WriteToLogFile("Get updates");
                    GetUpdates(updatesXml, AppDomain.CurrentDomain.BaseDirectory + "updates.xml", conn);
                    
                }
                
                log.WriteToLogFile("SetUpdates(updatesXml, \"updates.xml\", " + path);
                SetUpdates(updatesXml, AppDomain.CurrentDomain.BaseDirectory + "updates.xml", path);
                
                
                timerSend = new System.Timers.Timer(10000);
                timerSend.AutoReset = true;
                timerSend.Elapsed += new System.Timers.ElapsedEventHandler(timerSend_Tick);
                timerRecieve = new System.Timers.Timer(1200000);
                timerRecieve.AutoReset = true;
                timerRecieve.Elapsed += new System.Timers.ElapsedEventHandler(timerRecieve_Tick);
                log.WriteToLogFile("Timers created");
                DirectorySearcher searcher = new DirectorySearcher();
                searcher.Путь = AppDomain.CurrentDomain.BaseDirectory;
                //log.WriteToLogFile(searcher.Путь);
                searcher.Путь = searcher.Путь.Substring(0, searcher.Путь.Length - 1);
                //log.WriteToLogFile(searcher.Путь);
                searcher.Маска = "bill_service_????.log";
                //log.WriteToLogFile(searcher.Маска);
                searcher.КритерийПоиска = SearchOption.TopDirectoryOnly;
                searcher.SearchDirectory();
                if (searcher.Ошибка == null)
                {
                    if (searcher.СписокФайлов != null)
                    {
                        for (int n = 0; n < searcher.СписокФайлов.Count(); n++)
                        {
                            //log.WriteToLogFile("Найден: " + searcher.СписокФайлов[n]);
                            //log.WriteToLogFile(searcher.СписокФайлов[n].Substring(searcher.СписокФайлов[n].Length - 8, 2));
                            //log.WriteToLogFile(DateTime.Now.Month.ToString());
                            if (int.Parse(searcher.СписокФайлов[n].Substring(searcher.СписокФайлов[n].Length - 8, 2)) == DateTime.Now.Month)
                            {
                                if (int.Parse(Functions.AddZero(DateTime.Now.Day)) - int.Parse(searcher.СписокФайлов[n].Substring(searcher.СписокФайлов[n].Length - 6, 2)) >= 7)
                                {
                                    File.Delete(searcher.СписокФайлов[n]);
                                    //log.WriteToLogFile("Удалить: " + searcher.СписокФайлов[n]);
                                }
                            }
                            else
                            {
                                if (DateTime.DaysInMonth(DateTime.Now.Year, int.Parse(searcher.СписокФайлов[n].Substring(searcher.СписокФайлов[n].Length - 8, 2))) - int.Parse(searcher.СписокФайлов[n].Substring(searcher.СписокФайлов[n].Length - 6, 2)) + int.Parse(Functions.AddZero(DateTime.Now.Day)) >= 7)
                                {
                                    File.Delete(searcher.СписокФайлов[n]);
                                    //log.WriteToLogFile("Удалить: " + searcher.СписокФайлов[n]);
                                }
                            }
                        }
                    }
                    /*else
                    {
                        for (int g = 0; g >= searcher.Ошибка.Count(); g++)
                        {
                            log.WriteToLogFile(searcher.СписокФайлов[g]);
                        }
                    }*/
                }
                /*else
                {
                    for (int g = 0; g >= searcher.Ошибка.Count(); g++)
                    {
                        log.WriteToLogFile(searcher.Ошибка[g]);
                    }
                }*/
            }
            catch (Exception e)
            {
                log.WriteToLogFile(e.Source + " | " + e.Message);
            }
        }

        protected override void OnStart(string[] args)
        {
            log.WriteToLogFile("In OnStart");
            timerSend.Enabled = true;
            timerRecieve.Enabled = true;
            log.WriteToLogFile("Timers Enabled");
           
        }

        protected override void OnStop()
        {
            log.WriteToLogFile("In OnStop");
            timerSend.Enabled = false;
            timerRecieve.Enabled = false;
            log.WriteToLogFile("Timers Disabled");
        }

        private void timerSend_Tick(object sender, EventArgs e)
        {
            log.WriteToLogFile("Send Time");
            log.WriteToLogFile("Load Searcher");
            DirectorySearcher searcher = new DirectorySearcher();
            searcher.Путь = path;
            searcher.Маска = "wrksh*.xml";
            searcher.КритерийПоиска = System.IO.SearchOption.TopDirectoryOnly;
            searcher.SearchDirectory();
            if (searcher.Ошибка == null)
            {
                if (searcher.СписокФайлов != null)
                {
                    log.WriteToLogFile("Files found: " + searcher.СписокФайлов.Count());
                    XmlDocument file = new XmlDocument();
                    for (int i = 0; i < searcher.СписокФайлов.Count(); i++)
                    {
                        log.WriteToLogFile("Load file: " + searcher.СписокФайлов[i]);
                        file.Load(searcher.СписокФайлов[i]);
                        XmlNode node = dataXml.ImportNode(file.DocumentElement.ChildNodes[0], true);// SelectSingleNode("/wrksh/WORKSHEET");
                        try
                        {
                            dataXml.DocumentElement.AppendChild(node);
                            log.WriteToLogFile("Save file: " + AppDomain.CurrentDomain.BaseDirectory + "data.xml");
                            dataXml.Save(AppDomain.CurrentDomain.BaseDirectory + "data.xml");
                            log.WriteToLogFile("Delete file: " + searcher.СписокФайлов[i]);
                            File.Delete(searcher.СписокФайлов[i]);
                        }
                        catch (Exception ex)
                        {
                            log.WriteToLogFile("Xml error: " + ex.Message);
                        }

                        

                    }
                }
                else
                {
                    log.WriteToLogFile("Files found: 0");
                }
 
            }
            else
            {
                foreach (string error in searcher.Ошибка)
                {
                    log.WriteToLogFile("Searcher error: " + error);
                }
            }
            
            
            
            if (dataXml.DocumentElement.ChildNodes.Count > 0)
            {
                IPStatus status = IPStatus.Unknown;
                try
                {
                    status = new Ping().Send(serveraddr).Status;
                }
                catch (Exception ex)
                {
                    log.WriteToLogFile("ping error: " + ex.Message);
                }

                if (status == IPStatus.Success)
                {
                    log.WriteToLogFile("Ping success");
                    log.WriteToLogFile("SendDataToBase(dataXml, AppDomain.CurrentDomain.BaseDirectory + \"data.xml\");");
                    SendDataToBase(dataXml, AppDomain.CurrentDomain.BaseDirectory + "data.xml");
                }
                
                
            }
            log.WriteToLogFile("Send Over");

        }

        // отправка данных в базу
        private void SendDataToBase(XmlDocument dataToSendXml, string SaveFileName)
        {
            XmlNodeList nodelst = dataToSendXml.SelectNodes("/wrksh/WORKSHEET");

            if (nodelst != null)
            {
                foreach (XmlNode node in nodelst)
                {
                    int idn = -1;
                    try
                    {
                        idn = Int32.Parse(node.Attributes["IDN"].Value);
                    }
                    catch
                    { }
                    if (idn == -1)
                    {
                        cmdBill.Parameters["IN_NUM"].Value = int.Parse(node.Attributes["ID"].Value.Substring(0, 5));
                        cmdBill.Parameters["IN_DAT"].Value = node.Attributes["ID"].Value.Substring(5, 8);
                        cmdBill.Parameters["IN_SALON"].Value = int.Parse(node.Attributes["SALON"].Value);
                        cmdBill.Parameters["IN_WORKERNUM"].Value = int.Parse(node.Attributes["STAFFID"].Value);
                        cmdBill.Parameters["IN_WORKER"].Value = node.Attributes["NAME"].Value;
                        cmdBill.Parameters["IN_STIME"].Value = node.Attributes["ID"].Value.Substring(13, 4);
                        cmdBill.Parameters["IN_FTIME"].Value = "xxx";
                        if (Functions.ExecuteNonQuery(cmdBill, true))
                        {
                            idn = int.Parse(cmdBill.Parameters["RESULT"].Value.ToString());
                            XmlAttribute attr = dataToSendXml.CreateAttribute("IDN");
                            attr.Value = idn.ToString();
                            node.Attributes.Append(attr);
                            dataToSendXml.Save(SaveFileName);
                        }
                    }

                    if (idn != -1)
                    {
                        XmlNodeList nodelst2 = dataToSendXml.SelectNodes("/wrksh/WORKSHEET[@IDN='" + idn.ToString() + "']/ITEM");
                        foreach (XmlNode childNode in nodelst2)
                        {
                            cmdBillData.Parameters["IN_BILL"].Value = idn;
                            cmdBillData.Parameters["IN_SCODE"].Value = int.Parse(childNode.Attributes["ID"].Value);
                            cmdBillData.Parameters["IN_SNAME"].Value = childNode.Attributes["NAME"].Value;
                            cmdBillData.Parameters["IN_PRICE"].Value = childNode.Attributes["PRICE"].Value;
                            cmdBillData.Parameters["IN_QUANTITY"].Value = int.Parse(childNode.Attributes["QUANTITY"].Value);
                            cmdBillData.Parameters["IN_DISC"].Value = childNode.Attributes["DISCOUNT"].Value;
                            cmdBillData.Parameters["IN_SELLPRICE"].Value = childNode.Attributes["SELLPRICE"].Value;
                            cmdBillData.Parameters["IN_BO"].Value = int.Parse(childNode.Attributes["BO"].Value);
                            if (Functions.ExecuteNonQuery(cmdBillData, true))
                            {
                                node.RemoveChild(childNode);
                                dataToSendXml.Save(SaveFileName);
                            }
                        }

                        if (node.ChildNodes.Count == 0)
                        {
                            dataToSendXml.DocumentElement.RemoveChild(node);
                            dataToSendXml.Save(SaveFileName);
                        }

                    }
                }
            }
        }

        private void timerRecieve_Tick(object sender, EventArgs e)
        {
            log.WriteToLogFile("Recieve Time");
            IPStatus status = IPStatus.Unknown;
            try
            {
                status = new Ping().Send(serveraddr).Status;
            }
            catch (Exception ex)
            {
                log.WriteToLogFile("ping error: " + ex.Message);
            }

            if (status == IPStatus.Success)
            {
                timerRecieve.Interval = 1200000;
                log.WriteToLogFile("Ping success");
                XmlDocument updatesXml = new XmlDocument();
                updatesXml.Load(AppDomain.CurrentDomain.BaseDirectory + "updates.xml");
                log.WriteToLogFile("Get Updates");
                if (GetUpdates(updatesXml, AppDomain.CurrentDomain.BaseDirectory + "updates.xml", conn))
                {
                    log.WriteToLogFile("Set Updates");
                    SetUpdates(updatesXml, AppDomain.CurrentDomain.BaseDirectory + "updates.xml", path);
                }
            }
            else
            {
                timerRecieve.Interval = 60000;
            }
            
            log.WriteToLogFile("Recieve Over");
        }

         //Вход в получение обновлений
        private bool GetUpdates(XmlDocument updatesXml, string updatesPath, OracleConnection connUpdates)
        {
            //CheckForUpdates();
            //Проверка наличия обновлений
            
            OracleCommand cmd = new OracleCommand("SELECT MAX(IDN) FROM LONDA.SALON_UPDATES", connUpdates);
            DataTable updData = Functions.GetData(cmd, true);
            if (updData != null)
            {
                if (Int32.Parse(updData.Rows[0].ItemArray[0].ToString()) > Int32.Parse(updatesXml.DocumentElement.Attributes["NUMB"].Value.ToString()))
                {
                    updatesXml.DocumentElement.Attributes["NUMB"].Value = updData.Rows[0].ItemArray[0].ToString();
                    updatesXml.Save(updatesPath);
                }
                updData.Dispose();
            }
                      
            // Если есть обновления, то скачиваем описания обновлений
            if (Int32.Parse(updatesXml.DocumentElement.Attributes["NUMB"].Value.ToString()) > Int32.Parse(updatesXml.DocumentElement.Attributes["NUM"].Value.ToString()))
            {
                /*FormWaiting waiting = new FormWaiting("Завантаження опису оновлень", "SELECT IDN, UTABLE, UIDN, to_char(UDATE,'dd/mm/yyyy'), ACTION FROM LONDA.SALON_UPDATES WHERE IDN > " + updatesXml.DocumentElement.Attributes["NUM"].Value.ToString() + " ORDER BY IDN ASC", new string[] { "", "" }, null, null, connUpdates, "default", new string[] { "#", "IDN", "UTABLE", "UIDN", "UDATE", "ACTION" }, true);
                waiting.ShowDialog();*/
                cmd.CommandText = "SELECT IDN, UTABLE, UIDN, to_char(UDATE,'dd/mm/yyyy'), ACTION FROM LONDA.SALON_UPDATES WHERE IDN > " + updatesXml.DocumentElement.Attributes["NUM"].Value.ToString() + " ORDER BY IDN ASC";
                updData = Functions.GetData(cmd, true);

                if (updData != null)
                {
                    for (int i = 0; i < updData.Rows.Count; i++)
                    {
                        XmlElement lineXml = updatesXml.CreateElement("todownload");
                        lineXml.SetAttribute("IDN", updData.Rows[i].ItemArray[0].ToString());
                        lineXml.SetAttribute("UTABLE", updData.Rows[i].ItemArray[1].ToString());
                        lineXml.SetAttribute("UIDN", updData.Rows[i].ItemArray[2].ToString());
                        lineXml.SetAttribute("UDATE", updData.Rows[i].ItemArray[3].ToString());
                        lineXml.SetAttribute("ACTION", updData.Rows[i].ItemArray[4].ToString());
                        updatesXml.DocumentElement.AppendChild(lineXml);
                        updatesXml.DocumentElement.Attributes["NUM"].Value = lineXml.Attributes["IDN"].Value;
                    }
                    updatesXml.Save(updatesPath);
                    updData.Dispose();
                }
            }
            
            // Получаем обновления по их описаниям
            int num = Int32.Parse(updatesXml.DocumentElement.Attributes["NUM"].Value.ToString());
            int numa = Int32.Parse(updatesXml.DocumentElement.Attributes["NUMA"].Value.ToString());
            bool OnOff = false;
            if (num > numa)
            {
                int quantity;
                if ((num-numa) >= 100)
                {
                    quantity = (num - numa) / 100;
                }
                else
                {
                    quantity = 1;
                }


                int Cnt = 0;
                int Cnt2 = 1;

                bool error;
                for (int i = Int32.Parse(updatesXml.DocumentElement.Attributes["NUMA"].Value.ToString()) + 1; i <= Int32.Parse(updatesXml.DocumentElement.Attributes["NUM"].Value.ToString()); i++)
                {

                    XmlNode node = updatesXml.SelectSingleNode("/updates/todownload[@IDN='" + i + "']");
                    if (node != null)
                    {
                        error = true;
                        XmlElement lineXml;
                        switch (node.Attributes["UTABLE"].Value)
                        {
                            case "STAFF":
                                switch (node.Attributes["ACTION"].Value)
                                {
                                    case "INSERT":
                                        cmd.CommandText = "SELECT STAFFID, FNAME || ' ' || FSTNAME, STAGE, PROF, IDN FROM LONDA.STAFF WHERE STAFFID IS NOT NULL AND HIREDATE IS NOT NULL AND DROPDATE IS NULL AND IDN = " + node.Attributes["UIDN"].Value.ToString();
                                        updData = Functions.GetData(cmd, true);

                                        if (updData != null)
                                        {
                                            lineXml = updatesXml.CreateElement("updateitself");
                                            lineXml.SetAttribute("VERSION", node.Attributes["IDN"].Value);
                                            lineXml.SetAttribute("ID", updData.Rows[0].ItemArray[0].ToString());
                                            lineXml.SetAttribute("NAME", updData.Rows[0].ItemArray[1].ToString());
                                            lineXml.SetAttribute("LEVEL", updData.Rows[0].ItemArray[2].ToString());
                                            lineXml.SetAttribute("PROF", updData.Rows[0].ItemArray[3].ToString());
                                            lineXml.SetAttribute("IDN", updData.Rows[0].ItemArray[4].ToString());
                                            lineXml.SetAttribute("DATE", node.Attributes["UDATE"].Value);
                                            lineXml.SetAttribute("ACTION", "INSERT");
                                            lineXml.SetAttribute("TYPE", "STAFF");
                                            updatesXml.DocumentElement.AppendChild(lineXml);
                                            updatesXml.DocumentElement.Attributes["NUMA"].Value = i.ToString();
                                            updatesXml.DocumentElement.RemoveChild(node);
                                            error = false;
                                            updData.Dispose();
                                            if (DateTime.Parse(node.Attributes["UDATE"].Value) < DateTime.Now.Date)
                                            {
                                                OnOff = true;
                                            }
                                        }
                                        else
                                        {
                                            XmlNodeList nodelst = updatesXml.SelectNodes("/updates/todownload[@UTABLE='STAFF' and @ACTION='DELETE' and @UIDN='" + node.Attributes["UIDN"].Value.ToString() + "']");
                                            if (nodelst != null)
                                            {
                                                foreach (XmlNode lineNode in nodelst)
                                                {
                                                    if (Int32.Parse(lineNode.Attributes["IDN"].Value.ToString()) > i)
                                                    {
                                                        updatesXml.DocumentElement.Attributes["NUMA"].Value = i.ToString();
                                                        updatesXml.DocumentElement.RemoveChild(node);
                                                        error = false;
                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                        break;
                                    case "DELETE":
                                        lineXml = updatesXml.CreateElement("updateitself");
                                        lineXml.SetAttribute("VERSION", node.Attributes["IDN"].Value);
                                        lineXml.SetAttribute("IDN", node.Attributes["UIDN"].Value);
                                        lineXml.SetAttribute("DATE", node.Attributes["UDATE"].Value);
                                        lineXml.SetAttribute("ACTION", "DELETE");
                                        lineXml.SetAttribute("TYPE", "STAFF");
                                        updatesXml.DocumentElement.AppendChild(lineXml);
                                        updatesXml.DocumentElement.Attributes["NUMA"].Value = i.ToString();
                                        updatesXml.DocumentElement.RemoveChild(node);
                                        error = false;
                                        if (DateTime.Parse(node.Attributes["UDATE"].Value) < DateTime.Now.Date)
                                        {
                                            OnOff = true;
                                        }
                                        break;
                                }
                                break;
                            case "STAGES":
                                switch (node.Attributes["ACTION"].Value)
                                {
                                    case "INSERT":
                                        cmd.CommandText = "SELECT IDN, STAGE FROM LONDA.STAGES WHERE IDN = " + node.Attributes["UIDN"].Value.ToString();
                                        updData = Functions.GetData(cmd, true);

                                        if (updData != null)
                                        {
                                            lineXml = updatesXml.CreateElement("updateitself");
                                            lineXml.SetAttribute("VERSION", node.Attributes["IDN"].Value);
                                            lineXml.SetAttribute("ID", updData.Rows[0].ItemArray[0].ToString());
                                            lineXml.SetAttribute("NAME", updData.Rows[0].ItemArray[1].ToString());
                                            lineXml.SetAttribute("DATE", node.Attributes["UDATE"].Value);
                                            lineXml.SetAttribute("ACTION", "INSERT");
                                            lineXml.SetAttribute("TYPE", "STAGES");
                                            updatesXml.DocumentElement.AppendChild(lineXml);
                                            updatesXml.DocumentElement.Attributes["NUMA"].Value = i.ToString();
                                            updatesXml.DocumentElement.RemoveChild(node);
                                            error = false;
                                            updData.Dispose();
                                            if (DateTime.Parse(node.Attributes["UDATE"].Value) < DateTime.Now.Date)
                                            {
                                                OnOff = true;
                                            }
                                        }
                                        else
                                        {
                                            XmlNodeList nodelst = updatesXml.SelectNodes("/updates/todownload[@UTABLE='STAGES' and @ACTION='DELETE' and @UIDN='" + node.Attributes["UIDN"].Value.ToString() + "']");
                                            if (nodelst != null)
                                            {
                                                foreach (XmlNode lineNode in nodelst)
                                                {
                                                    if (Int32.Parse(lineNode.Attributes["IDN"].Value.ToString()) > i)
                                                    {
                                                        updatesXml.DocumentElement.Attributes["NUMA"].Value = i.ToString();
                                                        updatesXml.DocumentElement.RemoveChild(node);
                                                        error = false;
                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                        break;
                                    case "DELETE":
                                        lineXml = updatesXml.CreateElement("updateitself");
                                        lineXml.SetAttribute("VERSION", node.Attributes["IDN"].Value);
                                        lineXml.SetAttribute("ID", node.Attributes["UIDN"].Value);
                                        lineXml.SetAttribute("DATE", node.Attributes["UDATE"].Value);
                                        lineXml.SetAttribute("ACTION", "DELETE");
                                        lineXml.SetAttribute("TYPE", "STAGES");
                                        updatesXml.DocumentElement.AppendChild(lineXml);
                                        updatesXml.DocumentElement.Attributes["NUMA"].Value = i.ToString();
                                        updatesXml.DocumentElement.RemoveChild(node);
                                        error = false;
                                        if (DateTime.Parse(node.Attributes["UDATE"].Value) < DateTime.Now.Date)
                                        {
                                            OnOff = true;
                                        }
                                        break;
                                }
                                break;
                            case "CASH_REG_CODE":
                                switch (node.Attributes["ACTION"].Value)
                                {
                                    case "INSERT":
                                        cmd.CommandText = "SELECT c.CODE, s.SERV_NAME || ' ' || v.VNAME, c.PRICE, c.PRICE10, c.PRICE50, c.PRICESTAFF, c.IDN FROM LONDA.CASH_REG_CODE c, LONDA.SERV_INFO s, LONDA.SERV_VOL v WHERE c.SERVICE = s.IDN AND s.VOL = v.IDN AND c.IDN = " + node.Attributes["UIDN"].Value.ToString();
                                        updData = Functions.GetData(cmd, true);

                                        if (updData != null)
                                        {
                                            lineXml = updatesXml.CreateElement("updateitself");
                                            lineXml.SetAttribute("VERSION", node.Attributes["IDN"].Value);
                                            lineXml.SetAttribute("ID", updData.Rows[0].ItemArray[0].ToString());
                                            lineXml.SetAttribute("NAME", updData.Rows[0].ItemArray[1].ToString());
                                            lineXml.SetAttribute("PRICE0", updData.Rows[0].ItemArray[2].ToString());
                                            if (updData.Rows[0].ItemArray[3].ToString() != "")
                                            {
                                                lineXml.SetAttribute("PRICE10", updData.Rows[0].ItemArray[3].ToString());
                                            }
                                            if (updData.Rows[0].ItemArray[4].ToString() != "")
                                            {
                                                lineXml.SetAttribute("PRICE50", updData.Rows[0].ItemArray[4].ToString());
                                            }
                                            if (updData.Rows[0].ItemArray[5].ToString() != "")
                                            {
                                                lineXml.SetAttribute("PRICESTAFF", updData.Rows[0].ItemArray[5].ToString());
                                            }
                                            lineXml.SetAttribute("IDN", updData.Rows[0].ItemArray[6].ToString());
                                            lineXml.SetAttribute("DATE", node.Attributes["UDATE"].Value);
                                            lineXml.SetAttribute("ACTION", "INSERT");
                                            lineXml.SetAttribute("TYPE", "CASH_REG_CODE");
                                            updatesXml.DocumentElement.AppendChild(lineXml);
                                            updatesXml.DocumentElement.Attributes["NUMA"].Value = i.ToString();
                                            updatesXml.DocumentElement.RemoveChild(node);
                                            error = false;
                                            updData.Dispose();
                                            if (DateTime.Parse(node.Attributes["UDATE"].Value) < DateTime.Now.Date)
                                            {
                                                OnOff = true;
                                            }
                                        }
                                        else
                                        {
                                            XmlNodeList nodelst = updatesXml.SelectNodes("/updates/todownload[@UTABLE='CASH_REG_CODE' and @ACTION='DELETE' and @UIDN='" + node.Attributes["UIDN"].Value.ToString() + "']");
                                            if (nodelst != null)
                                            {
                                                foreach (XmlNode lineNode in nodelst)
                                                {
                                                    if (Int32.Parse(lineNode.Attributes["IDN"].Value.ToString()) > i)
                                                    {
                                                        updatesXml.DocumentElement.Attributes["NUMA"].Value = i.ToString();
                                                        updatesXml.DocumentElement.RemoveChild(node);
                                                        error = false;
                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                        break;
                                    case "DELETE":
                                        lineXml = updatesXml.CreateElement("updateitself");
                                        lineXml.SetAttribute("VERSION", node.Attributes["IDN"].Value);
                                        lineXml.SetAttribute("IDN", node.Attributes["UIDN"].Value);
                                        lineXml.SetAttribute("DATE", node.Attributes["UDATE"].Value);
                                        lineXml.SetAttribute("ACTION", "DELETE");
                                        lineXml.SetAttribute("TYPE", "CASH_REG_CODE");
                                        updatesXml.DocumentElement.AppendChild(lineXml);
                                        updatesXml.DocumentElement.Attributes["NUMA"].Value = i.ToString();
                                        updatesXml.DocumentElement.RemoveChild(node);
                                        error = false;
                                        if (DateTime.Parse(node.Attributes["UDATE"].Value) < DateTime.Now.Date)
                                        {
                                            OnOff = true;
                                        }
                                        break;
                                }
                                break;
                            case "SALONS":
                                switch (node.Attributes["ACTION"].Value)
                                {
                                    case "INSERT":
                                        cmd.CommandText = "SELECT s.IDN, st.STYPE, s.SNAME, s.ADRSS, s.TEL1, s.TEL2 FROM LONDA.SALONS s, LONDA.SALONTYPE st WHERE s.STYPE = st.IDN AND s.STATE = 1 AND s.IDN = " + node.Attributes["UIDN"].Value.ToString();
                                        updData = Functions.GetData(cmd, true);
                                        if (updData != null)
                                        {
                                            lineXml = updatesXml.CreateElement("updateitself");
                                            lineXml.SetAttribute("VERSION", node.Attributes["IDN"].Value);
                                            lineXml.SetAttribute("ID", updData.Rows[0].ItemArray[0].ToString());
                                            lineXml.SetAttribute("STYPE", updData.Rows[0].ItemArray[1].ToString());
                                            lineXml.SetAttribute("SUB", updData.Rows[0].ItemArray[2].ToString());
                                            lineXml.SetAttribute("ADR", updData.Rows[0].ItemArray[3].ToString());
                                            lineXml.SetAttribute("TEL1", updData.Rows[0].ItemArray[4].ToString());
                                            lineXml.SetAttribute("TEL2", updData.Rows[0].ItemArray[5].ToString());
                                            lineXml.SetAttribute("DATE", node.Attributes["UDATE"].Value);
                                            lineXml.SetAttribute("ACTION", "INSERT");
                                            lineXml.SetAttribute("TYPE", "SALONS");
                                            updatesXml.DocumentElement.AppendChild(lineXml);
                                            updatesXml.DocumentElement.Attributes["NUMA"].Value = i.ToString();
                                            updatesXml.DocumentElement.RemoveChild(node);
                                            error = false;
                                            updData.Dispose();
                                            if (DateTime.Parse(node.Attributes["UDATE"].Value) < DateTime.Now.Date)
                                            {
                                                OnOff = true;
                                            }
                                        }
                                        else
                                        {
                                            XmlNodeList nodelst = updatesXml.SelectNodes("/updates/todownload[@UTABLE='SALONS' and @ACTION='DELETE' and @UIDN='" + node.Attributes["UIDN"].Value.ToString() + "']");
                                            if (nodelst != null)
                                            {
                                                foreach (XmlNode lineNode in nodelst)
                                                {
                                                    if (Int32.Parse(lineNode.Attributes["IDN"].Value.ToString()) > i)
                                                    {
                                                        updatesXml.DocumentElement.Attributes["NUMA"].Value = i.ToString();
                                                        updatesXml.DocumentElement.RemoveChild(node);
                                                        error = false;
                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                        break;
                                    case "DELETE":
                                        lineXml = updatesXml.CreateElement("updateitself");
                                        lineXml.SetAttribute("VERSION", node.Attributes["IDN"].Value);
                                        lineXml.SetAttribute("ID", node.Attributes["UIDN"].Value);
                                        lineXml.SetAttribute("DATE", node.Attributes["UDATE"].Value);
                                        lineXml.SetAttribute("ACTION", "DELETE");
                                        lineXml.SetAttribute("TYPE", "SALONS");
                                        updatesXml.DocumentElement.AppendChild(lineXml);
                                        updatesXml.DocumentElement.Attributes["NUMA"].Value = i.ToString();
                                        updatesXml.DocumentElement.RemoveChild(node);
                                        error = false;
                                        if (DateTime.Parse(node.Attributes["UDATE"].Value) < DateTime.Now.Date)
                                        {
                                            OnOff = true;
                                        }
                                        break;
                                }
                                break;
                            case "PROFESSIONS":
                                switch (node.Attributes["ACTION"].Value)
                                {
                                    case "INSERT":
                                        cmd.CommandText = "SELECT IDN, PROF FROM LONDA.PROFESSIONS WHERE IDN = " + node.Attributes["UIDN"].Value.ToString();
                                        updData = Functions.GetData(cmd, true);

                                        if (updData != null)
                                        {
                                            lineXml = updatesXml.CreateElement("updateitself");
                                            lineXml.SetAttribute("VERSION", node.Attributes["IDN"].Value);
                                            lineXml.SetAttribute("ID", updData.Rows[0].ItemArray[0].ToString());
                                            lineXml.SetAttribute("PROF", updData.Rows[0].ItemArray[1].ToString());
                                            lineXml.SetAttribute("DATE", node.Attributes["UDATE"].Value);
                                            lineXml.SetAttribute("ACTION", "INSERT");
                                            lineXml.SetAttribute("TYPE", "PROFESSIONS");
                                            updatesXml.DocumentElement.AppendChild(lineXml);
                                            updatesXml.DocumentElement.Attributes["NUMA"].Value = i.ToString();
                                            updatesXml.DocumentElement.RemoveChild(node);
                                            error = false;
                                            updData.Dispose();
                                            if (DateTime.Parse(node.Attributes["UDATE"].Value) < DateTime.Now.Date)
                                            {
                                                OnOff = true;
                                            }
                                        }
                                        else
                                        {
                                            XmlNodeList nodelst = updatesXml.SelectNodes("/updates/todownload[@UTABLE='PROFESSIONS' and @ACTION='DELETE' and @UIDN='" + node.Attributes["UIDN"].Value.ToString() + "']");
                                            if (nodelst != null)
                                            {
                                                foreach (XmlNode lineNode in nodelst)
                                                {
                                                    if (Int32.Parse(lineNode.Attributes["IDN"].Value.ToString()) > i)
                                                    {
                                                        updatesXml.DocumentElement.Attributes["NUMA"].Value = i.ToString();
                                                        updatesXml.DocumentElement.RemoveChild(node);
                                                        error = false;
                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                        break;
                                    case "DELETE":
                                        lineXml = updatesXml.CreateElement("updateitself");
                                        lineXml.SetAttribute("VERSION", node.Attributes["IDN"].Value);
                                        lineXml.SetAttribute("ID", node.Attributes["UIDN"].Value);
                                        lineXml.SetAttribute("DATE", node.Attributes["UDATE"].Value);
                                        lineXml.SetAttribute("ACTION", "DELETE");
                                        lineXml.SetAttribute("TYPE", "PROFESSIONS");
                                        updatesXml.DocumentElement.AppendChild(lineXml);
                                        updatesXml.DocumentElement.Attributes["NUMA"].Value = i.ToString();
                                        updatesXml.DocumentElement.RemoveChild(node);
                                        error = false;
                                        if (DateTime.Parse(node.Attributes["UDATE"].Value) < DateTime.Now.Date)
                                        {
                                            OnOff = true;
                                        }
                                        break;
                                }
                                break;
                        }
                        if (error)
                        {
                            break;
                        }
                        updatesXml.Save(updatesPath);
                        
                    }
                    Cnt = Cnt + 1;
                    if (Cnt == quantity * Cnt2)
                    {
                        
                        Cnt2 = Cnt2 + 1;
                    }

                }
            }
            return OnOff;
            
        }


        // Запись обновлений в файлы данных программы
        private void SetUpdates(XmlDocument updatesXml, string updatesPath, string app_path)
        {
            XmlDocument staffXml = new XmlDocument();
            //log.WriteToLogFile("LOAD: " + app_path + "\\staff.xml");
            staffXml.Load(app_path + "\\staff.xml");
            XmlDocument salonXml = new XmlDocument();
            //log.WriteToLogFile("LOAD: " + app_path + "\\salon.xml");
            salonXml.Load(app_path + "\\salon.xml");
            XmlDocument priceXml = new XmlDocument();
            //log.WriteToLogFile("LOAD: " + app_path + "\\price.xml");
            priceXml.Load(app_path + "\\price.xml");

            bool jump = false;
            for (int i = (Int32.Parse(staffXml.DocumentElement["updates"].Attributes["ID"].Value) + 1); i <= Int32.Parse(updatesXml.DocumentElement.Attributes["NUMA"].Value); i++)
            {
                XmlNode node = updatesXml.SelectSingleNode("/updates/updateitself[@VERSION='" + i + "']");


                if (node != null)
                {
                    // Проверка даты обновлений
                    //log.WriteToLogFile("Проверка даты обновлений");
                    if (DateTime.Parse(node.Attributes["DATE"].Value) < DateTime.Now.Date)
                    {
                        XmlNode nodeToChange;
                        switch (node.Attributes["TYPE"].Value)
                        {
                            case "STAFF":
                                //log.WriteToLogFile("STAFF");
                                nodeToChange = staffXml.SelectSingleNode("/staff/worker[@IDN='" + node.Attributes["IDN"].Value + "']");
                                switch (node.Attributes["ACTION"].Value)
                                {
                                    case "INSERT":
                                        //log.WriteToLogFile("INSERT");
                                        if (nodeToChange != null)
                                        {
                                            nodeToChange.Attributes["ID"].Value = node.Attributes["ID"].Value;
                                            nodeToChange.Attributes["NAME"].Value = node.Attributes["NAME"].Value;
                                            nodeToChange.Attributes["LEVEL"].Value = node.Attributes["LEVEL"].Value;
                                            nodeToChange.Attributes["PROF"].Value = node.Attributes["PROF"].Value;
                                        }
                                        else
                                        {
                                            XmlElement element = staffXml.CreateElement("worker");
                                            element.SetAttribute("ID", node.Attributes["ID"].Value);
                                            element.SetAttribute("NAME", node.Attributes["NAME"].Value);
                                            element.SetAttribute("LEVEL", node.Attributes["LEVEL"].Value);
                                            element.SetAttribute("PROF", node.Attributes["PROF"].Value);
                                            element.SetAttribute("IDN", node.Attributes["IDN"].Value);
                                            staffXml.DocumentElement.AppendChild(element);
                                        }

                                        break;
                                    case "DELETE":
                                        //log.WriteToLogFile("DELETE");
                                        if (nodeToChange != null)
                                        {
                                            staffXml.DocumentElement.RemoveChild(nodeToChange);
                                        }
                                        break;
                                }
                                updatesXml.DocumentElement.RemoveChild(node);
                                break;
                            case "STAGES":
                                //log.WriteToLogFile("STAGES");
                                nodeToChange = staffXml.SelectSingleNode("/staff/rank[@ID='" + node.Attributes["ID"].Value + "']");
                                switch (node.Attributes["ACTION"].Value)
                                {
                                    case "INSERT":
                                        //log.WriteToLogFile("INSERT");
                                        if (nodeToChange != null)
                                        {
                                            nodeToChange.Attributes["ID"].Value = node.Attributes["ID"].Value;
                                            nodeToChange.Attributes["NAME"].Value = node.Attributes["NAME"].Value;
                                        }
                                        else
                                        {
                                            XmlElement element = staffXml.CreateElement("rank");
                                            element.SetAttribute("ID", node.Attributes["ID"].Value);
                                            element.SetAttribute("NAME", node.Attributes["NAME"].Value);
                                            staffXml.DocumentElement.AppendChild(element);
                                        }

                                        break;
                                    case "DELETE":
                                        //log.WriteToLogFile("DELETE");
                                        if (nodeToChange != null)
                                        {
                                            staffXml.DocumentElement.RemoveChild(nodeToChange);
                                        }
                                        break;
                                }
                                updatesXml.DocumentElement.RemoveChild(node);
                                break;
                            case "CASH_REG_CODE":
                                //log.WriteToLogFile("CASH_REG_CODE");
                                nodeToChange = priceXml.SelectSingleNode("/price/item[@IDN='" + node.Attributes["IDN"].Value + "']");
                                switch (node.Attributes["ACTION"].Value)
                                {
                                    case "INSERT":
                                        //log.WriteToLogFile("INSERT");
                                        if (nodeToChange != null)
                                        {
                                            priceXml.DocumentElement.RemoveChild(nodeToChange);
                                        }

                                        XmlElement element = priceXml.CreateElement("item");
                                        element.SetAttribute("ID", node.Attributes["ID"].Value);
                                        element.SetAttribute("NAME", node.Attributes["NAME"].Value);
                                        element.SetAttribute("PRICE0", node.Attributes["PRICE0"].Value);
                                        try
                                        {
                                            element.SetAttribute("PRICE10", node.Attributes["PRICE10"].Value);
                                        }
                                        catch
                                        { }
                                        try
                                        {
                                            element.SetAttribute("PRICE50", node.Attributes["PRICE50"].Value);
                                        }
                                        catch
                                        { }
                                        try
                                        {
                                            element.SetAttribute("PRICESTAFF", node.Attributes["PRICESTAFF"].Value);
                                        }
                                        catch
                                        { }
                                        element.SetAttribute("IDN", node.Attributes["IDN"].Value);
                                        priceXml.DocumentElement.AppendChild(element);

                                        break;
                                    case "DELETE":
                                        //log.WriteToLogFile("DELETE");
                                        if (nodeToChange != null)
                                        {
                                            priceXml.DocumentElement.RemoveChild(nodeToChange);
                                        }
                                        break;
                                }
                                updatesXml.DocumentElement.RemoveChild(node);
                                break;
                            case "SALONS":
                                //log.WriteToLogFile("SALONS");
                                nodeToChange = salonXml.SelectSingleNode("/salon/item[@ID='" + node.Attributes["ID"].Value + "']");
                                switch (node.Attributes["ACTION"].Value)
                                {
                                    case "INSERT":
                                        //log.WriteToLogFile("INSERT");
                                        if (nodeToChange != null)
                                        {
                                            nodeToChange.Attributes["STYPE"].Value = node.Attributes["STYPE"].Value;
                                            nodeToChange.Attributes["SUB"].Value = node.Attributes["SUB"].Value;
                                            nodeToChange.Attributes["ADR"].Value = node.Attributes["ADR"].Value;
                                            nodeToChange.Attributes["TEL1"].Value = node.Attributes["TEL1"].Value;
                                            nodeToChange.Attributes["TEL2"].Value = node.Attributes["TEL2"].Value;
                                        }
                                        else
                                        {
                                            XmlElement element = salonXml.CreateElement("item");
                                            element.SetAttribute("ID", node.Attributes["ID"].Value);
                                            element.SetAttribute("STYPE", node.Attributes["STYPE"].Value);
                                            element.SetAttribute("SUB", node.Attributes["SUB"].Value);
                                            element.SetAttribute("ADR", node.Attributes["ADR"].Value);
                                            element.SetAttribute("TEL1", node.Attributes["TEL1"].Value);
                                            element.SetAttribute("TEL2", node.Attributes["TEL2"].Value);
                                            salonXml.DocumentElement.AppendChild(element);
                                        }

                                        break;
                                    case "DELETE":
                                        //log.WriteToLogFile("DELETE");
                                        if (nodeToChange != null)
                                        {
                                            salonXml.DocumentElement.RemoveChild(nodeToChange);
                                        }
                                        break;
                                }
                                updatesXml.DocumentElement.RemoveChild(node);
                                break;
                            case "PROFESSIONS":
                                //log.WriteToLogFile("PROFESSIONS");
                                nodeToChange = staffXml.SelectSingleNode("/staff/profession[@ID='" + node.Attributes["ID"].Value + "']");
                                switch (node.Attributes["ACTION"].Value)
                                {
                                    case "INSERT":
                                        //log.WriteToLogFile("INSERT");
                                        if (nodeToChange != null)
                                        {
                                            nodeToChange.Attributes["ID"].Value = node.Attributes["ID"].Value;
                                            nodeToChange.Attributes["NAME"].Value = node.Attributes["PROF"].Value;
                                        }
                                        else
                                        {
                                            XmlElement element = staffXml.CreateElement("profession");
                                            element.SetAttribute("ID", node.Attributes["ID"].Value);
                                            element.SetAttribute("NAME", node.Attributes["PROF"].Value);
                                            staffXml.DocumentElement.AppendChild(element);
                                        }

                                        break;
                                    case "DELETE":
                                        //log.WriteToLogFile("DELETE");
                                        if (nodeToChange != null)
                                        {
                                            staffXml.DocumentElement.RemoveChild(nodeToChange);
                                        }
                                        break;
                                }
                                updatesXml.DocumentElement.RemoveChild(node);
                                break;
                        }
                        if (!jump)
                        {
                            staffXml.DocumentElement["updates"].Attributes["ID"].Value = node.Attributes["VERSION"].Value;
                            //this.Text = this.Text.Substring(0, this.Text.IndexOf(":") + 1) + " " + node.Attributes["VERSION"].Value;
                        }
                    }
                    else
                    {
                        jump = true;
                    }
                }


            }
            staffXml.Save(app_path + "\\staff.xml");
            //log.WriteToLogFile("Save: " + app_path + "\\staff.xml");
            salonXml.Save(app_path + "\\salon.xml");
            //log.WriteToLogFile("Save: " + app_path + "\\salon.xml");
            priceXml.Save(app_path + "\\price.xml");
            //log.WriteToLogFile("Save: " + app_path + "\\price.xml");
            
            
            
            updatesXml.Save(updatesPath);
        }

        protected override void OnContinue()
        {
            log.WriteToLogFile("Continue");
            timerSend.Start();
            timerRecieve.Start();
            log.WriteToLogFile("Timers Started");
        }

        protected override void OnPause()
        {
            log.WriteToLogFile("Pause");
            timerRecieve.Stop();
            timerSend.Stop();
            log.WriteToLogFile("Timers Stoped");
        }
    }
}
