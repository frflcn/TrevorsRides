using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrevorsRidesHelpers
{
    public class Helpers
    {
        public static bool IsTest {  get; set; }
        public static string Domain { get; set; }
        public static int Port { get; set; }
        static Helpers() 
        { 
            IsTest = false;
            Port = 7061;
            if (IsTest)
            {
                Domain = $"http://10.0.2.2:{Port}";
            }
            else
            {
                Domain = "https://www.trevorsrides.com";
            }
        }
    }
}
