using AxSMTSIGNENCDECOCXLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace SignPadPicker.Adaptor
{
    /// <summary>
    /// SignPadE30P.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SignPadE30P : UserControl
    {
        private readonly AxSMTSignEncDecOcx axSMTSignEncDecOcx = new AxSMTSignEncDecOcx();

        // #BS2021030401:싸인패드오류 보완작업-한우리아이티 새모듈 적용 김병수 20210319 DEL START
        //[DllImport(@"C:\Hanuriit\MainDll.dll", CharSet = CharSet.Ansi)]
        //public static extern int GetSignData([In, Out] StringBuilder Msg1, [In, Out] StringBuilder Msg2, [In, Out] StringBuilder org_signdata, [In, Out] StringBuilder hanuriit_signdata);
        // #BS2021030401:싸인패드오류 보완작업-한우리아이티 새모듈 적용 김병수 20210319 DEL END

        bool bSignatureCall = false;//2021-03-04, 김수진, 한우리요청 소스추가

        public SignPadE30P()
        {
            InitializeComponent();

            try
            {
                axSMTSignEncDecOcx.CreateControl();
                this.signcopy.Child = axSMTSignEncDecOcx;
            }
            catch (Exception ex)
            {
                //MessageBoxEx.Show(null, ex.Message);
            }
        }


        /// <summary>
        /// 서명 요청
        /// </summary>
        public string OnSignImage(object owner)
        {
            // #BS2021030401:싸인패드오류 보완작업-한우리아이티 새모듈 적용 김병수 MOD
            string signImgPath = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".jpg");

            if (bSignatureCall)//2021-03-04, 김수진, 한우리요청 소스추가
                return signImgPath;

            bSignatureCall = true;
            //# 통신 환경설정은 한우리 Agent로 설정
            int i = 0;
            StringBuilder Msg1 = new StringBuilder("서명 요청");            // 요청 문구1
            StringBuilder Msg2 = new StringBuilder();                       // 요청 문구2
            StringBuilder org_signdata = new StringBuilder(" ", 5000);      // Smartro 서명 데이터
            StringBuilder hanuriit_signdata = new StringBuilder(" ", 5000); // 한우리 서명 데이터

            //FileManager.WriteTxtFile("LOG", string.Empty, "OnSignImage Start");

            try
            {
                //FileManager.WriteTxtFile("LOG", string.Empty, "GetSignData Befor");

                // #BS2021030401:싸인패드오류 보완작업-한우리아이티 새모듈 적용 김병수 20210319 MOD START
                String strMsg = "서명이미지 생성 작업 중";
                strMsg += Environment.NewLine + "SIGN_ComPort : " + SmartroSign.SIGN_ComPortStr;
                strMsg += Environment.NewLine + "SIGN_ComSpeed : " + SmartroSign.SIGN_ComSpeedInt.ToString();

                try
                {
                    //FileManager.WriteTxtFile("LOG", string.Empty, strMsg);

                    //# 서명정보 가져오기
                    //i = GetSignData(Msg1, Msg2, org_signdata, hanuriit_signdata);
                    i = SmartroSign.GetSign(SmartroSign.SIGN_ComPortInt, SmartroSign.SIGN_ComSpeedInt, "", "", "", "", @signImgPath);

                    ////# GetSignData 함수 호출 후, MsgBox.Display 함수 호출되면 오버플로우 오류 발생
                    ////# 강제로 Exception 발생시켜서 오버플로우 오류 제거
                    //try { throw new Exception(); }
                    //catch { }
                }
                catch (Exception ex)
                {
                    strMsg = "서명이미지 생성 중 오류발생 하였습니다.";
                    strMsg += Environment.NewLine + "SIGN_ComPort : " + SmartroSign.SIGN_ComPortStr;
                    strMsg += Environment.NewLine + "SIGN_ComSpeed : " + SmartroSign.SIGN_ComSpeedInt.ToString();
                    //MessageBoxEx.Show(strMsg, "확인", 500);
                }

                //FileManager.WriteTxtFile("LOG", string.Empty, "GetSignData After");

                ////# 0 이면 성공 나머지는 실패
                ////if (i == 0)
                //if (0 < i)//스마트로 모듈 0보다 크면 정상, 0보다 작으면 오류 발생.
                //{
                //    FileManager.WriteTxtFile("LOG", string.Empty, "SaveTextToImage Befor");

                //    //string signData = org_signdata.ToString();
                //    //signImgPath = Path.Combine(EnvironmentVariables.BitmapLocalPath, DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".jpg");
                //    //SaveTextToImage(signData, signImgPath);

                //    FileManager.WriteTxtFile("LOG", string.Empty, "SaveTextToImage After");
                //}
                //else
                //{
                //    MessageBoxEx.Show("서명요청 실패하였습니다. 재서명 요청하세요", "확인", 1000);
                //    FileManager.WriteTxtFile("LOG", string.Empty, "서명요청 실패하였습니다. 재서명 요청하세요.01");  // #B2020062203:전자동의서 서명시 산술연산오류 김병수 20200925 ADD
                //}

                if (0 > i)//스마트로 모듈 0보다 크면 정상, 0보다 작으면 오류 발생.
                {
                    //MessageBoxEx.Show("서명요청 실패하였습니다. 재서명 요청하세요", "확인", 1000);
                    //FileManager.WriteTxtFile("LOG", string.Empty, "서명요청 실패하였습니다. 재서명 요청하세요.01");  // #B2020062203:전자동의서 서명시 산술연산오류 김병수 20200925 ADD
                }
                // #BS2021030401:싸인패드오류 보완작업-한우리아이티 새모듈 적용 김병수 20210319 MOD END
            }
            catch (Exception ex)
            {
                // #B2020062203:전자동의서 서명시 산술연산오류 김병수 MOD START
                //FileManager.WriteTxtFile("LOG", string.Empty, ex.Message + "02");  // #B2020062203:전자동의서 서명시 산술연산오류 김병수 20200925 ADD
                //# Dll 조회 오류가 아닐 때만 메세지 호출
                //if (ex.HResult == -2146233052)
                //{
                //}
                //// 산술 연산에서 오버플로 또는 언더플로가 발생했습니다.
                //else if (ex.HResult == -2147024362)
                //{
                //    MessageBoxEx.Show("서명이미지 전송 중 입니다.", "확인", 1000);
                //}
                //else
                //{
                //    MessageBoxEx.Show(owner, ex.Message, "확인", 1000);
                //}
                // #B2020062203:전자동의서 서명시 산술연산오류 김병수 MOD END
            }
            finally
            {
                bSignatureCall = false;//2021-03-04, 김수진, 한우리요청 소스추가
            }

            //FileManager.WriteTxtFile("LOG", string.Empty, "OnSignImage End");

            return signImgPath;
        }

        /// <summary>
        /// 이미지 변환
        /// </summary>
        private BitmapImage SaveTextToImage(string sTextData, string path)
        {
            BitmapImage bi = new BitmapImage();

            int i = axSMTSignEncDecOcx.SMTDecSignData("00", "A943124BFCF7C4FB", sTextData, path);

            if (i == 0)
            {
                using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(path)))
                {
                    try
                    {
                        bi.BeginInit();
                        bi.StreamSource = ms;
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.EndInit();
                        bi.Freeze();

                        return bi;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }

            return null;
        }
    }
}


