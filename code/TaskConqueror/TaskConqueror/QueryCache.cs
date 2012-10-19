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

        public void AddCacheItem(string name, object Value)
        {
            if (GetCacheItem(name) != null)
            {
                RemoveCacheItem(name);
            }

            CacheItems.Add(new QueryCacheItem() { Name = name, Value = Value });
        }

        public void RemoveCacheItem (string name)
        {
            QueryCacheItem requestedItem = CacheItems.FirstOrDefault(i => i.Name == name);
            if (requestedItem != null)
            {
                CacheItems.Remove(requestedItem);
            }
        }

        public void UpdateCacheItem(string name, object value)
        {
            QueryCacheItem selectedItem = GetCacheItem(name);

            if (selectedItem != null)
            {
                selectedItem.Value = value;
            }
            else
            {
                throw new ArgumentOutOfRangeException(Properties.Resources.CacheItem_Exception_Name, Properties.Resources.CacheItem_Exception_NotFound);
            }
        }

        public QueryCacheItem GetCacheItem(string name)
        {
            return CacheItems.FirstOrDefault(i => i.Name == name);
        }

        #endregion
    }
}
