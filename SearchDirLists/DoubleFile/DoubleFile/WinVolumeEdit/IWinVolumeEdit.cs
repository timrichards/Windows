namespace DoubleFile
{
    interface IWinVolumeEdit
    {
        string[] StringValues { get; set; }
        bool? ShowDialog(System.Windows.Window me);
    }
}
