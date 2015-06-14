using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class LV_VolumeDetailVM : IDisposable
    {
        internal LV_VolumeDetailVM()
        {
            _lsDisposable.Add(TreeSelect.VolumeDetailUpdated.Subscribe(VolumeDetailUpdated));

            var volumeDetail = LocalTV.TreeSelect_VolumeDetail;

            if (null != volumeDetail)
                VolumeDetailUpdated(Tuple.Create(volumeDetail, 0));
        }

        void VolumeDetailUpdated(Tuple<Tuple<IEnumerable<IEnumerable<string>>, string>, int> initiatorTuple)
        {
            var tuple = initiatorTuple.Item1;

            Util.Write("H");
            Title = tuple.Item2;
            ClearItems();

            if (null == tuple.Item1)
                return;

            Util.UIthread(() =>
                Add(tuple.Item1.Select(ieLine => new LVitem_VolumeDetailVM(ieLine.ToList()))));
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
