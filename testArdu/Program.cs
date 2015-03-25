using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mimo
{
    class Program
    {
        static void Main(string[] args)
        {
            var controller = new Controller.Core();
            while (true)
            {
                var key = Console.ReadKey().Key;
                if (key == ConsoleKey.RightArrow)
                {
                    controller.Action(UserActions.Next);
                }
                if (key == ConsoleKey.Backspace)
                {
                    controller.Action(UserActions.Back);
                }
                if (key == ConsoleKey.Enter)
                {
                    controller.Action(UserActions.Open);
                }
            }
            
        }
    }
}
