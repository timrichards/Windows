using System;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Linq;

namespace DoubleFile
{
    class LocalDispatcherFrame : DispatcherFrame
    {
        internal LocalDispatcherFrame(decimal nLocation) : base(true) { _nLocation = nLocation; }

        internal void PushFrameToTrue()
        {
            lock (_dispatcherFrames)
                _dispatcherFrames.Add(this);

            Continue = true;
            Dispatcher.PushFrame(this);

            lock (_dispatcherFrames)
            {
                var ix = _dispatcherFrames.IndexOf(this);

                if (-1 < ix)
                    _dispatcherFrames.RemoveAt(ix);
            }
        }

        internal static string ClearFrames()
        {
            var strRet = "";
            var dispatcherFrames = _dispatcherFrames.ToList();

            _dispatcherFrames = new List<LocalDispatcherFrame>();

            dispatcherFrames.ForEach(dispatcherFrame =>
            {
                if (dispatcherFrame.Continue)
                    strRet += dispatcherFrame._nLocation + " ";

                dispatcherFrame.Continue = false;
            });

            return strRet;
        }

        decimal
            _nLocation = -1;
        static List<LocalDispatcherFrame>
            _dispatcherFrames = new List<LocalDispatcherFrame>();
    }
}
