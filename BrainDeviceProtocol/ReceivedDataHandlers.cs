﻿using System;
using System.Collections.Generic;

namespace BrainDeviceProtocol
{
    public class SampleDataHandler : IReceivedDataProcessor
    {
        public byte FuncId => 1;

        public void Process(ArraySegment<byte> data)
        {
            var count = data.Count;
            var buf = data.Array;
            const int leastLen = 1 + 1 + 3 + 3 + 3;
            if (buf == null || count < leastLen)
            {
                AppLogger.Log("corruted sample data");
                return;
            }

            var startIdx = data.Offset;
            startIdx++;
            var order = buf[startIdx];
            startIdx++;
            var chan1 = new ArraySegment<byte>(buf, startIdx, 3);
            startIdx += 3;
            var chan2 = new ArraySegment<byte>(buf, startIdx, 3);
            startIdx += 3;
            var chan3 = new ArraySegment<byte>(buf, startIdx, 3);
            startIdx += 3;

            var endInd = data.Offset + count;
            var extraBlockCount = (count - leastLen + 2) / 3;
            var extraBlocks = new List<ArraySegment<byte>>(extraBlockCount);
            for (var i = 0; i < extraBlockCount; i++)
            {
                if (startIdx + 3 <= endInd)
                    extraBlocks[i] = new ArraySegment<byte>(buf, startIdx, 3);
                else
                    extraBlocks[i] = new ArraySegment<byte>(buf, startIdx, data.Offset + count - startIdx);

                startIdx += 3;
            }
            Console.WriteLine(extraBlocks.Show());
        }
    }

    public class SetSampleRateHandler : IReceivedDataProcessor
    {
        public byte FuncId => 11;

        public void Process(ArraySegment<byte> data)
        {
            var count = data.Count;
            var buf = data.Array;
            const int leastLen = 1 + 1;
            if (buf == null || count < leastLen)
            {
                AppLogger.Log("corruted sample data");
                return;
            }

            var startIdx = data.Offset;
            var flag = buf[startIdx + 1];
            Console.WriteLine(flag.Show());
        }
    }
}