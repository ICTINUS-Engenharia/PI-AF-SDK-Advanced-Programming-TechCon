using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using OSIsoft.AF.PI;
using OSIsoft.AF.Time;


namespace Exercise5
{
    public class WriteData
    {
        public static void WriteValues(List<AFElement> metersToUpdate)
        {
            var timeStamp = new AFTime("t");
            List<AFValue> valuesToWrite = new List<AFValue>(metersToUpdate.Count);
            foreach (var element in metersToUpdate)
            {
                var value = new AFValue(element.Attributes["power saver"], true, timeStamp, null, AFValueStatus.Good);
                valuesToWrite.Add(value);
            }

            var errors = AFListData.UpdateValues(valuesToWrite, AFUpdateOption.Insert);
        }
    }
}
