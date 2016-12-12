#region Copyright
//  Copyright 2016  OSIsoft, LLC
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
#endregion
using System;
using System.Diagnostics;
using OSIsoft.AF;
using OSIsoft.AF.Diagnostics;

namespace External
{
    public class AFProbe : IDisposable
    {
        private readonly string _name;
        private readonly PISystem _piSystem;
        private readonly Stopwatch _sw;

        private AFRpcMetric[] _startServer;
        private AFRpcMetric[] _startClient;

        public AFProbe(string name, PISystem piSystem)
        {
            _name = name;
            _piSystem = piSystem;

            _startServer = piSystem.GetRpcMetrics();
            _startClient = piSystem.GetClientRpcMetrics();

            _sw = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _sw.Stop();
            Console.WriteLine("Operation {0} took: {1} ms\n", _name, _sw.ElapsedMilliseconds);

            AFRpcMetric[] endClient = _piSystem.GetClientRpcMetrics();
            AFRpcMetric[] endServer = _piSystem.GetRpcMetrics();
            var diffClient = AFRpcMetric.SubtractList(endClient, _startClient);
            var diffServer = AFRpcMetric.SubtractList(endServer, _startServer);

            long numCalls = 0;
            Console.WriteLine("RPC Metrics");
            foreach (var clientMetric in diffClient)
            {
                foreach (var serverMetric in diffServer)
                {
                    if (clientMetric.Name == serverMetric.Name)
                    {
                        numCalls += clientMetric.Count;
                        Console.WriteLine("   {0}: {1} calls.  {2} ms/call on client. {3} ms/call on server.  Delta: {4} ms/call",
                            clientMetric.Name,
                            clientMetric.Count,
                            Math.Round(clientMetric.MillisecondsPerCall),
                            Math.Round(serverMetric.MillisecondsPerCall),
                            Math.Round(clientMetric.MillisecondsPerCall - serverMetric.MillisecondsPerCall));
                        break;
                    }
                }
            }
            Console.WriteLine();
            Console.WriteLine("Total RPCs: {0}", numCalls);
        }
    }
}
