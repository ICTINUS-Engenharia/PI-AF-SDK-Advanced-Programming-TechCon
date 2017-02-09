using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using OSIsoft.AF.PI;
using OSIsoft.AF.Time;
using OSIsoft.AF.UnitsOfMeasure;

namespace Exercise2
{
    public class ReadData
    {
        public static IList<AFValue> GetValue(AFAttributeList attributeList)
        {
            return attributeList.GetValue();
        }

        public static IEnumerable<AFValues> GetYesterdaysValues(AFAttributeList attributeList)
        {
            var timeRange = new AFTimeRange(new AFTime("y"), new AFTime("t"));
            var pointResolution = attributeList.GetPIPoint(); //resolve PI points in bulk (Caution: don't do this every time).
            return attributeList.Data.RecordedValues(timeRange, AFBoundaryType.Interpolated, null, false, 
                new PIPagingConfiguration(PIPageType.EventCount, 10000)); //PIPagingConfiguration defines chunks
        }

        public static IEnumerable<AFValue> GetYesterdaysMaximum(AFAttributeList attributeList) //Exercise3 Bonus
        {
            var timeRange = new AFTimeRange(new AFTime("y"), new AFTime("t"));
            return attributeList.Data.Summary(timeRange, AFSummaryTypes.Maximum, AFCalculationBasis.EventWeighted, AFTimestampCalculation.Auto, 
                new PIPagingConfiguration(PIPageType.EventCount, 10000)).Select(dict => dict[AFSummaryTypes.Maximum]); //PIPagingConfiguration defines chunks
        }
    }
}
