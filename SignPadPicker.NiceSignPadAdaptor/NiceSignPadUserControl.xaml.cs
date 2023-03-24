using System.Windows;
using System.Windows.Controls;

namespace SignPadPicker.Adaptor
{
    /// <summary>
    /// NiceSignPadUserControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NiceSignPadUserControl : UserControl
    {
        public NiceSignPadUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is NiceSignPadViewModel vm)
            {
                vm.Activate();
            }
        }
    }
}
