using Microsoft.Win32.SafeHandles;
using System.Drawing;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Point = System.Windows.Point;

namespace SignPadPicker.Adaptor
{
    /// <summary>
    /// ScreenSignPad.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ScreenSignPadUserControl : UserControl
    {
        private Point startPosition;
        //private Cursor customCursor;

        public ScreenSignPadUserControl()
        {
            InitializeComponent();

            //customCursor = CreateCustomCursor();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Window parentWindow = Window.GetWindow(this);
                startPosition = e.GetPosition(parentWindow);
                CaptureMouse();
            }
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsMouseCaptured)
            {
                Window parentWindow = Window.GetWindow(this);
                Point currentPosition = e.GetPosition(parentWindow);
                Vector diff = currentPosition - startPosition;
                parentWindow.Left += diff.X;
                parentWindow.Top += diff.Y;
            }
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }
        }

        private void ContentControl_MouseEnter(object sender, MouseEventArgs e)
        {
            //if (customCursor!= null)
            //    Mouse.OverrideCursor = customCursor;
        }

        private void ContentControl_MouseLeave(object sender, MouseEventArgs e)
        {
            //if (customCursor != null)
            //    Mouse.OverrideCursor = null;
        }

        //private Cursor CreateCustomCursor(int dotSize)
        //{
        //    try
        //    {
        //        // Create a new bitmap with a size of 32x32 (standard cursor size)
        //        Bitmap bitmap = new Bitmap(dotSize, dotSize);
        //        using (Graphics g = Graphics.FromImage(bitmap))
        //        {
        //            // Clear the bitmap with transparent color
        //            g.Clear(Color.Transparent);

        //            // Draw a red dot with a black border
        //            int centerX = bitmap.Width / 2;
        //            int centerY = bitmap.Height / 2;

        //            // Draw black border
        //            g.FillEllipse(Brushes.White, centerX - dotSize / 2 - 1, centerY - dotSize / 2 - 1, dotSize + 2, dotSize + 2);

        //            // Draw red dot
        //            g.FillEllipse(Brushes.Black, centerX - dotSize / 2, centerY - dotSize / 2, dotSize, dotSize);
        //        }

        //        // Convert bitmap to IntPtr
        //        IntPtr hIcon = bitmap.GetHicon();
        //        return CursorInteropHelper.Create(new SafeFileHandle(hIcon, true));
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}
    }
}
