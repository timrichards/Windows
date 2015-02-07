
namespace WPF
{
    // One tag at the first item, so the compare listviewer knows what the first listviewer's state is.
    // can't be struct because of null
    class LVitemFileTag
    {
        internal readonly string StrCompareDir = null;
        internal readonly long nNumFiles = 0;   // equivalent to number of items in the listviewer. Not currently used

        internal LVitemFileTag(string strCompareDir_in, long nNumFiles_in)
        {
            StrCompareDir = strCompareDir_in;
            nNumFiles = nNumFiles_in;
        }
    }
}
