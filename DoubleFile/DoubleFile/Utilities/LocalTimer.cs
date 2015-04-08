using System;
using System.Reactive.Linq;
using System.Timers;

namespace DoubleFile
{
    [System.ComponentModel.DesignerCategory("Code")]
    class LocalTimer : Timer
    {
        internal LocalTimer()
        { }

        internal LocalTimer(Action callback)
            : this(500.0, callback)
        { }

        internal LocalTimer(int seconds, Action callback)
            : this(seconds * 1000.0, callback)
        { }

        internal LocalTimer(double milliseconds, Action callback)
            : base(milliseconds)
        {
            Observable.FromEventPattern(this, "Elapsed")
                .Subscribe(args => callback());
        }

        internal new LocalTimer Start()
        {
            if (MBoxStatic.Assert(99937, Interval >= 33))
            {
                base.Start();
                return this;
            }

            return null;
        }
    }

}
