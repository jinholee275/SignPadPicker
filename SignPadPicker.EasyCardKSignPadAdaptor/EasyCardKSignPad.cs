﻿using SignPadPicker.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using WinHttp;

namespace SignPadPicker.Adaptor
{
    public class EasyCardKSignPad : ISignPadPlugin
    {
        public string Name => "EasyCardKSignPad";

        public string Description => "EasyCardKSignPad Plugin";

        public bool IsAvailable => throw new NotImplementedException();

        #region DllImports

        [DllImport(@"kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder refVal, int size, string filePath);

        #endregion

        public string Activate()
        {
            if (!RunSignPadAppIfNotRunning())
            {
                throw new SignPadNotInstalledException("Kicc 프로그램 설치가 필요합니다. \r\n1. EasyCardK \r\n2. EzMSR \r\n설치경로 : C:\\Kicc");
            }

            string signData = OnSignPad();

            return SaveSignDataDB(signData);
        }

        /// <summary>
        /// name         : 서명패드 활성화
        /// desc         : Ez카드 프로그램에게 전문을 보내서 서명패드를 활성화시킨다.
        ///                서명요청 보내기 ( EzCard2 프로그램 설치 및 값 셋팅 필수 )
        /// author       : 이상현
        /// create date  : 2018-02-20 
        /// </summary>
        private string OnSignPad()
        {
            string port = GetPort();

            if (string.IsNullOrEmpty(port))
            {
                throw new SignPadNotAvailableException("포트번호를 확인해주세요");
            }

            // 변경금지
            string cb = "callback=jsonp12345678983543344";
            string req = "REQ=SR^^^^^^^^^^^30^^^^^^^^^^^^^^^^^^^^^";
            string url = $"http://127.0.0.1:{port}/?{cb}&{req}";

            WinHttpRequest winHttp = new WinHttpRequest();

            //winHttp.SetTimeouts(60000, 60000, 60000, 60000);

            winHttp.Open("POST", url, true); //뒤에 bool값이 동기/비동기방식을 나타냄(Async)
            winHttp.Send("");
            winHttp.WaitForResponse(120000); //120초후 timeout

            object resp = winHttp.ResponseBody;

            if (resp == null)
            {
                throw new SignPadNotAvailableException();
            }

            string sentence = Encoding.Default.GetString((byte[])resp);
            string[] delimiters = { "'SUC':", "'" };
            string[] wordsSplit = sentence.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            string suc = wordsSplit[1];

            // 서명패드 요청 실패
            if (suc != "00")
            {
                throw new SignFailException("서명 취소 또는 시간이 초과되었습니다.");
            }

            // 서명데이터가 넘어왔을때
            if (wordsSplit[5].Substring(0, 1) != "$")
            {
                throw new SignFailException("서명 취소 또는 시간이 초과되었습니다.");
            }

            return wordsSplit[5];
        }

        /// <summary>
        /// name         : 서명데이터 이미지 저장
        /// desc         : 서명데이터를 받아서 이미지 파일로 변환하여 저장 ( 서명패드가 없는경우 )
        /// author       : 이상현
        /// create date  : 2018-02-20 
        /// </summary>
        /// <returns>저장한 서명파일경로</returns>
        public string SaveSignDataDB(string sTextData)
        {
            if (string.IsNullOrEmpty(sTextData))
            {
                throw new EmptySignException();
            }

            // 임시 파일명 생성
            string filePath = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".bmp");
            string cb = "callback=jsonp12345678983543342";
            string url = $"http://127.0.0.1:{GetPort()}/?{cb}&REQ=BM^{filePath}^0^{sTextData}";

            WinHttpRequest winHttp = new WinHttpRequest();

            try
            {
                winHttp.Open("POST", url, true); //뒤에 bool값이 동기/비동기방식을 나타냄(Async)
                winHttp.Send("");
                winHttp.WaitForResponse(300000); //30초후 timeout

                object resp = winHttp.ResponseBody;

                if (resp == null)
                {
                    RequestCancel();
                    throw new SignPadNotAvailableException();
                }

                return filePath;
            }
            catch (Exception)
            {
                RequestCancel();
                throw new SignFailException("서명 이미지 변환에 실패했습니다.");
            }
        }

        private void RequestCancel()
        {
            WinHttpRequest winHttp = new WinHttpRequest();

            string cb = "callback=jsonp12345678983543342";
            string url = $"http://127.0.0.1:{GetPort()}/?{cb}&REQ=CC^"; //실행 취소

            winHttp.Open("POST", url, true); //뒤에 bool값이 동기/비동기방식을 나타냄(Async)
            winHttp.Send("");
            winHttp.WaitForResponse(300000); //30초후 timeout
        }

        /// <summary>
        /// LSH20180509 파일이 실행중인지 체크하는 메서드
        /// 2018-05-09, 이상현B, 파일명 리스트를 받아 현재 프로세스에서 실행중인지 체크하여 실행중이 아닐경우 실행해줌
        /// 원래 공통으로 빼려고 했으나..kicc 프로그램만 사용..
        /// </summary>
        /// <returns></returns>
        private bool RunSignPadAppIfNotRunning()
        {
            Dictionary<string, string> files = new Dictionary<string, string>
            {
                { "EasyCard", "C:\\Kicc\\EasyCardK\\EasyCard.exe" },
                { "EzMSR", "C:\\Kicc\\EzMSR2\\EzMSR.exe" }
            };

            var runningProcesses = Process.GetProcesses().Select(p => p.ProcessName);
            var filesNotRunning = files.Keys.Except(runningProcesses).ToList();

            try
            {
                filesNotRunning.ForEach(key => Process.Start(files[key]));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// name         : 포트번호 Get
        /// desc         : C:\Kicc\EasyCard2\Setup.Ini에 설정된 포트번호를 가져온다.
        /// author       : 이상현
        /// create date  : 2018-02-20 
        /// <returns></returns>
        /// </summary>
        private string GetPort()
        {
            StringBuilder retPort = new StringBuilder();

            // 서명패드 PORT가 설정된 ini파일 읽어오기
            //GetPrivateProfileString("KICC", "HTTPPORT", "", retPort, 32, @"C:\Kicc\EasyCard2\Setup.Ini");
            // EasyCard2 > EasyCardK로 프로그램 변경되면서 경로 및 파일이 수정됨  ( LSH20180611 ) -------------- S

            if (string.IsNullOrEmpty(retPort.ToString()))
            {
                GetPrivateProfileString("SETUP", "HTTP_PORT", "", retPort, 32, @"C:\Kicc\EasyCardK\SETUP\OPTION.ini");
            }

            if (string.IsNullOrEmpty(retPort.ToString()))
            {
                retPort.Append("8090");
            }

            return retPort.ToString();
        }
    }
}