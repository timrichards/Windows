namespace DoubleFile
{
    public interface ILocalWindow
    {
        bool LocalIsClosed { get; }
        bool SimulatingModal { get; set; }
        bool Activate();
    }
}
