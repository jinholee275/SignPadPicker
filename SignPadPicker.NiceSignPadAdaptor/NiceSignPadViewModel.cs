using AxSIGNPOSLib;
using SignPadPicker.Exceptions;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Threading;

namespace SignPadPicker.Adaptor
{
    public class NiceSignPadViewModel : NotifyPropertyImpl
    {
        #region Properties

        public Window Owner { get; set; }

        public SignResult Result { get; set; }

        private const short PORT_NUMBER = 3;

        private string destFileName;

        private readonly AxSignPos axSignPos;

        private readonly DispatcherTimer _timer = new DispatcherTimer();

        private string cancelBtnContent = "닫기";

        /// <summary>
        /// 취소 버튼 컨텐트
        /// </summary>
        public string CancelBtnContent { get => cancelBtnContent; set { cancelBtnContent = value; OnPropertyChanged(() => CancelBtnContent); } }

        private WindowsFormsHost signPad = new WindowsFormsHost();
        public WindowsFormsHost SignPad { get => signPad; set { signPad = value; OnPropertyChanged(() => SignPad); } }

        private StrokeCollection _strokes = new StrokeCollection();
        public StrokeCollection Strokes { get => _strokes; set { _strokes = value; OnPropertyChanged(() => Strokes); } }

        #endregion

        #region Commands

        private ICommand cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (cancelCommand == null) cancelCommand = new RelayCommand(p => this.Close());
                return cancelCommand;
            }
        }

        #endregion

        public NiceSignPadViewModel()
        {
            try
            {
                axSignPos = new AxSignPos();
                axSignPos.CreateControl();
                SignPad.Child = axSignPos;
            }
            catch
            {
                axSignPos = null;
            }
        }

        internal void Activate()
        {
            if (axSignPos == null)
            {
                Result = new SignResult
                {
                    Exception = new SignPadNotAvailableException(),
                };

                Owner.DialogResult = false;
                return;
            }

            int result = axSignPos.OpenPort(PORT_NUMBER, 115200);

            if (result == 0)
            {
                Result = new SignResult
                {
                    Exception = new SignPadNotAvailableException(),
                };

                Owner.DialogResult = false;
                return;
            }

            destFileName = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".jpg");

            axSignPos.NICESIGNPOS(0x36, destFileName);
            axSignPos.NICESIGNPOS(0x42, "                             ");

            _timer.Tick += Timer_Tick;
            _timer.Interval = new TimeSpan(0, 0, 3);
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();

            int k = axSignPos.ArrvData; // default : 2
            bool result;

            //서명 최종 완료 후 마지막으로 데이터를 처리하는 로직
            if (k == 1) // 서명 데이터 도착
            {
                _ = axSignPos.get_RetSignData(1);

                // 전달 된 서명데이터는 앞네자리를 잘라서, 길이를 계산하고, 네자리를 제외한 나머지 서명데이터를
                // Pos_Send 함수의 서명input에 세팅하면 된다.
                // 서명 최대길이 2096

                // 서명 사용할 경우에 변경되는 내용(일반 거래에 비해서)
                // 1. 신용 승인 및 취소 모두 서명 사용.
                // 2. 전문 헤더 길이 변경(295+서명길이)
                // 3. 기존 전문 + "Y" + (서명길이 + 34) + "1" + (Catid + 일련번호4자리 + 카드번호 ) 등 추가됨.
                //MessageBox.Show(signdata);

                result = true;
            }
            else // if (k < 1) // 각종 에러 발생
            {
                result = false;
            }

            axSignPos.ClosePort();

            Result = new SignResult
            {
                SignImgFilePath = result ? destFileName : null,
            };

            Owner.DialogResult = result;
        }

        private void Close()
        {
            axSignPos.ClosePort();

            Result = new SignResult
            {
                Exception = new SignCancelException(),
            };

            Owner.DialogResult = false;
        }
    }
}
