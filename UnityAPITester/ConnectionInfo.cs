using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityAPITester
{
    internal static class ConnectionInfo
    {
        static ConnectionInfo()
        {
            AppServerURL = System.Configuration.ConfigurationManager.AppSettings["AppServerURL"];
            Username = System.Configuration.ConfigurationManager.AppSettings["Username"];
            Password = System.Configuration.ConfigurationManager.AppSettings["Password"];
            DataSource = System.Configuration.ConfigurationManager.AppSettings["DataSource"];
        }

        public static string AppServerURL { get; }
        public static string Username { get; }
        public static string Password { get; }
        public static string DataSource { get; }
    }
}
