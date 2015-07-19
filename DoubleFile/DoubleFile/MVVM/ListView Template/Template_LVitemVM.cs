namespace Template      // prevents smart tag rename command from renaming the templates after you've copied them and rename them
{
    using DoubleFile;
    using System.Collections.Generic;

    class Template_LVitemVM : ListViewItemVM_Base
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string ColumnNameHere { get { return SubItems[0]; } internal set { SetProperty(0, value); } }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 0;

        protected override string[] _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static string[] _propNamesA = null;

        internal Template_LVitemVM(Template_ListViewVM LV, string[] arrStr)
            : base(LV, arrStr) { }
    }
}
