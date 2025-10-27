using System;
using System.Windows;

namespace Petrov_122
{
    public partial class MainWindow : Window
    {
        private bool isBuddhistTheme = false;

        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new Pages.AuthPage());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.IsEnabled = true;
            timer.Tick += (o, t) =>
            {
                DateTimeNow.Text = DateTime.Now.ToString();
            };
            timer.Start();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите закрыть окно?", "Message",
                MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                e.Cancel = false;
            }
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isBuddhistTheme)
            {
                var uri = new Uri("Resources/BuddhistDictionary.xaml", UriKind.Relative);
                ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
                Application.Current.Resources.Clear();
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);
                ThemeButton.Content = "Базовый";
                isBuddhistTheme = true;
            }
            else
            {
                var uri = new Uri("Resources/Dictionary.xaml", UriKind.Relative);
                ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
                Application.Current.Resources.Clear();
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);
                ThemeButton.Content = "Буддизм";
                isBuddhistTheme = false;
            }
        }
    }
}