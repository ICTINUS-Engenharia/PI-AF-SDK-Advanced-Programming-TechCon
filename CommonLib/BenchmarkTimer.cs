using OSIsoft.AF;
using OSIsoft.AF.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CommonLib
{
    public class BenchmarkTimer : IDisposable
    {
        private const long MB = 1024 * 1024;

        private readonly PISystem _piSystem;
        private readonly string _name;
        private readonly Stopwatch _stopwatch;
        private readonly long _virtualBytes;
        private readonly int _gen0CollectionCount;
        private readonly TimeSpan _totalProcessorTime;
        private readonly AFRpcMetric[] _metrics;

        public BenchmarkTimer(string name)
        {
            PISystems piSystems = new PISystems();
            _piSystem = piSystems.DefaultPISystem;

            _name = name;
            _metrics = _piSystem.GetClientRpcMetrics();
            var process = Process.GetCurrentProcess();
            _virtualBytes = process.VirtualMemorySize64;

            try
            {
                _totalProcessorTime = process.TotalProcessorTime;
            }
            catch { _totalProcessorTime = TimeSpan.MinValue; }

            GC.Collect(2, GCCollectionMode.Forced, blocking: true);
            _gen0CollectionCount = GC.CollectionCount(0);
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            var gen0CollectionCount = GC.CollectionCount(0);
            var process = Process.GetCurrentProcess();
            var virtualBytes = process.VirtualMemorySize64;

            TimeSpan totalProcessorTime;
            try
            {
                totalProcessorTime = process.TotalProcessorTime;
            }
            catch { totalProcessorTime = TimeSpan.MinValue; }

            string totalProcessorTimeString;
            if (totalProcessorTime == TimeSpan.MinValue || _totalProcessorTime == TimeSpan.MinValue)
                totalProcessorTimeString = "NA";
            else
                totalProcessorTimeString = (totalProcessorTime - _totalProcessorTime).TotalMilliseconds.ToString();

            AFRpcMetric[] newMetrics = _piSystem.GetClientRpcMetrics();
            IList<AFRpcMetric> diffMetrics = AFRpcMetric.SubtractList(newMetrics, _metrics);
            Console.WriteLine(_name);
            Console.WriteLine("   in {1:N0} ms with: {2:N0} ms CPU time, grew {3:N0} MB and {4:N0} collections",
                _name, _stopwatch.ElapsedMilliseconds, totalProcessorTimeString, (virtualBytes - _virtualBytes) / MB, (gen0CollectionCount - _gen0CollectionCount));
            if (null != diffMetrics && diffMetrics.Count > 0)
            {
                Console.WriteLine("   RPC Metrics to AF Server:");
                Console.WriteLine("{0,30} | {1,7} | {2,12} | {3,10}", "_RPC_", "_Count_", "_Total (ms)_", "_Average (ms/call)_");
                foreach (AFRpcMetric item in diffMetrics)
                {
                    Console.WriteLine("{0,30} | {1,7} | {2,12} | {3,10}",
                        item.Name, item.Count.ToString("N0"), item.Milliseconds.ToString("F1"), item.MillisecondsPerCall.ToString("F3"));

                }
            }
        }
    }
}
