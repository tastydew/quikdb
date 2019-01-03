using log4net;
using log4net.Config;
using MaterialDesignThemes.Wpf;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;


namespace QuikDB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SplashScreenView : Window, INotifyPropertyChanged
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string AppGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAtt‌​ributes(typeof(GuidA‌​ttribute), true)[0]).Value;
        private static readonly Mutex Mutex = new Mutex(false, @"Global\" + AppGuid);

        #region Bindings
        private string _appVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public string AppVersion { get { return _appVersion; } }
        private string splashScreenStatus;
        public string SplashScreenStatus { get { return splashScreenStatus; } set { splashScreenStatus = value; OnPropertyChanged(nameof(splashScreenStatus)); } }

        #endregion

        #region Boilerplate
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        BackgroundWorker splashWorker = new BackgroundWorker();
        #endregion

        public SplashScreenView()
        {
#if RELEASE
            if (!Mutex.WaitOne(0, false))
            {
                //TODO Use a custom messagebox here.
                System.Windows.MessageBox.Show("Error");
                System.Windows.Application.Current.Shutdown();
            }
#endif
            DataContext = this;
            XmlConfigurator.Configure();
            splashWorker.DoWork += splashWorker_DoWork;
            splashWorker.RunWorkerCompleted += splashWorker_WorkComplete;
            splashWorker.RunWorkerAsync();
        }

        private void splashWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            SplashScreenStatus = Properties.Resources.SPLASH_INITIALIZE_DIRECTORIES;
            Util.QuikDBFileHelper.createInitialDirectories();
            SplashScreenStatus = Properties.Resources.SPLASH_CHECK_PREREQUISITES;
        }

        private void splashWorker_WorkComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Views.LoginView login = new Views.LoginView();
                login.Show();
                this.Close();
            }
        }

    }
}
