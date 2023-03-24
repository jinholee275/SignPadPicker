using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SignPadPicker
{
    public static class VisualCaptureExtension
    {
        public static bool ToSaveAsImage(this FrameworkElement element, string path)
            => ToSaveAsImage(element, path, null);

        public static bool ToSaveAsImage(this FrameworkElement element, string path, System.Windows.Media.Brush background)
        {
            FileInfo fi = new FileInfo(path);

            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }

            var rtb = new RenderTargetBitmap((int)element.RenderSize.Width, (int)element.RenderSize.Height, 96d, 96d, PixelFormats.Default);
            var drawingVisual = new DrawingVisual();

            if (background != null)
            {
                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                    drawingContext.DrawRectangle(background, null, new Rect(0, 0, element.RenderSize.Width, element.RenderSize.Height));

                rtb.Render(drawingVisual);
            }

            rtb.Render(element);
            rtb.Freeze();

            var frame = BitmapFrame.Create(rtb);

            using (Stream stream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                var encoder = new JpegBitmapEncoder
                {
                    QualityLevel = 70
                };

                encoder.Frames.Add(frame);
                encoder.Save(stream);
            }

            return true;
        }
    }
}
