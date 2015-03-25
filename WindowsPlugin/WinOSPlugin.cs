using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace mimo
{
    public class WinOSPlugin : Plugin
    {
        public WinOSPlugin()
            : base("WinOS")
        {
            var root = new XElement("root",
                new XElement("item", new XAttribute("Text", "Temperature")),
                new XElement("item", new XAttribute("Text", "Coolers")),
                new XElement("item", new XAttribute("Text", "CPU and Memo"))
            );

            items = root.Elements();
            currentItem = items.First();
        }

        public override void Start()
        {
            //initialize
        }

        public override string Poll()
        {
            //fill windows info based on current item

            return base.Poll();
        }
    }
}
