using SignPadPicker.Exceptions;
using SignPadPicker.Extensions;
using System;
using System.Configuration;
using System.Windows;
using Screen = System.Windows.Forms.Screen;

namespace SignPadPicker.Adaptor
{
    public class NiceSignPad : ISignPadPlugin
    {
        public string Name => "NiceSignPad";

        public string Description => "NiceSignPad Plugin";

        public bool IsPhysicalDevice => true;

        public bool IsAvailable
        {
            get
            {
                NiceSignPadUserControl uc = new NiceSignPadUserControl();

                if (uc.DataContext is NiceSignPadViewModel vm)
                {
                    if (!vm.OpenPort())
                        return false;

                    vm.ClosePort();
                }

                return true;
            }
        }

        private int ComPort => Convert.ToInt32(ConfigurationManager.AppSettings["SignPadPicker.NiceSignPadAdaptor.ComPort"].IfEmptyReplace(null) ?? "3");
        private int ComSpeed => Convert.ToInt32(ConfigurationManager.AppSettings["SignPadPicker.NiceSignPadAdaptor.ComSpeed"].IfEmptyReplace(null) ?? "115200");


        public string Activate()
        {
            SignPadConfig config = new SignPadConfig
            {
                ComPort = ComPort,
                ComSpeed = ComSpeed,
            };

            return Activate(config);
        }

        public string Activate(SignPadConfig config)
        {
            Window win = CreateWindow();

            _ = win.ShowDialog();

            if (win.Content is NiceSignPadUserControl uc &&
                uc.DataContext is NiceSignPadViewModel vm)
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

        private Window CreateWindow()
        {
            Size winSize = new Size(500, 380);

            Window win = new Window
            {
                Owner = Application.Current.MainWindow,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Left = Screen.PrimaryScreen.WorkingArea.Left
                    + Screen.PrimaryScreen.WorkingArea.Width / 2
                    - winSize.Width / 2,
                Top = Screen.PrimaryScreen.WorkingArea.Top
                    + Screen.PrimaryScreen.WorkingArea.Height / 2
                    - winSize.Height / 2,
                Width = winSize.Width,
                Height = winSize.Height,
                WindowStyle = WindowStyle.None,
            };

            NiceSignPadUserControl uc = new NiceSignPadUserControl();

            if (uc.DataContext is NiceSignPadViewModel viewModel)
            {
                viewModel.Owner = win;
            }

            win.Content = uc;

            return win;
        }
    }
}
