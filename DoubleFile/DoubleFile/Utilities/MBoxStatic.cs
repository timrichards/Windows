using System;
using System.Linq;
using System.Windows;

namespace DoubleFile
{
    delegate MessageBoxResult MBoxDelegate(
        string strMessage,
        string strTitle = null,
        MessageBoxButton? buttons = null);

    static class MBoxStatic
    {
        static internal LocalMbox MessageBox { get; private set; }

        static double static_nLastAssertLoc = -1;
        static DateTime static_dtLastAssert = DateTime.MinValue;

#if (DEBUG == false)
        static bool static_bAssertUp = false;
#endif

        static internal bool Assert(double nLocation, bool bCondition, string strError_in = null,
            bool bTraceOnly = false)
        {
            if (bCondition)
            {
                return true;
            }

            if ((static_nLastAssertLoc == nLocation) &&
                (1 > (DateTime.Now - static_dtLastAssert).Seconds))
            {
                return false;
            }

            var strError = "Assertion failed at location " + nLocation + ".";

            if (false == string.IsNullOrWhiteSpace(strError_in))
            {
                strError += "\n\nAdditional information: " + strError_in;
            }

            UtilProject.WriteLine(strError);
#if (DEBUG)
            System.Diagnostics.Debug.Assert(false, strError);
#else
            if (static_bAssertUp == false)
            {
                var bTrace = false; // Trace.Listeners.Cast<TraceListener>().Any(i => i is DefaultTraceListener);

                Action messageBox = () =>
                {
                    MBoxStatic.ShowDialog(strError +
                        "\n\nPlease discuss this bug at http://sourceforge.net/projects/searchdirlists/.".PadRight(100),
                        "SearchDirLists Assertion Failure");
                    static_bAssertUp = false;
                };

                if (bTrace)
                {
                    messageBox();
                }
                else if (bTraceOnly == false)
                {
                    static_nLastAssertLoc = nLocation;
                    static_dtLastAssert = DateTime.Now;
                    static_bAssertUp = true;
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
        static internal MessageBoxResult ShowDialog(string strMessage, string strTitle = null,
            MessageBoxButton? buttons_in = null, LocalWindow owner = null)
        {
            if (App.LocalExit ||
                (null == MainWindow.GetMainWindow()) || 
                MainWindow.GetMainWindow().LocalIsClosed)
            {
                return MessageBoxResult.None;
            }

            UtilProject.UIthread(() => MessageBoxKill());

            var msgBoxRet = MessageBoxResult.None;
            var buttons = buttons_in ?? MessageBoxButton.OK;
            
            UtilProject.UIthread(() =>
                MessageBox = new LocalMbox(owner ?? MainWindow.GetMainWindow(), strMessage, strTitle, buttons));

            UtilProject.UIthread(() => msgBoxRet = MessageBox.ShowDialog());

            if (null == MessageBox)
                msgBoxRet = MessageBoxResult.None;          // cancelled externally
            else
                UtilProject.UIthread(() => MessageBoxKill());

            MessageBox = null;
            return msgBoxRet;
        }
    }
}
