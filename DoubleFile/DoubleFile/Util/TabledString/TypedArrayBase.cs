namespace DoubleFile
{
    abstract class TabledStringTypesBase
    {
        internal abstract int
            Type { get; }
        static internal TabledStringBase[]
            Types = new TabledStringBase[2];
    }
    class TabledStringType_Folders : TabledStringTypesBase { internal override int Type => 0; }
    class TabledStringType_Files : TabledStringTypesBase { internal override int Type => 1; }
}
