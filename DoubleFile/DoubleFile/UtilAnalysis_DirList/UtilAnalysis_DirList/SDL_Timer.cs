﻿using DoubleFile;
using System;
using System.Windows.Threading;

namespace SearchDirLists
{
    class SDL_Timer : DispatcherTimer
    {
        internal SDL_Timer()
            : base()
        { }

        internal SDL_Timer(TimeSpan interval, DispatcherPriority priority, EventHandler callback, Dispatcher dispatcher)
            : base(interval, priority, callback, dispatcher)
        { }

        internal new void Start()
        {
            if (MBox.Assert(0, Interval.TotalMilliseconds > 0))
            {
                base.Start();
            }
        }
    }
}
