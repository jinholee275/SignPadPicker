using SignPadPicker.Exceptions;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SignPadPicker.MipSignPadAdaptor
{
    public class MipSignPad : ISignPadPlugin
    {
        public string Name => "MipSignPad";

        public string Description => "MipSignPad Plugin";

        public bool IsAvailable
        {
            get
            {
                try { return AutoComPort() == 0; }
                catch { return false; }
            }
        }

        #region DllImports

        [DllImport("MipDll.dll")]
        public static extern int AutoComPort();

        [DllImport("MipDll.dll")]
        public static extern int CloseComPort();

        [DllImport("MipDll.dll")]
        public static extern int SignInputCommand();

        [DllImport("MipDll.dll")]
        public static extern int LcdAllCleanCommand();

        [DllImport("MipDll.dll")]
        public static extern int LcdTextCommand(int x, int y, String text);

        [DllImport("MipDll.dll")]
        public static extern int ProgramStopCommand();

        [DllImport("MipDll.dll")]
        public static extern int SignEndCommand();

        [DllImport("MipDll.dll")]
        public static extern int SignImagePath(StringBuilder path);

        [DllImport("MipDll.dll")]
        public static extern int ModelNameCommand();

        [DllImport("MipDll.dll")]
        public static extern int NameDataRead();

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

        private void Init()
        {
            ModelNameCommand();

            for (int i = 0; i < 8; i++)
            {
                model += Convert.ToChar(NameDataRead());
            }

            for (int i = 0; i < 4; i++)
            {
                NameDataRead();
            }

            for (int i = 0; i < 3; i++)
            {
                version += Convert.ToChar(NameDataRead());
            }
        }

        private string Sign()
        {
            LcdAllCleanCommand();
            LcdTextCommand(3, 24, "서명을 입력 하세요!");
            SignInputCommand();

            bool isEndCommand = false;

            while (!isEndCommand)
            {
                isEndCommand = SignEndCommand() == 0;
            }

            ProgramStopCommand();

            StringBuilder nPath = new StringBuilder(4096);

            SignImagePath(nPath);

            ProgramStopCommand();

            CloseComPort();

            return nPath.ToString();
        }
    }
}
