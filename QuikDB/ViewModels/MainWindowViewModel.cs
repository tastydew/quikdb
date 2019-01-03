using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using QuikDB.Util;
using QuikDB.Views;

namespace QuikDB.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _drawerEnabled;
        public bool IsMainDrawerEnabled
        {
            get { return _drawerEnabled; }
            set
            {
                _drawerEnabled = value;
                RaisePropertyChanged(nameof(IsMainDrawerEnabled));
            }
        }

        RelayCommand _changeViewModel;
        public ICommand ChangeCurrentViewModel
        {
            get
            {
                if (_changeViewModel == null)
                {
                    _changeViewModel = new RelayCommand((param) => { SwitchCurrentView(param as string); });
                }
                return _changeViewModel;
            }
        }

        private void SwitchCurrentView(string param)
        {
            switch (param)
            {
                case "HomeView":
                    CurrentViewModel = new HomeViewModel();
                    break;
                case "RecoveryOpsView":
                    CurrentViewModel = new RecoveryOperationsViewModel();
                    break;
                case "SpecialProceduresView":
                    CurrentViewModel = new SpecialProceduresViewModel();
                    break;
                case "ToolsView":
                    CurrentViewModel = new ToolsViewModel();
                    break;
                case "SystemInfoView":
                    CurrentViewModel = new SystemInformationViewModel();
                    break;
                case "DatabaseRestoreView":
                    CurrentViewModel = new DatabaseRestoreViewModel();
                    break;
                default:
                    CurrentViewModel = new HomeViewModel();
                    break;
            }
        }

        private ViewModelBase _currentVM;
        public ViewModelBase CurrentViewModel
        {
            get { return _currentVM; }
            set
            {
                _currentVM = value;
                RaisePropertyChanged(nameof(CurrentViewModel));
            }
        }

        public MainWindowViewModel()
        {
            _drawerEnabled = true;
            CurrentViewModel = new HomeViewModel();
        }


    }
}
