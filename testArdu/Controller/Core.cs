using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;
using System.IO;

namespace mimo.Controller
{
    public class Core
    {
        protected List<Plugin> plugins;
        protected Plugin currentPlugin;
        protected Boolean inPluginMenu;
        protected System.Timers.Timer poller;
        protected mimo.UI.Engine.ArduinoInterface ui;

        public Core()
        {
            //load plugins
            plugins = new List<Plugin>();

            foreach (var file in Directory.GetFiles(@".\"))
            {
                if (file.EndsWith("Plugin.dll", StringComparison.OrdinalIgnoreCase))
                {
                    var fi = new FileInfo(file);
                    var x = (Plugin)Activator.CreateInstance(Type.GetType(String.Format("mimo.{0}, {0}", fi.Name.Replace(fi.Extension,"")), true));
                    plugins.Add(x);
                }
            }

            if (!plugins.Any()) { throw new DllNotFoundException("No Plugins Configured :("); }

            currentPlugin = plugins.First();
            inPluginMenu = true;

            //poll plugins
            poller = new System.Timers.Timer();
            poller.Interval = 1000;
            poller.Elapsed += poller_Elapsed;
            //poller.Start(); <-- moved to onConnect
            
            //ui
            ui = new UI.Engine.ArduinoInterface();
            ui.OnConnect += ui_OnConnect;
            ui.OnDisconnect += ui_OnDisconnect;
            ui.ActionClick += ui_ActionClick;
            ui.BackClick += ui_BackClick;
            ui.NextClick += ui_NextClick;
            ui.Start();
        }

        void ui_NextClick()
        {
            Action(UserActions.Next);
            poller_Elapsed(poller, null);
        }

        void ui_BackClick()
        {
            Action(UserActions.Back);
            poller_Elapsed(poller, null);
        }

        void ui_ActionClick()
        {
            Action(UserActions.Open);
            poller_Elapsed(poller, null);
        }

        void ui_OnDisconnect()
        {
            //
        }

        void ui_OnConnect()
        {
            poller.Start();
        }

        ~Core()
        {
            poller.Elapsed -= poller_Elapsed;
            poller.Stop();
        }

        public void Action(UserActions action)
        {
            //menu of plugins
            if (inPluginMenu)
            {
                switch (action)
                {
                    case UserActions.Next:
                        var idx = plugins.IndexOf(currentPlugin);
                        currentPlugin = (idx == (plugins.Count-1)) ? plugins.First() : plugins.ElementAt(idx + 1);
                        break;
                    case UserActions.Back:
                        break;
                    case UserActions.Open:
                        inPluginMenu = false;
                        break;
                }
            }
            //items of current plugin
            else
            {
                switch (action)
                {
                    case UserActions.Next: currentPlugin.Next(); break;
                    case UserActions.Back:
                        if (!currentPlugin.Back()) { inPluginMenu = true; }
                        break;
                    case UserActions.Open: currentPlugin.Open(); break;
                }
            }
        }

        void poller_Elapsed(object sender, ElapsedEventArgs e)
        {
            var displayText = String.Empty;

            if (inPluginMenu)
            {
                displayText = currentPlugin.Name;
            }
            else
            {
                displayText = currentPlugin.Poll();
            }
            
            //TODO: show in monitor
            ui.Display(displayText);
            //Console.Clear();
            //Console.WriteLine(displayText);
        }

        #region reflection
        public static List<Type> FindAllDerivedTypes<T>()
        {
            return FindAllDerivedTypes<T>(Assembly.GetAssembly(typeof(T)));
        }

        public static List<Type> FindAllDerivedTypes<T>(Assembly assembly)
        {
            var derivedType = typeof(T);
            return assembly
                .GetTypes()
                .Where(t =>
                    t != derivedType &&
                    derivedType.IsAssignableFrom(t)
                    ).ToList();

        } 
        #endregion
    }
}
