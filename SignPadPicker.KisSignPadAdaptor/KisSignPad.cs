using SignPadPicker.Exceptions;
using SignPadPicker.Extensions;
using System;
using System.Configuration;
using System.Text;

namespace SignPadPicker.Adaptor
{
    public class KisSignPad : ISignPadPlugin
    {
        public string Name => "KisSignPad";

        public string Description => "KisSignPad Plugin";

        public bool IsAvailable
        {
            get
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("1".PadRight(2, ' ')); //시간(초)
                    sb.Append(string.Empty.PadRight(16, ' '));
                    sb.Append("연결 중 ...".PadRight(16, ' '));
                    sb.Append(string.Empty.PadRight(16, ' '));
                    sb.Append(string.Empty.PadRight(16, ' '));

                    ASYNC_Approval(ComPort, ComSpeed, "ED", sb.ToString(), sb.Length);

                    return true;
                }
                catch
                {
                    return false;
                }
                finally
                {
                    DLLFunction.Dongle_Release();
                }
            }
        }

        public bool IsPhysicalDevice => true;

        private const string TEMP_SIGN_FILE_PATH = @"C:\KIS\Sign.bmp";
        private int ComPort => Convert.ToInt32(ConfigurationManager.AppSettings["SignPadPicker.KisSignPadAdaptor.ComPort"].IfEmptyReplace(null) ?? "8");
        private int ComSpeed => Convert.ToInt32(ConfigurationManager.AppSettings["SignPadPicker.KisSignPadAdaptor.ComSpeed"].IfEmptyReplace(null) ?? "57600");
        private string Message1 => ConfigurationManager.AppSettings["SignPadPicker.KisSignPadAdaptor.Message1"] ?? "";
        private string Message2 => ConfigurationManager.AppSettings["SignPadPicker.KisSignPadAdaptor.Message2"] ?? "";
        private string Message3 => ConfigurationManager.AppSettings["SignPadPicker.KisSignPadAdaptor.Message3"] ?? "";
        private string Message4 => ConfigurationManager.AppSettings["SignPadPicker.KisSignPadAdaptor.Message4"] ?? "";

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
            //! 전문에 입력할 길이제한으로 `Path.GetTempPath()` 사용불가
            string filePath = TEMP_SIGN_FILE_PATH;

            StringBuilder sb = new StringBuilder();
            sb.Append("0".PadLeft(8, ' ')); //금액 8자리
            sb.Append(filePath.PadRight(100, ' '));
            sb.Append(config.Message1.PadRight(16, ' '));
            sb.Append(config.Message2.PadRight(16, ' '));
            sb.Append(config.Message3.PadRight(16, ' '));
            sb.Append(config.Message4.PadRight(16, ' '));

            ASYNC_Approval(config.ComPort, config.ComSpeed, "EA", sb.ToString(), sb.Length);

            return filePath;
        }

        /// <summary>
        /// 비동기 처리 방식
        /// </summary>
        /// <param name="nPortNo"></param>
        /// <param name="nBaudRate"></param>
        /// <param name="sCommandID"></param>
        /// <param name="inSendData"></param>
        /// <param name="nSendDataLen"></param>
        private void ASYNC_Approval(int nPortNo, int nBaudRate, string sCommandID, string inSendData, int nSendDataLen)
        {
            bool isWait = true;
            int state = 0;
            byte[] recvData = new byte[2048];

            int nRet = DLLFunction.Dongle_Init();

            if (nRet != 0)
            {
                DLLFunction.Dongle_Release();
                throw new SignPadNotAvailableException();
            }

            _ = DLLFunction.Dongle_Config("UI", "Y");

            nRet = DLLFunction.Dongle_Approval(nPortNo, nBaudRate, sCommandID, inSendData, nSendDataLen);

            if (nRet != 0)
            {
                DLLFunction.Dongle_Release();
                throw new SignPadNotAvailableException();
            }

            DLLFunction.Dongle_Wait(1, 20000);

            if (nRet != 0)
            {
                DLLFunction.Dongle_Release();
                throw new SignPadNotAvailableException();
            }

            while (isWait)
            {
                Delay(500);

                state = DLLFunction.Dongle_State();
                isWait = state < (int)DLLFunction.STATE_CODE.STATE_RECV;
            }

            DLLFunction.STATE_CODE stateObj = (DLLFunction.STATE_CODE)Enum.ToObject(typeof(DLLFunction.STATE_CODE), state);

            switch (stateObj)
            {
                case DLLFunction.STATE_CODE.STATE_ERROR:
                    DLLFunction.Dongle_Release();
                    throw new SignPadNotAvailableException();
                case DLLFunction.STATE_CODE.STATE_CANCEL:
                case DLLFunction.STATE_CODE.STATE_TIMEOUT:
                    DLLFunction.Dongle_Release();
                    throw new SignFailException();
                case DLLFunction.STATE_CODE.STATE_RECV:
                    int recvDataLen = DLLFunction.Dongle_GetData(recvData);

                    if (recvDataLen <= 0)
                    {
                        DLLFunction.Dongle_Release();
                        throw new SignFailException(GetErrorMessage(recvDataLen));
                    }

                    string strcode_gubn = Encoding.Default.GetString(recvData, 0, 2);

                    if (!strcode_gubn.Equals("00"))
                    {
                        DLLFunction.Dongle_Release();
                        throw new SignFailException(ResCodeToMsg(strcode_gubn));
                    }

                    _ = Encoding.Default.GetString(recvData, 2, recvDataLen - 2);
                    DLLFunction.Dongle_Release();
                    break;
                default:
                    DLLFunction.Dongle_Release();
                    break;
            }
        }

        void Delay(int ms)
        {
            DateTime dateTimeNow = DateTime.Now;

            TimeSpan duration = new TimeSpan(0, 0, 0, 0, ms);

            DateTime dateTimeAdd = dateTimeNow.Add(duration);

            while (dateTimeAdd >= dateTimeNow)
            {
                System.Windows.Forms.Application.DoEvents();
                dateTimeNow = DateTime.Now;
            }
        }

        private string ResCodeToMsg(string strcode)
        {
            switch (strcode)
            {
                case "70": return "사용불가카드";
                case "8C": return "IC 카드 APDU 응답 오류";
                case "8D": return "거래 조건이 맞지 않음";
                case "90": return "재거래 요청";
                case "91": return "취소(오류복구) 요청";
                case "94": return "CA Key 오류";
                case "95": return "명령/파라미터 오류";
                case "96": return "지원되지 않는 카드";
                case "99": return "SAM 잔액 부족 오류";
                case "9A": return "잔액 부족 오류";
                case "9B": return "카드 인식 오류";
                case "9C": return "마지막 거래 내역 없음";
                case "9F": return "처리 불가 기능";
                case "C0": return "TMK 처리 에러";
                case "C1": return "TMK 버전 오류(개시거래 요청)";
                case "C3": return "비밀번호 오류";
                case "C4": return "비밀번호 잠김";
                case "C6": return "계좌 정보 오류";
                case "CC": return "망 취소 (호스트 승인 후 카드 거절)";
                case "CD": return "단말기나 POS가 취소 시";
                case "CE": return "카드 거래 거절";
                case "CF": return "IC EMV 거래 FALLBACK";
                case "D0": return "서명 값이 존재 하지 않음";
                case "D1": return "암호화 KEY가 존재하지 않음";
                case "D2": return "단말기 ID가 일치하지 않음";
                case "E1": return "MSR 동작 오류";
                case "E2": return "IFM  동작 오류";
                case "E4": return "PMF 검증 오류";
                case "E6": return "Safecard Key 일련번호 불일치(최초) PMF Index불일치(갱신)";
                case "E8": return "IC 거래 우선 요망";
                case "E9": return "FALLBACK 거래 아님";
                case "EC": return "Safecard Key 일련번호가 없음(공장초기화 안됨)";
                case "EE": return "다른 Application 카드";
                case "F2": return "카드가 존재하지 않음";
                case "F3": return "여러장의 카드가 입력됨";
                case "F5": return "지원되지 않는 카드";
                case "F6": return "동글에 sam 없음";
                case "F7": return "PayOn SAM 등록 오류";
                case "F8": return "요청 Message의 Data 오류";
                case "F9": return "등록된 PayOn Sam과 서로 상이";
                case "FA": return "Reader 인증 오류";
                case "FB": return "Reader 인증이 되지 않음";
                case "FC": return "다운로드 프로그램 무결성 훼손";
                case "FD": return "암호화 키 무결성 훼손";
                case "FF": return "실패 Fallback 미처리";
                default: return "미정의 코드";
            }
        }

        private string GetErrorMessage(int getDataResult)
        {
            switch (getDataResult)
            {
                case -1: return "[  -1]초기화오류";
                case -2: return "[  -2]CMD ID오류";
                case -3: return "[  -3]응답 완료 상태 아님";
                case -4: return "[  -4]CMD ID오류";
                case -5: return "[  -5]Release되어있지 않음";
                case -6: return "[  -6]스테이트 확인 할수 없음";
                case -7: return "[  -7]WaitType 오류";
                case -8: return "[  -8]EVENT방식이지만Callback함수NULL";
                case -21: return "[ -21]FAIL_READER_NOT_PORTNO, 포트번호오류";
                case -22: return "[ -22]FAIL_READER_PORT_OPEN, 포트오픈실패";
                case -23: return "[ -23]FAIL_READER_ACK";
                case -24: return "[ -24]FAIL_READER_RECV_DUMMYDATA";
                case -25: return "[ -25]FAIL_READER_INIT, 상호인증오류";
                case -26: return "[ -26]FAIL_READER_NAK";
                case -61: return "[ -61]FAIL_SIGNPAD_NOT_PORTNO";
                case -62: return "[ -62]FAIL_SIGNPAD_PORT_OPEN";
                case -63: return "[ -63]FAIL_SIGNPAD_ACK";
                case -64: return "[ -64]FAIL_SIGNPAD_RECV_DUMMYDATA";
                case -65: return "[ -65]FAIL_SIGNPAD_INIT";
                case -66: return "[ -66]FAIL_SIGNPAD_NAK";
                case -67: return "[ -67]FAIL_SIGNPAD_ESC";
                case -69: return "[ -69]서명 파일저장 오류";
                case -121: return "[-121]FAIL_PREPAID_NOT_PORTNO";
                case -122: return "[-122]FAIL_PREPAID_PORT_OPEN";
                case -123: return "[-123]Dongle 무반응";
                case -124: return "[-124]FAIL_READER_NOT_PORTNO";
                case -125: return "[-125]FAIL_PREPAID_INIT";
                case -126: return "[-126]FAIL_PREPAID_NAK";
                case 999: return "[ 999]DONGLE_CANCEL";
                default: return "[    ]미정의 코드";
            }
        }
    }
}
