using System.IO;
using System.Linq;
using System.Windows;
using System;
using System.Reactive.Linq;
using System.Text;

namespace DoubleFile
{
    partial class LV_ProjectVM
    {
        static internal IObservable<Tuple<bool, int>>   // bool is a no-op: generic placeholder
            Modified => _modified;
        static readonly LocalSubject<bool> _modified = new LocalSubject<bool>();
        static readonly int _nModifiedOnNextAssertLoc = 99838;
        internal void
            SetModified() => _modified.LocalOnNext(false, _nModifiedOnNextAssertLoc);

        internal int
            CanLoadCount => ItemsCast.Where(item => item.CanLoad).Count();

        internal bool
            Unsaved
        {
            get { return _unsaved; }
            set
            {
                if (value)
                    SetModified();

                _unsaved = value; 
            }
        }
        bool _unsaved = false;

        internal
            LV_ProjectVM(LV_ProjectVM lvProjectVM = null)
        {
            if (null == lvProjectVM)
                return;

            Add(lvProjectVM.ItemsCast.Select(lvItemVM => new LVitem_ProjectVM(lvItemVM)), bQuiet: true);
            _unsaved = lvProjectVM.Unsaved;
            RaisePropertyChanged("Visible");
        }

        internal new void
            Add(ListViewItemVM_Base doNotUseThisMethod, bool itIsOverloaded) =>
            // making sure that LVitem_ProjectVM doesn't get interpreted as ListViewItemVM_Base
            // bQuiet is always explicitly named, so renamed it here to be sure
            // the signature remains the same which makes it a valid new: hiding the base method
            Util.Assert(99936, false);

        internal bool
            Add(LVitem_ProjectVM lvItem, bool bQuiet = false)
        {
            if (AlreadyInProject(lvItem.ListingFile))
                return false;

            base.Add(lvItem, bQuiet);
            RaisePropertyChanged("Visible");
            return true;
        }

        internal bool
            AlreadyInProject(string strFilename, LVitem_ProjectVM lvCurrentItem = null, bool bQuiet = false)
        {
            if (string.IsNullOrWhiteSpace(strFilename))
                return false;

            var s = strFilename.ToLower();

            var bAlreadyInProject = ItemsCast.Any(item =>
                (item.ListingFile.ToLower() == s) &&
                (lvCurrentItem != item) &&
                (Util.Assert(99855, null != item)));

            if (bAlreadyInProject &&
                (false == bQuiet))
            {
                MBoxStatic.ShowOverlay("Listing file already in the project.", "Add Listing File");
            }

            return bAlreadyInProject;
        }

        internal bool
            ContainsUnsavedPath(string strPath)
        {
            if (string.IsNullOrWhiteSpace(strPath))
                return false;

            var s = strPath.ToLower();

            return ItemsCast
                .Where(item => (item.SourcePath.ToLower() == s) && item.WouldSave)
                .FirstOnlyAssert(x => MBoxStatic.ShowOverlay("Source path is already set to be scanned.", "Add Listing File"));
        }

        internal void
            EditListingFile()
        {
            SelectedItems()
                .FirstOnlyAssert(lvItem =>
            {
                var lvItemTemp = new LVitem_ProjectVM(lvItem);

                if (FileParse.ksError == lvItemTemp.Status)
                    lvItemTemp.Status = FileParse.ksNotSaved;

                for (;;)
                {
                    var dlg =
                        lvItemTemp.WouldSave
                        ? new WinVolumeNew()
                        : (WinVolumeEditBase)new WinVolumeEdit();

                    dlg.LVitemVolumeTemp = new LVitem_ProjectVM(lvItemTemp);

                    if (false == (dlg.ShowDialog() ?? false))
                        break;  // user canceled

                    lvItemTemp = new LVitem_ProjectVM(dlg.LVitemVolumeTemp);

                    if (AlreadyInProject(lvItemTemp.ListingFile, lvItem))
                        continue;

                    var dlgEdit = dlg.As<WinVolumeEdit>();

                    if (null != dlgEdit)
                    {
                        if (ModifyListingFile(lvItem, lvItemTemp, dlgEdit.formUC_VolumeEdit.DriveLetter))
                            FileParse.ReadHeader(lvItemTemp.ListingFile, ref lvItemTemp);
                        else if (lvItem.LocalEquals(lvItemTemp))
                            break;  // no change to volume group; include y/n: columns that aren't in the listing file

                        Unsaved = true;
                    }
                    else if (FileExists(lvItemTemp.ListingFile) ||
                        ((lvItemTemp.SourcePath != lvItem.SourcePath) && ContainsUnsavedPath(lvItemTemp.SourcePath)))
                    {
                        // WinVolumeNew
                        continue;
                    }

                    lvItem.SubItems = lvItemTemp.SubItems;
                    break;
                }
            });
        }

        internal void
            EditVolumeGroupLabel()
        {
            var dlg = new WinVolumeGroup
            {
                Text =
                    SelectedItems()
                    .Select(lvItem => lvItem.VolumeGroup)
                    .FirstOrDefault()
            };

            if (dlg.ShowDialog() ?? false)
            {
                var strLabel = dlg.Text.Trim();
                bool bUnsaved = false;

                foreach (var lvItem in SelectedItems())
                {
                    if (("" + lvItem.VolumeGroup).Equals(strLabel))
                        continue;

                    lvItem.VolumeGroup = strLabel;
                    bUnsaved = true;
                }

                if (bUnsaved)
                    Unsaved = true;
            }
        }

        internal bool
            FileExists(string strListingFile)
        {
            if (LocalIsoStore.FileExists(strListingFile) &&
                ((false == strListingFile.StartsWith(LocalIsoStore.TempDir)) || FileParse.ValidateFile(strListingFile).Item1))
            {
                MBoxStatic.ShowOverlay("Listing file exists. Please manually delete it using the Save Listing\n" +
                    "File dialog by clicking the icon button after this alert closes.", "New Listing File");

                return true;
            }

            return false;
        }

        bool
            ModifyListingFile(LVitem_ProjectVM lvItem_Orig, LVitem_ProjectVM lvItemVolumeTemp, char driveLetter)
        {
            var bDriveModel_Todo = ("" + lvItem_Orig.DriveModel != "" + lvItemVolumeTemp.DriveModel);
            var bDriveSerial_Todo = ("" + lvItem_Orig.DriveSerial != "" + lvItemVolumeTemp.DriveSerial);
            var bNickname_Todo = ("" + lvItem_Orig.Nickname != "" + lvItemVolumeTemp.Nickname);

            driveLetter = char.ToUpper(driveLetter);

            var driveLetterOrig = lvItem_Orig.SourcePath.ToUpper()[0];
            var bDriveLetter_Todo = (char.IsLetter(driveLetter) && (driveLetter != driveLetterOrig));

            if (false == (bDriveModel_Todo || bDriveSerial_Todo || bNickname_Todo || bDriveLetter_Todo))
                return false;

            if (false == FileParse.ValidateFile(lvItem_Orig.ListingFile).Item1)
            {
                MBoxStatic.ShowOverlay("Bad listing file.", "Edit Listing File");
                return false;
            }

            var strIsoFile = LocalIsoStore.TempDir + Path.GetFileName(lvItem_Orig.ListingFile);

            if (':' == lvItem_Orig.ListingFile[1])
                lvItem_Orig.ListingFile.FileMoveToIso(strIsoFile);

            var strFile_01 = FileParse.StrFile_01(strIsoFile);

            if (LocalIsoStore.FileExists(strFile_01))
                LocalIsoStore.DeleteFile(strFile_01);

            LocalIsoStore.MoveFile(strIsoFile, strFile_01);

            var sbOut = new StringBuilder();

            using (var sr = new StreamReader(LocalIsoStore.OpenFile(strFile_01, FileMode.Open)))
            {
                string strLine = null;

                while ((bDriveModel_Todo || bDriveSerial_Todo || bNickname_Todo) &&
                    (null !=
                    (strLine = sr.ReadLine())))
                {
                    if (strLine.StartsWith(FileParse.ksLineType_Start))
                    {
                        // already past the header: nothing left to replace.
                        Util.Assert(99989, false);
                        sbOut.AppendLine(strLine);
                        break;
                    }

                    var sbLine = new StringBuilder(strLine);

                    Action<string> Replace = s =>
                    {
                        var astr = ("" + sbLine).Split('\t').ToList();

                        Util.Assert(1308.9312m, 3 == astr.Count);

                        while (3 > astr.Count)
                            astr.Add("");

                        astr[2] = s;
                        sbLine = new StringBuilder(string.Join("\t", astr));
                    };

                    if (bDriveModel_Todo &&
                        strLine.StartsWith(FileParse.ksLineType_VolumeInfo_DriveModel))
                    {
                        Replace(lvItemVolumeTemp.DriveModel);
                        bDriveModel_Todo = false;
                    }
                    else if (bDriveSerial_Todo &&
                        strLine.StartsWith(FileParse.ksLineType_VolumeInfo_DriveSerial))
                    {
                        Replace(lvItemVolumeTemp.DriveSerial);
                        bDriveSerial_Todo = false;
                    }
                    else if (bNickname_Todo &&
                        strLine.StartsWith(FileParse.ksLineType_Nickname))
                    {
                        Replace(lvItemVolumeTemp.Nickname);
                        bNickname_Todo = false;
                    }

                    sbOut.AppendLine("" + sbLine);
                }

                Action ModifyDriveLetter = () => { };

                if (bDriveLetter_Todo)
                    ModifyDriveLetter = () => sbOut.Replace("\t" + driveLetterOrig + @":\", "\t" + driveLetter + @":\");

                if (0 < sbOut.Length)
                {
                    ModifyDriveLetter();

                    using (var sw = new StreamWriter(LocalIsoStore.CreateFile(strIsoFile)))
                        sw.Write("" + sbOut);
                }

                using (var sw = new StreamWriter(LocalIsoStore.OpenFile(strIsoFile, FileMode.Append)))
                    Util.CopyStream(sr, sw, (buffer, nRead) =>
                {
                    sbOut.Clear();
                    sbOut.Append(buffer, 0, nRead);
                    ModifyDriveLetter();
                    sbOut.CopyTo(0, buffer, 0, nRead);
                    return buffer;      // from lambda
                });

                bDriveLetter_Todo = false;
            }

            Util.Assert(99987, (false == (bDriveModel_Todo || bDriveSerial_Todo || bNickname_Todo || bDriveLetter_Todo)));

            if (':' == lvItem_Orig.ListingFile[1])
            {
                File.Delete(lvItem_Orig.ListingFile);

                using (var sr = new StreamReader(LocalIsoStore.OpenFile(strIsoFile, FileMode.Open)))
                using (var sw = new StreamWriter(File.OpenWrite(lvItem_Orig.ListingFile)))
                    Util.CopyStream(sr, sw);

                LocalIsoStore.DeleteFile(strIsoFile);
            }

            return true;
        }

        internal void
            RemoveListingFile()
        {
            var strMessage = "";

            if (SelectedItems().Any(lvItem => lvItem.WouldSave))
                strMessage = "Selected listings have";
            else if (Unsaved)
                strMessage = "Project has";

            if ((0 < strMessage.Length) &&
                (MessageBoxResult.Yes !=
                MBoxStatic.ShowOverlay(strMessage + " not been saved. Continue?", "Remove Listing File",
                MessageBoxButton.YesNo)))
            {
                return;
            }

            SelectedItems()
                .ToList()
                .ForEach(lvItem => Items.Remove(lvItem));

            _unsaved = 0 < Items.Count;
            SetModified();
            RaisePropertyChanged("Visible");
            UC_Project.OKtoNavigate_UpdateSaveListingsLink();
        }

        internal void
            ToggleInclude()
        {
            SelectedItems()
                .ForEach(lvItem => lvItem.Include = (false == lvItem.Include));

            Unsaved = true;
        }
    }
}
