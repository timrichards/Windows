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

        static internal void UIthread(Action action)
        {
            if (App.LocalExit ||
                (null == MainWindow.static_MainWindow) ||
                MainWindow.static_MainWindow.LocalIsClosed)
                return;

            var owner = MainWindow.static_MainWindow;

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

        static internal T UIthread<T>(Func<T> action, Control owner = null)
        {
            if (null == owner)
            {
                if (App.LocalExit ||
                    (null == MainWindow.static_MainWindow) ||
                    MainWindow.static_MainWindow.LocalIsClosed)
                {
                    return default(T);
                }

                owner = MainWindow.static_MainWindow;
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
