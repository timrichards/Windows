using System;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class LV_FolderDetailVM : IDisposable
    {
        internal LV_FolderDetailVM()
        {
            _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Subscribe(initiatorTuple =>
            {
                var tuple = initiatorTuple.Item1;

                Util.Write("G");
                Util.UIthread(() =>
                {
                    Title = null;
                    ClearItems();

                    if (null == tuple.Item2)
                        return;     // from lambda

                    foreach (var ieLine in tuple.Item1)
                        Add(new LVitem_FolderDetailVM(ieLine), bQuiet: true);

                    var strFG_Description = UtilColor.Description[tuple.Item2.ForeColor];
                    var strBG_Description = UtilColor.Description[tuple.Item2.BackColor];

                    if (false == string.IsNullOrEmpty(strFG_Description))
                    {
                        Add(new LVitem_FolderDetailVM(new[] { "", strFG_Description })
                        {
                            Foreground = tuple.Item2.Foreground
                        }, bQuiet: true);
                    }

                    if (false == string.IsNullOrEmpty(strBG_Description))
                    {
                        Add(new LVitem_FolderDetailVM(new[] { "", strBG_Description })
                        {
                            Background = tuple.Item2.Background
                        }, bQuiet: true);
                    }
#if DEBUG
                    Add(new LVitem_FolderDetailVM(new[] { "Hash Parity", "" + tuple.Item2.NodeDatum.HashParity }), bQuiet: true);
#endif
                    Title = tuple.Item2.Text;
                    RaiseItems();
                });
            }));
        }

        public void Dispose()
        {
            foreach (var d in _lsDisposable)
                d.Dispose();
        }

        List<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
