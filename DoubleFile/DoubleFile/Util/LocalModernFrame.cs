using FirstFloor.ModernUI.Windows.Controls;
using System.Windows;

namespace DoubleFile
{
	class LocalModernFrame : ModernFrame
	{
		// Created on 9/19/15 but not put into play
		// Positioning, while same as the M:UI progress bar, looks funky
		// Treemap can't shut it off
		// when implementing, grep 9962 6 and 9962 7.
		// Kept in order to have more control over the frame; study dependency properties; study templates

        public static readonly DependencyProperty
            ShowLocalProgressbarProperty = DependencyProperty.Register("ShowLocalProgressbar", typeof(bool), typeof(LocalModernFrame), new PropertyMetadata(false));
        internal void
            ShowProgressbar(bool bShow = true) => SetValue(ShowLocalProgressbarProperty, bShow);
        internal void
            HideProgressbar() => SetValue(ShowLocalProgressbarProperty, false);
	}
}