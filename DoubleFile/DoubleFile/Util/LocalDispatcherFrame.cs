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

        internal static string ClearFrames()
        {
            var strRet = "";

            _dispatcherFrames.ForEach(dispatcherFrame =>
            {
                if (dispatcherFrame.Continue)
                    strRet += dispatcherFrame._nLocation + " ";

                dispatcherFrame.Continue = false;
            });

            _dispatcherFrames = new List<LocalDispatcherFrame>();
            return strRet;
        }

        decimal
            _nLocation = -1;
        static List<LocalDispatcherFrame>
            _dispatcherFrames = new List<LocalDispatcherFrame>();
    }
}
