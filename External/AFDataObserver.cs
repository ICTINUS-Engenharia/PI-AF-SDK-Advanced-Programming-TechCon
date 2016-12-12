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
using System.Threading;
using System.Threading.Tasks;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;

namespace External
{
    public delegate void ProcessAFDataPipeEventDelegate(AFDataPipeEvent evt);

    public class AFDataObserver : IObserver<AFDataPipeEvent>, IDisposable
    {
        // The list of attributes to monitor
        public AFAttributeList AttributeList { get; set; }

        // The underlying AFDataPipe that provides incoming values
        public AFDataPipe DataPipe { get; set; }

        // Interval to wait in between calling the data pipe
        private int _threadSleepTimeInMilliseconds;

        // The client provides this delegate to call during OnNext()
        private ProcessAFDataPipeEventDelegate _processEvent;

        public AFDataObserver(AFAttributeList attrList, ProcessAFDataPipeEventDelegate processEvent, int pollInterval = 5000)
        {
            AttributeList = attrList;
            DataPipe = new AFDataPipe();
            _threadSleepTimeInMilliseconds = pollInterval;
            _processEvent = processEvent;
        }

        public AFErrors<AFAttribute> Start()
        {
            // Subscribe this object (Observer) to the AFDataPipe (Observable)
            DataPipe.Subscribe(this);

            // The data pipe will provide updates from attributes inside AttributeList
            AFErrors<AFAttribute> errors = DataPipe.AddSignups(AttributeList);

            if (errors != null)
            {
                return errors;
            }
            else
            {
                // This task loop calls GetObserverEvents every 5 seconds
                Task mainTask = Task.Run(() =>
                {
                    bool hasMoreEvents = false;
                    while (true)
                    {
                        AFErrors<AFAttribute> results = DataPipe.GetObserverEvents(out hasMoreEvents);
                        if (!hasMoreEvents)
                            Thread.Sleep(_threadSleepTimeInMilliseconds);
                    }
                });
                return null;
            }
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { throw error; }

        public void OnNext(AFDataPipeEvent dpEvent)
        {
            // AFDataPipeEvent contains the AFValue representing the incoming event
            _processEvent(dpEvent);
        }

        public void Dispose()
        {
            DataPipe.Dispose();
            DataPipe = null;
        }
    }
}
