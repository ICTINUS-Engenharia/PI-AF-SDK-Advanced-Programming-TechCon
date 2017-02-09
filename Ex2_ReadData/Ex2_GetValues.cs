using OSIsoft.AF.Asset;
using OSIsoft.AF.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise2
{
    /// <summary>
    /// Hints:
    /// An AF Attribute List can Get Data!
    /// <see cref="https://techsupport.osisoft.com/Documentation/PI-AF-SDK/html/T_OSIsoft_AF_Asset_AFAttributeList.htm"/>
    ///
    /// It also has a Data property ...
    /// <see cref="https://techsupport.osisoft.com/Documentation/PI-AF-SDK/html/T_OSIsoft_AF_Data_AFListData.htm"/>
    /// </summary>
    public static class ReadData
    {
        public static IList<AFValue> GetValue(AFAttributeList attributeList)
        {
            AFValue[] values = new AFValue[attributeList.Count];

            for (int i = 0; i < attributeList.Count; ++i)
            {
                values[i] = attributeList[i].GetValue();
            }

            return values;
        }

        public static IEnumerable<AFValues> GetYesterdaysValues(AFAttributeList attributeList)
        {
            var timeRange = new AFTimeRange(new AFTime("y"), new AFTime("t"));
            AFValues[] values = new AFValues[attributeList.Count];

            for (int i = 0; i < attributeList.Count; ++i)
            {
                values[i] = attributeList[i].Data.RecordedValues(timeRange, OSIsoft.AF.Data.AFBoundaryType.Interpolated, null, null, false);
            }

            return values;
        }
    }
}
