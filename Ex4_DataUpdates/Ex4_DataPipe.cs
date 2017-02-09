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
    /// <summary>
    /// Hints:
    /// AF Data Pipe Class
    /// <see cref="https://techsupport.osisoft.com/Documentation/PI-AF-SDK/html/T_OSIsoft_AF_Data_AFDataPipe.htm"/>
    /// 
    /// IObserver Interface Explanations
    /// <see cref="https://msdn.microsoft.com/en-us/library/dd783449(v=vs.110).aspx"/>
    /// 
    /// 
    /// </summary>
    /// 
    public class DataPipe : IDisposable
    {
        private readonly MaximumValueObserver _maxValueObserver = new MaximumValueObserver();
        private readonly AFAttributeList _attributeList;


        public DataPipe(AFAttributeList attributesToSubscribe)
        {
            // *** Instantiate the AFDataPipe here and add the observer ***
            _attributeList = attributesToSubscribe;
        }

        public AFValue CheckForMaximumValue()
        {
            // *** poll the pipe here ***
            var values = _attributeList.GetValue();
            foreach (var value in values)
            {
                _maxValueObserver.OnNext(value);
            }

            return _maxValueObserver.GetMaximumValue();
        }

        public void Dispose()
        {
        }
    }

    class MaximumValueObserver : IObserver<AFValue>
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

        public void OnNext(AFValue value)
        {
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
