using SignPadPicker.Exceptions;
using SignPadPicker.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SignPadPicker.Adaptor
{
    public class KscatSignPad : ISignPadPlugin
    {
        public string Name => "KscatSignPad";

        public string Description => "KscatSignPad Plugin";

        public bool IsPhysicalDevice => true;

        public bool IsAvailable
        {
            get
            {
                try
                {
                    int httpPort = int.Parse(GetPort());

                    Init(port: httpPort);

                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        #region DllImports

        [DllImport(@"ksnetcomm.dll")]
        public static extern int KSCATApproval(byte[] responseTelegram, string ip, int port, string requestTelegram, int RequestLen, int option);

        [DllImport(@"kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder refVal, int size, string filePath);

        #endregion

        private readonly char PAD_CHAR = ' ';
        private readonly char PAD_INT = '0';

        private readonly Encoding myEncode = Encoding.GetEncoding("ks_c_5601-1987");

        private readonly string STX = Convert.ToChar(2).ToString();
        private readonly string ETX = Convert.ToChar(3).ToString();
        private readonly string CR = Convert.ToChar(13).ToString();
        private readonly string FS = Convert.ToChar(28).ToString();

        private string ImgFile => ConfigurationManager.AppSettings["SignPadPicker.KscatSignPadAdaptor.ImgFile"].IfEmptyReplace(null) ?? @"C:\KSCAT\Sign.bmp";
        private string IniFile => ConfigurationManager.AppSettings["SignPadPicker.KscatSignPadAdaptor.IniFile"].IfEmptyReplace(null) ?? @"C:\KSCAT\config.ini";
        private string m_HttpPort => ConfigurationManager.AppSettings["SignPadPicker.KscatSignPadAdaptor.HttpPort"] ?? "27015";
        private string Message1 => ConfigurationManager.AppSettings["SignPadPicker.KscatSignPadAdaptor.Message1"] ?? "";
        private string Message2 => ConfigurationManager.AppSettings["SignPadPicker.KscatSignPadAdaptor.Message2"] ?? "";
        private string Message3 => ConfigurationManager.AppSettings["SignPadPicker.KscatSignPadAdaptor.Message3"] ?? "";
        private string Message4 => ConfigurationManager.AppSettings["SignPadPicker.KscatSignPadAdaptor.Message4"] ?? "";

        public string Activate()
        {
            SignPadConfig config = new SignPadConfig
            {
                HttpPort = int.Parse(GetPort()),
                ImgFilePath = ImgFile
            };

            return Activate(config);
        }

        public string Activate(SignPadConfig config)
        {
            Init(port: config.HttpPort);

            return Sign(
                port: config.HttpPort,
                outFilePath: config.ImgFilePath);
        }

        /// <summary>
        /// 서명패드 초기화
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void Init(int port)
        {
            string ip = "127.0.0.1";
            string request_msg = string.Empty;
            byte[] recv_msg = new byte[4096];
            int[] SEND_FLD_SIZE = { 1, 4, 2, 1, 1 };
            int[] RECV_FLD_SIZE = { 1, 4, 2, 4, 1, 1 };

            string msg_len_str = (((ICollection<int>)SEND_FLD_SIZE).Sum() - SEND_FLD_SIZE[1]).ToString();

            // 전문 구성 
            request_msg += STX;                                             // STX
            request_msg += msg_len_str.PadLeft(SEND_FLD_SIZE[1], PAD_INT);  // 전문 길이
            request_msg += "P1".PadRight(SEND_FLD_SIZE[2], PAD_CHAR);       // Command ID
            request_msg += ETX;                                             // ETX
            request_msg += CR;                                              // CR

            // KSCATApproval 호출
            int result = KSCATApproval(recv_msg, ip, port, request_msg, request_msg.Length, 0);

            if (result <= 0)
            {
                string responseData = myEncode.GetString(recv_msg, 0, recv_msg.Length);
                throw new SignPadNotAvailableException(responseData);
            }

            string errCode = myEncode.GetString(recv_msg, 7, RECV_FLD_SIZE[3]); //Error Code, 응답 코드 : "0000" = 정상, 그 외 KSCAT 응답 코드 참고

            if (!errCode.Equals("0000"))
            {
                throw new SignPadNotAvailableException();
            }
        }

        /// <summary>
        /// 전자서명 요청
        /// </summary>
        /// <returns></returns>
        private string Sign(int port, string outFilePath)
        {
            string ip = "127.0.0.1";
            string request_msg = string.Empty;
            byte[] recv_msg = new byte[4096];
            int[] SEND_FLD_SIZE = { 1, 4, 2, 16, 16, 16, 16, 1, 1, 1 };
            int[] RECV_FLD_SIZE = { 1, 4, 2, 4, 4, 2, 16, 4, 1536, 1, 1 };

            string msg_len_str = (((ICollection<int>)SEND_FLD_SIZE).Sum() - SEND_FLD_SIZE[1]).ToString();

            // 전문 구성 
            request_msg += STX;                                             // STX
            request_msg += msg_len_str.PadLeft(SEND_FLD_SIZE[1], PAD_INT);  // 전문 길이
            request_msg += "P2".PadRight(SEND_FLD_SIZE[2], PAD_CHAR);       // Command ID
            request_msg += Message1.PadRight(SEND_FLD_SIZE[3], PAD_CHAR); // Message1
            request_msg += Message2.PadRight(SEND_FLD_SIZE[4], PAD_CHAR); // Message2
            request_msg += Message3.PadRight(SEND_FLD_SIZE[5], PAD_CHAR); // Message3
            request_msg += Message4.PadRight(SEND_FLD_SIZE[6], PAD_CHAR); // Message4
            request_msg += "O".PadRight(SEND_FLD_SIZE[7], PAD_CHAR);        // 서명 저장 유무
            request_msg += ETX;                                             // ETX
            request_msg += CR;                                              // CR

            // KSCATApproval 호출
            int result = KSCATApproval(recv_msg, ip, port, request_msg, request_msg.Length, 0);

            if (result <= 0)
            {
                string responseData = myEncode.GetString(recv_msg, 0, recv_msg.Length);
                throw new SignPadNotAvailableException(responseData);
            }

            string errCode = myEncode.GetString(recv_msg, 7, RECV_FLD_SIZE[3]); //Error Code, 응답 코드 : "0000" = 정상, 그 외 KSCAT 응답 코드 참고

            if (errCode.Equals("0000"))
            {
                return outFilePath;
            }
            else if (errCode.Equals("1000"))
            {
                throw new SignCancelException();
            }
            else
            {
                throw new SignFailException();
            }
        }

        /// <summary>
        /// name         : 포트번호 Get
        /// desc         : KSCAT 프로그램에 설정된 포트번호를 가져온다.
        /// author       : 이진호
        /// create date  : 2019-12-06
        /// <returns></returns>
        /// </summary>
        private string GetPort()
        {
            StringBuilder retPort = new StringBuilder();

            // 서명패드 PORT가 설정된 ini파일 읽어오기
            if (string.IsNullOrEmpty(retPort.ToString()))
            {
                GetPrivateProfileString("daemon", "port", m_HttpPort, retPort, 32, IniFile);
            }

            if (string.IsNullOrEmpty(retPort.ToString()))
            {
                retPort.Append(m_HttpPort);
            }

            return retPort.ToString();
        }
    }
}
