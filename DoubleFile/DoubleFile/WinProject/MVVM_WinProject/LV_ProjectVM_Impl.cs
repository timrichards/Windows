using System.IO;
using System.Linq;
using System.Windows;
using System;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace DoubleFile
{
    partial class LV_ProjectVM
    {
        public Visibility Visible
        {
            get
            {
                ModernWindow1.WithMainWindow(mainWindow =>
                {
                    mainWindow.ShowLinks(Items.IsEmpty());
                    return false;   // from lambda
                });

                return Items.IsEmpty() ? Visibility.Hidden : Visibility.Visible;
            }
        }

        static internal IObservable<bool>   // bool is a no-op: generic placeholder
            Modified { get { return _modified.AsObservable(); } }
        static readonly Subject<bool> _modified = new Subject<bool>();
        internal void SetModified() { _modified.OnNext(false); }

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

            foreach (var lvItemVM in lvProjectVM.ItemsCast)
                Add(new LVitem_ProjectVM(lvItemVM), bQuiet: true);

            _unsaved = lvProjectVM.Unsaved;
            RaisePropertyChanged("Visible");
        }

        internal bool AlreadyInProject(LVitem_ProjectVM lvCurrentItem, string strFilename = null)
        {
            bool bAlreadyInProject =
                (null != ContainsListingFile(lvCurrentItem, strFilename));

            if (bAlreadyInProject)
                MBoxStatic.ShowDialog("Listing file already in the project.", "Add Listing File");

            return bAlreadyInProject;
        }

        internal void EditListingFile()
        {
            Selected()
                .FirstOnlyAssert(lvItem =>
            {
                var lvItemVolumeTemp = new LVitem_ProjectVM(lvItem);

                if (FileParse.ksError == lvItemVolumeTemp.Status)
                    lvItemVolumeTemp.Status = FileParse.ksNotSaved;

                for (; ; )
                {
                    var dlg =
                        lvItemVolumeTemp.WouldSave
                        ? new WinVolumeNew()
                        : (WinVolumeEditBase)new WinVolumeEdit();

                    dlg.LVitemVolumeTemp = new LVitem_ProjectVM(lvItemVolumeTemp);

                    if (false == (dlg.ShowDialog() ?? false))
                        break;  // user canceled

                    lvItemVolumeTemp = new LVitem_ProjectVM(dlg.LVitemVolumeTemp);

                    if (AlreadyInProject(lvItem, lvItemVolumeTemp.ListingFile))
                        continue;

                    var dlgEdit = dlg as WinVolumeEdit;

                    if (null != dlgEdit)
                    {
                        if (ModifyListingFile(lvItem, lvItemVolumeTemp, dlgEdit.formUC_VolumeEdit.DriveLetter))
                            FileParse.ReadHeader(lvItemVolumeTemp.ListingFile, out lvItemVolumeTemp);
                        else if (lvItem.LocalEquals(lvItemVolumeTemp))
                            break;  // no change to volume group; include y/n: columns that aren't in the listing file

                        Unsaved = true;
                    }
                    else if (FileExists(lvItemVolumeTemp.ListingFile))   // WinVolumeNew
                    {
                        continue;
                    }

                    lvItem.StringValues = lvItemVolumeTemp.StringValues;
                    break;
                }
            });
        }

        internal bool FileExists(string strListingFile)
        {
            if (File.Exists(strListingFile) &&
                ((false == strListingFile.StartsWith(ProjectFile.TempPath)) || FileParse.ValidateFile(strListingFile)))
            {
                MBoxStatic.ShowDialog("Listing file exists. Please manually delete it using the Save Listing\n" +
                    "File dialog by clicking the icon button after this alert closes.", "New Listing File");

                return true;
            }

            return false;
        }

        internal bool NewItem(LVitem_ProjectVM lvItem, bool bQuiet = false)
        {
            return Add(lvItem.StringValues, bQuiet);
        }

        internal override bool Add(string[] arrStr, bool bQuiet = false)
        {
            var lvItem = new LVitem_ProjectVM(arrStr);

            if (false == AlreadyInProject(lvItem))
            {
                base.Add(lvItem, bQuiet);
                RaisePropertyChanged("Visible");
                return true;
            }

            return false;
        }

        internal void RemoveListingFile()
        {
            if (Selected().Any(lvItem => lvItem.WouldSave) &&
                (MBoxStatic.ShowDialog("Selected listings have not been saved. Continue?", "Remove Listing File",
                System.Windows.MessageBoxButton.YesNo) !=
                System.Windows.MessageBoxResult.Yes))
            {
                return;
            }

            Selected()
                .ToList()
                .ForEach(lvItem => Items.Remove(lvItem));

            _unsaved = (false == Items.IsEmpty());
            SetModified();
            RaisePropertyChanged("Visible");
        }

        internal void EditVolumeGroupLabel()
        {
            var dlg = new WinVolumeGroup();

            Selected().First(lvItem => dlg.Text = lvItem.VolumeGroup);

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

        LVitem_ProjectVM ContainsListingFile(LVitem_ProjectVM lvItem_Current, string t)
        {
            if (string.IsNullOrEmpty(t))
                return null;

            var s = t.ToLower();

            return ItemsCast
                .FirstOrDefault(item =>
                    (item.ListingFile.ToLower() == s) &&
                    (lvItem_Current != item));
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

            if (false == FileParse.ValidateFile(lvItem_Orig.ListingFile))
            {
                MBoxStatic.ShowDialog("Bad listing file.", "Edit Listing File");
                return false;
            }

            var strFile_01 = FileParse.StrFile_01(lvItem_Orig.ListingFile);

            if (File.Exists(strFile_01))
                File.Delete(strFile_01);

            File.Move(lvItem_Orig.ListingFile, strFile_01);

            var sbOut = new System.Text.StringBuilder();

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

                    var sbLine = new System.Text.StringBuilder(strLine);

                    System.Action<string> Replace = s =>
                    {
                        var astr = ("" + sbLine).Split('\t').ToList();

                        MBoxStatic.Assert(1308.9312, astr.Count == 3);

                        while (astr.Count < 3)
                            astr.Add("");

                        astr[2] = s;
                        sbLine = new System.Text.StringBuilder(string.Join("\t", astr));
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
                    catch (System.Exception e)
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
