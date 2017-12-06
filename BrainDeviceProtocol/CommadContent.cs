namespace BrainDeviceProtocol
{
    public enum DevCommandEnum
    {
        Start,
        Stop,
    }

    public sealed partial class DevCommandSender
    {
        public class FillStartCommadContent : ICommandContent
        {
            public DevCommandEnum CmdName => DevCommandEnum.Start;
            public int CntSize => 2;
            public byte FuncId => 1;

            public void FillCnt(byte[] buffer, object[] args)
            {
                buffer[1] = 1;
            }
        }

        public void Start()
        {
            ExecCmd(DevCommandEnum.Start, null);
        }

        public class FillStopCommadContent : ICommandContent
        {
            public DevCommandEnum CmdName => DevCommandEnum.Stop;
            public int CntSize => 2;
            public byte FuncId => 1;

            public void FillCnt(byte[] buffer, object[] args)
            {
                buffer[1] = 0;
            }
        }

        public void Stop()
        {
            ExecCmd(DevCommandEnum.Stop, null);
        }
    }
}