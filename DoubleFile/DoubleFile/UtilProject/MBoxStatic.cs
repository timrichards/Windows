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
        static LocalWindow m_LocalMbox = null;

        static double static_nLastAssertLoc = -1;
        static DateTime static_dtLastAssert = DateTime.MinValue;

#if (DEBUG == false)
        static bool static_bAssertUp = false;
#endif

        internal static bool Assert(double nLocation, bool bCondition, string strError_in = null,
            bool bTraceOnly = false)
        {
            if (bCondition) return true;

            if ((static_nLastAssertLoc == nLocation) &&
                ((DateTime.Now - static_dtLastAssert).Seconds < 1))
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

        internal static void MessageBoxKill(string strMatch = null)
        {
            if ((m_LocalMbox != null) &&
                new[] { null, m_LocalMbox.Title }.Contains(strMatch))
            {
                m_LocalMbox.Close();
                m_LocalMbox = null;
            }
        }

        // make MessageBox modal from a worker thread
        internal static MessageBoxResult ShowDialog(string strMessage, string strTitle = null,
            MessageBoxButton? buttons_in = null, LocalWindow owner = null)
        {
            if (GlobalData.static_MainWindow.IsClosed)
            {
                return MessageBoxResult.None;
            }

            UtilProject.UIthread(() => MessageBoxKill());

            var msgBoxRet = MessageBoxResult.None;
            var buttons = buttons_in ?? MessageBoxButton.OK;
            LocalMbox mbox = null;
            
            UtilProject.UIthread(() =>
                mbox = new LocalMbox(owner ?? GlobalData.static_Dialog, strMessage, strTitle, buttons));

            m_LocalMbox = mbox;
            UtilProject.UIthread(() => msgBoxRet = mbox.ShowDialog());

            if (null == m_LocalMbox)
            {
                // cancelled externally
                msgBoxRet = MessageBoxResult.None;
            }
            else
            {
                UtilProject.UIthread(() => MessageBoxKill());
            }

            return msgBoxRet;
        }
    }
}
