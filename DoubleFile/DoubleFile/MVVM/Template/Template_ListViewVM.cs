
namespace Template      // prevents smart tag rename command from renaming the templates after you've copied them and rename them
{
    using DoubleFile;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    class Template_ListViewVM : ListViewVM_GenericBase<Template_LVitemVM>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public string WidthColumnNameHere { get { return SCW; } }

        internal override bool NewItem(string[] arrStr, bool bQuiet = false) { Add(new Template_LVitemVM(this, arrStr), bQuiet); return true; }
        internal override int NumCols { get { return Template_LVitemVM.NumCols_; } }
    }
}
