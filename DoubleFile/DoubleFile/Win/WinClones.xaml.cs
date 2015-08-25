using System.Collections.Generic;
using System.Globalization;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFolderList.xaml
    /// </summary>
    partial class UC_Clones
    {
        static internal readonly IReadOnlyDictionary<string, string>
            FolderListFragments = new Dictionary<string, string>
        {
            {FolderListSolitary,    "Solitary"      },
            {FolderListSameVol,     "Same volume"   },
            {FolderListClones,      "Clones"        }
        };

        internal const string FolderListSolitary = "solitary";
        internal const string FolderListSameVol = "sameVol";
        internal const string FolderListClones = "clones";

        public UC_Clones()
        {
            InitializeComponent();            
        }

        protected override void LocalNavigatedTo() =>  _bNavigatedTo = true;

        protected override void LocalFragmentNavigation(string strFragment)
        {
            if (false == _bNavigatedTo)
                _bNicknames = formChk_Nicknames.IsChecked ?? false;
            
            _bNavigatedTo = false;
            _winFormsLVVM = WinClonesVM.FactoryGetHolder(strFragment, _bNicknames);
            DataContext = _winFormsLVVM;

            LocalTitle =
                new CultureInfo("en-US", false).TextInfo            // future proof to title case
                .ToTitleCase(FolderListFragments[strFragment]);
        }

        protected override void LocalNavigatedFrom()
        {
            _bNicknames = formChk_Nicknames.IsChecked ?? false;
            _winFormsLVVM.Dispose();

            DataContext =
                _winFormsLVVM =
                null;
        }

        WinClonesVM
            _winFormsLVVM = null;
        bool
            _bNicknames = false;
        bool
            _bNavigatedTo = false;
    }
}
