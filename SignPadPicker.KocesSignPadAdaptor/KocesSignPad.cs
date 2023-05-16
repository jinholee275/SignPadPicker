using SignPadPicker.Exceptions;
using SignPadPicker.Extensions;
using System;
using System.Configuration;
using System.IO;
using System.Runtime.InteropServices;

namespace SignPadPicker.KocesSignPadAdaptor
{
    public class KocesSignPad : ISignPadPlugin
    {
        public string Name => "KocesSignPad";

        public string Description => "KocesSignPad Plugin";

        public bool IsPhysicalDevice => true;

        public bool IsAvailable
        {
            get
            {
                try { throw new NotImplementedException(); }
                catch { return false; }
            }
        }

        #region DllImports

        [DllImport(@"KocesSignPad.dll", EntryPoint = "UserSignRequest", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int UserSignRequest(int nPort, int nSpeed, string pMsg1, string pMsg2, string pMsg3, string pMsg4, string Bmp);

        #endregion

        private int ComPort => Convert.ToInt32(ConfigurationManager.AppSettings["SignPadPicker.KocesSignPadAdaptor.ComPort"].IfEmptyReplace(null) ?? "4");
        private int ComSpeed => Convert.ToInt32(ConfigurationManager.AppSettings["SignPadPicker.KocesSignPadAdaptor.ComSpeed"].IfEmptyReplace(null) ?? "38400");
        private string Message1 => ConfigurationManager.AppSettings["SignPadPicker.KocesSignPadAdaptor.Message1"] ?? "";
        private string Message2 => ConfigurationManager.AppSettings["SignPadPicker.KocesSignPadAdaptor.Message2"] ?? "";
        private string Message3 => ConfigurationManager.AppSettings["SignPadPicker.KocesSignPadAdaptor.Message3"] ?? "";
        private string Message4 => ConfigurationManager.AppSettings["SignPadPicker.KocesSignPadAdaptor.Message4"] ?? "";

        public string Activate()
        {
            SignPadConfig config = new SignPadConfig
            {
                ComPort = ComPort,
                ComSpeed = ComSpeed,
                Message1 = Message1,
                Message2 = Message2,
                Message3 = Message3,
                Message4 = Message4,
            };

            return Activate(config);
        }

        public string Activate(SignPadConfig config)
        {
            string filePath = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".jpg");
            int nRtn = UserSignRequest(
                config.ComPort,
                config.ComSpeed,
                config.Message1 ?? string.Empty,
                config.Message2 ?? string.Empty,
                config.Message3 ?? string.Empty,
                config.Message4 ?? string.Empty,
                filePath);

            switch (nRtn)
            {
                case -1: throw new SignPadNotAvailableException("사인패드 포트 연결 실패");
                case -2: throw new SignPadNotAvailableException("사인패드 초기화 실패");
                case -3: throw new SignPadNotAvailableException("사인패드 요청 실패");
                case -4: throw new SignPadNotAvailableException("사인패드 얻기 실패");
                case -5: throw new SignCancelException();
                case -6: throw new SignPadNotAvailableException("사인패드 등록 필요");
                default: return filePath;
            }
        }
    }
}
