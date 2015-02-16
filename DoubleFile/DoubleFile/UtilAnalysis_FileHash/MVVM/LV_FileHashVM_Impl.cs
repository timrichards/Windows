
namespace DoubleFile
{
    partial class LV_FileHashVM
    {
        internal LV_FileHashVM(LV_FileHashVM lvFileHashVM_in = null)
        {
            if (lvFileHashVM_in != null)
            {
                foreach (var lvItemVM in lvFileHashVM_in.ItemsCast)
                {
                    Add(lvItemVM, bQuiet: true);
                }
            }
        }
    }
}
