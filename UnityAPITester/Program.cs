using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WV = Hyland.Unity.WorkView;

namespace UnityAPITester
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var applicationName = "SBPWC-10466";
                var className = "Project";
                var relatedClassName = "Milestone";

                using (Hyland.Unity.Application app = Hyland.Unity.Application.Connect(Hyland.Unity.Application.CreateOnBaseAuthenticationProperties(ConnectionInfo.AppServerURL, ConnectionInfo.Username, ConnectionInfo.Password, ConnectionInfo.DataSource)))
                {
                    Console.WriteLine(string.Format("[{0}] Connected", DateTime.Now));

                    var applicationAndClass = GetApplicationAndClass(app, applicationName, className);
                    var wvApp = applicationAndClass.WVApplication;
                    var wvClass = applicationAndClass.WVClass;

                    var results = GetFilterQueryResults(wvClass);

                    if(results.Count == 0)
                    {
                        Console.WriteLine("No results found");
                    }

                    foreach (var result in results)
                    {
                        Console.WriteLine($"Object ID: '{result.ObjectID}'");
                        var wvObject = LoadObjectByID(result.ObjectID, wvClass);
                        if (wvObject != null)
                        {
                            EnumerateAttributeValuesForObject(wvObject);
                        }
                    }
                }

                Console.WriteLine(string.Format("[{0}] Disconnected", DateTime.Now));
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.Write(string.Format("[{0}] Press any key to exit", DateTime.Now));
                Console.ReadLine();
            }
        }

        private static (WV.Application WVApplication, WV.Class WVClass) GetApplicationAndClass(Hyland.Unity.Application app, string applicationName, string className)
        {
            WV.Application wvApp = null;
            WV.Class wvClass = null;

            try
            {
                wvApp = app.WorkView.Applications.Find(applicationName);
                wvClass = wvApp.Classes.Find(className);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR");
                Console.WriteLine(ex.Message);
            }

            return (wvApp, wvClass);
        }

        private static WV.FilterQueryResultItemList GetFilterQueryResults(WV.Class wvClass)
        {
            WV.FilterQueryResultItemList results = null;

            try
            {
                var dfq = wvClass.CreateDynamicFilterQuery();
                results = dfq.Execute(int.MaxValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR");
                Console.WriteLine(ex.Message);
            }

            return results;
        }

        private static WV.Object LoadObjectByID(long objectID, WV.Class wvClass)
        {
            WV.Object wvObject = null;

            try
            {
                wvObject = wvClass.GetObjectByID(objectID);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR");
                Console.WriteLine(ex.Message);
            }

            return wvObject;
        }

        private static void EnumerateAttributeValuesForObject(WV.Object wvObject)
        {
            foreach (var av in wvObject.AttributeValues)
            {
                Console.WriteLine($"Attribute: '{av.Attribute.Name}' :: Value: '{av.Value}'");
            }
        }
    }
}
