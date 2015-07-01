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
        public Visibility Visible
        {
            get
            {
                MainWindow.WithMainWindowA(mainWindow =>
                    mainWindow.ShowLinks(Items.IsEmpty()));

                return Items.IsEmpty() ? Visibility.Hidden : Visibility.Visible;
            }
        }

        static internal IObservable<Tuple<bool, int>>   // bool is a no-op: generic placeholder
            Modified { get { return _modified.AsObservable(); } }
        static readonly LocalSubject<bool> _modified = new LocalSubject<bool>();
        static readonly int _nModifiedOnNextAssertLoc = 99840;
        internal void SetModified() { _modified.LocalOnNext(false, _nModifiedOnNextAssertLoc); }

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

        internal LV_ProjectVM(LV_ProjectVM lvProjectVM = null)
        {
            if (null == lvProjectVM)
                return;

            Add(lvProjectVM.ItemsCast.Select(lvItemVM => new LVitem_ProjectVM(lvItemVM)), bQuiet: true);
            _unsaved = lvProjectVM.Unsaved;
            RaisePropertyChanged("Visible");
        }

        internal bool AlreadyInProject(string strFilename, LVitem_ProjectVM lvCurrentItem = null, bool bQuiet = false)
        {
            if (string.IsNullOrEmpty(strFilename))
                return false;

            var s = strFilename.ToLower();

            var bAlreadyInProject = ItemsCast.Any(item =>
                (item.ListingFile.ToLower() == s) &&
                (lvCurrentItem != item) &&
                (MBoxStatic.Assert(99855, null != item)));

            if (bAlreadyInProject &&
                (false == bQuiet))
            {
                MBoxStatic.ShowDialog("Listing file already in the project.", "Add Listing File");
            }

            return bAlreadyInProject;
        }

        internal void EditListingFile()
        {
            Selected()
                .FirstOnlyAssert(lvItem =>
            {
                var lvItemTemp = new LVitem_ProjectVM(lvItem);

                if (FileParse.ksError == lvItemTemp.Status)
                    lvItemTemp.Status = FileParse.ksNotSaved;

                for (; ; )
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

                    var dlgEdit = dlg as WinVolumeEdit;

                    if (null != dlgEdit)
                    {
                        if (ModifyListingFile(lvItem, lvItemTemp, dlgEdit.formUC_VolumeEdit.DriveLetter))
                            FileParse.ReadHeader(lvItemTemp.ListingFile, out lvItemTemp);
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

                    lvItem.StringValues = lvItemTemp.StringValues;
                    break;
                }
            });
        }

        internal bool FileExists(string strListingFile)
        {
            if (File.Exists(strListingFile) &&
                ((false == strListingFile.StartsWith(ProjectFile.TempPath)) || FileParse.ValidateFile(strListingFile).Item1))
            {
                MBoxStatic.ShowDialog("Listing file exists. Please manually delete it using the Save Listing\n" +
                    "File dialog by clicking the icon button after this alert closes.", "New Listing File");

                return true;
            }

            return false;
        }

        internal new void Add(ListViewItemVM_Base doNotUseThisMethod, bool itIsOverloaded)
        {
            // making sure that LVitem_ProjectVM doesn't get interpreted as ListViewItemVM_Base
            // bQuiet is always explicitly named, so renamed it here to be sure
            // the signature remains the same which makes it a valid new: hiding the base method
            MBoxStatic.Assert(99936, false);
        }

        internal bool Add(LVitem_ProjectVM lvItem, bool bQuiet = false)
        {
            if (AlreadyInProject(lvItem.ListingFile))
                return false;

            base.Add(lvItem, bQuiet);
            RaisePropertyChanged("Visible");
            return true;
        }

        internal void RemoveListingFile()
        {
            if (Selected().Any(lvItem => lvItem.WouldSave) &&
                (System.Windows.MessageBoxResult.Yes !=
                MBoxStatic.ShowDialog("Selected listings have not been saved. Continue?", "Remove Listing File",
                System.Windows.MessageBoxButton.YesNo)))
            {
                return;
            }

            Selected()
                .ToList()
                .ForEach(lvItem => Items.Remove(lvItem));

            _unsaved = (false == Items.IsEmpty());
            SetModified();
            RaisePropertyChanged("Visible");
            WinProject.OKtoNavigate_UpdateSaveListingsLink();
        }

        internal void EditVolumeGroupLabel()
        {
            var dlg = new WinVolumeGroup
            {
                Text =
                    Selected()
                    .Select(lvItem => lvItem.VolumeGroup)
                    .FirstOrDefault()
            };

            if (dlg.ShowDialog() ?? false)
            {
                var strLabel = dlg.Text.Trim();
                bool bUnsaved = false;

                foreach (var lvItem in Selected())
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

        internal void ToggleInclude()
        {
            Selected()
                .ForEach(lvItem => lvItem.Include = (false == lvItem.Include));

            Unsaved = true;
        }

        internal bool ContainsUnsavedPath(string strPath)
        {
            if (string.IsNullOrEmpty(strPath))
                return false;

            var s = strPath.ToLower();

            return ItemsCast
                .Where(item => (item.SourcePath.ToLower() == s) && item.WouldSave)
                .FirstOnlyAssert(x => MBoxStatic.ShowDialog("Source path is already set to be scanned.", "Add Listing File"));
        }

        bool ModifyListingFile(LVitem_ProjectVM lvItem_Orig, LVitem_ProjectVM lvItemVolumeTemp, char driveLetter)
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
                MBoxStatic.ShowDialog("Bad listing file.", "Edit Listing File");
                return false;
            }

            var strFile_01 = FileParse.StrFile_01(lvItem_Orig.ListingFile);

            if (File.Exists(strFile_01))
                File.Delete(strFile_01);

            File.Move(lvItem_Orig.ListingFile, strFile_01);

            var sbOut = new StringBuilder();

            using (var reader = File.OpenText(strFile_01))
            {
                string strLine = null;

                while ((bDriveModel_Todo || bDriveSerial_Todo || bNickname_Todo) &&
                    (null != 
                    (strLine = reader.ReadLine())))
                {
                    if (strLine.StartsWith(FileParse.ksLineType_Start))
                    {
                        // already past the header: nothing left to replace.
                        MBoxStatic.Assert(99989, false);
                        sbOut.AppendLine(strLine);
                        break;
                    }

                    var sbLine = new StringBuilder(strLine);

                    System.Action<string> Replace = s =>
                    {
                        var astr = ("" + sbLine).Split('\t').ToList();

                        MBoxStatic.Assert(1308.9312m, astr.Count == 3);

                        while (astr.Count < 3)
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

                if (sbOut.Length > 0)
                {
                    File.WriteAllText(lvItem_Orig.ListingFile, "" + sbOut);
                }
                else
                {
                    MBoxStatic.Assert(99988, false == File.Exists(lvItem_Orig.ListingFile));

                    try
                    {
                        File.Delete(lvItem_Orig.ListingFile);
                    }
                    catch (Exception e)
                    {
                        MBoxStatic.ShowDialog("Couldn't overwrite file.\n" + e.GetBaseException().Message, "Edit Listing File");
                        return false;
                    }
                }

                using (var fileWriter = File.AppendText(lvItem_Orig.ListingFile))
                {
                    const int kBufSize = 1024 * 1024 * 4;
                    var buffer = new char[kBufSize];
                    var nRead = 0;

                    while (0 <
                        (nRead = reader.Read(buffer, 0, kBufSize)))
                    {
                        sbOut.Clear();
                        sbOut.Append(buffer, 0, nRead);

                        if (bDriveLetter_Todo)
                            sbOut.Replace("\t" + driveLetterOrig + @":\", "\t" + driveLetter + @":\");

                        fileWriter.Write("" + sbOut);
                    }
                }

                bDriveLetter_Todo = false;
            }

            MBoxStatic.Assert(99987, (false == (bDriveModel_Todo || bDriveSerial_Todo || bNickname_Todo || bDriveLetter_Todo)));
            return true;
        }
    }
}
