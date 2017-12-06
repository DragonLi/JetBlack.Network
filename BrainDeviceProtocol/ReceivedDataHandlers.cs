using System;

namespace BrainDeviceProtocol
{
    public class SampleDataHandler:IReceivedDataProcessor
    {
        public byte FuncId => 1;
        public void Process(ArraySegment<byte> data)
        {
            var count = data.Count;
            var buf = data.Array;
            if (buf == null || count < 1 + 1 + 3 + 3 + 3)
            {
                AppLogger.Log("corruted sample data");
                return;
            }
            var startIdx = data.Offset;
            var order = buf[startIdx + 1];
            var chan1=new ArraySegment<byte>(buf,startIdx+2,3);
            var chan2=new ArraySegment<byte>(buf,startIdx+2+3,3);
            var chan3=new ArraySegment<byte>(buf,startIdx+2+3+3,3);
            //TODO dynamic length channel datas
        }
    }
}