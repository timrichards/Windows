using System;
using System.Diagnostics;
using System.Windows;

namespace DoubleFile
{
    static class MBoxStatic
    {
        static internal bool AssertUp { get; private set; }

        static internal bool
            Assert(decimal nLocation, bool bCondition, string strError_in, bool bTraceOnly)
        {
            if (bCondition)
                return true;

            if ((_nLastAssertLoc == nLocation) &&
                (1 > (DateTime.Now - _dtLastAssert).Seconds))
            {
                return false;
            }

            var strError = "Assertion failed at location " + nLocation + ".";

            if (false == string.IsNullOrWhiteSpace(strError_in))
                strError += "\n\nAdditional information: " + strError_in;

            Util.WriteLine(strError);
#if (DEBUG && (false == LOCALMBOX))
            Debug.Assert(false, strError);
            return false;
#else
            if (bTraceOnly)
                return false;

            var strErrorOut =
                strError +
                "\n\nPlease discuss this bug at http://sourceforge.net/projects/searchdirlists/.";

            _nLastAssertLoc = nLocation;
            _dtLastAssert = DateTime.Now;

            if (AssertUp)
            {
                MessageBox.Show(strErrorOut + "\n(LocalMbox: there is a local assert box already up.)");
            }
            else
            {
                AssertUp = true;
                MBoxStatic.ShowDialog(strErrorOut, "DoubleFile Assert");
                AssertUp = false;
            }
#if (DEBUG && LOCALMBOX)
            Debugger.Break();
#endif
            return false;
#endif
        }

        static internal void
            Kill()
        {
            _messageBox?.Close();
            _messageBox = null;
        }

        static internal void
            Restart()
        {
            _restart = true;
            Kill();
        }

        // make MessageBox modal from a worker thread
        static internal MessageBoxResult
            ShowDialog(string strMessage, string strTitle = null, MessageBoxButton? buttons = null, ILocalWindow owner = null)
        {
            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true))
            {
                MessageBox.Show(strMessage + "\n(LocalMbox: application shutting down.)", strTitle, buttons ?? MessageBoxButton.OK);
                return MessageBoxResult.None;
            }

            if (null == owner)
            {
                Util.UIthread(99916, () =>
                owner =
                    (Statics.TopWindow is IModalWindow)
                    ? Statics.TopWindow
                    : (ILocalWindow)Application.Current.MainWindow);
            } 

            if (owner?.LocalIsClosed ?? false)
                owner = null;

            var msgBoxRet = MessageBoxResult.None;
            
            do
            {
                _restart = false;

                Util.UIthread(99888, () =>
                    msgBoxRet = MainWindow.WithMainWindow(w => w.ShowMessagebox(strMessage, strTitle, buttons)));

                if (_restart)
                    Util.Block(250);
            }
            while (_restart);

            _messageBox = null;
            return msgBoxRet;
        }

        static decimal
            _nLastAssertLoc = -1;
        static DateTime
            _dtLastAssert = DateTime.MinValue;
        static LocalMbox
            _messageBox = null;
        static bool
            _restart = false;
    }
}
