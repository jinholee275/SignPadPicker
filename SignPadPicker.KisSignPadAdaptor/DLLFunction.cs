using System.Runtime.InteropServices;

namespace SignPadPicker.Adaptor
{
    class DLLFunction
    {
        public enum STATE_CODE { STATE_INIT, STATE_REQ, STATE_WAIT, STATE_RECV, STATE_ERROR, STATE_CANCEL, STATE_TIMEOUT };
        public enum WAIT_TYPE { WAIT_SYNC, WAIT_ASYNC, WAIT_EVENT }

        public delegate void OnKisDongleDllEnd(int nState);

        [DllImport("KisDongleDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int Dongle_Config(string sKey, string sValue);

        [DllImport("KisDongleDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int Dongle_Approval(int nPortNo, int nBaudRate, string sCommandID, string inSendData, int nSendDataLen);

        [DllImport("KisDongleDll.dll")]
        public extern static int Dongle_State();

        [DllImport("KisDongleDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int Dongle_GetData([MarshalAs(UnmanagedType.LPArray)] byte[] outRecvData);

        [DllImport("KisDongleDll.dll")]
        public extern static int Dongle_Init();

        [DllImport("KisDongleDll.dll")]
        public extern static void Dongle_Release();

        [DllImport("KisDongleDll.dll")]
        public extern static void Dongle_Stop();

        [DllImport("KisDongleDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int Dongle_Wait(int nType, int nTimeout);
        //public extern static int Dongle_Wait(int nType, int nTimeout, OnKisDongleDllEnd Callback);

        [DllImport("KisPosDll.dll")]
        public extern static int NACF_Approval_VANKEY(string inIPAddress, int inPortNo, string inSendData, int inSendDataLen, string inSignGubun, string inSignData, int inSignDataLen, [MarshalAs(UnmanagedType.LPArray)] byte[] outRecvData);
    }
}
