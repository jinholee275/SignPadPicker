using SignPadPicker.Exceptions;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Threading;

namespace SignPadPicker.Adaptor
{
    public class ScreenSignPadViewModel : NotifyPropertyImpl
    {
        #region Properties
        
        public Window Owner { get; set; }
        
        public SignResult Result { get; set; }

        private string confirmBtnContent = "저장";

        /// <summary>
        /// 확인 버튼 컨텐트
        /// </summary>
        public string ConfirmBtnContent { get => confirmBtnContent; set { confirmBtnContent = value; OnPropertyChanged(() => ConfirmBtnContent); } }

        private string clearBtnContent = "지우기";

        /// <summary>
        /// 지우기 버튼 컨텐트
        /// </summary>
        public string ClearBtnContent { get => clearBtnContent; set { clearBtnContent = value; OnPropertyChanged(() => ClearBtnContent); } }

        private string cancelBtnContent = "닫기";

        /// <summary>
        /// 취소 버튼 컨텐트
        /// </summary>
        public string CancelBtnContent { get => cancelBtnContent; set { cancelBtnContent = value; OnPropertyChanged(() => CancelBtnContent); } }

        private InkCanvas signPad;
        public InkCanvas SignPad { get => signPad; set { signPad = value; OnPropertyChanged(() => SignPad); } }

        private StrokeCollection _strokes = new StrokeCollection();
        public StrokeCollection Strokes { get => _strokes; set { _strokes = value; OnPropertyChanged(() => Strokes); } }

        #endregion

        #region Commands

        private ICommand confirmCommand;
        public ICommand ConfirmCommand
        {
            get
            {
                if (confirmCommand == null) confirmCommand = new RelayCommand(p => Confirm());
                return confirmCommand;
            }
        }

        private ICommand clearCommand;
        public ICommand ClearCommand
        {
            get
            {
                if (clearCommand == null) clearCommand = new RelayCommand(p => Clear());
                return clearCommand;
            }
        }

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

        public ScreenSignPadViewModel()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                SignPad = new InkCanvas
                {
                    EditingMode = InkCanvasEditingMode.Ink
                };

                SignPad.SetBinding(InkCanvas.StrokesProperty, new Binding("Strokes") { Source = this, Mode = BindingMode.TwoWay });
            }), DispatcherPriority.Input);
        }

        private void Confirm()
        {
            if (Strokes.Count == 0)
            {
                MessageBox.Show("서명 해주세요.");
                return;
            }

            string filePath = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".jpg");
            
            SignPad.ToSaveAsImage(filePath);
            
            Result = new SignResult
            {
                SignImgFilePath = filePath,
            };

            if (Owner != null) Owner.DialogResult = true;
        }

        private void Clear()
        {
            Strokes.Clear();
        }

        private void Close()
        {
            Result = new SignResult
            {
                Exception = new SignCancelException(),
            };

            if (Owner != null) Owner.DialogResult = false;
        }
    }
}
