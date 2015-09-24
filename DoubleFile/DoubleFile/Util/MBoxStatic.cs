using System;
using System.Diagnostics;
using System.Windows;

namespace DoubleFile
{
    static class MBoxStatic
    {
        static internal bool FailUp { get; private set; }

        static internal bool
            Fail(decimal nLocation, string strError_in, bool bTraceOnly)
        {
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

            if (FailUp ||
                UC_Messagebox.Showing)
            {
                var owner = (LocalModernWindowBase)Application.Current?.MainWindow;

                if (owner?.LocalIsClosing ?? true)
                    owner = null;

                MessageBox.Show(owner, strErrorOut + "\n(MBoxStatic: there is a local assert box already up.)");
            }
            else
            {
                FailUp = true;
                ShowOverlay(strErrorOut, "DoubleFile Assert", MessageBoxButton.OK);
                FailUp = false;
            }
#if (DEBUG && LOCALMBOX)
            if (Debugger.IsAttached)
                Debugger.Break();
#endif
            return false;
#endif
        }

        // make MessageBox modal from a worker thread
        static internal MessageBoxResult
            AskToCancel(string strTitle) => ShowOverlay(_ksAskToCancel, strTitle, MessageBoxButton.YesNo);

        static internal MessageBoxResult
            ShowOverlay(string strMessage, string strTitle = null, MessageBoxButton? buttons = null, ILocalWindow owner = null)
        {
            MessageBoxResult retVal = MessageBoxResult.None;

            Util.UIthread(99916, () =>
                retVal = ShowOverlay_(strMessage, strTitle, buttons, owner));

            return retVal;
        }
        static MessageBoxResult ShowOverlay_(string strMessage, string strTitle, MessageBoxButton? buttons, ILocalWindow owner)
        {
            if (Application.Current?.Dispatcher.HasShutdownStarted ?? true)
                return MessageBox.Show(strMessage + "\n(MBoxStatic: application shutting down.)", strTitle, buttons ?? MessageBoxButton.OK);

            var mainWindow = (LocalModernWindowBase)Application.Current.MainWindow;

            if (null == mainWindow)
                return MessageBox.Show(strMessage + "\n(MBoxStatic: no main window.)", strTitle, buttons ?? MessageBoxButton.OK);

            return mainWindow.ShowMessagebox(strMessage, strTitle, buttons);
        }

        static readonly string
            _ksAskToCancel = Util.Localized("MBoxStatic_AskToCancel");
        static decimal
            _nLastAssertLoc = -1;
        static DateTime
            _dtLastAssert = DateTime.MinValue;
    }
}
