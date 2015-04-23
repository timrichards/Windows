using System;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class LV_VolumeDetailVM : IDisposable
    {
        internal LV_VolumeDetailVM()
        {
            _lsDisposable.Add(TreeSelect.VolumeDetailUpdated.Subscribe(tuple =>
            {
                UtilDirList.Write("H");
                UtilProject.UIthread(() =>
                {
                    Title = tuple.Item2;
                    Items.Clear();

                    if (null == tuple.Item1)
                        return;     // from lambda

                    foreach (var ieLine in tuple.Item1)
                        Add(new LVitem_VolumeDetailVM(ieLine), bQuiet: true);

                    RaiseItems();
                });
            }));
        }

        public void Dispose()
        {
            foreach (var d in _lsDisposable)
                d.Dispose();
        }

        List<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
