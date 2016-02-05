﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;

namespace Ex3_Real_Time_Analytics_Sln
{
    public class AssetRankProvider : IObserver<AFDataPipeEvent>, IRankProvider
    {
        public AFAttributeTemplate AttributeTemplate { get; set; }
        public AFDataPipe DataPipe { get; set; }

        private Comparison<AFValue> _afValueComparer;
        
        private Dictionary<AFElement, AFValue> _lastValues;

        private bool _disposed = false;
        private CancellationTokenSource _tokenSource;
        private int _threadSleepTimeInMilliseconds = 5000;
        private CancellationToken _ct;
        private Task _mainTask;

        public AssetRankProvider(AFAttributeTemplate attrTemplate)
        {
            if (attrTemplate.Type != typeof(double) && 
                attrTemplate.Type != typeof(float) && 
                attrTemplate.Type != typeof(Int32))
            {
                throw new ArgumentException("Cannot rank attributes with value type {0}", attrTemplate.Type.Name);
            }

            AttributeTemplate = attrTemplate;

            _afValueComparer = new Comparison<AFValue>(CompareAFValue);

            DataPipe = new AFDataPipe();
            _lastValues = new Dictionary<AFElement, AFValue>();

            _tokenSource = new CancellationTokenSource();
            _ct = _tokenSource.Token;
        }

        public void Start()
        {
            if (_disposed)
                throw new ObjectDisposedException("AssetRankProvider was disposed.");

            // Gets all attributes from the AttributeTemplate
            AFAttributeList attrList = GetAttributes();

            Console.WriteLine("{0} | Signing up for updates for {1} attributes", DateTime.Now, attrList.Count);
            DataPipe.Subscribe(this);

            // Throw exception on errors
            AFErrors<AFAttribute> errors = DataPipe.AddSignups(attrList);
            if (errors != null)
            {
                throw new Exception(errors.ToString(), new AggregateException(errors.Errors.Values));
            }

            Console.WriteLine("{0} | Signed up for updates for {1} attributes\n", DateTime.Now, attrList.Count);

            _mainTask = Task.Run(() =>
            {
                bool hasMoreEvents = false;

                while (true)
                {
                    if (_ct.IsCancellationRequested)
                    {
                        // The main task in AssetRankProvider is cancelled.
                        _ct.ThrowIfCancellationRequested();
                        // NOTE!!! A "OperationCanceledException was unhandled  
                        // by user code" error will be raised here if "Just My Code" 
                        // is enabled on your computer. On Express editions JMC is  
                        // enabled and cannot be disabled. The exception is benign.  
                        // Just press F5 to continue executing your code.  
                    }

                    AFErrors<AFAttribute> results = DataPipe.GetObserverEvents(out hasMoreEvents);

                    if (results != null)
                    {
                        Console.WriteLine("Errors in GetObserverEvents: {0}", results.ToString());
                    }

                    if (!hasMoreEvents)
                        Thread.Sleep(_threadSleepTimeInMilliseconds);
                }
            }, _ct);
        }

        public void OnCompleted()
        {
            Console.WriteLine("{0} | AssetRankProvider has completed", DateTime.Now);
        }

        public void OnError(Exception error)
        {
            throw error;
        }

        public void OnNext(AFDataPipeEvent dpEvent)
        {
            if (dpEvent.Action == AFDataPipeAction.Add && dpEvent.Value.IsGood)
            {
                AFElement element = dpEvent.Value.Attribute.Element as AFElement;
                if (element != null)
                {
                    _lastValues.Remove(element);
                    _lastValues.Add(element, dpEvent.Value);
                }            
            }
            else if (dpEvent.Value.Value is Exception)
            {
                Exception e = (Exception) dpEvent.Value.Value;
                Console.WriteLine("Error receiving event for {0}: {1}", dpEvent.Value.Attribute.Name, e.Message);
            }
        }

        public IList<AFRankedValue> GetRankings()
        {   
            // We will perform the sort on demand, operating under the assumption that requests for rankings
            // occur less frequently than the event arrival rate.
            // In the case of more frequent ranking requests and slow event rate,
            // we could use other internal data structures (tree, heap) other than the simple dictionary that would
            // maintain the sort condition after events are added.

            var tempList = _lastValues.ToList();
            tempList.Sort((x, y) =>
            {
                return _afValueComparer(x.Value, y.Value)*-1; // -1 to sort descending
            });

            int r = 1;
            return tempList.Select(kvp => new AFRankedValue { Value = kvp.Value, Ranking = r++ }).ToList();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_tokenSource != null)
                {
                    _tokenSource.Cancel();

                    try
                    {
                        if (_mainTask != null)
                        {
                            _mainTask.Wait();
                        }
                    }
                    catch (AggregateException e)
                    {
                        foreach (var v in e.InnerExceptions)
                        {
                            if (!(v is TaskCanceledException))
                                Console.WriteLine("Exception in the main task : {0}", v);
                        }
                        Console.WriteLine();
                    }
                    finally
                    {
                        _tokenSource.Dispose();
                    }
                }

                if (DataPipe != null)
                    DataPipe.Dispose();
            }

            _tokenSource = null;
            DataPipe = null;
            _disposed = true;
        }

        private AFAttributeList GetAttributes()
        {
            int startIndex = 0;
            int totalCount;
            int pageSize = 1000;

            AFAttributeList attrList = new AFAttributeList();
            do
            {
                AFAttributeList attrListTemp = AFAttribute.FindElementAttributes(
                    database: AttributeTemplate.Database,
                    searchRoot: null,
                    nameFilter: "*",
                    elemCategory: null,
                    elemTemplate: AttributeTemplate.ElementTemplate,
                    elemType: AFElementType.Any,
                    attrNameFilter: AttributeTemplate.Name,
                    attrCategory: null,
                    attrType: TypeCode.Empty,
                    searchFullHierarchy: true,
                    sortField: AFSortField.Name,
                    sortOrder: AFSortOrder.Ascending,
                    startIndex: startIndex,
                    maxCount: pageSize,
                    totalCount: out totalCount);

                attrList.AddRange(attrListTemp);

                startIndex += pageSize;
            } while (startIndex < totalCount);

            return attrList;
        }

        private int CompareAFValue(AFValue val1, AFValue val2)
        {
            if (val1.ValueTypeCode != val2.ValueTypeCode)
            {
                throw new InvalidOperationException("Types of inputs do not match");
            }

            if (val1.ValueTypeCode == TypeCode.Double)
            {
                return val1.ValueAsDouble().CompareTo(val2.ValueAsDouble());
            }
            else if (val1.ValueTypeCode == TypeCode.Single)
            {
                return val1.ValueAsSingle().CompareTo(val2.ValueAsSingle());
            }
            else if (val1.ValueTypeCode == TypeCode.Int32)
            {
                return val1.ValueAsInt32().CompareTo(val2.ValueAsInt32());
            }
            else
            {
                throw new InvalidOperationException(string.Format("Cannot compare type {0}", val1.ValueType.Name));
            }
        }
    }
}
