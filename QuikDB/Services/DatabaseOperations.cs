using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using log4net;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace QuikDB.Services
{
    public class DatabaseOperations
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public class Connection
        {
            //TODO implement proper credential gathering here.
            private string _login = "lolwut";
            private string _password = "thisisnotacorrectpassword";
            private string _instance = "SQLEXPRESS";

            public Server OpenDatabaseServerConnection()
            {
                ServerConnection conn = new ServerConnection()
                {
                    ServerInstance = @".\" + _instance,
                    LoginSecure = false,
                    Login = _login,
                    Password = _password
                };

                try
                {
                    Server srv = new Server(conn);
                    Log.Info("Establishing Connection to Database");
                    Log.Info("Database Engine Version: " + srv.Information.Version);
                    return srv;
                }
                catch (Exception e)
                {
                    //TODO replace this with a custom message box
                    MessageBox.Show("Cannot connect to DB Instance");
                    Log.Fatal("Cannot connect to the database engine: " + e.Message);
                    if (e.StackTrace != null)
                    {
                        Log.Debug("Stack Trace: " + e.StackTrace);
                    }
                    throw e;
                }

            }

            public Database OpenDatabaseConnection(string database)
            {
                Server srv = OpenDatabaseServerConnection();

                try
                {
                    return new Database(srv, database);
                }
                catch (SmoException ex)
                {
                    MessageBox.Show("Could not establish a connection to database, " + database);
                    Log.Fatal("Cannot connect to the database engine: " + ex.Message);
                    if (ex.StackTrace != null)
                    {
                        Log.Debug("Stack Trace: " + ex.StackTrace);
                    }
                    throw ex;
                }
            }
        }

        public class Recovery
        {
            public int TransactionLogs { get; set; }
            public delegate void ReportRestoreProgress(int percentage);
            public event ReportRestoreProgress ReportRestoreProgressEvent;

            /// <summary>
            /// Restores a database backup file (.bak) to the provided <code>Server</code> object
            /// </summary>
            /// <param name="srv"></param>
            /// <param name="filePath"></param>
            /// <param name="db"></param>
            /// <param name="noRecovery"></param>
            public void RestoreDatabase(Server srv, string filePath, string database, bool noRecovery = true)
            {
                Restore res = new Restore()
                {
                    Database = database,
                    NoRecovery = noRecovery,
                    ReplaceDatabase = true,
                };

                res.PercentCompleteNotification = 1;
                res.Devices.AddDevice(filePath, DeviceType.File);

                res.PercentComplete += (o, e) =>
                {
                    ReportRestoreProgressEvent(e.Percent);
                };

                res.Information += (ob, ev) =>
                {
                    Log.Info(ev.Error.Message);
                };

                res.Complete += (obj, evt) =>
                {

                    Log.Info(evt.Error.Message);
                };
                try
                {
                    res.SqlRestore(srv);
                }
                catch (SmoException ex)
                {
                    Log.Fatal("An SMO Exception has occurred when restoring the database: " + database + ": " + ex.Message);
                    throw ex;
                }
                catch (Exception ex)
                {
                    Log.Fatal("An exception has occurred when restoring the database:  " + database + ": " + ex.Message);
                }
            }

            /// <summary>
            /// Runs a VERIFYONLY on the provided SQL database backup file.
            /// </summary>
            /// <param name="srv"><code>Server</code> connection isntance used to connect to SQL Server</param>
            /// <param name="filePath">string containing the file path of the database backup file (.bak)</param>
            /// <returns></returns>
            public bool VerifyBackupFile(Server srv, string filePath)
            {
                Restore res = new Restore();
                res.Devices.AddDevice(filePath, DeviceType.File);
                return res.SqlVerify(srv);
            }

            /// <summary>
            /// Restore Transaction Logs for a database backup file. The database must be restore with NORECOVERY in order for transaction
            /// logs to restore properly.
            /// </summary>
            /// <param name="filePath">The DirectoryInfo object and file path containing the transaction logs</param>
            /// <param name="dbType">The <code>DatabaseType</code> enum value of the database to be restored</param>
            public void RestoreTransactionLogs(Server srv, DirectoryInfo filePath, string database)
            {
                Restore res = new Restore()
                {
                    Database = database,
                    Action = RestoreActionType.Log,
                    ReplaceDatabase = false,
                    NoRecovery = true

                };


                FileInfo[] files = filePath.Parent.GetFiles("*.trn");
                TransactionLogs = files.Length;
                var totalProgressInterval = 100 / files.Length;

                foreach (FileInfo f in files)
                {
                    res.Devices.AddDevice(f.FullName, DeviceType.File);

                    try
                    {
                        res.SqlRestore(srv);
                        ReportRestoreProgressEvent(totalProgressInterval);
                    }
                    catch (SmoException ex)
                    {
                        Log.Fatal("An SMO Exception has occurred when restoring the database: " + database + ": " + ex.Message);
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        Log.Fatal("An exception has occurred when restoring the database:  " + database + ": " + ex.Message);
                        throw ex;
                    }

                    res.Devices.Clear();

                }


            }

            public DataSet CloseRestoreProcess(string database)
            {
                Connection conn = new Connection();
                Database db = conn.OpenDatabaseConnection("master");
                return db.ExecuteWithResults("RESTORE DATABASE " + database + " WITH RECOVERY");
            }

            public void DetachDatabase(Server srv, string database)
            {
                try
                {
                    srv.DetachDatabase(database, false);
                }
                catch (SmoException smoex)
                {
                    Log.Fatal("An SMO Exception has occurred when attempting to detach database " + database);
                    Log.Fatal(smoex.Message, smoex);
                    throw smoex;
                }
            }

        }

        public class Backup
        {

        }
    }
}
