using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Controls;
using System.Reflection;
using System.Linq.Expressions;
using Drawing = System.Drawing;

#if (true)
namespace SearchDirLists
{
    partial class VolumeTabViewModel
    {
        bool AddVolume()
        {
            bool bFileOK = false;

            if (SaveFields(false) == false)
            {
                return false;
            }

            if (Utilities.StrValid(CBSaveAs.S) == false)
            {
                gd.FormError(m_app.xaml_cbSaveAs, "Must have a file to load or save directory listing to.", "Volume Save As");
                return false;
            }

            if (mo_lvVolViewModelList.ContainsSaveAs(CBSaveAs.S))
            {
                gd.FormError(m_app.xaml_cbSaveAs, "File already in use in list of volumes.", "Volume Save As");
                return false;
            }

            bool bOpenedFile = (Utilities.StrValid(CBPath.S) == false);

            if (File.Exists(CBSaveAs.S) && Utilities.StrValid(CBPath.S))
            {
                gd.m_blinky.Go(m_app.xaml_cbSaveAs, clr: Drawing.Color.Red);

                if (Utilities.MBox(CBSaveAs + ("\nalready exists. Overwrite?").PadRight(100), "Volume Save As", MBoxBtns.YesNo)
                    != MBoxRet.Yes)
                {
                    gd.m_blinky.Go(m_app.xaml_cbVolumeName, clr: Drawing.Color.Yellow, Once: true);
                    m_app.xaml_cbVolumeName.Text = String.Empty;
                    gd.m_blinky.Go(m_app.xaml_cbPath, clr: Drawing.Color.Yellow, Once: true);
                    CBPath.S = String.Empty;
                    Utilities.Assert(1308.9306, SaveFields(false));
                }
            }

            if ((File.Exists(CBSaveAs.S) == false) && (Utilities.StrValid(CBPath.S) == false))
            {
                gd.m_blinky.Go(m_app.xaml_cbPath, clr: Drawing.Color.Red);
                Utilities.MBox("Must have a path or existing directory listing file.", "Volume Source Path");
                gd.m_blinky.Go(m_app.xaml_cbPath, clr: Drawing.Color.Red, Once: true);
                return false;
            }

            if (Utilities.StrValid(CBPath.S) && (Directory.Exists(CBPath.S) == false))
            {
                gd.m_blinky.Go(m_app.xaml_cbPath, clr: Drawing.Color.Red);
                Utilities.MBox("Path does not exist.", "Volume Source Path");
                gd.m_blinky.Go(m_app.xaml_cbPath, clr: Drawing.Color.Red, Once: true);
                return false;
            }

            String strStatus = "Not Saved";

            if (File.Exists(CBSaveAs.S))
            {
                if (Utilities.StrValid(CBPath.S) == false)
                {
                    bFileOK = ReadHeader();

                    if (bFileOK)
                    {
                        strStatus = Utilities.mSTRusingFile;
                    }
                    else
                    {
                        if (Utilities.StrValid(CBPath.S))
                        {
                            strStatus = "File is bad. Will overwrite.";
                        }
                        else
                        {
                            gd.m_blinky.Go(m_app.xaml_cbPath, clr: Drawing.Color.Red);
                            Utilities.MBox("File is bad and path does not exist.", "Volume Source Path");
                            gd.m_blinky.Go(m_app.xaml_cbPath, clr: Drawing.Color.Red, Once: true);
                            return false;
                        }
                    }
                }
                else
                {
                    strStatus = "Will overwrite.";
                }
            }

            if ((bFileOK == false) && (mo_lvVolViewModelList.ContainsPath(CBPath.S)))
            {
                gd.FormError(m_app.xaml_cbPath, "Path already added.", "Volume Source Path");
                return false;
            }

            if (Utilities.StrValid(CBVolumeName.S))
            {
                if (mo_lvVolViewModelList.ContainsVolumeName(CBVolumeName.S))
                {
                    gd.m_blinky.Go(m_app.xaml_cbVolumeName, clr: Drawing.Color.Red);

                    if (Utilities.MBox("Nickname already in use. Use it for more than one volume?", "Volume Save As", MBoxBtns.YesNo)
                        != MBoxRet.Yes)
                    {
                        gd.m_blinky.Go(m_app.xaml_cbVolumeName, clr: Drawing.Color.Red, Once: true);
                        return false;
                    }
                }
            }
            else if (bOpenedFile == false)
            {
                gd.m_blinky.Go(m_app.xaml_cbVolumeName, clr: Drawing.Color.Red);

                if (Utilities.MBox("Continue without entering a nickname for this volume?", "Volume Save As", MBoxBtns.YesNo)
                    != MBoxRet.Yes)
                {
                    gd.m_blinky.Go(m_app.xaml_cbVolumeName, clr: Drawing.Color.Red, Once: true);
                    return false;
                }
            }

            {
                SDL_ListViewItem lvItem = new SDL_ListViewItem(new String[] { CBVolumeName.S, CBPath.S, CBSaveAs.S, strStatus, "Yes" });

                if (bFileOK == false)
                {
                    lvItem.Name = CBPath.S;    // indexing by path, only for unsaved volumes
                }

                m_app.xaml_lvVolumesMain.Items.Add(lvItem);
            }

            //            form_btnSaveDirList.Enabled = true;

            return bFileOK;
        }

        bool ReadHeader()
        {
            if (Utilities.ValidateFile(CBSaveAs.S) == false)
            {
                return false;
            }

            using (StreamReader file = new StreamReader(CBSaveAs.S))
            {
                String line = null;

                if ((line = file.ReadLine()) == null) return false;
                if ((line = file.ReadLine()) == null) return false;
                if (line.StartsWith(Utilities.mSTRlineType_Nickname) == false) return false;

                String[] arrLine = line.Split('\t');
                String strName = String.Empty;

                if (arrLine.Length > 2) strName = arrLine[2];
                CBVolumeName.S = strName;
                if ((line = file.ReadLine()) == null) return false;
                if (line.StartsWith(Utilities.mSTRlineType_Path) == false) return false;
                arrLine = line.Split('\t');
                if (arrLine.Length < 3) return false;
                CBPath.S = arrLine[2];
            }

            return SaveFields(false);
        }

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
            CBVolumeName.S = Utilities.NotNull(m_app.xaml_cbVolumeName.Text).Trim();
            CBPath.S = Utilities.NotNull(m_app.xaml_cbPath.Text).Trim();

            if (Utilities.StrValid(CBPath.S))
            {
                CBPath.S += Path.DirectorySeparatorChar;

                String str = FormatPath(CBPath.S, m_app.xaml_cbPath, bFailOnDirectory);

                if (str != null)
                {
                    CBPath.S = str;
                }
                else
                {
                    return false;
                }
            }

            if (Utilities.StrValid(m_app.xaml_cbSaveAs.Text))
            {
                try
                {
                    CBSaveAs.S = Path.GetFullPath(m_app.xaml_cbSaveAs.Text.Trim());
                }
                catch
                {
                    gd.FormError(m_app.xaml_cbSaveAs, "Error in save listings filename.", "Save Fields");
                    return false;
                }

                if (Directory.Exists(Path.GetDirectoryName(CBSaveAs.S)) == false)
                {
                    gd.FormError(m_app.xaml_cbSaveAs, "Directory to save listings to doesn't exist.", "Save Fields");
                    return false;
                }

                if (Directory.Exists(CBSaveAs.S))
                {
                    gd.FormError(m_app.xaml_cbSaveAs, "Must specify save filename. Only directory entered.", "Save Fields");
                    return false;
                }

                String str = FormatPath(CBSaveAs.S, m_app.xaml_cbSaveAs, bFailOnDirectory);

                if (str != null)
                {
                    CBSaveAs.S = str;
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
