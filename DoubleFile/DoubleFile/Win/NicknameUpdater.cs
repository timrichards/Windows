using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    interface INicknameUpdater
    {
        void RaiseNicknameChange();
    }

    // can't be struct because of pass-by-reference
    class NicknameUpdater                                   // One per search ore folder window
    {
        internal bool UseNickname;

        internal void LastGet(INicknameUpdater lvItemVM)
        {
            if (false == _bUpdating)
                _lastGets.Add(lvItemVM);
        }

        internal void UpdateNicknames(bool bUseNickname)
        {
            UseNickname = bUseNickname;
            _lastGets = _lastGets.Skip(Math.Max(0, _lastGets.Count - 1024)).ToList();
            _bUpdating = true;

            foreach (var lvitemSearchVM in _lastGets)
                lvitemSearchVM.RaiseNicknameChange();

            _bUpdating = false;
        }

        internal void Clear() =>
            _lastGets = new List<INicknameUpdater> { };

        List<INicknameUpdater>
            _lastGets = new List<INicknameUpdater> { };
        bool
            _bUpdating = false;
    }
}
