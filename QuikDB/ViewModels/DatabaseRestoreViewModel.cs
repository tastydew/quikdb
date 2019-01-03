using System.Windows.Forms; 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using QuikDB.Services;
using System.ComponentModel;
using System.Data;

namespace QuikDB.ViewModels
{
    public class DatabaseRestoreViewModel : ViewModelBase
    {

        #region Fields / Properties

        BackgroundWorker restoreWorker = new BackgroundWorker();

        private bool _isRestoreEnabled = false;

        public bool IsRestoreEnabled
        {
            get { return _isRestoreEnabled; }
            set
            {
                _isRestoreEnabled = value;
                RaisePropertyChanged(nameof(IsRestoreEnabled));
            }
        }

        private bool _preRestoreWorking = false;

        public bool IsPreRestoreWorking
        {
            get { return _preRestoreWorking; }
            set
            {
                _preRestoreWorking = value;
                RaisePropertyChanged(nameof(IsPreRestoreWorking));
            }
        }

        private bool _isRestoreInProgress = false;

        public bool IsRestoreInProgress
        {
            get { return _isRestoreInProgress; }
            set
            {
                _isRestoreInProgress = value;
                RaisePropertyChanged(nameof(IsRestoreInProgress));
            }
        }

        private bool _isAdditionalDataRestoring = false;

        public bool IsAdditionalDataRestoring
        {
            get { return _isAdditionalDataRestoring; }
            set
            {
                _isAdditionalDataRestoring = value;
                RaisePropertyChanged(nameof(IsAdditionalDataRestoring));
            }
        }

        private bool _isPostRestoreInProgress = false;

        public bool IsPostRestoreInProgress
        {
            get => _isPostRestoreInProgress; set
            {
                _isPostRestoreInProgress = value;
                RaisePropertyChanged(nameof(IsPostRestoreInProgress));
            }
        }

        private string _dbFileLocation;

        public string DBFileLocation
        {
            get { return _dbFileLocation; }
            set
            {
                _dbFileLocation = value;
                RaisePropertyChanged(nameof(DBFileLocation));
            }
        }

        private int _dbTransactionLogs = 0;

        public int DBTransactionLogs
        {
            get { return _dbTransactionLogs; }
            set
            {
                _dbTransactionLogs = value;
                RaisePropertyChanged(nameof(DBTransactionLogs));
            }
        }

        private RelayCommand _selectBackup;

        public ICommand SelectBackupLocation
        {
            get
            {
                if (_selectBackup == null)
                {
                    _selectBackup = new RelayCommand((param) =>
                    {
                        DBFileLocation = GetBackupFileLocation();
                        IsRestoreEnabled = DBFileLocation != null;
                    });
                }
                return _selectBackup;
            }
        }

        private RelayCommand _startRestore;

        public ICommand StartRestore
        {
            get
            {
                if (_startRestore == null)
                {
                    _startRestore = new RelayCommand((param) =>
                    {
                        restoreWorker.RunWorkerAsync();
                    });
                }

                return _startRestore;
            }
        }

        private RelayCommand _resetRestoreForm;

        public ICommand ResetRestoreForm
        {
            get
            {
                _resetRestoreForm = new RelayCommand((param) =>
                {
                    ResetFormElements();
                });
                return _resetRestoreForm;
            }
        }

        private string _currentDatabase;

        public string CurrentDatabase
        {
            get { return _currentDatabase; }
            set
            {
                _currentDatabase = value;
                RaisePropertyChanged(nameof(CurrentDatabase));
            }
        }

        private int _preRestoreProgress = 0;

        public int PreRestoreProgress
        {
            get { return _preRestoreProgress; }
            set
            {
                _preRestoreProgress = value;
                RaisePropertyChanged(nameof(PreRestoreProgress));
            }
        }

        private int _dbRestoreProgress = 0;

        public int DBRestoreProgress
        {
            get { return _dbRestoreProgress; }
            set
            {
                _dbRestoreProgress = value;
                RaisePropertyChanged(nameof(DBRestoreProgress));
            }
        }

        private int _additionalDataRestoreProgress = 0;

        public int AdditionalDataRestoreProgress
        {
            get { return _additionalDataRestoreProgress; }
            set
            {
                _additionalDataRestoreProgress = value;
                RaisePropertyChanged(nameof(AdditionalDataRestoreProgress));
            }
        }

        private int _postRestoreProgress = 0;

        public int PostRestoreProgress
        {
            get { return _postRestoreProgress; }
            set
            {
                _postRestoreProgress = value;
                RaisePropertyChanged(nameof(PostRestoreProgress));
            }
        }

        private string _currentStatus = "Ready...";

        public string CurrentStatus
        {
            get { return _currentStatus; }
            set
            {
                _currentStatus = value;
                RaisePropertyChanged(nameof(CurrentStatus));
            }
        }

        #endregion

        #region Ctor
        public DatabaseRestoreViewModel()
        {
            restoreWorker.DoWork += RestoreWorker_DoWork;
            restoreWorker.RunWorkerCompleted += RestoreWorker_WorkerCompleted;
        }

        #endregion

        #region BackgroundWorker
        private void RestoreWorker_WorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                //TODO implement error on worker completed
            }
            else
            {
                //TODO implement better message box here.
                CurrentStatus = "Restore Complete.";
                MessageBox.Show("Restore Completed");
                ResetFormElements();
            }
        }

        private void RestoreWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            IsRestoreEnabled = false;
            //Pre-Restore Process
            CurrentStatus = "Performing Pre-Restoration Checks";
            if (DBFileLocation != null)
                DBTransactionLogs = GetTransactionLogCount(DBFileLocation);

            if (DBFileLocation != null)
            {
                CurrentStatus = "Verifying Database Backup File.";
                CurrentDatabase = "Main DB";
                VerifyRestoreFile(DBFileLocation);
                //Restore Process
                CurrentStatus = "Restoring Database";
                RestoreDatabase(DBFileLocation, CurrentDatabase);
                //Additional Data Restore
                CurrentStatus = "Restoring Additional Data";
                if (DBTransactionLogs > 0) { 
                RestoreTransactionLogs(DBFileLocation, CurrentDatabase);
                }
                else
                //Post Restore
                CurrentStatus = "Performing Post-Restore Procedures";
                PerformPostRestore(CurrentDatabase);
            }
        }

        #endregion

        #region Methods

        private string GetBackupFileLocation()
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                InitialDirectory = @"C:\",
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "SQL Backup Files (*.bak)|*.bak",
                Multiselect = false,
                Title = "Select Database Backup File Location"
            };


            if (ofd.ShowDialog() == DialogResult.OK)
            {
                return ofd.FileName;
            }
            else
            {
                return null;
            }

        }

        private void RestoreDatabase(string filePath, string database)
        {
            IsRestoreInProgress = true;
            DatabaseOperations.Connection conn = new DatabaseOperations.Connection();
            var serverConnection = conn.OpenDatabaseServerConnection();
            DatabaseOperations.Recovery recoveryOps = new DatabaseOperations.Recovery();
            recoveryOps.ReportRestoreProgressEvent += (prg) =>
            {
                DBRestoreProgress = prg;
            };

            recoveryOps.RestoreDatabase(serverConnection, filePath, database);
        }

        private void RestoreTransactionLogs(string filePath, string database)
        {
            IsAdditionalDataRestoring = true;
            DatabaseOperations.Connection conn = new DatabaseOperations.Connection();
            var serverConnection = conn.OpenDatabaseServerConnection();
            DatabaseOperations.Recovery recoveryOps = new DatabaseOperations.Recovery();
            recoveryOps.ReportRestoreProgressEvent += (prg) =>
            {
                AdditionalDataRestoreProgress += prg;
            };

            try { 
            recoveryOps.RestoreTransactionLogs(serverConnection, new DirectoryInfo(filePath), database);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                if (e.InnerException != null)
                {
                    MessageBox.Show(e.InnerException.Message);
                }
                throw e;
            }
            finally
            {
                IsAdditionalDataRestoring = false;
                AdditionalDataRestoreProgress = 100;
            }
        }

        private void VerifyRestoreFile(string filePath)
        {
            IsPreRestoreWorking = true;
            DatabaseOperations.Connection conn = new DatabaseOperations.Connection();
            var serverConnection = conn.OpenDatabaseServerConnection();
            DatabaseOperations.Recovery verifyOps = new DatabaseOperations.Recovery();

            if (verifyOps.VerifyBackupFile(serverConnection, filePath))
            {
                PreRestoreProgress += 50;
            }

        }

        private void PerformPostRestore(string database)
        {
            //Insert any post restore processes here.
            IsPostRestoreInProgress = true;
            CloseDatabaseRestoreProcess(database);


            PostRestoreProgress = 100;
        }


        private void CloseDatabaseRestoreProcess(string database)
        {
            DatabaseOperations.Recovery recoveryOps = new DatabaseOperations.Recovery();
            var result = recoveryOps.CloseRestoreProcess(database);
        }

        private int GetTransactionLogCount(string filePath)
        {
            PreRestoreProgress += 50;
            FileInfo f = new FileInfo(filePath);
            return new DirectoryInfo(f.DirectoryName).GetFiles("*.trn").Length;
        }

        private void ResetFormElements()
        {
            DBFileLocation = null;
            IsPreRestoreWorking = false;
            IsRestoreEnabled = false;
            IsRestoreInProgress = false;
            IsAdditionalDataRestoring = false;
            IsPostRestoreInProgress = false;
            CurrentDatabase = "";
            PreRestoreProgress = 0;
            DBRestoreProgress = 0;
            AdditionalDataRestoreProgress = 0;
            PostRestoreProgress = 0;
            DBTransactionLogs = 0;
            CurrentStatus = "Ready...";

        }

        #endregion

    }
}
