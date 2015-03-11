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
            if (App.LocalExit ||
                (null == GlobalData.static_MainWindow) ||
                GlobalData.static_MainWindow.LocalIsClosed)
                return;

            var owner = GlobalData.static_MainWindow;

            if ((null == owner) ||
                (null == owner.Dispatcher) ||
                owner.Dispatcher.HasShutdownStarted ||
                owner.Dispatcher.HasShutdownFinished)
            {
                return;
            }

            if (owner.Dispatcher.CheckAccess())
                action();
            else
                owner.Dispatcher.Invoke(action);
        }

        internal static T UIthread<T>(Func<T> action, Control owner = null)
        {
            if (null == owner)
            {
                if (App.LocalExit ||
                    (null == GlobalData.static_MainWindow) ||
                    GlobalData.static_MainWindow.LocalIsClosed)
                {
                    return default(T);
                }

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
