﻿using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.Threading;
using JetBlack.Network.Common;

namespace BrainDeviceProtocol
{
    public interface ICommandContent
    {
        DevCommandEnum CmdName { get; }
        int CntSize { get; }
        byte FuncId { get; }
        bool DontCheckResponse { get; }
        bool ReponseHasErrorFlag { get; }
        void FillCnt(byte[] buffer, object[] args);
    }

    public sealed partial class DevCommandSender
    {
        private readonly IObserver<DisposableValue<ArraySegment<byte>>> _clientFrameSender;
        private readonly BufferManager _bufMgr;
        private readonly Dictionary<DevCommandEnum, ICommandContent> _cmdhandler;

        public DevCommandSender(IObserver<DisposableValue<ArraySegment<byte>>> clientFrameSender, BufferManager bufMgr)
        {
            _cmdhandler = new Dictionary<DevCommandEnum, ICommandContent>();
            foreach (var cmdcnt in ReflectionHelper.GetAllInterfaceImpl<ICommandContent>())
            {
                AddCommand(cmdcnt);
            }
            
            _clientFrameSender = clientFrameSender;
            _bufMgr = bufMgr;
            _cts = new CancellationTokenSource();
        }

        public void AddCommand(ICommandContent cnt)
        {
            _cmdhandler.Add(cnt.CmdName, cnt);
        }

        /*public void ExecCmd(DevCommandEnum cmd, params object[] args)
        {
            if (!_cmdhandler.TryGetValue(cmd, out var handler))
            {
                AppLogger.Log("Device Command Not Found:");
                return;
            }
            var buf = new RecycleBuffer(handler.CntSize, _bufMgr);
            var buffer = buf.Buffer;
            handler.FillCnt(buffer, args);
            buffer[0] = handler.FuncId;
            _clientFrameSender.OnNext(DisposableValue.Create(new ArraySegment<byte>(buffer), buf));
        }*/
    }
}