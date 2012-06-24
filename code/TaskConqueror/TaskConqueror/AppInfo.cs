using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskConqueror
{
    public sealed class AppInfo
    {
        /// <summary>
        /// This is an expensive resource we need to only store in one place.
        /// </summary>
        Data.TaskConquerorEntities _gcContext;
        List<Data.Status> _statusList;
        List<Data.TaskPriority> _priorityList;

        /// <summary>
        /// Allocate ourselves. We have a private constructor, so no one else can.
        /// </summary>
        static readonly AppInfo _instance = new AppInfo();

        /// <summary>
        /// Access AppInfo.Instance to get the singleton object.
        /// Then call methods on that instance.
        /// </summary>
        public static AppInfo Instance
        {
	        get { return _instance; }
        }

        public List<Data.Status> StatusList
        {
            get { return _statusList; }
        }

        public List<Data.TaskPriority> PriorityList
        {
            get { return _priorityList; }
        }

        public Data.TaskConquerorEntities GcContext
        {
            get { return _gcContext; }
        }

        /// <summary>
        /// This is a private constructor, meaning no outsiders have access.
        /// </summary>
        private AppInfo()
        {
	        // Initialize members, etc. here.
            _gcContext = new Data.TaskConquerorEntities();
            _priorityList = (from p in _gcContext.TaskPriorities select p).ToList();
            _statusList = (from s in _gcContext.Status select s).ToList();
        }
    }
}
