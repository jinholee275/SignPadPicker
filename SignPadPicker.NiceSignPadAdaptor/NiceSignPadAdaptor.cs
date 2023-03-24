using AxSIGNPOSLib;
using System;
using System.IO;

namespace SignPadPicker.Adaptor
{
    public class NiceSignPadAdaptor
    {
        public AxSignPos AxSignPos { get; } = new AxSignPos();

        public string DestFileName { get; set; }

        private readonly short _port;

        public NiceSignPadAdaptor(short port = 3)
        {
            _port = port;
        }

        public int OpenPort() => AxSignPos.OpenPort(_port, 115200); // com port 및 속도 세팅.

        public int ClosePort() => AxSignPos.ClosePort();

        public string Activate()
        {
            DestFileName = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".jpg");

            AxSignPos.NICESIGNPOS(0x36, DestFileName);
            AxSignPos.NICESIGNPOS(0x42, "                             ");

            return DestFileName;
        }

        public bool Result
        {
            get
            {
                int k = AxSignPos.ArrvData; // default : 2

                //서명 최종 완료 후 마지막으로 데이터를 처리하는 로직
                if (k == 1) // 서명 데이터 도착
                {
                    _ = AxSignPos.get_RetSignData(1);

                    // 전달 된 서명데이터는 앞네자리를 잘라서, 길이를 계산하고, 네자리를 제외한 나머지 서명데이터를
                    // Pos_Send 함수의 서명input에 세팅하면 된다.
                    // 서명 최대길이 2096

                    // 서명 사용할 경우에 변경되는 내용(일반 거래에 비해서)
                    // 1. 신용 승인 및 취소 모두 서명 사용.
                    // 2. 전문 헤더 길이 변경(295+서명길이)
                    // 3. 기존 전문 + "Y" + (서명길이 + 34) + "1" + (Catid + 일련번호4자리 + 카드번호 ) 등 추가됨.
                    //MessageBox.Show(signdata);

                    return true;
                }
                else // if (k < 1) // 각종 에러 발생
                {
                    return false;
                }
            }
        }
    }
}
