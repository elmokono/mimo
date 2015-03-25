using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace mimo
{
    public enum UserActions
    {
        Open,
        Back,
        Next,
    }

    //base
    public interface IConnectable
    {
        void Start();

        //events
        bool Back();

        void Next();

        void Open();

        //status
        String Poll();
    }

    public abstract class Plugin : IConnectable
    {
        private String _name;
        public String Name { get { return _name; } }

        protected IEnumerable<XElement> items;
        protected XElement currentItem;
        
        public Plugin(String name)
        {
            _name = name;
        }

        public virtual void Start()
        {
            
            throw new NotImplementedException();
        }
        
        //go back -> currentMenu
        public bool Back()
        {
            if (currentItem.Parent.Name == "root")
            {
                currentItem = items.First();
                return false; //flag to controller
            }

            currentItem = currentItem.Parent;
            return true;
        }

        //go next -> currentMenu
        public void Next()
        {
            if (currentItem.ElementsAfterSelf().Any())
            {
                currentItem = currentItem.ElementsAfterSelf().First();
            }
            else
            {
                currentItem = currentItem.Parent.Elements().First();
            }
        }

        public virtual void Open()
        {
            if (currentItem.Elements().Any())
            {
                currentItem = currentItem.Elements().First();
            }
            else
            {
                //end item, nothing to do
            }
        }
        
        //show current status
        public virtual string Poll()
        {
            return currentItem.Attribute("Text").Value;
        }
    }
}
