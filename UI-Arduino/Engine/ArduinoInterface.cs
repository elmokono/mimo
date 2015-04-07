using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mimo.UI.Engine
{    
    public delegate void Click();
    public delegate void Connect();
    public delegate void Disconnect();

    enum ProgramState
    { 
        LookingUpArduino,
        RetievingCapabilities,
        InSync,
    }

    public class ArduinoInterface
    {
        private ProgramState state;
        private System.Timers.Timer timerSync;
        private bool inSync;
        private DateTime lastSync;
        private bool hasCapabilities;

        private const int SYNCTIMEOUT = 3000;
        private const int BAUDRATE = 19200;

        public event Click BackClick;
        public event Click NextClick;
        public event Click ActionClick;
        public event Connect OnConnect;
        public event Connect OnDisconnect;
                
        private SerialPort serial;
        private int _ledCount;
        private String[] _ledColors;
        private int _btnCount;
        private String[] _btnActions;
        private int _lcdRows;
        private int _lcdCols;

        public void Start()
        {
            state = ProgramState.LookingUpArduino;
            hasCapabilities = false;
            inSync = false;

            while (true)
            {
                if (state == ProgramState.LookingUpArduino)
                {
                    Console.WriteLine("Looking for arduino..");
                    while (!lookupArduino())
                    {
                        Thread.Sleep(500);
                    }

                    //sync timer
                    inSync = false;
                    timerSync = new System.Timers.Timer(2500);
                    timerSync.Elapsed += timerSync_Elapsed;
                    timerSync.Start();

                    Console.WriteLine("Syncing..");
                    while (!inSync)
                    {
                        Thread.Sleep(500);
                    }

                    state = ProgramState.RetievingCapabilities;
                }


                if (state == ProgramState.RetievingCapabilities)
                {
                    Console.WriteLine("Retrieving capabilities..");
                    while (!hasCapabilities)
                    {
                        sendCommand(new CapabilitiesCommand());
                        Thread.Sleep(5000);
                    }

                    if (OnConnect != null) { OnConnect(); }
                    state = ProgramState.InSync;
                    //lastSync = DateTime.Now;
                }
                
                
                if (state == ProgramState.InSync)
                {
                    if (!deviceAvailable())
                    {
                        dispose();
                        state = ProgramState.LookingUpArduino;
                    }
                }

                Thread.Sleep(250);
                //running!
            }
            
        }

        #region events

        void timerSync_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (DateTime.Now.Subtract(lastSync).TotalMilliseconds > SYNCTIMEOUT)
            {
                sendCommand(new SyncCommand());
            }
        }

        void serial_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            Console.WriteLine("Pingchanged", e.EventType.ToString());
        }

        void serial_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Console.WriteLine("ErrorReceived", e.EventType.ToString());
        }

        void serial_Disposed(object sender, EventArgs e)
        {
            if (OnDisconnect != null) { OnDisconnect(); }
            Console.WriteLine("Disposed");
        }

        void serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (!deviceAvailable() || serial.BytesToRead == 0) { return; }

                SerialPort sp = (SerialPort)sender;
                var s = sp.ReadLine();

                //Console.ForegroundColor = ConsoleColor.Cyan;
                //Console.WriteLine(s);
                //Console.ForegroundColor = ConsoleColor.Gray;

                processResponse(s);
            }
            catch (System.Runtime.InteropServices.SEHException c)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(c.StackTrace);
                Console.ForegroundColor = ConsoleColor.Gray;
                dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("!: {0}", ex.Message);
            }
        }
        
        #endregion

        void processResponse(String s)
        {
            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<ArduinoResponse>(s);

            //all kind of responses are considered sync signals
            lastSync = DateTime.Now;
            inSync = true;

            if (response.cmd != null && response.succeeded)
            {
                if (response.cmd.Equals("Sync"))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Arduino Sync Response!");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    return;
                }

                if (response.cmd.Equals("Button"))
                { 
                    if (response.val.Equals("ok") && ActionClick != null) { ActionClick(); }
                    if (response.val.Equals("back") && BackClick != null) { BackClick(); }
                    if (response.val.Equals("menu") && NextClick != null) { NextClick(); }
                }
            }
            else
            {
                var responseCaps = Newtonsoft.Json.JsonConvert.DeserializeObject<ArduinoCapabilitiesResponse>(s);
                if (responseCaps != null)
                {
                    _ledCount = responseCaps.Leds.Count;
                    _ledColors = responseCaps.Leds.Colors.Split(',');
                    _btnActions = responseCaps.Buttons.Actions.Split(',');
                    _btnCount = responseCaps.Buttons.Count;
                    _lcdRows = responseCaps.Display.Rows;
                    _lcdCols = responseCaps.Display.Cols;
                    hasCapabilities = true;
                    return;
                }
            }
        }

        bool lookupArduino()
        {
            foreach (var portName in SerialPort.GetPortNames())
            {
                Console.WriteLine("Trying port {0}..", portName);

                serial = new SerialPort(portName, BAUDRATE);
                serial.DataBits = 8;
                serial.Parity = Parity.None;
                serial.StopBits = StopBits.One;
                serial.Handshake = Handshake.None;
                serial.DataReceived += serial_DataReceived;
                serial.Disposed += serial_Disposed;
                serial.ErrorReceived += serial_ErrorReceived;
                serial.PinChanged += serial_PinChanged;
                
                //wait until port is closed
                var callTime = DateTime.Now;
                while (serial.IsOpen)
                {
                    System.Threading.Thread.Sleep(1000);
                    if (DateTime.Now.Subtract(callTime).TotalMinutes > 0) { break; } //port is opened by someone else..lookup next port
                }

                //wait until arduino is in sync
                try
                {
                    //open and sync
                    serial.Open();
                    System.Threading.Thread.Sleep(500);
                    sendCommand(new SyncCommand());

                    callTime = DateTime.Now;
                    while (!inSync)
                    {
                        System.Threading.Thread.Sleep(1000);
                        if (DateTime.Now.Subtract(callTime).TotalSeconds > 10) { break; } //not arduino?
                    }

                    sendCommand(new DisplayCommand(String.Format("Connected to {0}", System.Net.Dns.GetHostName())));
                    return true;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return false; //no ports available or arduino online
        }

        void sendCommand(ArduinoCommand command)
        {
            write(command.ToString());
        }

        bool deviceAvailable()
        {
            return 
                (serial != null)
                && serial.IsOpen 
                && SerialPort.GetPortNames().Contains(serial.PortName);
        }

        ~ArduinoInterface()
        {
            try
            {
                if (deviceAvailable())
                {
                    serial.Close();
                }
            }
            catch { }
            finally
            {
                serial.Dispose();
            }

            Console.WriteLine("Disposed..");
        }

        void dispose()
        {
            timerSync.Elapsed -= timerSync_Elapsed;
            if (serial.IsOpen) { serial.Close(); }
            serial.Dispose();
            serial = null;
            hasCapabilities = false;
            inSync = false;
        }

        void write(String buffer)
        {
            if (!deviceAvailable()) { throw new InvalidOperationException("Serial port is closed :("); }

            try
            {
                Console.WriteLine(">: {0}", buffer);
                serial.WriteLine(buffer);
            }
            catch (System.Runtime.InteropServices.SEHException c)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(c.StackTrace);
                Console.ForegroundColor = ConsoleColor.Gray;
                dispose();
            }
            catch (IOException) //disconnection or not sync
            {
                dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("!: {0}", ex.Message);
                dispose();
            }
        }

        public void Led(int idx, bool state)
        {
            sendCommand(new LedCommand(idx, state));
        }

        /// <summary>
        /// display text in "n" rows
        /// </summary>
        /// <param name="text">Split lines with \n</param>
        public void Display(string text)
        {
            var rows = text.Split("\r\n".ToCharArray()).Where(x => x.Length > 0);
            var sb = new StringBuilder();
            foreach (var row in rows)
            {
                if (row.Length > _lcdCols)
                {
                    sb.Append(row.Substring(0, _lcdCols));
                }
                else
                {
                    sb.Append(row + new String(' ', _lcdCols - row.Length));
                }
            }
            
            sendCommand(new DisplayCommand(sb.ToString()));            
        }

        public String GetCurrentStatus()
        {
            return state.ToString();
        }
    }
}
