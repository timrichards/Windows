using System.Collections.Generic;
using System.Globalization;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFolderList.xaml
    /// </summary>
    partial class WinClones
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

        public WinClones()
        {
            InitializeComponent();            
        }

        protected override void LocalFragmentNavigation(string strFragment)
        {
            DataContext = 
                _winFormsLVVM =
                WinClonesVM.FactoryGetHolder(strFragment);

            LocalTitle =
                new CultureInfo("en-US", false).TextInfo            // future proof to title case
                .ToTitleCase(FolderListFragments[strFragment]);
        }

        protected override void LocalNavigatedFrom()
        {
            _winFormsLVVM.Dispose();

            DataContext =
                _winFormsLVVM =
                null;
        }

        WinClonesVM
            _winFormsLVVM = null;
    }
}
