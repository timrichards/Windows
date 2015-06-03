using System;
using System.Linq;
using System.Windows;

namespace DoubleFile
{
    static class MBoxStatic
    {
        static internal LocalMbox MessageBox { get; private set; }

#if (DEBUG == false)
        static bool _bAssertUp = false;
#endif

        static internal bool Assert(double nLocation, bool bCondition, string strError_in = null,
            bool bTraceOnly = false)
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
#if (DEBUG)
            System.Diagnostics.Debug.Assert(false, strError);
#else
            if (_bAssertUp == false)
            {
                var bTrace = false; // Trace.Listeners.Cast<TraceListener>().Any(i => i is DefaultTraceListener);

                Action messageBox = () =>
                {
                    MBoxStatic.ShowDialog(strError +
                        "\n\nPlease discuss this bug at http://sourceforge.net/projects/searchdirlists/.".PadRight(100),
                        "SearchDirLists Assertion Failure");
                    _bAssertUp = false;
                };

                if (bTrace)
                {
                    messageBox();
                }
                else if (bTraceOnly == false)
                {
                    _nLastAssertLoc = nLocation;
                    _dtLastAssert = DateTime.Now;
                    _bAssertUp = true;
                    new System.Threading.Thread(new System.Threading.ThreadStart(messageBox)).Start();
                }
            }
#endif
            return false;
        }

        static internal void MessageBoxKill(string strMatch = null)
        {
            if ((MessageBox != null) &&
                new[] { null, MessageBox.Title }.Contains(strMatch))
            {
                MessageBox.Close();
                MessageBox = null;
            }
        }

        // make MessageBox modal from a worker thread
        static internal MessageBoxResult
            ShowDialog(string strMessage, string strTitle = null, MessageBoxButton? buttons_in = null, ILocalWindow owner = null)
        {
            var mainWindow = App.LocalMainWindow;

            if (App.LocalExit ||
                (null == mainWindow) || 
                mainWindow.LocalIsClosed)
            {
                return MessageBoxResult.None;
            }

            Util.UIthread(() => MessageBoxKill());

            if ((null != owner) &&
                owner.LocalIsClosed)
            {
                return MessageBoxResult.None;
            }

            var msgBoxRet = MessageBoxResult.None;
            var buttons = buttons_in ?? MessageBoxButton.OK;

            Util.UIthread(() =>
                MessageBox = new LocalMbox(owner ?? mainWindow, strMessage, strTitle, buttons));

            bool bCompleted = false;

            Util.UIthread(() =>
            {
                msgBoxRet = MessageBox.ShowDialog();
                bCompleted = true;
            });

            while (false == bCompleted)
                Util.Block(100);

            if (null == MessageBox)
                msgBoxRet = MessageBoxResult.None;          // canceled externally
            else
                Util.UIthread(() => MessageBoxKill());

            return msgBoxRet;
        }

        static double
            _nLastAssertLoc = -1;
        static DateTime
            _dtLastAssert = DateTime.MinValue;
    }
}
