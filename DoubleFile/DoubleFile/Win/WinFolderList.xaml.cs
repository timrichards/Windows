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
            {"large"  , "ANOVA weighted large" },   // These are all menu case (sentence case), even though
            {"small"  , "ANOVA weighted small" },   // M:UI makes them all caps: future proof.
            {"random" , "ANOVA weighted random"},
            {"unique" , "Unique"               },
            {"sameVol", "Same volume"          },
            {"clones" , "Clones"               }
        });

        internal const string FolderListLarge = "ANOVA weighted large";

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
