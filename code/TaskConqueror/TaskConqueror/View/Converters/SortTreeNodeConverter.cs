using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;

namespace TaskConqueror
{
    public class SortTreeNodeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IEnumerable<ITreeNodeViewModel> treeNodes = value as IEnumerable<ITreeNodeViewModel>;
            ListCollectionView lcv = (ListCollectionView)CollectionViewSource.GetDefaultView(treeNodes);
            lcv.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));
            return lcv;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
