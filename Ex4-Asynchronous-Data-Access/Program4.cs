using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using External;

namespace Ex4_Asynchronous_Data_Access
{
    class Program4
    {
        static void Main(string[] args)
        {
            AFDatabase db = ConnectionHelper.GetDatabase("PISRV01", "Feeder Voltage Monitoring");

            AFAttributeList attrList = GetAttributes(db);

            try
            {
                Task<IList<IDictionary<AFSummaryTypes, AFValue>>> summariesTask = AFAsyncDataReader.GetSummariesAsync(attrList);

                // Wait for the summaries result
                IList<IDictionary<AFSummaryTypes, AFValue>> summaries = summariesTask.Result;
                foreach (var summary in summaries)
                {
                    WriteSummaryItem(summary);
                }
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("{0}", ae.Flatten().InnerException.Message);
            }

            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }

        private static AFAttributeList GetAttributes(AFDatabase database)
        {
            int startIndex = 0;
            int pageSize = 1000;
            int totalCount;

            AFAttributeList attrList = new AFAttributeList();

            do
            {
                AFAttributeList results = AFAttribute.FindElementAttributes(
                     database: database,
                     searchRoot: null,
                     nameFilter: null,
                     elemCategory: null,
                     elemTemplate: database.ElementTemplates["Feeder"],
                     elemType: AFElementType.Any,
                     attrNameFilter: "Power",
                     attrCategory: null,
                     attrType: TypeCode.Empty,
                     searchFullHierarchy: true,
                     sortField: AFSortField.Name,
                     sortOrder: AFSortOrder.Ascending,
                     startIndex: startIndex,
                     maxCount: pageSize,
                     totalCount: out totalCount);

                attrList.AddRange(results);

                startIndex += pageSize;
            } while (startIndex < totalCount);

            return attrList;
        }

        private static void WriteSummaryItem(IDictionary<AFSummaryTypes, AFValue> summary)
        {
            Console.WriteLine("Summary for {0}", summary[AFSummaryTypes.Minimum].Attribute.Element);
            Console.WriteLine("  Minimum: {0:N0}", summary[AFSummaryTypes.Minimum].ValueAsDouble());
            Console.WriteLine("  Maximum: {0:N0}", summary[AFSummaryTypes.Maximum].ValueAsDouble());
            Console.WriteLine("  Average: {0:N0}", summary[AFSummaryTypes.Average].ValueAsDouble());
            Console.WriteLine("  Total: {0:N0}", summary[AFSummaryTypes.Total].ValueAsDouble());
            Console.WriteLine();
        }
    }
}
