using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace StarResonanceDpsAnalysis.WPF.Controls
{
    public class MultiSelectorItemsHost : ItemsControl
    {
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is MultiSelectorItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new MultiSelectorItem();
        }
    }

    class MultiSelectorItem : ContentControl
    {
    }
}
