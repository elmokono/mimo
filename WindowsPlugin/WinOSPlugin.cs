using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Diagnostics;

namespace mimo
{
    public class WinOSPlugin : Plugin
    {
        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;

        public WinOSPlugin()
            : base("Windows Counters")
        {
            var root = new XElement("root",new XAttribute("ID", "CPUM"), 
                new XElement("item", new XAttribute("ID", "TEMP"), new XAttribute("Text", "Temperature")),
                new XElement("item", new XAttribute("ID", "COOL"), new XAttribute("Text", "Coolers")),
                new XElement("item", new XAttribute("Text", "CPU and Memo"), new XElement("CPUM"))
            );

            items = root.Elements();
            currentItem = items.First();

            //counters
            cpuCounter = new PerformanceCounter()
            {
                CategoryName = "Processor",
                CounterName = "% Processor Time",
                InstanceName = "_Total",
            };
            ramCounter = new PerformanceCounter("Memory", "Available MBytes"); 
        }

        public override void Start()
        {
            //initialize
        }

        public override string Poll()
        {
            //fill windows info based on current item
            if (currentItem.Name.LocalName.Equals("CPUM"))
            {                
                return String.Format("CPU {0:#0.00}%\r\nRAM {1} MB", cpuCounter.NextValue(), ramCounter.NextValue());
            }

            return base.Poll();
        }
    }
}
