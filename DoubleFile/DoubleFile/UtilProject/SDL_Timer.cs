using System;
using System.Timers;

namespace DoubleFile
{
    [System.ComponentModel.DesignerCategory("Code")]
    class SDL_Timer : Timer
    {
        internal SDL_Timer()
        { }

        internal SDL_Timer(Action callback)
            : this(500.0, callback)
        { }

        internal SDL_Timer(int seconds, Action callback)
            : this(seconds * 1000.0, callback)
        { }

        internal SDL_Timer(double milliseconds, Action callback)
            : base(milliseconds)
        {
            Elapsed += _ElapsedHandler = (o, e) => callback();
        }

        public new void Dispose()
        {
            Elapsed -= _ElapsedHandler;
            base.Dispose();
            _bDisposed = true;
        }

        internal new SDL_Timer Start()
        {
            if (_bDisposed)
            {
                MBoxStatic.Assert(0, false, bTraceOnly: true);
                return null;
            }

            if (MBoxStatic.Assert(99937, Interval >= 33))
            {
                base.Start();
                return this;
            }

            return null;
        }

        ElapsedEventHandler _ElapsedHandler = null;
        bool _bDisposed = false;
    }

}
