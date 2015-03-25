using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace mimo
{
    public class uTPlugin : Plugin
    {
        public uTPlugin()
            : base("uT")
        {
            var root = new XElement("root",
                new XElement("item", new XAttribute("Text", "Active")),
                new XElement("item", new XAttribute("Text", "Seeding")),
                new XElement("item", new XAttribute("Text", "Close uTorrent"))
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
            //fill torrents info based on current item
            return base.Poll();
        }
    }

}
