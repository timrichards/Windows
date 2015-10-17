using System;
using System.Diagnostics;
using System.Windows;

namespace DoubleFile
{
    static class MBoxStatic
    {
        static internal decimal? FailUp { get; private set; }

        static internal bool
            Fail(decimal nLocation, string strError_in)
        {
            LocalModernWindowBase owner = null;

            Util.UIthread(99607, () => owner = (LocalModernWindowBase)Application.Current?.MainWindow);

            if ((_nLastAssertLoc == nLocation) &&
                (1 > (DateTime.Now - _dtLastAssert).Seconds))
            {
                if ((null != owner) && (FailUp == nLocation))           // bombarded
                    Win32Screen.FlashWindow(owner);

                return false;
            }

            if ((null != owner) && (FailUp == nLocation))               // hit again while up
                return false;

            var strError = "Assertion failed at location " + nLocation + ".";

            if (false == string.IsNullOrWhiteSpace(strError_in))
                strError += "\n\nAdditional information: " + strError_in;

            Util.WriteLine(strError);
#if (DEBUG && (false == LOCALMBOX))
            Debug.Assert(false, strError);
            return false;
#else
            var strErrorOut =
                strError +
                "\n\nPlease discuss this bug at http://sourceforge.net/projects/searchdirlists/.";

            _nLastAssertLoc = nLocation;
            _dtLastAssert = DateTime.Now;

            if ((null != FailUp) ||
                UC_Messagebox.Showing)
            {
                if (owner?.LocalIsClosing ?? true)
                    MessageBox.Show(strErrorOut + "\n(MBoxStatic: there is a local assert box already up.)");
                else
                    Util.UIthread(99595, () => MessageBox.Show(owner, strErrorOut + "\n(MBoxStatic: there is a local assert box already up.)"));
            }
            else
            {
                FailUp = nLocation;
                ShowOverlay(strErrorOut, "DoubleFile Assert", MessageBoxButton.OK);
                FailUp = null;
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
