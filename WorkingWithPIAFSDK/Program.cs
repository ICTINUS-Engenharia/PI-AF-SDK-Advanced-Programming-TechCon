using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using OSIsoft.AF.Time;
using QueryData;
using Search;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkingWithPIAFSDK
{
    class Program
    {
        static void Main(string[] args)
        {
            var sys = (new PISystems())["dmo-srv"];
            var db = sys.Databases["perf"];
            var et = db.ElementTemplates["meter"];

            // run on small hierarchies to ensure everything is JIT compiled
            IList<AFAttribute> attributes;
            attributes = FindAttributes.FindAttributesInChildElementsWithIteration(db.Elements["10 meters"], et.AttributeTemplates["reading 1"]);
            attributes = FindAttributes.FindAttributesInChildElementsWithPreLoad(db.Elements["10 meters"], et.AttributeTemplates["reading 1"]);
            attributes = FindAttributes.FindAttributesInChildElementsWithPartialLoad(db.Elements["10 meters"], et.AttributeTemplates["reading 1"]);

            Console.WriteLine("Ready");
            Console.ReadLine();

            // setup a new hierarchy
            using(new BenchmarkTimer("Iteration"))
            {
                var cleanSystem = (new PISystems(forceNewInstance: true))["dmo-srv"];
                var cleanDB = cleanSystem.Databases["perf"];
                var cleanET = cleanDB.ElementTemplates["meter"];
                attributes = FindAttributes.FindAttributesInChildElementsWithIteration(cleanDB.Elements["1k meters"], cleanET.AttributeTemplates["reading 1"]);
            }

            Console.ReadLine();

            // setup a new hierarchy
            using (new BenchmarkTimer("Pre-Load"))
            {
                var cleanSystem = (new PISystems(forceNewInstance: true))["dmo-srv"];
                var cleanDB = cleanSystem.Databases["perf"];
                var cleanET = cleanDB.ElementTemplates["meter"];
                attributes = FindAttributes.FindAttributesInChildElementsWithPreLoad(cleanDB.Elements["1k meters"], cleanET.AttributeTemplates["reading 1"]);
            }

            Console.ReadLine();

            // setup a new hierarchy
            using (new BenchmarkTimer("Partial Load"))
            {
                var cleanSystem = (new PISystems(forceNewInstance: true))["dmo-srv"];
                var cleanDB = cleanSystem.Databases["perf"];
                var cleanET = cleanDB.ElementTemplates["meter"];
                attributes = FindAttributes.FindAttributesInChildElementsWithPartialLoad(cleanDB.Elements["1k meters"], cleanET.AttributeTemplates["reading 1"]);
            }

            Console.ReadLine();
            return;
            string searchString = @"\\dmo-srv\perf\pi|Attributes[@name=t*]";

            using (var cancellationSource = new CancellationTokenSource())
            {
                var dataPipeTask = DataPipe.PollInBackground(attributes, cancellationSource.Token);

                int i = 1;
                while (true)
                {
                    Console.WriteLine("Press enter to write, any other key to exit");
                    if (Console.ReadKey().Key != ConsoleKey.Enter)
                        break;

                    var timestamp = AFTime.Now;
                    List<AFValue> values = new List<AFValue>(attributes.Count);
                    foreach (var attribute in attributes)
                    {
                        values.Add(new AFValue(attribute, i, timestamp, null));
                    }
                    AFListData.UpdateValues(values, AFUpdateOption.Insert);
                    ++i;
                }

                cancellationSource.Cancel();
                Task.WaitAll(dataPipeTask);
            }
        }
    }
}
