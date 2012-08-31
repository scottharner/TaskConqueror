using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskConqueror
{
    public class QueryCacheItem
    {
        #region Properties

        public string Name
        {
            get;
            set;
        }

        public string FilterTerm
        {
            get;
            set;
        }

        public object Value
        {
            get;
            set;
        }

        public SortableProperty SortColumn
        {
            get;
            set;
        }

        #endregion
    }
}
