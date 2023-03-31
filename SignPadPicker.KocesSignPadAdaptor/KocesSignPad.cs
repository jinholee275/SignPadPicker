using SignPadPicker.Exceptions;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SignPadPicker.KocesSignPadAdaptor
{
    public class KocesSignPad : ISignPadPlugin
    {
        public string Name => "KocesSignPad";

        public string Description => "KocesSignPad Plugin";

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

        public string Activate()
        {
            string filePath = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".jpg");
            int nRtn = UserSignRequest(4, 38400, "", "서명을", "입력하세요", "", filePath);

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
