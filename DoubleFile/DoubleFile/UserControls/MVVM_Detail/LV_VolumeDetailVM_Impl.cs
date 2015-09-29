using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class LV_VolumeDetailVM : IDisposable
    {
        internal LV_VolumeDetailVM()
        {
            _lsDisposable.Add(TreeSelect.VolumeDetailUpdated.Observable.LocalSubscribe(99705, VolumeDetailUpdated));

            var volumeDetail = LocalTV.TreeSelect_VolumeDetail;

            if (null != volumeDetail)
                VolumeDetailUpdated(Tuple.Create(volumeDetail, 0));
        }

        void VolumeDetailUpdated(Tuple<TreeSelect.VolumeDetailUpdated, int> initiatorTuple)
        {
            var volumeDetail = initiatorTuple.Item1;

            Util.Write("H");
            Title = volumeDetail.strVolume;
            ClearItems();

            if (null == volumeDetail.ieDetail)
                return;

            Util.UIthread(99819, () =>
                Add(volumeDetail.ieDetail.Select(ieLine => new LVitem_VolumeDetailVM(ieLine.ToList()))));
        }

        public void Dispose() => Util.LocalDispose(_lsDisposable);

        readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable> { };
    }
}
