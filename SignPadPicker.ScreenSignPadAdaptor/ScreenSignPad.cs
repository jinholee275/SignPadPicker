using Microsoft.Win32.SafeHandles;
using SignPadPicker.Exceptions;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Screen = System.Windows.Forms.Screen;
using Size = System.Windows.Size;

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
            Window win = CreateWindow(config, owner);

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

        private Window CreateWindow(SignPadConfig config, Window owner = null)
        {
            Size winSize = new Size(
                width: double.IsNaN(config.ScreenSizeWidth) ? 500 : config.ScreenSizeWidth,
                height: double.IsNaN(config.ScreenSizeHeight) ? 380 : config.ScreenSizeHeight);

            Window win = new Window
            {
                Owner = owner ?? Application.Current.MainWindow,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Left = double.IsNaN(config.ScreenPositionLeft)
                    ? (Screen.PrimaryScreen.WorkingArea.Left + Screen.PrimaryScreen.WorkingArea.Width / 2 - winSize.Width / 2)
                    : config.ScreenPositionLeft,
                Top = double.IsNaN(config.ScreenPositionTop)
                    ? (Screen.PrimaryScreen.WorkingArea.Top + Screen.PrimaryScreen.WorkingArea.Height / 2 - winSize.Height / 2)
                    : config.ScreenPositionTop,
                Width = winSize.Width,
                Height = winSize.Height,
                WindowStyle = WindowStyle.None,
                WindowState = config.ScreenIsMaximized ? WindowState.Maximized : WindowState.Normal,
            };


            ScreenSignPadUserControl uc = new ScreenSignPadUserControl();

            if (uc.DataContext is ScreenSignPadViewModel viewModel)
            {
                viewModel.Owner = win;
            }

            win.Content = uc;

            // Set the custom cursor
            SetCustomCursor();

            // Restore the default cursor when the window is closed
            win.Closed += (s, e) =>
            {
                ResetCursor();
            };

            return win;
        }

        private Cursor CreateCustomCursor()
        {
            // Create a new bitmap with a size of 32x32 (standard cursor size)
            Bitmap bitmap = new Bitmap(64, 64);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // Clear the bitmap with transparent color
                g.Clear(Color.Transparent);

                // Draw a red dot with a black border
                int dotSize = 32;
                int centerX = bitmap.Width / 2;
                int centerY = bitmap.Height / 2;

                // Draw black border
                g.FillEllipse(Brushes.White, centerX - dotSize / 2 - 1, centerY - dotSize / 2 - 1, dotSize + 2, dotSize + 2);

                // Draw red dot
                g.FillEllipse(Brushes.Black, centerX - dotSize / 2, centerY - dotSize / 2, dotSize, dotSize);
            }

            // Convert bitmap to IntPtr
            IntPtr hIcon = bitmap.GetHicon();
            return CursorInteropHelper.Create(new SafeFileHandle(hIcon, true));
        }

        private void SetCustomCursor()
        {
            Cursor customCursor = CreateCustomCursor();
            Mouse.OverrideCursor = customCursor;
        }

        private void ResetCursor()
        {
            Mouse.OverrideCursor = null;
        }
    }
}
