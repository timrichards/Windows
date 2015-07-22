using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFolderList.xaml
    /// </summary>
    partial class WinFormsLV
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

        public WinFormsLV()
        {
            InitializeComponent();            
        }

        protected override void LocalFragmentNavigation(string strFragment)
        {
            DataContext = 
                _winFormsLVVM =
                new WinFormsLVVM(strFragment)
                .Init();

            LocalTitle =
                new CultureInfo("en-US", false).TextInfo            // future proof to title case
                .ToTitleCase(FolderListFragments[strFragment]);
        }

        protected override void LocalNavigatedFrom()
        {
            DataContext =
                _winFormsLVVM =
                null;
        }

        WinFormsLVVM
            _winFormsLVVM = null;
    }
}
