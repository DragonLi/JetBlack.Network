using System;
using System.Collections.Generic;

namespace BrainDeviceProtocol
{
    public interface IReceivedDataProcessor
    {
        byte FuncId { get; }
        void Process(ArraySegment<byte> data);
    }

    public sealed class ReceivedDataProcessor
    {
        public static readonly ReceivedDataProcessor Instance = new ReceivedDataProcessor();

        private readonly Dictionary<byte, IReceivedDataProcessor> _processorMap;
        
        private ReceivedDataProcessor()
        {
            _processorMap = new Dictionary<byte, IReceivedDataProcessor>();
            foreach (var processor in ReflectionHelper.GetAllInterfaceImpl<IReceivedDataProcessor>())
            {
                _processorMap.Add(processor.FuncId,processor);
            }
        }

        public void AddProcessor(IReceivedDataProcessor processor)
        {
            _processorMap.Add(processor.FuncId,processor);
        }

        public bool Process(ArraySegment<byte> data)
        {
            if (data.Array == null)
            {
                AppLogger.Log("invalid data");
                return false;
            }
            if (data.Count <= 0)
            {
                AppLogger.Log("invalid data");
                return false;
            }

            var funcId = data.Array[data.Offset];
            if (!_processorMap.TryGetValue(funcId, out var processor))
            {
                AppLogger.Log($"function id not register:{funcId}");
                return false;
            }
            processor.Process(data);
            return true;
        }
    }
}