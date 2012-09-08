using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq.Expressions;

namespace TaskConqueror
{
    /// <summary>
    /// Represents a source of tasks in the application.
    /// </summary>
    public class TaskData : IDisposable
    {
        #region Fields

        private AppInfo _appInfo;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Creates a new task data access class.
        /// </summary>
        public TaskData()
        {
            _appInfo = AppInfo.Instance;

            this.TaskAdded += this.AllTasksOnTaskAdded;
            this.TaskUpdated += this.AllTasksOnTaskUpdated;
            this.TaskDeleted += this.AllTasksOnTaskDeleted;

            this.TaskAdded += this.ActiveTasksOnTaskAdded;
            this.TaskUpdated += this.ActiveTasksOnTaskUpdated;
            this.TaskDeleted += this.ActiveTasksOnTaskDeleted;
        }

        #endregion // Constructor

        #region Public Interface

        /// <summary>
        /// Raised when a task is placed into the repository.
        /// </summary>
        public event EventHandler<TaskAddedEventArgs> TaskAdded;

        /// <summary>
        /// Places the specified task into the db.
        /// If the task is already in the repository, an
        /// exception is thrown.
        /// </summary>
        public int AddTask(Task task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            Data.Task dbTask = new Data.Task();
            dbTask.TaskID = AppInfo.Instance.GcContext.Tasks.Max(t => t.TaskID) + 1;
            dbTask.StatusID = task.StatusId;
            dbTask.PriorityID = task.PriorityId;
            dbTask.IsActive = task.IsActive;
            dbTask.CreatedDate = task.CreatedDate;
            dbTask.CompletedDate = task.CompletedDate;
            dbTask.Title = task.Title;
            if (task.ParentProject != null)
            {
                dbTask.Projects.Add((from p in _appInfo.GcContext.Projects
                                        where p.ProjectID == task.ParentProject.ProjectId
                                        select p).FirstOrDefault());
            }

            _appInfo.GcContext.AddToTasks(dbTask);
            _appInfo.GcContext.SaveChanges();

            if (this.TaskAdded != null)
                this.TaskAdded(this, new TaskAddedEventArgs(task));

            return dbTask.TaskID;
        }

        /// <summary>
        /// Raised when a task is modified in the db.
        /// </summary>
        public event EventHandler<TaskUpdatedEventArgs> TaskUpdated;

        /// <summary>
        /// Update the specified task into the db.
        /// If the task is not already in the repository, an
        /// exception is thrown.
        /// </summary>
        public void UpdateTask(Task task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            if (task.TaskId == 0)
                throw new InvalidOperationException("modify task");

            Data.Task dbTask = (from t in _appInfo.GcContext.Tasks
             where t.TaskID == task.TaskId
             select t).FirstOrDefault();

            if (dbTask == null)
            {
                throw new InvalidDataException("task id");
            }
            else
            {
                dbTask.StatusID = task.StatusId;
                dbTask.PriorityID = task.PriorityId;
                dbTask.IsActive = task.IsActive;
                dbTask.CreatedDate = task.CreatedDate;
                dbTask.CompletedDate = task.CompletedDate;
                dbTask.Title = task.Title;

                _appInfo.GcContext.SaveChanges();

                if (this.TaskUpdated != null)
                    this.TaskUpdated(this, new TaskUpdatedEventArgs(task));
            }
        }

        /// <summary>
        /// Raised when a task is deleted in the db.
        /// </summary>
        public event EventHandler<TaskDeletedEventArgs> TaskDeleted;

        /// <summary>
        /// Delete the specified task from the db.
        /// If the task is not already in the repository, an
        /// exception is thrown.
        /// </summary>
        public void DeleteTask(Task task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            if (task.TaskId == 0)
                throw new InvalidOperationException("delete task");

            var query = (from t in _appInfo.GcContext.Tasks
                         where t.TaskID == task.TaskId
                         select t).First();
            
            _appInfo.GcContext.DeleteObject(query);
            _appInfo.GcContext.SaveChanges();

            if (this.TaskDeleted != null)
                this.TaskDeleted(this, new TaskDeletedEventArgs(task));
        }

        /// <summary>
        /// Returns true if the specified task exists in the
        /// db, or false if it is not.
        /// </summary>
        public bool TaskExists(Task task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            return _appInfo.GcContext.Tasks.FirstOrDefault(t => t.TaskID == task.TaskId) != null;
        }

        /// <summary>
        /// Returns a shallow-copied list of all tasks in the repository.
        /// </summary>
        public List<Task> GetTasks(string filterTerm = "", SortableProperty sortColumn = null, int? pageNumber = null)
        {
            QueryCacheItem allTasksCacheItem = _appInfo.GlobalQueryCache.GetCacheItem(Constants.AllTasksCacheItem, filterTerm);
            List<Data.Task> allTasksList;

            // retrieve the query from cache if available
            // this will avoid retrieving all records when only a page is needed
            if (allTasksCacheItem == null)
            {
                IQueryable<Data.Task> allTasks = GetAllTasksQuery(filterTerm);

                allTasksList = GetOrderedList(allTasks, sortColumn);

                _appInfo.GlobalQueryCache.AddCacheItem(Constants.AllTasksCacheItem, filterTerm, sortColumn, allTasksList);
            }
            else
            {
                allTasksList = (List<Data.Task>)allTasksCacheItem.Value;
                
                if (allTasksCacheItem.SortColumn != sortColumn)
                {
                    _appInfo.GlobalQueryCache.UpdateCacheItem(Constants.AllTasksCacheItem, filterTerm, sortColumn, allTasksList);
                    SortList(allTasksList, sortColumn);
                }
            }
            
            if (pageNumber.HasValue)
            {
                allTasksList = allTasksList.Skip(Constants.RecordsPerPage * (pageNumber.Value - 1))
                                .Take(Constants.RecordsPerPage).ToList();
            }

            List<Task> tasks = new List<Task>();

            foreach (Data.Task dbTask in allTasksList)
            {
                tasks.Add(Task.CreateTask(dbTask));
            }

            return tasks;
        }

        /// <summary>
        /// Returns the count of all tasks in the repository.
        /// </summary>
        public int GetTasksCount(string filterTerm = "")
        {
            IQueryable<Data.Task> allTasks = GetAllTasksQuery(filterTerm);

            return allTasks.Count();
        }

        /// <summary>
        /// Returns a shallow-copied list of all tasks in the repository that are not assigned to a project.
        /// </summary>
        public List<Task> GetUnassignedTasks()
        {
            List<Data.Task> dbTasks = (from t in _appInfo.GcContext.Tasks
                                       where t.Projects.Count == 0
                                       orderby t.Title
                                       select t).ToList();

            List<Task> tasks = new List<Task>();

            foreach (Data.Task dbTask in dbTasks)
            {
                tasks.Add(Task.CreateTask(dbTask));
            }

            return tasks;
        }

        /// <summary>
        /// Returns a shallow-copied list of all inactive tasks in the repository that are not assigned to a project.
        /// </summary>
        public List<Task> GetUnassignedInactiveTasks()
        {
            List<Data.Task> dbTasks = (from t in _appInfo.GcContext.Tasks
                                       where t.Projects.Count == 0 && t.IsActive == false && t.StatusID != Statuses.Completed && t.StatusID != Statuses.Abandoned
                                       orderby t.Title
                                       select t).ToList();

            List<Task> tasks = new List<Task>();

            foreach (Data.Task dbTask in dbTasks)
            {
                tasks.Add(Task.CreateTask(dbTask));
            }

            return tasks;
        }

        /// <summary>
        /// Returns a shallow-copied list of all active tasks in the repository.
        /// </summary>
        public List<Task> GetActiveTasks(string filterTerm = "", SortableProperty sortColumn = null, int? pageNumber = null)
        {
            QueryCacheItem activeTasksCacheItem = _appInfo.GlobalQueryCache.GetCacheItem(Constants.ActiveTasksCacheItem, filterTerm);
            List<Data.Task> activeTasksList;

            // retrieve the query from cache if available
            // this will avoid retrieving all records when only a page is needed
            if (activeTasksCacheItem == null)
            {
                IQueryable<Data.Task> activeTasks = GetActiveTasksQuery(filterTerm);

                activeTasksList = GetOrderedList(activeTasks, sortColumn);

                _appInfo.GlobalQueryCache.AddCacheItem(Constants.ActiveTasksCacheItem, filterTerm, sortColumn, activeTasksList);
            }
            else
            {
                activeTasksList = (List<Data.Task>)activeTasksCacheItem.Value;

                if (activeTasksCacheItem.SortColumn != sortColumn)
                {
                    _appInfo.GlobalQueryCache.UpdateCacheItem(Constants.ActiveTasksCacheItem, filterTerm, sortColumn, activeTasksList);
                    SortList(activeTasksList, sortColumn);
                }
            }
            
            if (pageNumber.HasValue)
            {
                activeTasksList = activeTasksList.Skip(Constants.RecordsPerPage * (pageNumber.Value - 1))
                                .Take(Constants.RecordsPerPage).ToList();
            }

            List<Task> tasks = new List<Task>();

            foreach (Data.Task dbTask in activeTasksList)
            {
                tasks.Add(Task.CreateTask(dbTask));
            }

            return tasks;
        }

        /// <summary>
        /// Returns the count of active tasks in the repository.
        /// </summary>
        public int GetActiveTasksCount(string filterTerm = "")
        {
            IQueryable<Data.Task> activeTasks = GetActiveTasksQuery(filterTerm);

            return activeTasks.Count();
        }

        /// <summary>
        /// Returns a shallow-copied list of all completed tasks within the given date range.
        /// </summary>
        public List<Task> GetCompletedTasksByDate(DateTime startDate, DateTime endDate)
        {
            List<Data.Task> dbTasks = (from t in _appInfo.GcContext.Tasks
                                       orderby t.Title
                                       where t.StatusID == Statuses.Completed && 
                                       t.CompletedDate >= startDate && 
                                       t.CompletedDate <= endDate
                                       select t).ToList();

            List<Task> tasks = new List<Task>();

            foreach (Data.Task dbTask in dbTasks)
            {
                tasks.Add(Task.CreateTask(dbTask));
            }

            return tasks;
        }

        /// <summary>
        /// Returns a shallow-copied list of a single task by task id.
        /// </summary>
        public Task GetTaskByTaskId(int taskId)
        {
            Task requestedTask = null;
            Data.Task requestedDbTask = (from t in _appInfo.GcContext.Tasks
                                       where t.TaskID == taskId
                                       select t).FirstOrDefault();

            if (requestedDbTask != null)
            {
                requestedTask = Task.CreateTask(requestedDbTask);
            }

            return requestedTask;
        }

        public void AddTasksToProject(Project parentProject, List<int> childTaskIds)
        {
            // add tasks to project
            Data.Project parentDbProject = (from p in _appInfo.GcContext.Projects
                                            where p.ProjectID == parentProject.ProjectId
                                            select p).FirstOrDefault();
            foreach (int taskId in childTaskIds)
            {
                Data.Task childDbTask = (from t in _appInfo.GcContext.Tasks
                                         where t.TaskID == taskId
                                         select t).FirstOrDefault();
                parentDbProject.Tasks.Add(childDbTask);

                _appInfo.GcContext.SaveChanges();

                if (this.TaskUpdated != null)
                    this.TaskUpdated(this, new TaskUpdatedEventArgs(Task.CreateTask(childDbTask)));
            }
        }

        public void PurgeAbandonedTasks()
        {
            List<Data.Task> abandonedDbTasks = (from t in _appInfo.GcContext.Tasks
                                              where t.StatusID == Statuses.Abandoned
                                                select t).ToList();

            foreach (Data.Task abandonedDbTask in abandonedDbTasks)
            {
                Task abandonedTask = Task.CreateTask(abandonedDbTask);
                
                _appInfo.GcContext.DeleteObject(abandonedDbTask);

                _appInfo.GcContext.SaveChanges();

                if (this.TaskDeleted != null)
                    this.TaskDeleted(this, new TaskDeletedEventArgs(abandonedTask));
            }
        }

        public void PurgeCompletedTasks()
        {
            List<Data.Task> completedDbTasks = (from t in _appInfo.GcContext.Tasks
                                                where t.StatusID == Statuses.Completed
                                                select t).ToList();

            foreach (Data.Task completedDbTask in completedDbTasks)
            {
                Task completedTask = Task.CreateTask(completedDbTask);

                _appInfo.GcContext.DeleteObject(completedDbTask);

                _appInfo.GcContext.SaveChanges();

                if (this.TaskDeleted != null)
                    this.TaskDeleted(this, new TaskDeletedEventArgs(completedTask));
            }
        }

        /// <summary>
        /// Cleans up event handlers when finished using this object
        /// </summary>
        public virtual void Dispose()
        {
            foreach (Delegate d in TaskAdded.GetInvocationList())
            {
                TaskAdded -= (EventHandler<TaskConqueror.TaskAddedEventArgs>)d;
            }

            foreach (Delegate d in TaskUpdated.GetInvocationList())
            {
                TaskUpdated -= (EventHandler<TaskConqueror.TaskUpdatedEventArgs>)d;
            }

            foreach (Delegate d in TaskDeleted.GetInvocationList())
            {
                TaskDeleted -= (EventHandler<TaskConqueror.TaskDeletedEventArgs>)d;
            }
        }

        #endregion // Public Interface

        #region Private Helpers

        private List<Data.Task> GetOrderedList(IQueryable<Data.Task> tasksQuery, SortableProperty sortColumn = null)
        {
            List<Data.Task> dbTaskList;

            if (sortColumn == null)
            {
                dbTaskList = tasksQuery.OrderBy(t => t.Title).ToList();
            }
            else
            {
                switch (sortColumn.Name)
                {
                    case "StatusId":
                        dbTaskList = tasksQuery.OrderBy(t => t.StatusID).ToList();
                        break;
                    case "PriorityId":
                        dbTaskList = tasksQuery.OrderByDescending(t => t.PriorityID).ToList();
                        break;
                    case "ProjectTitle":
                        dbTaskList = tasksQuery.OrderBy(t => t.Projects.FirstOrDefault() == null ? "" : t.Projects.FirstOrDefault().Title).ToList();
                        break;
                    case "CreatedDate":
                        dbTaskList = tasksQuery.OrderBy(t => t.CreatedDate).ToList();
                        break;
                    case "CompletedDate":
                        dbTaskList = tasksQuery.OrderBy(t => t.CompletedDate).ToList();
                        break;
                    default:
                        dbTaskList = tasksQuery.OrderBy(t => t.Title).ToList();
                        break;
                }
            }

            return dbTaskList;
        }

        private void SortList(List<Data.Task> dbTaskList, SortableProperty sortColumn = null)
        {
            if (sortColumn == null)
            {
                dbTaskList = dbTaskList.OrderBy(t => t.Title).ToList();
            }
            else
            {
                switch (sortColumn.Name)
                {
                    case "StatusId":
                        dbTaskList = dbTaskList.OrderBy(t => t.StatusID).ToList();
                        break;
                    case "PriorityId":
                        dbTaskList = dbTaskList.OrderByDescending(t => t.PriorityID).ToList();
                        break;
                    case "ProjectTitle":
                        dbTaskList = dbTaskList.OrderBy(t => t.Projects.FirstOrDefault() == null ? "" : t.Projects.FirstOrDefault().Title).ToList();
                        break;
                    case "CreatedDate":
                        dbTaskList = dbTaskList.OrderBy(t => t.CreatedDate).ToList();
                        break;
                    case "CompletedDate":
                        dbTaskList = dbTaskList.OrderBy(t => t.CompletedDate).ToList();
                        break;
                    default:
                        dbTaskList = dbTaskList.OrderBy(t => t.Title).ToList();
                        break;
                }
            }
        }

        private IQueryable<Data.Task> GetAllTasksQuery(string filterTerm = "")
        {
            IQueryable<Data.Task> allTasks;

            if (string.IsNullOrEmpty(filterTerm))
            {
                allTasks = from t in _appInfo.GcContext.Tasks
                            select t;
            }
            else
            {
                allTasks = (from t in _appInfo.GcContext.Tasks
                            where t.Title.Contains(filterTerm)
                            select t);
            }

            return allTasks;
        }

        private IQueryable<Data.Task> GetActiveTasksQuery(string filterTerm = "")
        {
            IQueryable<Data.Task> activeTasks;

            if (string.IsNullOrEmpty(filterTerm))
            {
                activeTasks = (from t in _appInfo.GcContext.Tasks
                               where t.IsActive == true
                               select t);
            }
            else
            {
                activeTasks = (from t in _appInfo.GcContext.Tasks
                               where t.Title.Contains(filterTerm) && t.IsActive == true
                               select t);
            }

            return activeTasks;
        }

        /// <summary>
        /// Updates the all tasks cached query results when a task is added
        /// </summary>
        void AllTasksOnTaskAdded(object sender, TaskAddedEventArgs e)
        {
            QueryCacheItem cachedQuery = _appInfo.GlobalQueryCache.GetCacheItem(Constants.AllTasksCacheItem);

            if (cachedQuery != null)
            {
                // check if the added item satisfies the filter term
                if (cachedQuery.FilterTerm == null || e.NewTask.Title.Contains(cachedQuery.FilterTerm))
                {
                    // add the added item to the cached query results
                    List<Data.Task> allTasks = (List<Data.Task>)cachedQuery.Value;
                    Data.Task addedTask = _appInfo.GcContext.Tasks.FirstOrDefault(t => t.TaskID == e.NewTask.TaskId);
                    if (addedTask != null)
                    {
                        allTasks.Add(addedTask);
                        // sort the query results according to the sort column
                        SortList(allTasks, cachedQuery.SortColumn);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the all tasks cached query results when a task is deleted
        /// </summary>
        void AllTasksOnTaskDeleted(object sender, TaskDeletedEventArgs e)
        {
            QueryCacheItem cachedQuery = _appInfo.GlobalQueryCache.GetCacheItem(Constants.AllTasksCacheItem);

            if (cachedQuery != null)
            {
                List<Data.Task> allTasks = (List<Data.Task>)cachedQuery.Value;
                Data.Task deletedTask = allTasks.FirstOrDefault(t => t.TaskID == e.DeletedTask.TaskId);
                if (deletedTask != null)
                {
                    allTasks.Remove(deletedTask);
                }
            }
        }

        /// <summary>
        /// Updates the all tasks cached query results when a task is updated
        /// </summary>
        void AllTasksOnTaskUpdated(object sender, TaskUpdatedEventArgs e)
        {
            QueryCacheItem cachedQuery = _appInfo.GlobalQueryCache.GetCacheItem(Constants.AllTasksCacheItem);

            if (cachedQuery != null)
            {
                // updated the query results if needed
                List<Data.Task> allTasks = (List<Data.Task>)cachedQuery.Value;
                if (cachedQuery.FilterTerm == null || e.UpdatedTask.Title.Contains(cachedQuery.FilterTerm))
                {
                    Data.Task oldTask = allTasks.FirstOrDefault(t => t.TaskID == e.UpdatedTask.TaskId);
                    Data.Task newTask = allTasks.FirstOrDefault(t => t.TaskID == e.UpdatedTask.TaskId);
                    if (oldTask != null && newTask != null)
                    {
                        allTasks.Remove(oldTask);
                        allTasks.Add(newTask);
                    }
                    else if (newTask != null)
                    {
                        allTasks.Add(newTask);
                    }

                    SortList(allTasks, cachedQuery.SortColumn);
                }
                else
                {
                    // the updated task doesnt meet the filter term so remove if it exists in list
                    Data.Task oldTask = allTasks.FirstOrDefault(t => t.TaskID == e.UpdatedTask.TaskId);
                    if (oldTask != null)
                    {
                        allTasks.Remove(oldTask);
                    }
                }
            }
        }

        /// <summary>
        /// Triggers update event handlers on all child tasks when a project is updated
        /// </summary>
        public void UpdateTasksByProject(int projectId)
        {
            if (this.TaskUpdated != null)
            {
                List<Data.Task> childTasks = (from t in _appInfo.GcContext.Tasks
                                                    where t.Projects.FirstOrDefault().ProjectID == projectId
                                                    select t).ToList();

                foreach (var child in childTasks)
                {
                    this.TaskUpdated(this, new TaskUpdatedEventArgs(Task.CreateTask(child)));
                }
            }
        }

        /// <summary>
        /// Updates the active tasks cached query results when a task is added
        /// </summary>
        void ActiveTasksOnTaskAdded(object sender, TaskAddedEventArgs e)
        {
            QueryCacheItem cachedQuery = _appInfo.GlobalQueryCache.GetCacheItem(Constants.ActiveTasksCacheItem);

            if (cachedQuery != null)
            {
                // check if the added item satisfies the filter term and is active
                if (e.NewTask.IsActive &&
                    (cachedQuery.FilterTerm == null || e.NewTask.Title.Contains(cachedQuery.FilterTerm)))
                {
                    // add the added item to the cached query results
                    List<Data.Task> activeTasks = (List<Data.Task>)cachedQuery.Value;
                    Data.Task addedTask = _appInfo.GcContext.Tasks.FirstOrDefault(t => t.TaskID == e.NewTask.TaskId);
                    if (addedTask != null)
                    {
                        activeTasks.Add(addedTask);
                        // sort the query results according to the sort column
                        SortList(activeTasks, cachedQuery.SortColumn);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the active tasks cached query results when a task is deleted
        /// </summary>
        void ActiveTasksOnTaskDeleted(object sender, TaskDeletedEventArgs e)
        {
            QueryCacheItem cachedQuery = _appInfo.GlobalQueryCache.GetCacheItem(Constants.ActiveTasksCacheItem);

            if (cachedQuery != null)
            {
                List<Data.Task> activeTasks = (List<Data.Task>)cachedQuery.Value;
                Data.Task deletedTask = activeTasks.FirstOrDefault(t => t.TaskID == e.DeletedTask.TaskId);
                if (deletedTask != null)
                {
                    activeTasks.Remove(deletedTask);
                }
            }
        }

        /// <summary>
        /// Updates the active tasks cached query results when a task is updated
        /// </summary>
        void ActiveTasksOnTaskUpdated(object sender, TaskUpdatedEventArgs e)
        {
            QueryCacheItem cachedQuery = _appInfo.GlobalQueryCache.GetCacheItem(Constants.ActiveTasksCacheItem);

            if (cachedQuery != null)
            {
                // updated the query results if needed
                List<Data.Task> activeTasks = (List<Data.Task>)cachedQuery.Value;
                if (e.UpdatedTask.IsActive && 
                    (cachedQuery.FilterTerm == null || e.UpdatedTask.Title.Contains(cachedQuery.FilterTerm)))
                {
                    Data.Task oldTask = activeTasks.FirstOrDefault(t => t.TaskID == e.UpdatedTask.TaskId);
                    Data.Task newTask = activeTasks.FirstOrDefault(t => t.TaskID == e.UpdatedTask.TaskId);
                    if (oldTask != null && newTask != null)
                    {
                        activeTasks.Remove(oldTask);
                        activeTasks.Add(newTask);
                    }
                    else if (newTask != null)
                    {
                        activeTasks.Add(newTask);
                    }

                    SortList(activeTasks, cachedQuery.SortColumn);
                }
                else
                {
                    // the updated task doesnt meet the filter term so remove if it exists in list
                    Data.Task oldTask = activeTasks.FirstOrDefault(t => t.TaskID == e.UpdatedTask.TaskId);
                    if (oldTask != null)
                    {
                        activeTasks.Remove(oldTask);
                    }
                }
            }
        }

        #endregion // Private Helpers
    }
}