using SignPadPicker.Exceptions;
using System;
using System.Collections;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SignPadPicker.TestApplication
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            LoadPlugins();
        }

        private void LoadPlugins()
        {
            try
            {
                SignPadLoader loader = new SignPadLoader();
                loader.LoadPlugins(".");
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Plugins couldn't be loaded: {0}", ex.Message));
                Environment.Exit(0);
            }
        }

        private void SetResults(string imageFilePath, string[] results)
        {
            if (!string.IsNullOrEmpty(imageFilePath))
            {
                SignImage.Source = new BitmapImage(new Uri(imageFilePath));
            }

            StringBuilder sb = new StringBuilder();

            foreach (string result in results)
            {
                sb.AppendLine(result);
            }

            ResultTbx.Text = sb.ToString();
        }

        private void ActivateSignPad(string pluginName = "")
        {
            try
            {
                ISignPadPlugin plugin;

                if (string.IsNullOrEmpty(pluginName))
                {
                    plugin = SignPadLoader.GetPlugin(onlyPhysicalDevice: true);
                }
                else
                {
                    plugin = pluginName.Split(',').Length > 1
                        ? SignPadLoader.GetPlugin(names: pluginName.Split(','))
                        : SignPadLoader.GetPlugin(name: pluginName);
                }

                string filePath = plugin.Activate();

                SetResults(filePath, new string[] { });
            }
            catch (NoPluginFoundException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (SignPadNotInstalledException)
            {
                MessageBox.Show("사인패드 제어 프로그램 설치가 필요합니다.");
            }
            catch (SignPadNotAvailableException)
            {
                MessageBox.Show("현재 PC에 연결되어 있는 전자서명 패드 장비가 없거나 연결 실패 하였습니다.");
            }
            catch (SignFailException)
            {
                MessageBox.Show("서명 취소 또는 시간이 초과되었습니다.");
            }
            catch (SignCancelException)
            {
                MessageBox.Show("서명이 취소 되었습니다.");
            }
            catch (Exception ex)
            {
                SetResults(null, new string[] { $"Caught exception: {ex.Message}" });
            }
        }

        private void ActivateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                ActivateSignPad(btn.Tag as string);
            }
        }
    }
}
