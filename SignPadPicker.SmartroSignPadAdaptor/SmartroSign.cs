using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace SignPadPicker.Adaptor
{
    internal class SmartroSign
    {
        /// Return Type: int
        ///iPortNum: int
        ///lBaud: int
        [System.Runtime.InteropServices.DllImportAttribute(@"C:\Hanuriit\SmartroSign.dll",
        EntryPoint = "SMT_Dongle_Start",
        CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
        public static extern int SMT_Dongle_Start(
            int iPortNum,
            int lBaud);

        /// Return Type: int
        ///iFlag: int
        ///ucpSignpadInfo: BYTE*
        [System.Runtime.InteropServices.DllImportAttribute(@"C:\Hanuriit\SmartroSign.dll",
        EntryPoint = "SMT_Dongle_Initial",
        CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
        public static extern int SMT_Dongle_Initial(
            int iFlag,
            ref byte ucpSignpadInfo);

        /// Return Type: int
        [System.Runtime.InteropServices.DllImportAttribute(@"C:\Hanuriit\SmartroSign.dll",
        EntryPoint = "SMT_Dongle_Stop",
        CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
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
        [System.Runtime.InteropServices.DllImportAttribute(@"C:\Hanuriit\SmartroSign.dll",
        EntryPoint = "SMT_Get_Sign_Screen_Free",
        CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
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

        /// <summary>
        /// 스마트로 서명패드로 서명을 한다.
        /// </summary>
        /// <param name="iPortNum"></param>
        /// <param name="lBaud"></param>
        /// <param name="strLine1"></param>
        /// <param name="strLine2"></param>
        /// <param name="strLine3"></param>
        /// <param name="strLine4"></param>
        /// <param name="strImgFileSavePath"></param>
        /// <returns>
        /// 0보다 작으면 오류.
        /// </returns>
        public static int GetSign(
            int iPortNum
            , int lBaud
            //,int? iFlag
            , string strLine1
            , string strLine2
            , string strLine3
            , string strLine4
            //,ref byte[] ucpSignData
            //,ref byte[] ucpPadVersion
            //,ref byte[] ucpHashData
            , string strImgFileSavePath
            )
        {
            int ret = -1;

            try
            {
                var fi_strImgFileSavePath = new FileInfo(strImgFileSavePath);
                if (fi_strImgFileSavePath.Directory.Exists)
                    fi_strImgFileSavePath.Directory.Create();

                Console.WriteLine("동글 통신 시작");
                ret = SMT_Dongle_Start(iPortNum, lBaud);
                if (0 > ret)
                    return ret;

                Console.WriteLine("동글 초기화");
                var ucpSignpadInfo = new byte[1024 * 10];
                int iFlag = 2;
                ret = SMT_Dongle_Initial(iFlag, ref ucpSignpadInfo[0]);
                if (0 > ret)
                    return ret;

                var cpWorkingKey = Encoding.ASCII.GetBytes("A943124BFCF7C4FB" + "\0");
                byte cKeyIndex = Convert.ToByte('0');
                var byteTextLine1 = Encoding.ASCII.GetBytes(strLine1 + "\0");
                var byteTextLine2 = Encoding.ASCII.GetBytes(strLine2 + "\0");
                var byteTextLine3 = Encoding.ASCII.GetBytes(strLine3 + "\0");
                var byteTextLine4 = Encoding.ASCII.GetBytes(strLine4 + "\0");
                var ucpSignData = new byte[1024 * 10];
                var ucpPadVersion = new byte[1024 * 10];
                var ucpHashData = new byte[1024 * 10];
                //var ucpImgFileNm = new byte[1024 * 10];
                var ucpImgFileNm = Encoding.ASCII.GetBytes(strImgFileSavePath + "\0");
                var strText = new byte[1024 * 10];

                Console.WriteLine("동글 서명 요청");
                ret = SMT_Get_Sign_Screen_Free(
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
                if (0 > ret)
                    return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("ret={0}", ret);
                ret = -1;
            }
            finally
            {
                Console.WriteLine("동글 통신 종료");
                SMT_Dongle_Stop();
            }

            return ret;
        }

        [DllImport("kernel32", CharSet = CharSet.Ansi)]
        private static extern bool WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Ansi)]
        private static extern uint GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        private static readonly String iniSmartRPAFilepath = @"C:\Hanuriit\SmartRPA.ini";

        public static String SIGN_ComPortStr
        {
            get { return GetINIValue("VtrInfo", "SIGN_ComPort", "COM20", iniSmartRPAFilepath); }
        }

        public static int SIGN_ComPortInt
        {
            get
            {
                String strSIGN_ComPort = GetINIValue("VtrInfo", "SIGN_ComPort", "COM20", iniSmartRPAFilepath);
                return Convert.ToInt32(Regex.Replace(strSIGN_ComPort, "[^0-9]", ""));
            }
        }

        public static int SIGN_ComSpeedInt
        {
            get
            {
                String strSIGN_ComSpeed = GetINIValue("VtrInfo", "SIGN_ComSpeed", "115200", iniSmartRPAFilepath);
                return Convert.ToInt32(strSIGN_ComSpeed);
            }
        }

        private static String GetINIValue(string section, string key, string def, string filePath, int buffersize = 2000)
        {
            if (!File.Exists(filePath))
            {
                //MessageBoxEx.Show(String.Format("한우리 환경설정 파일[{0}]을 찾을 수 없습니다.", filePath), "확인", 1000);
                return "";
            }

            StringBuilder buffer = new StringBuilder(buffersize);
            GetPrivateProfileString(section, key, def, buffer, buffer.Capacity, filePath);
            return buffer.ToString();
        }

        private static bool SetINIValue(string section, string key, string value, string filePath)
        {
            if (!File.Exists(filePath))
            {
                //MessageBoxEx.Show(String.Format("한우리 환경설정 파일[{0}]을 찾을 수 없습니다.", filePath), "확인", 1000);
                return false;
            }

            return WritePrivateProfileString(section, key, value, filePath);
        }
    }
}
