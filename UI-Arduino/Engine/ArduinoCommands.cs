using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mimo.UI.Engine
{
    abstract class ArduinoCommand
    {
        public String Type { get; set; }
        public String Params { get; set; }
        public ArduinoCommand(String type, String parameters)
        {
            Type = type;
            Params = parameters;
        }
        public ArduinoCommand() { }

        public override string ToString()
        {
            return "{" + String.Format("\"{0}\":\"{1}\"", Type, Params.Replace("\"", "'")) + "}";
        }
    }

    class LedCommand : ArduinoCommand
    {
        public LedCommand(int ledIndex, bool state)
        {
            Type = "Led";
            Params = String.Format("{0}{1}", state ? "" : "-", ledIndex);
        }
    }

    class DisplayCommand : ArduinoCommand
    {
        public DisplayCommand(string text)
        {
            Type = "Display";
            Params = text;
        }
    }

    class SystemCommand : ArduinoCommand
    {
        public SystemCommand(string option)
        {
            Type = "System";
            Params = option;
        }
    }

    class CapabilitiesCommand : SystemCommand
    {
        public CapabilitiesCommand()
            : base("Capabilities")
        {
        }
    }

    class SyncCommand : ArduinoCommand
    {
        public SyncCommand()
        {
            Type = "Sync";
            Params = String.Empty;
        }
    }
}
