using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Bogus;
using Bogus.DataSets;

using Hyland.Unity.WorkView;

using static System.Net.Mime.MediaTypeNames;

using WV = Hyland.Unity.WorkView;

namespace UnityAPITester
{
    internal static class Program
    {
        static Dictionary<string, List<long>> relatedClassIDList = new Dictionary<string, List<long>>();
        static void Main(string[] args)
        {
            try
            {
                int noOfNewObjectsToCreate = 999;
                var applicationName = "Order Inventory";
                var className = "Order";
                var doModifyAttributeTest = false;
                var doCreateNewObjectTest = true;
                using (Hyland.Unity.Application app = EstablishConnection(applicationName, className))
                {
                    Console.WriteLine(string.Format("[{0}] Connected", DateTime.Now));

                    var applicationAndClass = GetApplicationAndClass(app, applicationName, className);
                    var wvApp = applicationAndClass.WVApplication;
                    var wvClass = applicationAndClass.WVClass;

                    if (wvApp == null || wvClass == null)
                    {
                        Console.WriteLine("Application or Class not found");
                        return;
                    }

                    for (int i = 0; i < noOfNewObjectsToCreate; i++)
                    {
                        Console.WriteLine($"Creating new object: {i}");
                        var o = wvClass.CreateObject();
                        var modifier = o.CreateAttributeValueModifier();

                        string IdentifierAttributeName = "OrderID";

                        var relatedClassForCustomer = wvApp.Classes.Find("Customer");
                        var dfq = relatedClassForCustomer.CreateDynamicFilterQuery();
                        dfq.AddFilterColumn("ObjectID");
                        dfq.AddConstraint("CustomerID", Operator.Equal, 100);
                        var customer = dfq.Execute(1).First();

                        Dictionary<string, string> relationAttributeResolver = new Dictionary<string, string>
                        {
                            { "Customer",  customer.ObjectID.ToString()},
                            { "Product", "Random" }
                        };

                        var attributeNameValueMap = GenerateRandomObjectData(wvClass, IdentifierAttributeName, relationAttributeResolver);
                        foreach (var attr in wvClass.Attributes)
                        {
                            var attributeName = attr.Name;
                            if(attributeNameValueMap.ContainsKey(attributeName))
                            {
                                var targetAttributeValue = attributeNameValueMap[attributeName];

                                switch (attr.AttributeType)
                                {
                                    case WV.AttributeType.Integer:
                                        modifier.SetAttributeValue(attr.Name, long.Parse(targetAttributeValue.ToString()));
                                        break;
                                    case WV.AttributeType.Decimal:
                                    case WV.AttributeType.Currency:
                                        modifier.SetAttributeValue(attr.Name, decimal.Parse(targetAttributeValue.ToString()));
                                        break;
                                    case WV.AttributeType.Float:
                                        modifier.SetAttributeValue(attr.Name, double.Parse(targetAttributeValue.ToString()));
                                        break;
                                    case WV.AttributeType.Date:
                                    case WV.AttributeType.DateTime:
                                        modifier.SetAttributeValue(attr.Name, ParseDateTimeValue(targetAttributeValue.ToString()));
                                        break;
                                    case WV.AttributeType.Alphanumeric:
                                    case WV.AttributeType.EncryptedAlphanumeric:
                                    case WV.AttributeType.Text:
                                    case WV.AttributeType.FormattedText:
                                        modifier.SetAttributeValue(attr.Name, (string)targetAttributeValue);
                                        break;
                                    case WV.AttributeType.Relation:
                                        modifier.SetAttributeValue(attr, long.Parse(targetAttributeValue.ToString()));
                                        break;
                                    case WV.AttributeType.Boolean:
                                        modifier.SetAttributeValue(attr.Name, bool.Parse(targetAttributeValue.ToString()));
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }

                        modifier.ApplyChanges();
                    }

                    #region Commented Code
                    //for (int i = 0; i < 50; i++)
                    //{
                    //    Console.WriteLine($"Creating new object: {i}");
                    //    var p = GenerateProductData();
                    //    var o = wvClass.CreateObject();
                    //    var modifier = o.CreateAttributeValueModifier();
                    //    foreach (var attr in o.AttributeValues)
                    //    {
                    //        switch (attr.Name.ToUpper())
                    //        {
                    //            case "PRODUCTID":
                    //                modifier.SetAttributeValue(attr.Name, ++i);
                    //                break;
                    //            case "NAME":
                    //                modifier.SetAttributeValue(attr.Name, p.Name);
                    //                break;
                    //            case "DESCRIPTION":
                    //                modifier.SetAttributeValue(attr.Name, p.Description);
                    //                break;
                    //            case "PRICE":
                    //                modifier.SetAttributeValue(attr.Name, p.Price);
                    //                break;
                    //            default:
                    //                break;
                    //        }
                    //    }

                    //    modifier.ApplyChanges();
                    //}
                    #endregion

                    #region Commented Code
                    //var results = GetFilterQueryResults(wvClass);

                    //if (results.Count == 0)
                    //{
                    //    Console.WriteLine("No results found");
                    //}

                    //if (doCreateNewObjectTest)
                    //{
                    //    try
                    //    {
                    //        Console.WriteLine("Creating a new object");

                    //        var o = wvClass.CreateObject();
                    //        var modifier = o.CreateAttributeValueModifier();
                    //        var attrs = o.AttributeValues;

                    //        var changes = new List<(string attributeName, object value, Type t)>();

                    //        foreach (var av in attrs)
                    //        {
                    //            Random random = new Random();
                    //            switch (av.Attribute.AttributeType)
                    //            {
                    //                case WV.AttributeType.Alphanumeric:
                    //                    changes.Add((av.Attribute.Name, GetRandomString(random.Next(1, 20)), typeof(string)));
                    //                    break;
                    //                case WV.AttributeType.Integer:
                    //                    changes.Add((av.Attribute.Name, random.Next(1, 100), typeof(int)));
                    //                    break;
                    //                case WV.AttributeType.DateTime:
                    //                    changes.Add((av.Attribute.Name, GetRandomDate(), typeof(DateTime)));
                    //                    break;
                    //                case WV.AttributeType.Boolean:
                    //                    changes.Add((av.Attribute.Name, random.Next(1, 100) % 2 == 0, typeof(bool)));
                    //                    break;
                    //                default:
                    //                    break;
                    //            }
                    //        }

                    //        foreach (var change in changes)
                    //        {
                    //            if (change.t == typeof(bool))
                    //            {
                    //                modifier.SetAttributeValue(change.attributeName, (bool)change.value);
                    //            }
                    //            else if (change.t == typeof(DateTime))
                    //            {
                    //                modifier.SetAttributeValue(change.attributeName, (DateTime)change.value);
                    //            }
                    //            else if (change.t == typeof(int))
                    //            {
                    //                modifier.SetAttributeValue(change.attributeName, (int)change.value);
                    //            }
                    //            else if (change.t == typeof(string))
                    //            {
                    //                modifier.SetAttributeValue(change.attributeName, change.value.ToString());
                    //            }
                    //        }

                    //        modifier.ApplyChanges();
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        Console.WriteLine("ERROR");
                    //        Console.WriteLine(ex.Message);
                    //    }
                    //}

                    //results = GetFilterQueryResults(wvClass);

                    //foreach (var result in results)
                    //{
                    //    Console.WriteLine($"Object ID: '{result.ObjectID}'");
                    //    var wvObject = LoadObjectByID(result.ObjectID, wvClass);
                    //    if (wvObject != null)
                    //    {
                    //        EnumerateAttributeValuesForObject(wvObject);

                    //        if (doModifyAttributeTest)
                    //        {
                    //            try
                    //            {
                    //                var targetAttributeName = "Name";
                    //                var targetAttribute = wvObject.AttributeValues.FirstOrDefault(av => av.Attribute.Name == targetAttributeName);
                    //                if (targetAttribute != null)
                    //                {
                    //                    var updatedTargetAttributeValue = string.Empty;

                    //                    if(targetAttribute.HasValue)
                    //                    {
                    //                        var counter = 0;
                    //                        var targetAttrValue = targetAttribute.Value;
                    //                        Match match = Regex.Match(targetAttrValue.ToString(), @"(\d+)$");
                    //                        if (match.Success)
                    //                        {
                    //                            counter = int.Parse(match.Value);
                    //                            updatedTargetAttributeValue = $"{targetAttrValue.ToString().TrimEnd(match.Value.ToCharArray())}{++counter}";
                    //                        }
                    //                        else
                    //                        {
                    //                            updatedTargetAttributeValue = $"{targetAttrValue}_Updated";

                    //                        }
                    //                    }
                    //                    else
                    //                    {
                    //                        updatedTargetAttributeValue = $"{targetAttributeName}_Updated";
                    //                    }

                    //                    var modifier = wvObject.CreateAttributeValueModifier();
                    //                    modifier.SetAttributeValue(targetAttributeName, $"{updatedTargetAttributeValue}");
                    //                    modifier.ApplyChanges();
                    //                }
                    //            }
                    //            catch (Exception ex)
                    //            {
                    //                Console.WriteLine("ERROR");
                    //                Console.WriteLine(ex.Message);
                    //            }
                    //        }
                    //    }
                    //}
                    #endregion
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
        private static Hyland.Unity.Application EstablishConnection(string applicationName, string className)
        {
            Hyland.Unity.Application app = Hyland.Unity.Application.Connect(Hyland.Unity.Application.CreateOnBaseAuthenticationProperties(ConnectionInfo.AppServerURL, ConnectionInfo.Username, ConnectionInfo.Password, ConnectionInfo.DataSource));

            return app;
        }

        private static DateTime ParseDateTimeValue(string dateTimeValue)
        {
            DateTime result = DateTime.MinValue;
            if (DateTime.TryParse(dateTimeValue, out DateTime parsedDateTime))
            {
                result = parsedDateTime;
            }

            return result;
        }

        private static Product GenerateProductData()
        {
            var product = new Faker<Product>()
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Description, f => f.Commerce.ProductAdjective())
                .RuleFor(p => p.Price, f => f.Random.Number(1, 1000));

            return product.Generate();
        }

        private static Dictionary<string, object> GenerateRandomObjectData(WV.Class wvClass, string IdentifierAttributeName, Dictionary<string, string> relationAttributeResolver)
        {
            Dictionary<string, object> attributeValues = new Dictionary<string, object>();
            long maxID = 1;
            var existingObjectList = GetMaxID(wvClass, "OrderID");
            if(existingObjectList.Count > 0)
            {
                var existingMaxID = existingObjectList[0].GetFilterColumnValue(IdentifierAttributeName).IntegerValue;
                maxID = existingMaxID + 1;
            }

            attributeValues[IdentifierAttributeName] = maxID;

            var attributes = wvClass.Attributes;
            if (attributes.Count() > 0)
            {
                var targetAttributeValue = string.Empty;
                var faker = new Faker("en");
                foreach (var attribute in attributes)
                {
                    switch (attribute.AttributeType)
                    {
                        case WV.AttributeType.Integer:
                            if (!string.Equals(attribute.Name, IdentifierAttributeName, StringComparison.OrdinalIgnoreCase))
                                targetAttributeValue = faker.Random.Number(1, 1000).ToString();
                            else
                                continue;
                            break;
                        case WV.AttributeType.Decimal:
                        case WV.AttributeType.Currency:
                            targetAttributeValue = faker.Random.Decimal(1, 1000).ToString();
                            break;
                        case WV.AttributeType.Float:
                            targetAttributeValue = faker.Random.Float(1, 1000).ToString();
                            break;
                        case WV.AttributeType.Date:
                        case WV.AttributeType.DateTime:
                            targetAttributeValue = faker.Date.Past().ToString();
                            break;
                        case WV.AttributeType.Alphanumeric:
                        case WV.AttributeType.EncryptedAlphanumeric:
                            targetAttributeValue = faker.Lorem.Sentence();
                            break;
                        case WV.AttributeType.Text:
                        case WV.AttributeType.FormattedText:
                            targetAttributeValue = faker.Lorem.Paragraph();
                            break;
                        case WV.AttributeType.Relation:
                            var relatedClassName = attribute.RelatedClass.Name;
                            if (relationAttributeResolver.ContainsKey(relatedClassName))
                            {
                                var resolutionMode = relationAttributeResolver[relatedClassName];
                                if(string.Equals(resolutionMode, "Random", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (!relatedClassIDList.ContainsKey(relatedClassName))
                                    {
                                        var IDList = GetIDListForClass(attribute.RelatedClass);
                                        if (IDList.Count > 0)
                                        {
                                            relatedClassIDList[relatedClassName] = IDList.Select(o => o.ObjectID).ToList();
                                        }

                                    }

                                    List<long> classIDList = relatedClassIDList[relatedClassName];
                                    Random random = new Random();
                                    int randomIndex = random.Next(classIDList.Count);
                                    long randomRelatedClassID = classIDList[randomIndex];
                                    targetAttributeValue = randomRelatedClassID.ToString();
                                }
                                else
                                {
                                    targetAttributeValue = relationAttributeResolver[relatedClassName];
                                }
                            }
                            break;
                        case WV.AttributeType.Boolean:
                            targetAttributeValue = faker.Random.Bool().ToString();
                            break;
                        default:
                            break;
                    }

                    attributeValues[attribute.Name] = targetAttributeValue;
                }
            }

            return attributeValues;
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

        private static WV.FilterQueryResultItemList GetIDListForClass(WV.Class wvClass)
        {
            WV.FilterQueryResultItemList results = null;

            try
            {
                var dfq = wvClass.CreateDynamicFilterQuery();
                dfq.AddFilterColumn("ObjectID");
                results = dfq.Execute(int.MaxValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR");
                Console.WriteLine(ex.Message);
            }

            return results;
        }

        private static WV.FilterQueryResultItemList GetMaxID(WV.Class wvClass, string IdentifierAttributeName)
        {
            WV.FilterQueryResultItemList results = null;

            try
            {
                var dfq = wvClass.CreateDynamicFilterQuery();
                dfq.AddFilterColumn(IdentifierAttributeName);
                dfq.AddSort(IdentifierAttributeName, WV.SortType.DESCENDING);
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

        private static string GetRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            StringBuilder result = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }

        private static DateTime GetRandomDate()
        {
            DateTime start = new DateTime(1995, 1, 1);
            DateTime end = DateTime.Now;
            Random random = new Random();
            int range = (end - start).Days;
            return start.AddDays(random.Next(range));
        }
    }
}