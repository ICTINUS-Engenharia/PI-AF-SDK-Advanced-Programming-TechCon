using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using OSIsoft.AF.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Exercise4
{
    public class DataPipe : IDisposable
    {
        private readonly AFDataPipe _dataPipe = new AFDataPipe();
        private readonly MaximumValueObserver _maxValueObserver = new MaximumValueObserver();

        public DataPipe(IList<AFAttribute> attributesToSubscribe)
        {
            _dataPipe.Subscribe(_maxValueObserver);
            var signupResult = _dataPipe.AddSignupsWithInitEvents(attributesToSubscribe);
        }

        public AFValue CheckForMaximumValue()
        {
            _dataPipe.GetObserverEvents();
            return _maxValueObserver.GetMaximumValue();
        }

        public void Dispose()
        {
            _dataPipe.Dispose();
        }
    }

    class MaximumValueObserver : IObserver<AFDataPipeEvent>
    {
        private readonly Dictionary<AFAttribute, AFValue> _lastValueForAttribute = new Dictionary<AFAttribute, AFValue>();
        private AFValue _currentMax;
        private double _currentMaxValue;

        public AFValue GetMaximumValue()
        {
            if (_currentMax == null)
            {
                _currentMaxValue = FindMaximumValue(_lastValueForAttribute.Values, out _currentMax);
            }

            return _currentMax;
        }

        public void OnNext(AFDataPipeEvent dataPipeEvent)
        {
            AFValue value = dataPipeEvent.Value;
            if (_currentMax != null)
            {
                double measure = GetMeasurement(value);
                if (measure > _currentMaxValue)
                {
                    _currentMaxValue = measure;
                    _currentMax = value;
                }
                else if (_currentMax.Attribute == value.Attribute) // if the last max value changed and didn't increase, remove it
                {
                    _currentMax = null;
                }
            }

            _lastValueForAttribute[value.Attribute] = value;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }


        public static double FindMaximumValue(IEnumerable<AFValue> values, out AFValue maxValue)
        {
            double max = 0.0;
            maxValue = null;
            foreach (AFValue value in values)
            {
                try
                {
                    double measure = GetMeasurement(value);
                    if (measure > max)
                    {
                        max = measure;
                        maxValue = value;
                    }
                }
                catch (InvalidCastException) { }
            }

            return max;
        }

        private static double GetMeasurement(AFValue value)
        {
            if (value.Value is float)
                return (float)value.Value;
            else
                return (double)value.Value;
        }
    }
}

//Exercise4 Bonus
namespace Exercise4Bonus
{
    public class DataPipe : IDisposable
    {
        private readonly AFDataCache _dataCache = new AFDataCache();
        private readonly List<AFData> _dataItems;

        public DataPipe(IList<AFAttribute> attributesToSubscribe)
        {
            var signupResult = _dataCache.Add(attributesToSubscribe);
            _dataItems = signupResult.Results.Values.ToList();
        }

        public AFValue CheckForMaximumValue()
        {
            _dataCache.UpdateData();

            double max = 0.0;
            AFValue maxValue = null;
            foreach (AFData dataItem in _dataItems)
            {
                try
                {
                    var value = dataItem.RecordedValue(AFTime.Now, AFRetrievalMode.AtOrBefore, null);
                    double measure = GetMeasurement(value);
                    if (measure > max)
                    {
                        max = measure;
                        maxValue = value;
                    }
                }
                catch (InvalidCastException) { }
            }

            return maxValue;
        }

        public void Dispose()
        {
            _dataCache.Dispose();
        }

        private static double GetMeasurement(AFValue value)
        {
            if (value.Value is float)
                return (float)value.Value;
            else
                return (double)value.Value;
        }
    }
}

