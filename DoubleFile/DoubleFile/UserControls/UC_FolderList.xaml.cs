using System.Collections.Generic;
using System.Globalization;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFolderList.xaml
    /// </summary>
    partial class UC_FolderList
    {
        static internal readonly IReadOnlyDictionary<string, string>
            FolderListFragments = new Dictionary<string, string>
        {
            {FolderListLarge  , "ANOVA weighted large" },   // These are all menu case (sentence case), even though
            {FolderListSmall  , "ANOVA weighted small" },   // M:UI makes them all caps: future proof.
            {FolderListRandom , "ANOVA weighted random"}
        };

        internal const string FolderListLarge = "large";
        internal const string FolderListSmall = "small";
        internal const string FolderListRandom = "random";

        public UC_FolderList()
        {
            InitializeComponent();            
        }

        protected override void LocalNavigatedTo() =>
            _bNavigatedTo = true;

        protected override void LocalFragmentNavigation(string strFragment)
        {
            if (false == _bNavigatedTo)
                _bNicknames = formChk_Nicknames.IsChecked ?? false;

            _bNavigatedTo = false;

            DataContext = 
                _winFolderListVM =
                new UC_FolderListVM(strFragment) { UseNicknames = _bNicknames }
                .Init();

            LocalTitle =
                new CultureInfo("en-US", false).TextInfo            // future proof to title case
                .ToTitleCase(FolderListFragments[strFragment]);
        }

        protected override void LocalNavigatedFrom()
        {
            _bNicknames = formChk_Nicknames.IsChecked ?? false;
            _winFolderListVM.Dispose();

            DataContext =
                _winFolderListVM =
                null;
        }

        UC_FolderListVM
            _winFolderListVM = null;
        bool
            _bNicknames = false;
        bool
            _bNavigatedTo = false;
    }
}
