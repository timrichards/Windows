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

        internal static object CheckAndInvoke(Action action, object[] args = null)
        {
            return CheckAndInvoke(GlobalData.static_MainWindow, action as Delegate, args);
        }

        internal static object CheckAndInvoke(BoolAction action, object[] args = null)
        {
            return CheckAndInvoke(GlobalData.static_MainWindow, action as Delegate, args);
        }

        internal static object CheckAndInvoke(Control owner, Delegate action, object[] args = null)
        {
            if ((owner != null) && (false == owner.Dispatcher.CheckAccess()))
            {
                if (args == null)
                {
                    return owner.Dispatcher.Invoke(action);
                }
                else
                {
                    return owner.Dispatcher.Invoke(action, (object)args);
                }
            }
            else
            {
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
            }

            return null;
        }
    }
}
