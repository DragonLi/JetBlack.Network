using System;

namespace BrainDeviceProtocol
{
    public enum DevCommandEnum
    {
        Start,
        Stop,
        SetSampleRate,
    }

    public sealed partial class DevCommandSender
    {
        #region Start Command
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
            ExecCmd(DevCommandEnum.Start);
        }
        #endregion

        #region Stop Command
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
            ExecCmd(DevCommandEnum.Stop);
        }
        #endregion

        #region Set Sampling Rate Command
        public class FillSetSampleRateCommadContent : ICommandContent
        {
            public DevCommandEnum CmdName => DevCommandEnum.SetSampleRate;
            public int CntSize => 2;
            public byte FuncId => 11;

            public void FillCnt(byte[] buffer, object[] args)
            {
                var rate = (SampleRateEnum) args[0];
                byte rateB = 0;
                switch (rate)
                {
                    case SampleRateEnum.SPS_250:
                        rateB = 1;
                        break;
                    case SampleRateEnum.SPS_500:
                        rateB = 2;
                        break;
                    case SampleRateEnum.SPS_1k:
                        rateB = 3;
                        break;
                    case SampleRateEnum.SPS_2k:
                        rateB = 4;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                buffer[1] = rateB;
            }
        }

        public enum SampleRateEnum
        {
            SPS_250,
            SPS_500,
            SPS_1k,
            SPS_2k,
        }
        
        public void SetSampleRate(SampleRateEnum sampleRate)
        {
            ExecCmd(DevCommandEnum.SetSampleRate, sampleRate);
        }
        #endregion
    }
}