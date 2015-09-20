using System;
using System.Text;
using System.Windows.Interop;
using System.Windows;
using System.Runtime.InteropServices;

namespace DoubleFile
{
    class FolderBrowserDialog
    {
        internal bool ShowNewFolderButton;  // ignored: always do not show

        internal string
            Description { get; set; }
        internal string
            SelectedPath { get; set; }

        internal bool ShowDialog(Window owner)
        {
            var ownerHandle = null == owner ? NativeMethods.GetActiveWindow() : new WindowInteropHelper(owner).Handle;
            var rootItemIdList = IntPtr.Zero;
            var resultItemIdList = IntPtr.Zero;

            try
            {
                var info = new NativeMethods.BROWSEINFO
                {
                    hwndOwner = ownerHandle,
                    lpfn = BrowseCallbackProc,
                    lpszTitle = Description,
                    pidlRoot = rootItemIdList,
                    pszDisplayName = new string('\0', 260),

                    ulFlags = NativeMethods.BrowseInfoFlags.NewDialogStyle |
                        NativeMethods.BrowseInfoFlags.ReturnOnlyFsDirs |
                        NativeMethods.BrowseInfoFlags.NoNewFolderButton
                };

                resultItemIdList = NativeMethods.SHBrowseForFolder(ref info);

                if (IntPtr.Zero == resultItemIdList)
                    return false;

                var path = new StringBuilder(260);

                NativeMethods.SHGetPathFromIDList(resultItemIdList, path);
                SelectedPath = path.ToString();
                return true;
            }
            finally
            {
                if (null != rootItemIdList)
                {
                    var malloc = NativeMethods.SHGetMalloc();

                    malloc.Free(rootItemIdList);
                    Marshal.ReleaseComObject(malloc);
                }

                if (null != resultItemIdList)
                    Marshal.FreeCoTaskMem(resultItemIdList);
            }
        }

        int BrowseCallbackProc(IntPtr hwnd, NativeMethods.FolderBrowserDialogMessage msg, IntPtr lParam, IntPtr wParam)
        {
            switch (msg)
            {
                case NativeMethods.FolderBrowserDialogMessage.Initialized:
                {
                    if (false == string.IsNullOrEmpty(SelectedPath))
                        NativeMethods.SendMessage(hwnd, NativeMethods.FolderBrowserDialogMessage.SetSelection, new IntPtr(1), SelectedPath);

                    break;
                }

                case NativeMethods.FolderBrowserDialogMessage.SelChanged:
                {
                    if (IntPtr.Zero != lParam)
                    {
                        NativeMethods.SendMessage(hwnd, NativeMethods.FolderBrowserDialogMessage.EnableOk, IntPtr.Zero,
                            NativeMethods.SHGetPathFromIDList(lParam, new StringBuilder(260)) ? new IntPtr(1) : IntPtr.Zero);
                    }

                    break;
                }
            }

            return 0;
        }
    }
}
