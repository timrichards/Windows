using System.Windows.Threading;
using System.Linq;
using System.Collections.Concurrent;

namespace DoubleFile
{
    class LocalDispatcherFrame : DispatcherFrame
    {
        internal LocalDispatcherFrame(decimal nLocation) : base(true) { _nLocation = nLocation; }

        internal void PushFrameToTrue()
        {
            _dispatcherFrames.TryAdd(this, false);

            Continue = true;
            Dispatcher.PushFrame(this);

            var noop = false;

            _dispatcherFrames.TryRemove(this, out noop);
        }

        internal static string ClearFrames()
        {
            var strRet = "";
            var dispatcherFrames = _dispatcherFrames.Keys.ToList();

            _dispatcherFrames.Clear();

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
        static ConcurrentDictionary<LocalDispatcherFrame, bool>     // bool is a no-op: generic placeholder
            _dispatcherFrames = new ConcurrentDictionary<LocalDispatcherFrame, bool>();
    }
}
