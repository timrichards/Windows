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
                var owner = (LocalModernWindowBase)Application.Current?.MainWindow;

                if (owner?.LocalIsClosing ?? true)
                    owner = null;

                MessageBox.Show(owner, strErrorOut + "\n(LocalMbox: there is a local assert box already up.)");
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

        // make MessageBox modal from a worker thread
        static internal MessageBoxResult
            ShowDialog(string strMessage, string strTitle = null, MessageBoxButton? buttons = null, ILocalWindow owner = null)
        {
            MessageBoxResult retVal = MessageBoxResult.None;

            Util.UIthread(99916, () =>
                retVal = ShowDialog_(strMessage, strTitle, buttons, owner));

            return retVal;
        }
        static MessageBoxResult ShowDialog_(string strMessage, string strTitle, MessageBoxButton? buttons, ILocalWindow owner)
        {
            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true))
                return MessageBox.Show(strMessage + "\n(LocalMbox: application shutting down.)", strTitle, buttons ?? MessageBoxButton.OK);

            return MainWindow.WithMainWindow(w => w.ShowMessagebox(strMessage, strTitle, buttons));
        }

        static decimal
            _nLastAssertLoc = -1;
        static DateTime
            _dtLastAssert = DateTime.MinValue;
    }
}
