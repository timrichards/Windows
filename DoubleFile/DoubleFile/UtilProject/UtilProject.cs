using System;
using System.Windows.Controls;

namespace DoubleFile
{
    delegate bool BoolAction();
    delegate string StringAction();

    static internal class UtilProject
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
                return (args == null)
                    ? owner.Dispatcher.Invoke(action)
                    : owner.Dispatcher.Invoke(action, (object) args);
            }

            var action1 = action as Action;

            if (action1 != null)
            {
                action1();
                return null;
            }

            var boolAction = action as BoolAction;

            return boolAction != null
                ? boolAction()
                : action.DynamicInvoke(args); // late-bound and slow
        }
    }
}
