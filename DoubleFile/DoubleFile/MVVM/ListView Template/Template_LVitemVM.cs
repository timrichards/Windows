
namespace Template      // prevents smart tag rename command from renaming the templates after you've copied them and rename them
{
    using DoubleFile;

    class Template_LVitemVM : ListViewItemVM_Base
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string ColumnNameHere { get { return marr[0]; } set { SetProperty(0, value); } }

        protected override string[] PropertyNames { get { return new[] { "" }; } }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 0;

        internal Template_LVitemVM(Template_ListViewVM LV, string[] arrStr)
            : base(LV, arrStr) { }

    }
}
