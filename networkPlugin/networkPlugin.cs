using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace mimo
{
    public class networkPlugin : mimo.Plugin
    {
        private WebClient _wc;
        private long _downloadKbps;
        
        public networkPlugin()
            : base("Network")
        {
            var root = new XElement("root",
                new XElement("item", new XAttribute("Text", "ADSL Avg"), new XElement("ADSL", new XAttribute("Text", ""))),
                new XElement("item", new XAttribute("Text", "Modem Line"), new XElement("LINE", new XAttribute("Text", "")))
            );

            items = root.Elements();
            currentItem = items.First();

            _wc = new WebClient();
            _wc.Credentials = new NetworkCredential("admin", "yayayaya");
            _downloadKbps = 0;
            long lastValue = 0;

            while (true)
            {
                var sz = _wc.DownloadString("http://192.168.1.1/fetchif.cgi?vlan2");
                var values = sz.Split(' ');
                var v = long.Parse(values[9]);
                _downloadKbps = v - lastValue;
                lastValue = v;
                Thread.Sleep(1000);
            }

        }

        public override void Start()
        {
            //initialize
        }

        public override string Poll()
        {
            if (currentItem.Name.LocalName.Equals("ADSL"))
            {
                //_downloading = true;
                //var sz = _wc.DownloadString("http://192.168.1.1/fetchif.cgi?vlan2");
                //var values = sz.Split(' ');
               // _downloading = false;

                //var currentValue = long.Parse(values[9]);
                currentItem.Attribute("Text").Value = _downloadKbps.ToString() + "Kbps";
            }

            if (currentItem.Name.LocalName.Equals("LINE"))
            {
                //TODO:
            }

            //fill torrents info based on current item
            return base.Poll();
        }
    }
}
