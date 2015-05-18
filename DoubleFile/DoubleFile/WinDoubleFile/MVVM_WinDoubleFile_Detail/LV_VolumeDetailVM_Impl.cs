using System;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class LV_VolumeDetailVM : IDisposable
    {
        internal LV_VolumeDetailVM()
        {
            _lsDisposable.Add(TreeSelect.VolumeDetailUpdated.Subscribe(tupleA =>
            {
                MBoxStatic.Assert(tupleA.Item2 + .5, false);

                var tuple = tupleA.Item1;

                UtilDirList.Write("H");
                UtilProject.UIthread(() =>
                {
                    Title = tuple.Item2;
                    ClearItems();

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
