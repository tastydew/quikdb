using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

using QuikDB.ViewModels;

namespace QuikDB.Views
{
    /// <summary>
    /// Interaction logic for MainMenuView.xaml
    /// </summary>
    public partial class MainMenuView : Window
    {
        public MainMenuView()
        {
            InitializeComponent();
        }

        private void MenuItemsListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MenuToggleButton.IsChecked = false;
        }

        private void btnLogin_Exit_Click(object sender, RoutedEventArgs e)
        {
            //TODO Use a custom messagebox here to confirm if the user actually wants to log off.
            Application.Current.Shutdown();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }
    }

}
