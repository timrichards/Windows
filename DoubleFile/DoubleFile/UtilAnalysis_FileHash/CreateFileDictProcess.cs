using System;
using System.Collections.Generic;

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
            gd_old = GlobalData.Instance;

            m_winProgress = new WinProgress();
            m_winProgress.InitProgress(new string[] { ksProgressKey }, new string[] { ksProgressKey });
            m_winProgress.WindowTitle = ksProgressKey;

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
                }
                else if (nProgress >= 0)
                {
                    m_winProgress.SetProgress(ksProgressKey, nProgress);
                }
            }));
        }

        readonly GlobalData_Base gd = null;
        GlobalData gd_old = null;
        WinProgress m_winProgress = null;
        const string ksProgressKey = "Creating file dictionary...";
    }
}
