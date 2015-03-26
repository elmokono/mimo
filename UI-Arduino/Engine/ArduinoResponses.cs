using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mimo.UI.Engine
{
    class ArduinoResponse
    {
        public String cmd { get; set; }
        public String val { get; set; }
        public Boolean succeeded { get; set; }
    }

    class ArduinoCapabilitiesResponse
    {
        public class CapableField
        {
            public Boolean Capable { get; set; }
        }

        public class SystemField
        {
            public String Model { get; set; }
        }

        public class DisplayField : CapableField
        {
            public Int32 Rows { get; set; }
            public Int32 Cols { get; set; }
        }
        public class LedsField : CapableField
        {
            public Int32 Count { get; set; }
            public String Colors { get; set; }
        }
        public class ButtonsField : CapableField
        {
            public Int32 Count { get; set; }
            public String Actions { get; set; }
        }
        public class RelaysField : CapableField
        {
            public Int32 Count { get; set; }
        }

        public SystemField System { get; set; }
        public DisplayField Display { get; set; }
        public LedsField Leds { get; set; }
        public ButtonsField Buttons { get; set; }
        public RelaysField Relays { get; set; }
    }

}
