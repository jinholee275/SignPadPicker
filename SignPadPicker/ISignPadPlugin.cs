namespace SignPadPicker
{
    public interface ISignPadPlugin
    {
        /// <summary>
        /// 플러그인 이름
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 플러그인 설명
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 장치가 연결된 상태인지 여부
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// 물리적인 장치인지 여부
        /// </summary>
        bool IsPhysicalDevice { get; }

        /// <summary>
        /// 서명패드를 활성화하여 서명을 받을 준비를 한다.
        /// </summary>
        /// <returns>서명한 이미지 파일의 경로</returns>
        string Activate();
    }
}
