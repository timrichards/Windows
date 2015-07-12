﻿namespace DoubleFile
{
    interface IModalWindow : ILocalWindow { }
    interface ICantBeTopWindow { }

    public interface ILocalWindow
    {
        bool LocalIsClosed { get; }
        bool SimulatingModal { get; set; }
        bool Activate();
    }
}
