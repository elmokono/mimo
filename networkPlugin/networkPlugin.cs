using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace mimo
{
    public class networkPlugin : mimo.Plugin
    {
        private WebClient _wc;
        private double _downloadKbps;
        private long _activeConnections;
        BackgroundWorker bw = new BackgroundWorker();

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

            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = false;
            bw.DoWork += bw_DoWork;
            bw.RunWorkerAsync();
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            double lastValue = 0;
            double lastPoll = 0;
            BackgroundWorker worker = sender as BackgroundWorker;

            while (!worker.CancellationPending)
            {
                //DD-WRT

                //bandwith
                var sz = _wc.DownloadString("http://192.168.1.1/fetchif.cgi?vlan2");
                var values = sz.Split(' ').Where(x => !String.IsNullOrEmpty(x)).ToArray();

                var dateValue = DateTimeToUnixTimestamp(DateTime.Parse("2000-01-01 " + values[3]));
                var inValue = double.Parse(values[7]);
                var outValue = double.Parse(values[15]);

                _downloadKbps = (inValue - lastValue) / (dateValue - lastPoll);
                
                lastPoll = dateValue;
                lastValue = inValue;

                //live connections
                sz = _wc.DownloadString("http://192.168.1.1/Status_Router.live.asp");
                var match = Regex.Match(sz, @"\{ip_conntrack::(?<connections>[\d]*)\}");
                _activeConnections = long.Parse("0" + match.Groups["connections"].Value);

                Thread.Sleep(5000);
            }
            e.Cancel = true;
        }

        private double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (dateTime - new DateTime(1970, 1, 1).ToLocalTime()).TotalMilliseconds / 1000;
        }

        private String formatSpeedBytes(double speed)
        {
            // format speed in bytes/sec, input:  bytes/sec
            if (speed < 1048576) return Math.Round(speed / 10.24) / 100 + " KB/s";
            if (speed < 1073741824) return Math.Round(speed / 10485.76) / 100 + " MB/s";
            // else
            return Math.Round(speed / 10737418.24) / 100 + " MB/s";  // wow!
        }

        public override void Start()
        {
            //initialize
        }

        public override string Poll()
        {
            if (currentItem.Name.LocalName.Equals("ADSL"))
            {
                currentItem.Attribute("Text").Value = String.Format("WAN {0}\r\nConns: {1}", formatSpeedBytes(_downloadKbps), _activeConnections);
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
