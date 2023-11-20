using SignPadPicker.Exceptions;
using System.Windows;
using Screen = System.Windows.Forms.Screen;

namespace SignPadPicker.Adaptor
{
    public class ScreenSignPad : ISignPadPlugin
    {
        public string Name => "ScreenSignPad";

        public string Description => "ScreenSignPad Plugin";

        public bool IsPhysicalDevice => false;

        public bool IsAvailable => true;

        public string Activate(Window owner = null)
        {
            return Activate(config: null, owner);
        }

        public string Activate(SignPadConfig config, Window owner = null)
        {
            Window win = CreateWindow(owner);

            _ = win.ShowDialog();

            if (win.Content is ScreenSignPadUserControl uc &&
                uc.DataContext is ScreenSignPadViewModel vm)
            {
                if (vm.Result?.Exception != null)
                {
                    throw vm.Result.Exception;
                }

                if (!string.IsNullOrEmpty(vm.Result?.SignImgFilePath))
                {
                    return vm.Result.SignImgFilePath;
                }
            }

            throw new SignCancelException();
        }

        private Window CreateWindow(Window owner = null)
        {
            Size winSize = new Size(500, 380);

            Window win = new Window
            {
                Owner = owner ?? Application.Current.MainWindow,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Left = Screen.PrimaryScreen.WorkingArea.Left
                    + Screen.PrimaryScreen.WorkingArea.Width / 2 - winSize.Width / 2,
                Top = Screen.PrimaryScreen.WorkingArea.Top
                    + Screen.PrimaryScreen.WorkingArea.Height / 2 - winSize.Height / 2,
                Width = winSize.Width,
                Height = winSize.Height,
                WindowStyle = WindowStyle.None,
            };

            ScreenSignPadUserControl uc = new ScreenSignPadUserControl();

            if (uc.DataContext is ScreenSignPadViewModel viewModel)
            {
                viewModel.Owner = win;
            }

            win.Content = uc;

            return win;
        }
    }
}
