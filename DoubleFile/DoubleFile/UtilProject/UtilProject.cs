using System;
using System.Windows.Controls;

namespace DoubleFile
{
    internal class UtilProject
    {
        static internal void WriteLine(string str = null)
        {
#if (DEBUG)
            System.Console.WriteLine(str);
#endif
        }

        internal static object UIthread(Action action, object[] args = null)
        {
            return UIthread(GlobalData.static_MainWindow, action as Delegate, args);
        }

        internal static object UIthread(BoolAction action, object[] args = null)
        {
            return UIthread(GlobalData.static_MainWindow, action as Delegate, args);
        }

        internal static object UIthread(Control owner, Delegate action, object[] args = null)
        {
            if (owner == null)
            {
                return null;
            }

            if (false == owner.Dispatcher.CheckAccess())
            {
                return (args == null) ?
                    owner.Dispatcher.Invoke(action) :
                    owner.Dispatcher.Invoke(action, (object)args);
            }

            if (action is Action)
            {
                (action as Action)();
            }
            else if (action is BoolAction)
            {
                return (action as BoolAction)();
            }
            else
            {
                return action.DynamicInvoke(args);     // late-bound and slow
            }

            return null;
        }
    }
}
