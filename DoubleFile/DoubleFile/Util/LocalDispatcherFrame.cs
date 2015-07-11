using System.Collections.Generic;
using System.Windows.Threading;

namespace DoubleFile
{
    class LocalDispatcherFrame : DispatcherFrame
    {
        internal LocalDispatcherFrame(decimal nSource) : base(true) { Source = nSource; }
        decimal Source = -1;

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

        static List<DispatcherFrame> _dispatcherFrames = new List<DispatcherFrame>();
    }
}
