using SignPadPicker.Exceptions;
using SignPadPicker.Extensions;
using System;
using System.Configuration;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace SignPadPicker.Adaptor
{
    public class SCSProSignPad : ISignPadPlugin
    {
        public string Name => "SCSProSignPad";

        public string Description => "SCSProSignPad Plugin";

        public bool IsPhysicalDevice => true;

        public bool IsAvailable
        {
            get
            {
                try
                {
                    Init(ComPort, ComSpeed);
                    FD_SignPadSearch();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        #region DllImports

        private const string MODULE_FILE_NAME = @"POS4SignPad_Ctl.dll";

        [DllImport(MODULE_FILE_NAME, EntryPoint = "FD_SignPADService", CharSet = CharSet.None)]
        private static extern int FD_SignPADService(string InTerminalNumber,
                                                    string InCardNo,
                                                    string InSeq,
                                                    string InMsg1,
                                                    string InMsg2,
                                                    string InMsg3,
                                                    string InMsg4);

        [DllImport(MODULE_FILE_NAME, EntryPoint = "FD_GetSignData", CharSet = CharSet.None)]
        private static extern int FD_GetSignData(byte[] OutSignImagePath,
                                                 byte[] OutSignData,
                                                 byte[] OutSignDataSize,
                                                 byte[] OutHashData);

        [DllImport(MODULE_FILE_NAME, EntryPoint = "FD_GetSignPADInfoData", CharSet = CharSet.None)]
        private static extern int FD_GetSignPADInfoData(byte[] OutLCDType,
                                                        byte[] OutDefFunc,
                                                        byte[] OutModelNum,
                                                        byte[] OutSWVer,
                                                        byte[] OutCompMethod);

        [DllImport(MODULE_FILE_NAME, EntryPoint = "FD_SignPadSearch", CharSet = CharSet.None)]
        private static extern int FD_SignPadSearch();

        [DllImport(MODULE_FILE_NAME, EntryPoint = "FD_SignImageToString", CharSet = CharSet.None)]
        private static extern int FD_SignImageToString(byte[] OUTszOutData);

        [DllImport(MODULE_FILE_NAME, EntryPoint = "FD_StringToSignImage", CharSet = CharSet.None)]
        private static extern int FD_StringToSignImage(string INstrString, string INstrBMPPath);

        [DllImport("kernel32", CharSet = CharSet.Ansi)]
        private static extern bool WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Ansi)]
        private static extern uint GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        #endregion

        private int ComPort => Convert.ToInt32(ConfigurationManager.AppSettings["SignPadPicker.SCSProSignPadAdaptor.ComPort"].IfEmptyReplace(null) ?? "20");
        private int ComSpeed => Convert.ToInt32(ConfigurationManager.AppSettings["SignPadPicker.SCSProSignPadAdaptor.ComSpeed"].IfEmptyReplace(null) ?? "57600");

        private void Init(int port, int speed)
        {
            // 포트 설정 로직 추가 가능
            WritePrivateProfileString("General", "COM_Port", "COM" + port, @"C:\Program Files\SNUBH\HIS\PA\POS4SignPAD.ini");
        }

        public string Activate(Window owner = null)
        {
            SignPadConfig config = new SignPadConfig
            {
                ComPort = ComPort,
                ComSpeed = ComSpeed,
            };

            return Activate(config, owner);
        }

        public string Activate(SignPadConfig config, Window owner = null)
        {
            Init(config.ComPort, config.ComSpeed);

            string signImgPath = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".bmp");

            var fi = new FileInfo(signImgPath);

            if (fi.Directory.Exists)
                fi.Directory.Create();

            int signRetVal;

            try
            {
                string strCATID = "10069371"; // 테스트 단말기 번호
                string strCardNo = "0000000000000000=0000";
                string strSeqNum = "000001"; // 예시 거래번호

                byte[] OutSignImagePath = new byte[256];
                byte[] OutSignData = new byte[2048];
                byte[] OutSignDataSize = new byte[256];
                byte[] OutHashData = new byte[256];
                byte[] OutLCDType = new byte[256];
                byte[] OutDefFunc = new byte[256];
                byte[] OutModelNum = new byte[256];
                byte[] OutSWVer = new byte[256];
                byte[] OutCompMethod = new byte[256];
                byte[] OutSignString = new byte[2048];

                signRetVal = FD_SignPADService(strCATID, strCardNo, strSeqNum, "서명해주세요", "", "", "");

                if (signRetVal == 1)
                {
                    signRetVal = FD_GetSignData(OutSignImagePath, OutSignData, OutSignDataSize, OutHashData);
                    if (signRetVal == 0)
                    {
                        signRetVal = FD_GetSignPADInfoData(OutLCDType, OutDefFunc, OutModelNum, OutSWVer, OutCompMethod);
                        signRetVal = FD_SignImageToString(OutSignString);
                        if (signRetVal == 0)
                        {
                            string signString = Encoding.Default.GetString(OutSignString, 0, Strlen(OutSignString));
                            SaveTextToImage(signString, signImgPath);
                        }
                        else
                        {
                            throw new SignFailException("서명 데이터 변환 실패");
                        }
                    }
                    else
                    {
                        throw new SignFailException("서명 데이터 가져오기 실패");
                    }
                }
                else
                {
                    throw new SignFailException("서명 패드 서비스 실패");
                }
            }
            catch
            {
                throw new SignFailException();
            }
            finally
            {
                FD_SignPadSearch();
            }

            if (signRetVal < 0)
            {
                throw new SignFailException();
            }

            return signImgPath;
        }

        private void SaveTextToImage(string sTextData, string filePath)
        {
            int iRet = FD_StringToSignImage(sTextData, filePath);
            if (iRet != 0)
            {
                throw new SignFailException("이미지 파일 저장 실패");
            }
        }

        private int Strlen(byte[] InDest)
        {
            for (int i = 0; i < InDest.Length; i++)
                if (InDest[i] == 0)
                    return i;
            return 0;
        }
    }
}
