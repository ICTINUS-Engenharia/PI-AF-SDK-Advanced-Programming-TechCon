﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OSIsoft.AF;
using OSIsoft.AF.Asset;

namespace Ex3_Real_Time_Analytics
{
    class Program
    {
        static void Main(string[] args)
        {
            PISystem ps = PISystem.CreatePISystem("PISRV01"); // This factory method is new in 2.7.5
            AFDatabase db = ps.Databases["Feeder Voltage Monitoring"];
            AFAttributeTemplate attrTemp = db.ElementTemplates["Feeder"].AttributeTemplates["Reactive Power"];

            AssetRankProvider rankProvider = new AssetRankProvider(attrTemp);

            rankProvider.Start();

            // Get rankings every 5 seconds. Do this 10 times.
            foreach (int i in Enumerable.Range(0, 10))
            {
                Thread.Sleep(5000);
                IList<AFRankedValue> rankings = rankProvider.GetRankings();
                foreach (var r in rankings)
                {
                    Console.WriteLine($"{r.Ranking} {r.Value.Attribute.Element.Name} {r.Value.Timestamp} {r.Value.Value}");
                }
                Console.WriteLine();
            }

            // Remove unmanaged resources and server-side signup.
            rankProvider.Dispose();

            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }
    }
}
