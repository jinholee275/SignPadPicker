using SignPadPicker.Exceptions;
using System;
using System.Runtime.InteropServices;

namespace SignPadPicker.MspSignPadAdaptor
{
    public class MspSignPad : ISignPadPlugin
    {
        public string Name => "MspSignPad";

        public string Description => "MspSignPad Plugin";

        public bool IsAvailable
        {
            get
            {
                try { return GetDeviceState() == 1 && SetPadConnect() == 0; }
                catch { return false; }
            }
        }

        #region DllImports

        [DllImport("MSP4SerialLib.dll")]
        public static extern int SetPadConnect();

        [DllImport("MSP4SerialLib.dll")]
        public static extern int SetPadDisconnect();

        [DllImport("MSP4SerialLib.dll")]
        public static extern int SetSignStart(int SignType, int SignSize, int ImageType, int SignWidt, int nFontSize, String nFontName, String nMessage);

        [DllImport("MSP4SerialLib.dll")]
        public static extern int SetPadInitial();

        [DllImport("MSP4SerialLib.dll")]
        public static extern void SetSignImagePath(String FolderName, String FileName);

        [DllImport("MSP4SerialLib.dll")]
        public static extern int GetButtonCheck();

        [DllImport("MSP4SerialLib.dll")]
        public static extern int GetSignSize();

        [DllImport("MSP4SerialLib.dll")]
        public static extern int GetSignPointX();

        [DllImport("MSP4SerialLib.dll")]
        public static extern int GetSignPointY();

        [DllImport("MSP4SerialLib.dll")]
        public static extern IntPtr GetModelName();

        [DllImport("MSP4SerialLib.dll")]
        public static extern IntPtr GetPadVersion();

        [DllImport("MSP4SerialLib.dll")]
        public static extern int SetSignDlg(int SignType, int SignSize, int ImageType, int SignWidt, int nFontSize, String nFontName, String nMessage);

        [DllImport("MSP4SerialLib.dll")]
        public static extern int SetYesNo(int Type);

        [DllImport("MSP4SerialLib.dll")]
        public static extern int GetYesNo();

        [DllImport("MSP4SerialLib.dll")]
        public static extern int SetTextMessage2(int Type, int x, int y, int size, String nText);

        [DllImport("MSP4SerialLib.dll")]
        public static extern IntPtr GetSignImagePath();

        [DllImport("MSP4SerialLib.dll")]
        public static extern int GetDeviceState();

        #endregion

        private string model = "Model : ";
        private string version = "Version : ";

        public string Activate()
        {
            if (!IsAvailable)
            {
                throw new SignPadNotAvailableException();
            }

            Init();

            return Sign();
        }


        private string Init()
        {
            model += Marshal.PtrToStringAnsi(GetModelName());
            version += Marshal.PtrToStringAnsi(GetPadVersion());

            return Sign();
        }

        private string Sign()
        {
            int nRtn = SetSignDlg(0, 0, 0, 3, 1, "굴림", "");
            SetPadInitial();

            if (nRtn == 0)
            {
                IntPtr ptrMsg = GetSignImagePath();

                string filePath = Marshal.PtrToStringAnsi(ptrMsg);

                SetPadInitial();

                SetPadDisconnect();

                return filePath;
            }
            else
            {
                throw new SignFailException();
            }
        }
    }
}
