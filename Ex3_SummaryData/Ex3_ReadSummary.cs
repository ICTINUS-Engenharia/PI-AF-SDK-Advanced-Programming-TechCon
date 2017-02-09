using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using OSIsoft.AF.PI;
using OSIsoft.AF.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise3
{
    /// <summary>
    /// Hints:
    /// AF Attribute List has a Data Propery...
    /// <see cref="https://techsupport.osisoft.com/Documentation/PI-AF-SDK/html/T_OSIsoft_AF_Asset_AFAttributeList.htm"/>
    /// <see cref="https://techsupport.osisoft.com/Documentation/PI-AF-SDK/html/T_OSIsoft_AF_Data_AFListData.htm"/>
    /// </summary>
    public class ReadSummary
    {
        public static IEnumerable<IDictionary<AFSummaryTypes, AFValue>> GetYesterdaysMaxAndAvg(AFAttributeList attributeList)
        {
            var timeRange = new AFTimeRange("y", "t");
            IDictionary<AFSummaryTypes, AFValue>[] values = new IDictionary<AFSummaryTypes, AFValue>[attributeList.Count];
            for (int i = 0; i < attributeList.Count; ++i)
            {
                values[i] = new Dictionary<AFSummaryTypes, AFValue>();
                values[i][AFSummaryTypes.Maximum] = attributeList[i].Data.Summary(timeRange, AFSummaryTypes.Maximum, AFCalculationBasis.EventWeighted, AFTimestampCalculation.Auto).First().Value;
                values[i][AFSummaryTypes.Average] = attributeList[i].Data.Summary(timeRange, AFSummaryTypes.Average, AFCalculationBasis.EventWeighted, AFTimestampCalculation.Auto).First().Value;
            }

            return values;
        }

        public static IEnumerable<IDictionary<AFSummaryTypes, AFValue>> GetTodaysMaxAndAvg(AFAttributeList attributeList)
        {
            var timeRange = new AFTimeRange("t", "*");
            IDictionary<AFSummaryTypes, AFValue>[] values = new IDictionary<AFSummaryTypes, AFValue>[attributeList.Count];
            for (int i = 0; i < attributeList.Count; ++i)
            {
                values[i] = new Dictionary<AFSummaryTypes, AFValue>();
                values[i][AFSummaryTypes.Maximum] = attributeList[i].Data.Summary(timeRange, AFSummaryTypes.Maximum, AFCalculationBasis.EventWeighted, AFTimestampCalculation.Auto).First().Value;
                values[i][AFSummaryTypes.Average] = attributeList[i].Data.Summary(timeRange, AFSummaryTypes.Average, AFCalculationBasis.EventWeighted, AFTimestampCalculation.Auto).First().Value;
            }

            return values;
        }
    }
}
