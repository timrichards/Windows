﻿using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            _statics = new Statics();
        }

        Statics
            _statics = null;
    }
}
