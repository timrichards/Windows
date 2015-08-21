namespace DoubleFile
{
    class LVitemProject_Updater<T> : LVitemProject_Explorer     // 2. Far fewer (# listing files), inside each listview item (N)
    {
        internal LVitemProject_Updater(LVitemProject_Explorer lvItemTemp, ListUpdater<T> listUpdater)
            : base(lvItemTemp)
        {
            ListUpdater = listUpdater;
        }

        internal readonly ListUpdater<T> ListUpdater;           // One per LV, inside (2.)
    }
}
