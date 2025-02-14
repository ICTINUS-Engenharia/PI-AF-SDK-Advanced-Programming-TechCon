﻿#region Copyright
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
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using OSIsoft.AF.Time;

namespace Ex4_Asynchronous_Data_Access_Sln
{
    public class AFAsyncDataReader
    {
        public static async Task<IList<IDictionary<AFSummaryTypes, AFValue>>> GetSummariesAsync(AFAttributeList attributeList)
        {
            Console.WriteLine("Calling GetSummariesAsync\n");

            Task<IDictionary<AFSummaryTypes, AFValue>>[] tasks = attributeList
                // Do not make the call if async is not supported
                .Where(attr => (attr.SupportedDataMethods & AFDataMethods.Asynchronous) == AFDataMethods.Asynchronous)
                .Select(async attr =>
                {
                    try
                    {
                        AFSummaryTypes mySummaries = AFSummaryTypes.Minimum | AFSummaryTypes.Maximum | AFSummaryTypes.Average | AFSummaryTypes.Total;
                        AFTimeRange timeRange = new AFTimeRange(new AFTime("*-1d"), new AFTime("*"));
                    
                        return await attr.Data.SummaryAsync(
                            timeRange: timeRange,
                            summaryType: mySummaries,
                            calculationBasis: AFCalculationBasis.TimeWeighted,
                            timeType: AFTimestampCalculation.Auto);
                    }
                    catch (AggregateException ae)
                    {
                        Console.WriteLine("{0}: {1}", attr.Name, ae.Flatten().InnerException.Message);
                        return null;
                    }
                })
                .ToArray();

            return await Task.WhenAll(tasks);
        }

        public static async Task<IList<IDictionary<AFSummaryTypes, AFValue>>> GetSummariesAsyncThrottled(AFAttributeList attributeList, int numConcurrent)
        {
            // Use "asynchronous semaphore" pattern (e.g. SemaphoreSlim.WaitAsync()) to throttle the calls

            Console.WriteLine("Calling GetSummariesAsyncThrottled");

            // Example: Limit to numConcurrent concurrent async I/O operations.
            SemaphoreSlim throttler = new SemaphoreSlim(initialCount: numConcurrent);

            Task<IDictionary<AFSummaryTypes, AFValue>>[] tasks = attributeList
                // Do not make the call if async is not supported
                .Where(attr => (attr.SupportedDataMethods & AFDataMethods.Asynchronous) == AFDataMethods.Asynchronous)
                .Select(async attr =>
                {
                    // asychronously try to acquire the semaphore
                    await throttler.WaitAsync();

                    try
                    {
                        AFSummaryTypes mySummaries = AFSummaryTypes.Minimum | AFSummaryTypes.Maximum | AFSummaryTypes.Average | AFSummaryTypes.Total;
                        AFTimeRange timeRange = new AFTimeRange(new AFTime("*-1d"), new AFTime("*"));

                        return await attr.Data.SummaryAsync(
                            timeRange: timeRange,
                            summaryType: mySummaries,
                            calculationBasis: AFCalculationBasis.TimeWeighted,
                            timeType: AFTimestampCalculation.Auto);
                    }
                    catch (AggregateException ae)
                    {
                        Console.WriteLine("{0}: {1}", attr.Name, ae.Flatten().InnerException.Message);
                        return null;
                    }
                    finally
                    {
                        // release the resource
                        throttler.Release();
                    }
                })
                .ToArray();

            return await Task.WhenAll(tasks);
        }

        public static async Task<IList<IDictionary<AFSummaryTypes, AFValue>>> GetSummariesAsyncWithTimeout(AFAttributeList attributeList, int timeoutInMilliseconds)
        {
            // Use a "competing tasks" pattern to place timeout on multiple async requests

            Console.WriteLine("Calling GetSummariesAsyncWithTimeout");

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            CancellationTokenSource ctsForTimer = new CancellationTokenSource();
            CancellationToken tokenForTimer = ctsForTimer.Token;

            Task<IDictionary<AFSummaryTypes, AFValue>>[] tasks = attributeList
                // Do not make the call if async is not supported
                .Where(attr => (attr.SupportedDataMethods & AFDataMethods.Asynchronous) == AFDataMethods.Asynchronous)
                .Select(async attr =>
                {
                    try
                    {
                        AFSummaryTypes mySummaries = AFSummaryTypes.Minimum | AFSummaryTypes.Maximum | AFSummaryTypes.Average | AFSummaryTypes.Total;
                        AFTimeRange timeRange = new AFTimeRange(new AFTime("*-1d"), new AFTime("*"));

                        return await attr.Data.SummaryAsync(
                            timeRange: timeRange,
                            summaryType: mySummaries,
                            calculationBasis: AFCalculationBasis.TimeWeighted,
                            timeType: AFTimestampCalculation.Auto,
                            cancellationToken: token);
                    }
                    catch (AggregateException ae)
                    {
                        Console.WriteLine("{0}: {1}", attr.Element.Name, ae.Flatten().InnerException.Message);
                        return null;
                    }
                    catch (OperationCanceledException oe)
                    {
                        Console.WriteLine("{0}: {1}", attr.Element.Name, oe.Message);
                        return null;
                    }
                })
                .ToArray();

            // Define a task that completes when all subtasks are complete
            Task<IDictionary<AFSummaryTypes, AFValue>[]> task = Task.WhenAll(tasks);

            // Asychronously wait for either the summaries or timer task to complete
            if (await Task.WhenAny(task, Task.Delay(timeoutInMilliseconds, tokenForTimer)) == task)
            {
                // Cancel the timer task
                ctsForTimer.Cancel();
                // Return summaries result
                return task.Result;
            }
            else
            {
                // Cancel the summaries task if timeout
                cts.Cancel();
                throw new TimeoutException("The operation has timed out.");
            }
        }
    }
}
