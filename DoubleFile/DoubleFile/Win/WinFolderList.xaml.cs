using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFolderList.xaml
    /// </summary>
    partial class WinFolderList
    {
        static internal readonly IDictionary<string, string>
            FolderListFragments = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
        {
            {FolderListLarge  , "ANOVA weighted large" },   // These are all menu case (sentence case), even though
            {FolderListSmall  , "ANOVA weighted small" },   // M:UI makes them all caps: future proof.
            {FolderListRandom , "ANOVA weighted random"},
            {FolderListUnique , "Unique"               },
            {FolderListSameVol, "Same volume"          },
            {FolderListClones , "Clones"               }
        });

        internal const string FolderListLarge = "large";
        internal const string FolderListSmall = "small";
        internal const string FolderListRandom = "random";
        internal const string FolderListUnique = "unique";
        internal const string FolderListSameVol = "sameVol";
        internal const string FolderListClones = "clones";

        public WinFolderList()
        {
            InitializeComponent();            
        }

        protected override void LocalFragmentNavigation(string strFragment)
        {
            DataContext = 
                _winFolderListVM =
                new WinFolderListVM(strFragment)
                .Init();

            LocalTitle =
                new CultureInfo("en-US", false).TextInfo            // future proof to title case
                .ToTitleCase(FolderListFragments[strFragment]);
        }

        protected override void LocalNavigatedFrom()
        {
            DataContext =
                _winFolderListVM =
                null;
        }

        WinFolderListVM
            _winFolderListVM = null;
    }
}
