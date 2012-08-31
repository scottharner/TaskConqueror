using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskConqueror
{
 /// <summary>
 /// Used to cache paged sql ce queries, since sql ce doesnt support paging
 /// </summary>
    public class QueryCache
    {
        #region Constructor

        public QueryCache()
        {
            CacheItems = new List<QueryCacheItem>();
        }

        #endregion

        #region Properties

        private List<QueryCacheItem> CacheItems
        {
            get;
            set;
        }

        #endregion

        #region Methods

        public void AddCacheItem(string name, string filterTerm, SortableProperty sortColumn, object Value)
        {
            if (GetCacheItem(name, filterTerm) != null)
            {
                RemoveCacheItem(name);
            }

            CacheItems.Add(new QueryCacheItem() { Name = name, Value = Value, FilterTerm = filterTerm, SortColumn = sortColumn });
        }

        public void RemoveCacheItem (string name)
        {
            QueryCacheItem requestedItem = CacheItems.FirstOrDefault(i => i.Name == name);
            if (requestedItem != null)
            {
                CacheItems.Remove(requestedItem);
            }
        }

        public void UpdateCacheItem(string name, string filterTerm, SortableProperty sortColumn, object value)
        {
            QueryCacheItem selectedItem = GetCacheItem(name, filterTerm);

            if (selectedItem != null)
            {
                selectedItem.Value = value;
                selectedItem.SortColumn = sortColumn;
            }
            else
            {
                throw new ArgumentOutOfRangeException(Properties.Resources.CacheItem_Exception_Name, Properties.Resources.CacheItem_Exception_NotFound);
            }
        }

        public QueryCacheItem GetCacheItem(string name, string filterTerm)
        {
            return CacheItems.FirstOrDefault(i => i.Name == name && i.FilterTerm == filterTerm);
        }

        public QueryCacheItem GetCacheItem(string name)
        {
            return CacheItems.FirstOrDefault(i => i.Name == name);
        }
        
        #endregion
    }
}
