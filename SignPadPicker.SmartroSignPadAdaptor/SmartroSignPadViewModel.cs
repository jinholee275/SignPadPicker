using AxSMTSIGNENCDECOCXLib;
using SignPadPicker.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Ink;

namespace SignPadPicker.Adaptor
{
    public class SmartroSignPadViewModel : NotifyPropertyImpl
    {
        #region Properties

        public Window Owner { get; set; }

        public SignResult Result { get; set; }

        private readonly AxSMTSignEncDecOcx axSMTSignEncDecOcx;

        private WindowsFormsHost signPad = new WindowsFormsHost();
        public WindowsFormsHost SignPad { get => signPad; set { signPad = value; OnPropertyChanged(() => SignPad); } }

        private StrokeCollection _strokes = new StrokeCollection();
        public StrokeCollection Strokes { get => _strokes; set { _strokes = value; OnPropertyChanged(() => Strokes); } }

        #endregion

        public SmartroSignPadViewModel()
        {
            try
            {
                axSMTSignEncDecOcx = new AxSMTSignEncDecOcx();
                axSMTSignEncDecOcx.CreateControl();
                SignPad.Child = axSMTSignEncDecOcx;
            }
            catch
            {
                axSMTSignEncDecOcx = null;
            }
        }

        internal void Activate()
        {
            if (axSMTSignEncDecOcx == null)
            {
                Result = new SignResult
                {
                    Exception = new SignPadNotAvailableException(),
                };

                Owner.DialogResult = false;
                return;
            }
        }
    }
}
