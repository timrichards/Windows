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
            var mainWindow = MainWindow.Instance;

            if (App.LocalExit ||
                (null == mainWindow) ||
                mainWindow.LocalIsClosed)
                return;

            if ((null == mainWindow) ||
                (null == mainWindow.Dispatcher) ||
                mainWindow.Dispatcher.HasShutdownStarted ||
                mainWindow.Dispatcher.HasShutdownFinished)
            {
                return;
            }

            if (mainWindow.Dispatcher.CheckAccess())
                action();
            else
                mainWindow.Dispatcher.Invoke(action);
        }

        static internal T UIthread<T>(Func<T> action, Control owner = null)
        {
            if (null == owner)
            {
                var mainWindow = MainWindow.Instance;

                if (App.LocalExit ||
                    (null == mainWindow) ||
                    mainWindow.LocalIsClosed)
                {
                    return default(T);
                }

                owner = mainWindow;
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
