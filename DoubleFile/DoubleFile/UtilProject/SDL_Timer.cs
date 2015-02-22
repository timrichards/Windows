﻿using System;
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
            Elapsed += (o, e) => callback();
        }

        internal new SDL_Timer Start()
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
