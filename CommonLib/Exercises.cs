using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib
{
    public static class Exercises
    {
        enum GetAttributeSecurityMethod
        {
            CurrentUser, // Exercise 1,2,3,4,5
            Impersonation, // Exercise 6
            Manual, // Exercise 7
        }

        static GetAttributeSecurityMethod Security = GetAttributeSecurityMethod.CurrentUser;
        static bool RunExercise2 = false;
        static bool RunExercise3 = false;
        static bool RunExercise4 = false;
        static bool RunExercise5 = false;
        static string DatabaseName = "Hamlet"; // "Hamlet", "Village", "Town"
        const string ElementTemplate = "meter template";
        static string AttributeTemplate = "power"; // "power", "power per resident"

        public static void Run(bool interactive)
        {
            ExeConfigurationFileMap configMap = new ExeConfigurationFileMap();
            //configMap.ExeConfigFilename = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Exercises.config"); //does not work for AFWebApplication because Exercises.config does not get copied into the corresponding ASP.NET folder.
            configMap.ExeConfigFilename = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase), "Exercises.config").TrimStart(@"file:\".ToCharArray());
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

            Security = (GetAttributeSecurityMethod)Enum.Parse(typeof(GetAttributeSecurityMethod), config.AppSettings.Settings["SecurityMethod"].Value);
            RunExercise2 = bool.Parse(config.AppSettings.Settings["RunExercise2"].Value);
            RunExercise3 = bool.Parse(config.AppSettings.Settings["RunExercise3"].Value);
            RunExercise4 = bool.Parse(config.AppSettings.Settings["RunExercise4"].Value);
            RunExercise5 = bool.Parse(config.AppSettings.Settings["RunExercise5"].Value);

            DatabaseName = config.AppSettings.Settings["Database"].Value;
            AttributeTemplate = config.AppSettings.Settings["AttributeTemplate"].Value;

            IDisposable cleanup = null;
            AFAttributeList attributeList;

            if (Security == GetAttributeSecurityMethod.CurrentUser)
            {
                var ex1 = new Exercise1.FindAttributes();
                using (new BenchmarkTimer("Exercise 1: Get Attributes"))
                {
                    attributeList = ex1.GetAttributes(DatabaseName, ElementTemplate, AttributeTemplate);
                    //attributeList = ex1.GetAttributes(DatabaseName, ElementTemplate, AttributeTemplate, fullLoad: true); //(overload available in Ex1_Search_Solution).
                }

                Console.WriteLine("   Found {0} attributes", attributeList.Count.ToString());
                Console.WriteLine();

                try
                {
                    RunDataAccess(attributeList, interactive);
                }
                finally
                {
                    if (cleanup != null)
                        cleanup.Dispose();
                }
            }
            else if (Security == GetAttributeSecurityMethod.Impersonation) //Exercise 6
            {
                cleanup = new Exercise6.AppImpersonation(); //Impersonation

                //GetAttributes (Run Exercise 1, with impersonation)
                var ex1 = new Exercise1.FindAttributes();
                using (new BenchmarkTimer("Exercise 6: Get Attributes (Impersonated Security)"))
                {
                    attributeList = ex1.GetAttributes(DatabaseName, ElementTemplate, AttributeTemplate);
                    //attributeList = ex1.GetAttributes(DatabaseName, ElementTemplate, AttributeTemplate, fullLoad: true); //(overload available in Ex1_Search_Solution).
                }
                Console.WriteLine("   Found {0} attributes", attributeList.Count.ToString());
                Console.WriteLine();

                //GetValue (Run Exercise 2, with impersonation)
                IList<AFValue> valueList;
                double max;
                AFValue maxValue;
                int badCount;
                int goodCount;
                using (new BenchmarkTimer("Exercise 6: Get Current Value (Impersonated Security)"))
                {
                    valueList = Exercise2.ReadData.GetValue(attributeList);

                    max = FindMaximumValue(valueList, out maxValue, out goodCount, out badCount);
                }
                Console.WriteLine("   Found {0} good and {1} bad current values.  Max: {2} at {3}", goodCount.ToString(), badCount.ToString(), max, GetAttributePath(maxValue));
                Console.WriteLine();

                if (cleanup != null)
                    cleanup.Dispose();
            }
            else //Exercise 7: Security == GetAttributeSecurityMethod.Manual
            {
                //GetAttributes (Similar to Exercise 1, with manual security)
                var ex7 = new Exercise7.AppCheckSecurity();
                using (new BenchmarkTimer("Exercise 7: Get Attributes (Manual Security)"))
                {
                    attributeList = ex7.GetAttributesForIdentity(DatabaseName, ElementTemplate, AttributeTemplate);
                    //attributeList = ex7.GetAttributesForIdentity(DatabaseName, ElementTemplate, AttributeTemplate, fullLoad: true);
                }
                Console.WriteLine("   Found {0} attributes", attributeList.Count.ToString());
                Console.WriteLine();

                //GetValue (Similar to Exercise 2, with manual security)
                IList<AFValue> valueList;
                double max;
                AFValue maxValue;
                int badCount;
                int goodCount;
                using (new BenchmarkTimer("Exercise 7: Get Current Value (Manual Security)"))
                {
                    valueList = ex7.GetValueForIdentity(attributeList);

                    max = FindMaximumValue(valueList, out maxValue, out goodCount, out badCount);
                }
                Console.WriteLine("   Found {0} good and {1} bad current values.  Max: {2} at {3}", goodCount.ToString(), badCount.ToString(), max, GetAttributePath(maxValue));
                Console.WriteLine();
            }

        }

        public static void RunDataAccess(AFAttributeList attributeList, bool interactive)
        {
            double max;
            AFValue maxValue;
            int badCount;
            int goodCount;

            if (RunExercise2)
            {
                using (new BenchmarkTimer("Exercise 2: Get Current Value"))
                {
                    IList<AFValue> valueList = Exercise2.ReadData.GetValue(attributeList);

                    max = FindMaximumValue(valueList, out maxValue, out goodCount, out badCount);
                }
                Console.WriteLine("   Found {0} good and {1} bad current values.  Max: {2:N1} at {3}", goodCount.ToString(), badCount.ToString(), max, GetAttributePath(maxValue));
                Console.WriteLine();

                using (new BenchmarkTimer("Exercise 2: Get Yesterday's Values"))
                {
                    IEnumerable<AFValues> valuesEnumerable = Exercise2.ReadData.GetYesterdaysValues(attributeList);

                    max = FindMaximumValue(valuesEnumerable.SelectMany(vals => vals), out maxValue, out goodCount, out badCount);
                }
                Console.WriteLine("   Found {0} good and {1} bad current values.  Max: {2:N1} at {3}", goodCount.ToString(), badCount.ToString(), max, GetAttributePath(maxValue));
                Console.WriteLine();
            }

            if (RunExercise3)
            {
                AFValue avgValue;
                double avg;
                int avgGoodCount, avgBadCount;
                using (new BenchmarkTimer("Exercise 3: Get Yesterday's Maximum and Average"))
                {
                    IEnumerable<IDictionary<AFSummaryTypes, AFValue>> valuesEnumerable = Exercise3.ReadSummary.GetYesterdaysMaxAndAvg(attributeList);

                    max = FindMaximumValue(valuesEnumerable.Select(d => d[AFSummaryTypes.Maximum]), out maxValue, out goodCount, out badCount);
                    avg = FindMaximumValue(valuesEnumerable.Select(d => d[AFSummaryTypes.Average]), out avgValue, out avgGoodCount, out avgBadCount);
                }
                Console.WriteLine("   Found {0} good and {1} bad current values.  Highest Max: {2:N1} at {3}", goodCount.ToString(), badCount.ToString(), max, GetAttributePath(maxValue));
                Console.WriteLine("   Found {0} good and {1} bad current values.  Highest Avg: {2:N1} at {3}", avgGoodCount.ToString(), avgBadCount.ToString(), avg, GetAttributePath(avgValue));
                Console.WriteLine();

                using (new BenchmarkTimer("Exercise 3: Get Today's Maximum and Average"))
                {
                    IEnumerable<IDictionary<AFSummaryTypes, AFValue>> valuesEnumerable = Exercise3.ReadSummary.GetTodaysMaxAndAvg(attributeList);

                    max = FindMaximumValue(valuesEnumerable.Select(d => d[AFSummaryTypes.Maximum]), out maxValue, out goodCount, out badCount);
                    avg = FindMaximumValue(valuesEnumerable.Select(d => d[AFSummaryTypes.Average]), out avgValue, out avgGoodCount, out avgBadCount);
                }
                Console.WriteLine("   Found {0} good and {1} bad current values.  Highest Max: {2:N1} at {3}", goodCount.ToString(), badCount.ToString(), max, GetAttributePath(maxValue));
                Console.WriteLine("   Found {0} good and {1} bad current values.  Highest Avg: {2:N1} at {3}", avgGoodCount.ToString(), avgBadCount.ToString(), avg, GetAttributePath(avgValue));
                Console.WriteLine();
            }

            if (RunExercise4)
            {
                Console.WriteLine("Exercise 4: Data Pipe");
                using (var dataPipeWrapper = new Exercise4.DataPipe(attributeList))
                {
                    Console.WriteLine("Press any key to poll, 'esc' to exit");
                    int pollingTrials = 5;
                    while ((interactive && Console.ReadKey().Key != ConsoleKey.Escape)
                        || (!interactive && (--pollingTrials > 0)))
                    {
                        AFValue currentMaxValue;
                        using (new BenchmarkTimer("Check for Maximum Value"))
                        {
                            currentMaxValue = dataPipeWrapper.CheckForMaximumValue();
                        }
                        Console.WriteLine("   Current max: {0} on {1} ({2})", currentMaxValue.Value.ToString(), currentMaxValue.Timestamp, GetAttributePath(currentMaxValue));

                        if (!interactive)
                            System.Threading.Thread.Sleep(1000);
                    }
                }
            }

            if (RunExercise5)
            {
                List<AFElement> top20percentPerforers;
                using (new BenchmarkTimer("Exercise 5: Get Yesterday's top 20%"))
                {
                    IEnumerable<AFValue> valuesEnumerable = Exercise3.ReadSummary.GetYesterdaysMaxAndAvg(attributeList).Select(d => d[AFSummaryTypes.Average]);

                    int top20percentCount = attributeList.Count / 5;
                    top20percentPerforers = valuesEnumerable.OrderByDescending(v => GetMeasurement(v)).Take(top20percentCount).Select(v => v.Attribute.Element as AFElement).ToList();
                }
                Console.WriteLine("   Found {0} top performers", top20percentPerforers.Count.ToString());
                Console.WriteLine();

                using (new BenchmarkTimer("Exercise 5: Write Data"))
                {
                    Exercise5.WriteData.WriteValues(top20percentPerforers);
                }
                Console.WriteLine("   Wrote {0} top performers", top20percentPerforers.Count.ToString());
                Console.WriteLine();
            }
        }

        public static string RunWithRedirection()
        {
            var originalOut = Console.Out;
            StringBuilder stringBuilder = new StringBuilder();
            StringWriter stringWriter = new StringWriter(stringBuilder);
            Console.SetOut(stringWriter);

            Run(interactive: false);

            Console.SetOut(originalOut);
            stringWriter.Flush();
            return stringBuilder.ToString();
        }

        private static double FindMaximumValue(IEnumerable<AFValue> values, out AFValue maxValue, out int goodCount, out int badCount)
        {
            badCount = 0;
            goodCount = 0;
            double max = 0.0;
            maxValue = null;
            foreach (AFValue value in values)
            {

                if (!value.IsGood)
                {
                    ++badCount;
                    continue;
                }
                else
                    ++goodCount;

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

        private static string GetAttributePath(AFValue value)
        {
            if (value != null && value.Attribute != null)
                return value.Attribute.Name;
            else
                return "<null>";
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
