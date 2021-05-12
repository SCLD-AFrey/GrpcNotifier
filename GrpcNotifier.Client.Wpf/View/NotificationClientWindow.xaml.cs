using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GrpcNotifier.Client.Wpf.ViewModel;

namespace GrpcNotifier.Client.Wpf.View
{
    public partial class NotificationClientWindow : UserControl
    {
        public NotificationClientWindow()
        {
            InitializeComponent();
            DataContext = new NotificationClientWindowViewModel();
        }

        private void BodyInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                (DataContext as NotificationClientWindowViewModel).WriteCommand.Execute(BodyInput.Text);
                BodyInput.Text = "";
            }
        }

        private void BodyInput_Loaded(object sender, RoutedEventArgs e)
        {
            BodyInput.Focus();
        }
    }
}