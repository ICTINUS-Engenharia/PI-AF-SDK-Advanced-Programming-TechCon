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
    /// <summary>
    /// AF List Data contains static methods that may interrest you...
    /// <see cref="https://techsupport.osisoft.com/Documentation/PI-AF-SDK/html/T_OSIsoft_AF_Data_AFListData.htm"/>
    /// </summary>
    public class WriteData
    {
        public static void WriteValues(List<AFElement> metersToUpdate)
        {
            var timeStamp = new AFTime("t");
            foreach(var element in metersToUpdate)
            {
                var attribute = element.Attributes["power saver"];
                attribute.SetValue(true, null);
            }
        }
    }
}
