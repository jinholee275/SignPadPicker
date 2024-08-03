namespace SignPadPicker
{
    public class SignPadConfig
    {
        public int HttpPort { get; set; }

        public int ComPort { get; set; }

        public int ComSpeed { get; set; }

        public string Message1 { get; set; }

        public string Message2 { get; set; }

        public string Message3 { get; set; }

        public string Message4 { get; set; }

        public string ImgFilePath { get; set; }

        /// <summary>
        /// 창의 Left 위치
        /// 일반모드로 설정했을 경우만 작동한다.
        /// </summary>
        public double ScreenPositionLeft { get; set; } = double.NaN;

        /// <summary>
        /// 창의 Top 위치
        /// 일반모드로 설정했을 경우만 작동한다.
        /// </summary>
        public double ScreenPositionTop { get; set; } = double.NaN;

        /// <summary>
        /// 창의 Width 크기
        /// 일반모드로 설정했을 경우만 작동한다.
        /// </summary>
        public double ScreenSizeWidth { get; set; } = double.NaN;

        /// <summary>
        /// 창의 Height 크기
        /// 일반모드로 설정했을 경우만 작동한다.
        /// </summary>
        public double ScreenSizeHeight { get; set; } = double.NaN;

        /// <summary>
        /// 창의 상태를 최대 크기로 설정하는지 여부
        /// </summary>
        public bool ScreenIsMaximized { get; set; } = false;

        /// <summary>
        /// 펜 브러쉬 크기
        /// </summary>
        public double PenBrushSize { get; set; } = 5;
    }
}
