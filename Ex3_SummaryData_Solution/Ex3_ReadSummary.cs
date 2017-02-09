using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using OSIsoft.AF.PI;
using OSIsoft.AF.Time;
using OSIsoft.AF.Search;


namespace Exercise3
{
    public class ReadSummary
    {
        public static IEnumerable<IDictionary<AFSummaryTypes, AFValue>> GetYesterdaysMaxAndAvg(AFAttributeList attributeList)
        {
            var timeRange = new AFTimeRange("y", "t");

            // NOTE: calling ToList to get an accurate timing for benchmark.  In general would not need to do this
            return attributeList.Data.Summary(timeRange, AFSummaryTypes.Maximum | AFSummaryTypes.Average, AFCalculationBasis.TimeWeighted, AFTimestampCalculation.EarliestTime, new PIPagingConfiguration(PIPageType.EventCount, 10000)).ToList();
        }

        public static IEnumerable<IDictionary<AFSummaryTypes, AFValue>> GetTodaysMaxAndAvg(AFAttributeList attributeList)
        {
            var timeRange = new AFTimeRange("t", "*");

            // NOTE: calling ToList to get an accurate timing for benchmark.  In general would not need to do this
            return attributeList.Data.Summary(timeRange, AFSummaryTypes.Maximum | AFSummaryTypes.Average, AFCalculationBasis.TimeWeighted, AFTimestampCalculation.EarliestTime, new PIPagingConfiguration(PIPageType.EventCount, 10000)).ToList();
        }
    }
}
