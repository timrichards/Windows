using System;
using System.Collections.Generic;
using System.Windows;

namespace DoubleFile
{
    delegate void CreateFileDictStatusDelegate(bool bDone = false, double nProgress = double.NaN);

    class CreateFileDictProcess
    {
        internal CreateFileDictProcess(GlobalData_Base gd_in,
            LV_ProjectVM lvProjectVM)
        {
            if (lvProjectVM == null)
            {
                return;
            }

            gd = gd_in;

            m_winProgress = new WinProgress();
            m_winProgress.InitProgress(new string[] { ksProgressKey }, new string[] { ksProgressKey });
            m_winProgress.WindowTitle = ksProgressKey;
            m_winProgress.WindowClosingCallback = (() =>
            {
                if (gd.FileDictionary == null)
                {
                    return true;
                }

                if (gd.FileDictionary.IsAborted)
                {
                    return true;
                }

                if (m_bCompleted)
                {
                    return true;
                }

                if (MBox.ShowDialog("Do you want to cancel?", ksProgressKey,
                    MessageBoxButton.YesNo) ==
                    MessageBoxResult.Yes)
                {
                    gd.FileDictionary.Abort();
                    return true;
                }

                return false;
            });


            gd.FileDictionary = new CreateFileDictionary(lvProjectVM, 
                CreateFileDictStatusCallback).DoThreadFactory();
            m_winProgress.ShowDialog();
        }

        internal void CreateFileDictStatusCallback(bool bDone = false, double nProgress = double.NaN)
        {
            UtilProject.CheckAndInvoke(new Action(() =>
            {
                if (gd.WindowClosed || (gd.FileDictionary == null) || gd.FileDictionary.IsAborted)
                {
                    m_winProgress.Aborted = true;
                    return;
                }

                if (bDone)
                {
                    m_winProgress.SetCompleted(ksProgressKey);
                    m_bCompleted = true;
                }
                else if (nProgress >= 0)
                {
                    m_winProgress.SetProgress(ksProgressKey, nProgress);
                }
            }));
        }

        bool m_bCompleted = false;
        readonly GlobalData_Base gd = null;
        WinProgress m_winProgress = null;
        const string ksProgressKey = "Creating file dictionary";
    }
}
