﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SignPadPicker.Adaptor
{
    /// <summary>
    /// ScreenSignPad.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ScreenSignPadUserControl : UserControl
    {
        private Point startPosition;

        public ScreenSignPadUserControl()
        {
            InitializeComponent();
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
    }
}
