using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkingWithPIAFSDK
{
    class BenchmarkTimer : IDisposable
    {
        private const long MB = 1024 * 1024;

        private readonly string _name;
        private readonly Stopwatch _stopwatch;
        private readonly long _virtualBytes;
        private readonly int _gen0CollectionCount;
        private readonly TimeSpan _totalProcessorTime;
        

        public BenchmarkTimer(string name)
        {
            _name = name;
            GC.Collect();
            var process = Process.GetCurrentProcess();
            _virtualBytes = process.VirtualMemorySize64;
            _totalProcessorTime = process.TotalProcessorTime;
            _gen0CollectionCount = GC.CollectionCount(0);
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            var gen0CollectionCount = GC.CollectionCount(0);
            var process = Process.GetCurrentProcess();
            var virtualBytes = process.VirtualMemorySize64;
            var totalProcessorTime = process.TotalProcessorTime;

            Console.WriteLine("{0} in {1:N0} ms with: {2:N0} ms CPU time, grew {3:N0} MB and {4:N0} collections",
                _name, _stopwatch.ElapsedMilliseconds, (totalProcessorTime - _totalProcessorTime).TotalMilliseconds, (virtualBytes - _virtualBytes) / MB, (gen0CollectionCount - _gen0CollectionCount));
        }
    }
}
