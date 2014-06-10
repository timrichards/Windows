using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Controls;
using System.Reflection;
using System.Linq.Expressions;

#if (true)
namespace SearchDirLists
{
    partial class LVvolViewModel
    {
        String FormatPath(String strPath, Control ctl, bool bFailOnDirectory = true)
        {
            if (Directory.Exists(Path.GetFullPath(strPath)))
            {
                String strCapDrive = strPath.Substring(0, strPath.IndexOf(":" + Path.DirectorySeparatorChar) + 2);

                strPath = Path.GetFullPath(strPath).Replace(strCapDrive, strCapDrive.ToUpper());

                if (strPath != strCapDrive.ToUpper())
                {
                    strPath = strPath.TrimEnd(Path.DirectorySeparatorChar);
                }
            }
            else if (bFailOnDirectory)
            {
                m_app.xaml_tabControlMain.SelectedItem = m_app.xaml_tabPageVolumes;
                gd.FormError(ctl, "Path does not exist.", "Save Fields");
                return null;
            }

            return strPath.TrimEnd(Path.DirectorySeparatorChar);
        }

        bool SaveFields(bool bFailOnDirectory = true)
        {
            gd.m_strVolumeName = Utilities.NotNull(CBVolumeName).Trim();
            CBPath = Utilities.NotNull(CBPath).Trim();

            if (Utilities.StrValid(CBPath))
            {
                CBPath += Path.DirectorySeparatorChar;

                String str = FormatPath(CBPath, m_app.xaml_cbPath, bFailOnDirectory);

                if (str != null)
                {
                    CBPath = str;
                }
                else
                {
                    return false;
                }
            }

            if (Utilities.StrValid(CBSaveAs))
            {
                try
                {
                    CBSaveAs = gd.m_strSaveAs = Path.GetFullPath(CBSaveAs.Trim());
                }
                catch
                {
                    gd.FormError(m_app.xaml_cbSaveAs, "Error in save listings filename.", "Save Fields");
                    return false;
                }

                if (Directory.Exists(Path.GetDirectoryName(gd.m_strSaveAs)) == false)
                {
                    gd.FormError(m_app.xaml_cbSaveAs, "Directory to save listings to doesn't exist.", "Save Fields");
                    return false;
                }

                if (Directory.Exists(gd.m_strSaveAs))
                {
                    gd.FormError(m_app.xaml_cbSaveAs, "Must specify save filename. Only directory entered.", "Save Fields");
                    return false;
                }

                String str = FormatPath(CBSaveAs, m_app.xaml_cbSaveAs, bFailOnDirectory);

                if (str != null)
                {
                    CBSaveAs = str;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
#endif
