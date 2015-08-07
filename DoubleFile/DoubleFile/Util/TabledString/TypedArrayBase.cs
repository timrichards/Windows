namespace DoubleFile
{
    abstract class TypedArrayBase
    {
        internal abstract int Type { get; }
        static internal TabledStringStatics[] tA = new TabledStringStatics[2];
    }
    class Tabled_Folders : TypedArrayBase { internal override int Type => 0; }
    class Tabled_Files : TypedArrayBase { internal override int Type => 1; }
}
