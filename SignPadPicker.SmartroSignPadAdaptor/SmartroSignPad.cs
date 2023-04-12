using SignPadPicker.Exceptions;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace SignPadPicker.Adaptor
{
    public class SmartroSignPad : ISignPadPlugin
    {
        public string Name => "SmartroSignPad";

        public string Description => "SmartroSignPad Plugin";

        public bool IsPhysicalDevice => true;

        public bool IsAvailable
        {
            get
            {
                try
                {
                    Init();
                    SMT_Dongle_Stop();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        private static readonly string iniFilePath = @"C:\Hanuriit\SmartRPA.ini";

        #region DllImports

        /// Return Type: int
        ///iPortNum: int
        ///lBaud: int
        [DllImport(@"C:\Hanuriit\SmartroSign.dll",
        EntryPoint = "SMT_Dongle_Start",
        CallingConvention = CallingConvention.StdCall)]
        public static extern int SMT_Dongle_Start(
            int iPortNum,
            int lBaud);

        /// Return Type: int
        ///iFlag: int
        ///ucpSignpadInfo: BYTE*
        [DllImport(@"C:\Hanuriit\SmartroSign.dll",
        EntryPoint = "SMT_Dongle_Initial",
        CallingConvention = CallingConvention.StdCall)]
        public static extern int SMT_Dongle_Initial(
            int iFlag,
            ref byte ucpSignpadInfo);

        /// Return Type: int
        [DllImport(@"C:\Hanuriit\SmartroSign.dll",
        EntryPoint = "SMT_Dongle_Stop",
        CallingConvention = CallingConvention.StdCall)]
        public static extern int SMT_Dongle_Stop();

        /// Return Type: int
        ///cpWorkingKey: BYTE*
        ///cKeyIndex: BYTE->unsigned char
        ///strLine1: BYTE*
        ///strLine2: BYTE*
        ///strLine3: BYTE*
        ///strLine4: BYTE*
        ///ucpSignData: BYTE*
        ///ucpPadVersion: BYTE*
        ///ucpHashData: BYTE*
        ///ucpImgFileNm: BYTE*
        [DllImport(@"C:\Hanuriit\SmartroSign.dll",
        EntryPoint = "SMT_Get_Sign_Screen_Free",
        CallingConvention = CallingConvention.StdCall)]
        public static extern int SMT_Get_Sign_Screen_Free(
            ref byte cpWorkingKey,
            byte cKeyIndex,
            ref byte strLine1,
            ref byte strLine2,
            ref byte strLine3,
            ref byte strLine4,
            ref byte ucpSignData,
            ref byte ucpPadVersion,
            ref byte ucpHashData,
            //ref byte ucpImgFileNm,
            //ref byte strText);
            ref byte ucpImgFileNm);

        [DllImport("kernel32", CharSet = CharSet.Ansi)]
        private static extern bool WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Ansi)]
        private static extern uint GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        #endregion

        public string Activate()
        {
            Init();

            string signImgPath = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".jpg");

            var fi = new FileInfo(signImgPath);

            if (fi.Directory.Exists)
                fi.Directory.Create();

            int signRetVal;

            try
            {
                var cpWorkingKey = Encoding.ASCII.GetBytes("A943124BFCF7C4FB" + "\0");
                byte cKeyIndex = Convert.ToByte('0');
                var byteTextLine1 = Encoding.ASCII.GetBytes("" + "\0");
                var byteTextLine2 = Encoding.ASCII.GetBytes("" + "\0");
                var byteTextLine3 = Encoding.ASCII.GetBytes("" + "\0");
                var byteTextLine4 = Encoding.ASCII.GetBytes("" + "\0");
                var ucpSignData = new byte[1024 * 10];
                var ucpPadVersion = new byte[1024 * 10];
                var ucpHashData = new byte[1024 * 10];
                //var ucpImgFileNm = new byte[1024 * 10];
                //var strText = new byte[1024 * 10];
                var ucpImgFileNm = Encoding.ASCII.GetBytes(signImgPath + "\0");

                signRetVal = SMT_Get_Sign_Screen_Free(
                    ref cpWorkingKey[0],
                    cKeyIndex,
                    ref byteTextLine1[0],
                    ref byteTextLine2[0],
                    ref byteTextLine3[0],
                    ref byteTextLine4[0],
                    ref ucpSignData[0],
                    ref ucpPadVersion[0],
                    ref ucpHashData[0],
                    //ref ucpImgFileNm[0],
                    //ref strText[0]);
                    ref ucpImgFileNm[0]);
            }
            catch { throw new SignFailException(); }
            finally { SMT_Dongle_Stop(); }

            if (signRetVal < 0)
            {
                throw new SignFailException();
            }

            return signImgPath;
        }

        private void Init()
        {
            if (SMT_Dongle_Start(ComPortInt, ComSpeedInt) < 0)
            {
                throw new SignPadNotAvailableException();
            }

            var ucpSignpadInfo = new byte[1024 * 10];
            int iFlag = 2;

            if (SMT_Dongle_Initial(iFlag, ref ucpSignpadInfo[0]) < 0)
            {
                throw new SignPadNotAvailableException();
            }
        }

        /*
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
        */

        private static string ComPortStr
            => GetIniValue("VtrInfo", "SIGN_ComPort", "COM20", iniFilePath);

        private static string ComSpeedStr
            => GetIniValue("VtrInfo", "SIGN_ComSpeed", "115200", iniFilePath);

        private static int ComPortInt
            => Convert.ToInt32(Regex.Replace(ComPortStr, "[^0-9]", ""));

        private static int ComSpeedInt
            => Convert.ToInt32(ComSpeedStr);

        private static string GetIniValue(string section, string key, string def, string filePath, int buffersize = 2000)
        {
            if (!File.Exists(filePath))
            {
                throw new SignPadNotInstalledException();
            }

            StringBuilder buffer = new StringBuilder(buffersize);
            GetPrivateProfileString(section, key, def, buffer, buffer.Capacity, filePath);
            return buffer.ToString();
        }

        private static bool SetIniValue(string section, string key, string value, string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new SignPadNotInstalledException();
            }

            return WritePrivateProfileString(section, key, value, filePath);
        }
    }
}
