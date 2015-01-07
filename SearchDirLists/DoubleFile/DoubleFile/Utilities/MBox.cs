using System;
using System.Linq;
using System.Windows;

namespace DoubleFile
{

    delegate MessageBoxResult MBoxDelegate(string strMessage, string strTitle = null, MessageBoxButton? buttons = null);

    class MBox
    {
        static System.Windows.Window m_form1MessageBoxOwner = null;

        static double static_nLastAssertLoc = -1;
        static DateTime static_dtLastAssert = DateTime.MinValue;

#if (DEBUG == false)
        static bool static_bAssertUp = false;
#endif

        internal static bool Assert(double nLocation, bool bCondition, string strError_in = null, bool bTraceOnly = false)
        {
            if (bCondition) return true;

            if ((static_nLastAssertLoc == nLocation) && ((DateTime.Now - static_dtLastAssert).Seconds < 1))
            {
                return false;
            }

            string strError = "Assertion failed at location " + nLocation + ".";

            if (false == string.IsNullOrWhiteSpace(strError_in))
            {
                strError += "\n\nAdditional information: " + strError_in;
            }

            Utilities.WriteLine(strError);
#if (DEBUG)
            System.Diagnostics.Debug.Assert(false, strError);
#else
            if (static_bAssertUp == false)
            {
                bool bTrace = false; // Trace.Listeners.Cast<TraceListener>().Any(i => i is DefaultTraceListener);

                Action messageBox = new Action(() =>
                {
                    MBox.ShowDialog(strError + "\n\nPlease discuss this bug at http://sourceforge.net/projects/searchdirlists/.".PadRight(100), "SearchDirLists Assertion Failure");
                    static_bAssertUp = false;
                });

                if (bTrace)
                {
                    messageBox();
                }
                else if (bTraceOnly == false)
                {
                    static_nLastAssertLoc = nLocation;
                    static_dtLastAssert = DateTime.Now;
                    static_bAssertUp = true;
                    new Thread(new ThreadStart(messageBox)).Start();
                }
            }
#endif
            return false;
        }

        internal static void MessageBoxKill(string strMatch = null)
        {
            if ((m_form1MessageBoxOwner != null) && new string[] { null, m_form1MessageBoxOwner.Title }.Contains(strMatch))
            {
                m_form1MessageBoxOwner.Close();
                m_form1MessageBoxOwner = null;
                GlobalData.static_wpfOrForm.Activate();
            }
        }

        // make MessageBox modal from a worker thread
        internal static MessageBoxResult ShowDialog(string strMessage, string strTitle = null, MessageBoxButton? buttons_in = null)
        {
            if (GlobalData.AppExit)
            {
                return MessageBoxResult.None;
            }

            if (GlobalData.static_wpfOrForm.Dispatcher.CheckAccess() == false) { return (MessageBoxResult)GlobalData.static_wpfOrForm.Dispatcher.Invoke(new MBoxDelegate(ShowDialog), new object[] { strMessage, strTitle, buttons_in }); }

            MessageBoxKill();
            m_form1MessageBoxOwner = new Window();
            m_form1MessageBoxOwner.Owner = GlobalData.static_wpfOrForm;
            m_form1MessageBoxOwner.Title = strTitle;
            m_form1MessageBoxOwner.Icon = GlobalData.static_wpfOrForm.Icon;

            MessageBoxButton buttons = (buttons_in != null) ? buttons_in.Value : MessageBoxButton.OK;
            MessageBoxResult msgBoxRet = (MessageBoxResult)MessageBox.Show(m_form1MessageBoxOwner, strMessage.PadRight(100), strTitle, (MessageBoxButton)buttons);

            if (m_form1MessageBoxOwner != null)
            {
                MessageBoxKill();
                return msgBoxRet;
            }

            // cancelled externally
            return MessageBoxResult.None;
        }
    }
}
