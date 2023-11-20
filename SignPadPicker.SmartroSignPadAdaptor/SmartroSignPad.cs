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
                    Init(ComPort, ComSpeed);
                    SMT_Dongle_Stop();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        #region DllImports

        private const string MODULE_FILE_NAME = @"SmartroSign.dll";

        /// <summary>
        /// 작업키 (슈퍼키)
        /// 일반적으로는 키를 다운로드 받고 작업을 진행한다. (POS용)
        /// EMR에서는 싸인패드 용으로만 쓰기 때문에 업체에 슈퍼키를 제공 받아 인터페이스 한다.
        /// </summary>
        private const string WORKING_KEY = "A943124BFCF7C4FB";

        /// <summary>
        /// 작업키 인덱스 (슈퍼키)
        /// 일반적으로는 키를 다운로드 받고 작업을 진행한다. (POS용)
        /// EMR에서는 싸인패드 용으로만 쓰기 때문에 업체에 슈퍼키를 제공 받아 인터페이스 한다.
        /// </summary>
        private const int KEY_INDEX = 48;

        /// <summary>
        /// 싸인패드 기기정보 (버전)
        /// </summary>
        private StringBuilder SingInfoData = new StringBuilder(new String('\0', 64));

        ///// Return Type: int
        /////iPortNum: int
        /////lBaud: int
        //[DllImport(MODULE_FILE_NAME,
        //EntryPoint = "SMT_Dongle_Start",
        //CallingConvention = CallingConvention.StdCall)]
        //public static extern int SMT_Dongle_Start(
        //    int iPortNum,
        //    int lBaud);

        ///// Return Type: int
        /////iFlag: int
        /////ucpSignpadInfo: BYTE*
        //[DllImport(MODULE_FILE_NAME,
        //EntryPoint = "SMT_Dongle_Initial",
        //CallingConvention = CallingConvention.StdCall)]
        //public static extern int SMT_Dongle_Initial(
        //    int iFlag,
        //    ref byte ucpSignpadInfo);

        ///// Return Type: int
        //[DllImport(MODULE_FILE_NAME,
        //EntryPoint = "SMT_Dongle_Stop",
        //CallingConvention = CallingConvention.StdCall)]
        //public static extern int SMT_Dongle_Stop();

        ///// Return Type: int
        /////cpWorkingKey: BYTE*
        /////cKeyIndex: BYTE->unsigned char
        /////strLine1: BYTE*
        /////strLine2: BYTE*
        /////strLine3: BYTE*
        /////strLine4: BYTE*
        /////ucpSignData: BYTE*
        /////ucpPadVersion: BYTE*
        /////ucpHashData: BYTE*
        /////ucpImgFileNm: BYTE*
        //[DllImport(MODULE_FILE_NAME,
        //EntryPoint = "SMT_Get_Sign_Screen_Free",
        //CallingConvention = CallingConvention.StdCall)]
        //public static extern int SMT_Get_Sign_Screen_Free(
        //    ref byte cpWorkingKey,
        //    byte cKeyIndex,
        //    ref byte strLine1,
        //    ref byte strLine2,
        //    ref byte strLine3,
        //    ref byte strLine4,
        //    ref byte ucpSignData,
        //    ref byte ucpPadVersion,
        //    ref byte ucpHashData,
        //    //ref byte ucpImgFileNm,
        //    //ref byte strText);
        //    ref byte ucpImgFileNm);

        /// <summary>
        /// 서명 OPEN
        /// </summary>
        /// <param name="nPortNum">포트번호</param>
        /// <param name="lBaud">통신속도</param>
        /// <returns></returns>
        [DllImport(MODULE_FILE_NAME, EntryPoint = "SMT_Dongle_Start", CharSet = CharSet.Ansi)]
        private static extern int SMT_Dongle_Start(int nPortNum, int lBaud);

        /// <summary>
        /// 서명 초기화
        /// </summary>
        /// <param name="nPortNum">1:동글 , 2:싸인패드</param>
        /// <param name="sSignpadInfo">기기정보</param>
        /// <returns></returns>
        [DllImport(MODULE_FILE_NAME, EntryPoint = "SMT_Dongle_Initial", CharSet = CharSet.Ansi)]
        private static extern int SMT_Dongle_Initial(int nPortNum, StringBuilder sSignpadInfo);

        /// <summary>
        /// 서명 종료
        /// </summary>
        /// <returns></returns>
        [DllImport(MODULE_FILE_NAME, EntryPoint = "SMT_Dongle_Stop", CharSet = CharSet.Ansi)]
        private static extern int SMT_Dongle_Stop();

        /// <summary>
        /// 서명
        /// </summary>
        /// <param name="sWorkKey">작업키</param>
        /// <param name="byKeyIdx">작업키 인덱스</param>
        /// <param name="sbLine1">첫번째 라인 메시지 (16Byte)</param>
        /// <param name="sbLine2">두번째 라인 메시지 (16Byte)</param>
        /// <param name="sbLine3">세번째 라인 메시지 (16Byte)</param>
        /// <param name="sbLine4">네번째 라인 메시지 (16Byte)</param>
        /// <param name="sbSignData">서명 DATA (Base64 인코딩)</param>
        /// <param name="sbPadVersion">패드 버전</param>
        /// <param name="sbHashData"></param>
        /// <param name="sFilePath">Output 파일 경로</param>
        /// <returns></returns>
        [DllImport(MODULE_FILE_NAME, EntryPoint = "SMT_Get_Sign_Screen_Free", CharSet = CharSet.Ansi)]
        public static extern int SMT_Get_Sign_Screen_Free(String sWorkKey, int byKeyIdx, StringBuilder sbLine1, StringBuilder sbLine2, StringBuilder sbLine3, StringBuilder sbLine4, StringBuilder sbSignData, StringBuilder sbPadVersion, StringBuilder sbHashData, String sFilePath);

        [DllImport("kernel32", CharSet = CharSet.Ansi)]
        private static extern bool WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Ansi)]
        private static extern uint GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        #endregion

        private int ComPort => Convert.ToInt32(ConfigurationManager.AppSettings["SignPadPicker.SmartroSignPadAdaptor.ComPort"].IfEmptyReplace(null) ?? "20");
        private int ComSpeed => Convert.ToInt32(ConfigurationManager.AppSettings["SignPadPicker.SmartroSignPadAdaptor.ComSpeed"].IfEmptyReplace(null) ?? "57600");

        private void Init(int port, int speed)
        {
            int iRet;

            //# 사인패드 연결
            iRet = SMT_Dongle_Start(port, speed);
            if (iRet < 0)
            {
                throw new SignPadNotAvailableException("사인패드 오픈 실패!! 싸인패드 환경설정에서 포트, 연결속도를 확인하고 다시 연결하고 시도해 주세요!");
            }

            //# 사인패드 초기화
            iRet = SMT_Dongle_Initial(2, SingInfoData);
            if (iRet < 0)
            {
                SMT_Dongle_Stop();
                throw new SignPadNotAvailableException("사인패드 초기화 실패!! 사인패드 연결을 해제 후, 다시 연결하고 시도해 주세요!");
            }
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

            string signImgPath = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".jpg");

            var fi = new FileInfo(signImgPath);

            if (fi.Directory.Exists)
                fi.Directory.Create();

            int signRetVal;

            try
            {
                StringBuilder sbSign = new StringBuilder(new String('\0', 2048));
                StringBuilder sbHash = new StringBuilder(new String('\0', 128));
                StringBuilder sbInfo = new StringBuilder(new String('\0', 64));
                StringBuilder sbLine1 = new StringBuilder("동의서 서명", 16);
                StringBuilder sbLine2 = new StringBuilder(new String('\0', 16));
                StringBuilder sbLine3 = new StringBuilder(new String('\0', 16));
                StringBuilder sbLine4 = new StringBuilder(new String('\0', 16));

                signRetVal = SMT_Get_Sign_Screen_Free(
                    WORKING_KEY,
                    KEY_INDEX,
                    sbLine1,
                    sbLine2,
                    sbLine3,
                    sbLine4,
                    sbSign,
                    sbInfo,
                    sbHash,
                    signImgPath);
            }
            catch { throw new SignFailException(); }
            finally { SMT_Dongle_Stop(); }

            if (signRetVal < 0)
            {
                throw new SignFailException();
            }

            return signImgPath;
        }
    }
}
