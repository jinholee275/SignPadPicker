using SignPadPicker.Exceptions;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SignPadPicker.NicePosSignPadAdaptor
{
    public class NiceSignPad : ISignPadPlugin
    {
        public string Name => "NicePosSignPad";

        public string Description => "NicePosSignPad Plugin";

        public bool IsPhysicalDevice => true;

        #region DllImports

        [DllImport("NicePosICV105.dll", CharSet = CharSet.Unicode)]
        public static extern int OpenPort(int iPort, int iBaud); //포트오픈

        [DllImport("NicePosICV105.dll", CharSet = CharSet.Unicode)]
        public static extern int ClosePort(); //포트Close
        
        [DllImport("NicePosICV105.dll", CharSet = CharSet.Unicode)]
        public static extern int NicePosSign(int fcode, byte[] SeBuf, byte[] ReBuf, byte[] HexString, byte[] errcode); //리더기리셋

        [DllImport("NicePosICV105.dll", CharSet = CharSet.Unicode)]
        public static extern int SetBmpFile(byte[] signData, int SignLen, byte[] Dir); //서명 String을 BMP로 저장

        #endregion

        private readonly int port = 5;
        private readonly int baud = 115200;

        public bool IsAvailable
        {
            get
            {
                try
                {
                    Init();
                    return true;
                }
                catch { return false; }
            }
        }

        private void Init()
        {
            if (OpenPort(port, baud) == 1)
            {
                byte[] sendBuf = Encoding.GetEncoding(51949).GetBytes("fffffssdfsd");
                byte[] recvBuf = new byte[5120];
                byte[] hexBuf = new byte[5120];
                byte[] err = new byte[512];

                int result = NicePosSign(0x41, sendBuf, recvBuf, hexBuf, err);

                _ = ClosePort();

                switch (Math.Sign(result))
                {
                    case 0: throw new SignCancelException();
                    case -1: throw new SignFailException();
                    default:
                        return;
                }
            }
            else
            {
                throw new SignPadNotAvailableException();
            }
        }

        public string Activate()
        {
            if (OpenPort(port, baud) == 1)
            {
                byte[] sendBuf = Encoding.GetEncoding(51949).GetBytes("                                서명해주세요");
                byte[] recvBuf = new byte[5120];
                byte[] hexBuf = new byte[5120];
                byte[] err = new byte[512];

                int activateResult = NicePosSign(0x42, sendBuf, recvBuf, hexBuf, err);

                _ = ClosePort();
                
                int len = Array.IndexOf(recvBuf, (byte)0);
                if (len == -1) len = recvBuf.Length;

                if (activateResult == 0)
                {
                    throw new SignCancelException();
                }
                else if (activateResult < 0)
                {
                    throw new SignFailException();
                }

                string filePath = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".bmp");
                int getBmpResult = SetBmpFile(recvBuf, len, Encoding.Default.GetBytes(filePath));

                if (getBmpResult < 0)
                {
                    throw new SignFailException();
                }
                        
                return filePath;
            }
            else
            {
                throw new SignPadNotAvailableException();
            }
        }
    }
}
