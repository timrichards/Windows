using System;
using System.Windows.Controls;

namespace DoubleFile
{
    static class UtilProject
    {
        static internal void WriteLine(string str = null)
        {
#if (DEBUG)
            System.Console.WriteLine(str);
#endif
        }

        internal static void UIthread(Action action)
        {
            var owner = GlobalData.static_MainWindow;

            if (owner.Dispatcher.CheckAccess())
                action();
            else
                owner.Dispatcher.Invoke(action);
        }

        internal static T UIthread<T>(Func<T> action, Control owner = null)
        {
            if (null == owner)
            {
                if (GlobalData.static_MainWindow.IsClosed)
                    return default(T);

                owner = GlobalData.static_MainWindow;
            }

            if ((null == owner) ||
                (null == owner.Dispatcher) ||
                owner.Dispatcher.HasShutdownStarted ||
                owner.Dispatcher.HasShutdownFinished)
            {
                return default(T);
            }

            return
                owner.Dispatcher.CheckAccess()
                ? action()
                : owner.Dispatcher.Invoke(action);      // cancellationToken? timeout?
        }
    }
}
