using System.Collections.Generic;
using System.Windows.Threading;

namespace DoubleFile
{
    class LocalDispatcherFrame : DispatcherFrame
    {
        internal LocalDispatcherFrame(decimal nLocation) : base(true) { _nLocation = nLocation; }

        internal void PushFrameToTrue()
        {
            _dispatcherFrames.Add(this);
            Continue = true;
            Dispatcher.PushFrame(this);
            _dispatcherFrames.Remove(this);
        }

        internal static void ClearFrames()
        {
            _dispatcherFrames.ForEach(dispatcherFrame =>
                dispatcherFrame.Continue = false);

            _dispatcherFrames = new List<DispatcherFrame>();
        }

        decimal
            _nLocation = -1;
        static List<DispatcherFrame>
            _dispatcherFrames = new List<DispatcherFrame>();
    }
}
