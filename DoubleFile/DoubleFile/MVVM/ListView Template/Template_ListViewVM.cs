namespace Template
{
    using DoubleFile;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    class Template_ListViewVM : ListViewVM_Base<Template_LVitemVM>
    {
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public string WidthColumnNameHere { get { return SCW; } }

        internal override int NumCols { get { return Template_LVitemVM.NumCols_; } }
    }
}
