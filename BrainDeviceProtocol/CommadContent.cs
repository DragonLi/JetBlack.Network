using System;

namespace BrainDeviceProtocol
{
    public enum DevCommandEnum
    {
        Start,
        Stop,
        SetSampleRate,
        SetTrap,
        SetFilter,
        QueryParam,
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

        #region Set Trap Command

        public class FillSetTrapCommadContent : ICommandContent
        {
            public DevCommandEnum CmdName => DevCommandEnum.SetTrap;
            public int CntSize => 2;
            public byte FuncId => 12;

            public void FillCnt(byte[] buffer, object[] args)
            {
                var trapOpt = (TrapSettingEnum) args[0];
                byte opt = 0;
                switch (trapOpt)
                {
                    case TrapSettingEnum.NoTrap:
                        opt = 0;
                        break;
                    case TrapSettingEnum.Trap_50:
                        opt = 10;
                        break;
                    case TrapSettingEnum.Trap_60:
                        opt = 11;
                        break;
                }
                buffer[1] = opt;
            }
        }

        public enum TrapSettingEnum
        {
            NoTrap,
            Trap_50,
            Trap_60,
        }

        public void SetTrap(TrapSettingEnum trapOption)
        {
            ExecCmd(DevCommandEnum.SetTrap, trapOption);
        }

        #endregion
        
        #region Set Filter Command

        public class FillSetFilterCommadContent : ICommandContent
        {
            public DevCommandEnum CmdName => DevCommandEnum.SetFilter;
            public int CntSize => 2;
            public byte FuncId => 13;

            public void FillCnt(byte[] buffer, object[] args)
            {
                var useFilter = (bool) args[0];
                buffer[1] = useFilter ? (byte)1 : (byte)0;
            }
        }

        public void SetFilter(bool useFilter)
        {
            ExecCmd(DevCommandEnum.SetFilter, useFilter);
        }

        #endregion
        
        #region Query Parameters Command

        public class FillQueryCommadContent : ICommandContent
        {
            public DevCommandEnum CmdName => DevCommandEnum.QueryParam;
            public int CntSize => 1;
            public byte FuncId => 21;

            public void FillCnt(byte[] buffer, object[] args)
            {
            }
        }

        public void QueryParam()
        {
            ExecCmd(DevCommandEnum.QueryParam);
        }

        #endregion
    }
}