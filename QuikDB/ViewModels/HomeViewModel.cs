using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuikDB.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        private string _appVersion;
        public string AppVersion
        {
            get { return _appVersion; }
            set { _appVersion = value; RaisePropertyChanged(nameof(AppVersion)); }
        }

        public HomeViewModel()
        {
            AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
