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


        internal static object CheckAndInvoke(Delegate action, object[] args = null)
        {
            return CheckAndInvoke(GlobalData.static_MainWindow, action, args);
        }

        internal static object CheckAndInvoke(Control owner, Delegate action, object[] args = null)
        {
            if (owner == null)
            {
                return null;
            }

            if (false == owner.Dispatcher.CheckAccess())
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
                    ((Action)action)();
                }
                else if (action is BoolAction)
                {
                    return ((BoolAction)action)();
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
