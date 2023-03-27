using SignPadPicker.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Screen = System.Windows.Forms.Screen;

namespace SignPadPicker.Adaptor
{
    public class SmartroSignPad : ISignPadPlugin
    {
        public string Name => "SmartroSignPad";

        public string Description => "SmartroSignPad Plugin";

        public bool IsAvailable => throw new NotImplementedException();

        public string Activate()
        {
            Window win = CreateWindow();

            _ = win.ShowDialog();

            if (win.Content is SmartroSignPadUserControl uc &&
                uc.DataContext is SmartroSignPadViewModel vm)
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
                Title = "SmartroSignPad",
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
            };

            SmartroSignPadUserControl uc = new SmartroSignPadUserControl();

            if (uc.DataContext is SmartroSignPadViewModel viewModel)
            {
                viewModel.Owner = win;
            }

            win.Content = uc;

            return win;
        }
    }
}
