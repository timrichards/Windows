namespace DoubleFile
{
    interface ILocalWindow
    {
        bool LocalIsClosed { get; }
        bool SimulatingModal { get; set; }
        bool Activate();
    }
}
