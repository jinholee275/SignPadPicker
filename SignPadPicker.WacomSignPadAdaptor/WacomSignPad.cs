using Florentis;
using SignPadPicker.Exceptions;
using System;
using System.Configuration;
using System.IO;
using System.Windows.Media.Imaging;

namespace SignPadPicker.WacomSignPadAdaptor
{
    public class WacomSignPad : ISignPadPlugin
    {
        public string Name => "WacomSignPad";

        public string Description => "WacomSignPad Plugin";

        public bool IsAvailable => throw new NotImplementedException();

        public bool IsPhysicalDevice => throw new NotImplementedException();

        private string License => ConfigurationManager.AppSettings["SignPadPicker.WacomSignPadAdaptor.License"] ?? "";

        public string Activate()
        {
            SignPadConfig config = new SignPadConfig
            {
                License = License,
            };

            return Activate(config);
        }

        public string Activate(SignPadConfig config)
        {
            SigCtl sigCtl = new SigCtl
            {
                Licence = config.License
            };

            DynamicCapture dc = new DynamicCaptureClass();
            DynamicCaptureResult res = dc.Capture(sigCtl, " ", " ", null, null);

            if (res != DynamicCaptureResult.DynCaptOK)
            {
                switch (res)
                {
                    case DynamicCaptureResult.DynCaptCancel:
                        throw new SignCancelException();
                    case DynamicCaptureResult.DynCaptPadError:
                        throw new SignPadNotAvailableException();
                    case DynamicCaptureResult.DynCaptOK:
                    case DynamicCaptureResult.DynCaptError:
                    case DynamicCaptureResult.DynCaptNotLicensed:
                    case DynamicCaptureResult.DynCaptAbort:
                    case DynamicCaptureResult.DynCaptIntegrityKeyInvalid:
                    default:
                        throw new SignFailException();
                }
            }

            try
            {
                string filePath = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".bmp");

                SigObj sigObj = (SigObj)sigCtl.Signature;

                _ = sigObj.RenderBitmap(
                    filePath,
                    200,
                    150,
                    "image/png",
                    1,
                    0x000000,
                    0xffffff,
                    5.0f,
                    5.0f,
                    RBFlags.RenderOutputFilename |
                    RBFlags.RenderColor32BPP |
                    RBFlags.RenderEncodeData);

                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(filePath, UriKind.Absolute);
                src.EndInit();

                return filePath;
            }
            catch (Exception ex)
            {
                throw new SignFailException(ex.Message);
            }
        }
    }
}
