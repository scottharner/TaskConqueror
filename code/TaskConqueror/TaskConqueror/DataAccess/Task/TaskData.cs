﻿using System;
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
            
            int nextId;
            if (AppInfo.Instance.GcContext.Tasks.Count() == 0)
            {
                nextId = 1;
            }
            else
            {
                nextId = AppInfo.Instance.GcContext.Tasks.Max(t => t.TaskID) + 1;
            }

            dbTask.TaskID = nextId;
            dbTask.StatusID = task.StatusId;
            dbTask.PriorityID = task.PriorityId;
            dbTask.IsActive = task.IsActive;
            dbTask.CreatedDate = task.CreatedDate;
            dbTask.CompletedDate = task.CompletedDate;
            dbTask.Title = task.Title;
            dbTask.SortOrder = task.SortOrder;
            if (task.ParentProject != null)
            {
                dbTask.Projects.Add((from p in _appInfo.GcContext.Projects
                                        where p.ProjectID == task.ParentProject.ProjectId
                                        select p).FirstOrDefault());
            }

            _appInfo.GcContext.AddToTasks(dbTask);
            _appInfo.GcContext.SaveChanges();

            task.TaskId = dbTask.TaskID;

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
                dbTask.SortOrder = task.SortOrder;

                _appInfo.GcContext.SaveChanges();

                if (this.TaskUpdated != null)
                    this.TaskUpdated(this, new TaskUpdatedEventArgs(task, dbTask));
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
            if (sortColumn == null)
                sortColumn = new SortableProperty() { Description = "Title", Name = "Title" };

            QueryCacheItem allTasksCacheItem = _appInfo.GlobalQueryCache.GetCacheItem(Constants.AllTasksCacheItem);
            List<Data.Task> allTasksList;

            // retrieve the query from cache if available
            // this will avoid retrieving all records when only a page is needed
            if (allTasksCacheItem == null)
            {
                // get the unordered, unfiltered list for caching
                IQueryable<Data.Task> allTasks = GetAllTasksQuery(filterTerm);
                allTasksList = GetOrderedList(allTasks, sortColumn);
                _appInfo.GlobalQueryCache.AddCacheItem(Constants.AllTasksCacheItem, allTasksList);
            }
            else
            {
                allTasksList = (List<Data.Task>)allTasksCacheItem.Value;
            }

            // now do the ordering and filtering
            if (!string.IsNullOrEmpty(filterTerm))
            {
                allTasksList = (from t in allTasksList
                               where t.Title.ToLower().Contains(filterTerm.ToLower())
                               select t).ToList();
            }

            if (sortColumn != null)
            {
                allTasksList = SortList(allTasksList, sortColumn);
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
                                       && t.StatusID != Statuses.Abandoned 
                                       && t.StatusID != Statuses.Completed 
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
            QueryCacheItem activeTasksCacheItem = _appInfo.GlobalQueryCache.GetCacheItem(Constants.ActiveTasksCacheItem);
            List<Data.Task> activeTasksList;

            // retrieve the query from cache if available
            // this will avoid retrieving all records when only a page is needed
            if (activeTasksCacheItem == null)
            {
                // get the unordered, unfiltered list for caching
                IQueryable<Data.Task> activeTasks = GetActiveTasksQuery();
                activeTasksList = GetOrderedList(activeTasks);
                _appInfo.GlobalQueryCache.AddCacheItem(Constants.ActiveTasksCacheItem, activeTasksList);
            }
            else
            {
                activeTasksList = (List<Data.Task>)activeTasksCacheItem.Value;
            }

            // now do the ordering and filtering
            if (!string.IsNullOrEmpty(filterTerm))
            {
                activeTasksList = (from t in activeTasksList
                               where t.Title.ToLower().Contains(filterTerm.ToLower())
                               select t).ToList();
            }

            if (sortColumn != null)
            {
                activeTasksList = SortList(activeTasksList, sortColumn);
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
        /// Returns the next available sort order number for an active task
        /// </summary>
        public int GetNextAvailableSortOrder()
        {
            int? sortOrder =
                (from t in _appInfo.GcContext.Tasks
                    where t.IsActive == true
                    select t.SortOrder)
                    .Max();

            if (sortOrder.HasValue)
                sortOrder++;
            else
                sortOrder = 1;

            return sortOrder.Value;
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
                    this.TaskUpdated(this, new TaskUpdatedEventArgs(Task.CreateTask(childDbTask), childDbTask));
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
                dbTaskList = tasksQuery.OrderBy(t => t.SortOrder).ToList();
            }
            else
            {
                switch (sortColumn.Name)
                {
                    case "StatusId":
                        dbTaskList = tasksQuery.OrderBy(t => t.Status.Description).ToList();
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
                    case "Title":
                        dbTaskList = tasksQuery.OrderBy(t => t.Title).ToList();
                        break;
                    default:
                        dbTaskList = tasksQuery.OrderBy(t => t.SortOrder).ToList();
                        break;
                }
            }

            return dbTaskList;
        }

        private List<Data.Task> SortList(List<Data.Task> dbTaskList, SortableProperty sortColumn = null)
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
                        dbTaskList = dbTaskList.OrderBy(t => t.Status.Description).ToList();
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
                    case "SortOrder":
                        dbTaskList = dbTaskList.OrderBy(t => t.SortOrder).ToList();
                        break;
                    default:
                        dbTaskList = dbTaskList.OrderBy(t => t.Title).ToList();
                        break;
                }
            }

            return dbTaskList;
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
                               orderby t.SortOrder
                               select t);
            }
            else
            {
                activeTasks = (from t in _appInfo.GcContext.Tasks
                               where t.Title.Contains(filterTerm) && t.IsActive == true
                               orderby t.SortOrder
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
                // add the added item to the cached query results
                List<Data.Task> allTasks = (List<Data.Task>)cachedQuery.Value;
                Data.Task addedTask = _appInfo.GcContext.Tasks.FirstOrDefault(t => t.TaskID == e.NewTask.TaskId);
                if (addedTask != null)
                {
                    allTasks.Add(addedTask);
                    allTasks = SortList(allTasks);
                    _appInfo.GlobalQueryCache.UpdateCacheItem(Constants.AllTasksCacheItem, allTasks);
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
                    _appInfo.GlobalQueryCache.UpdateCacheItem(Constants.AllTasksCacheItem, allTasks);
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
                Data.Task oldTask = allTasks.FirstOrDefault(t => t.TaskID == e.UpdatedTask.TaskId);
                Data.Task newTask = e.UpdatedDbTask;
                if (oldTask != null && newTask != null)
                {
                    allTasks.Remove(oldTask);
                    allTasks.Add(newTask);
                }
                else if (newTask != null)
                {
                    allTasks.Add(newTask);
                }

                allTasks = SortList(allTasks);

                _appInfo.GlobalQueryCache.UpdateCacheItem(Constants.AllTasksCacheItem, allTasks);
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
                    this.TaskUpdated(this, new TaskUpdatedEventArgs(Task.CreateTask(child), child));
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
                if (e.NewTask.IsActive)
                {
                    // add the added item to the cached query results
                    List<Data.Task> activeTasks = (List<Data.Task>)cachedQuery.Value;
                    Data.Task addedTask = _appInfo.GcContext.Tasks.FirstOrDefault(t => t.TaskID == e.NewTask.TaskId);
                    if (addedTask != null)
                    {
                        activeTasks.Add(addedTask);
                        activeTasks = SortList(activeTasks, new SortableProperty() { Description = "Sort Order", Name = "SortOrder" });
                        _appInfo.GlobalQueryCache.UpdateCacheItem(Constants.ActiveTasksCacheItem, activeTasks);
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
                    _appInfo.GlobalQueryCache.UpdateCacheItem(Constants.ActiveTasksCacheItem, activeTasks);
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
                if (e.UpdatedTask.IsActive)
                {
                    Data.Task oldTask = activeTasks.FirstOrDefault(t => t.TaskID == e.UpdatedTask.TaskId);
                    Data.Task newTask = e.UpdatedDbTask;
                    if (oldTask != null && newTask != null)
                    {
                        activeTasks.Remove(oldTask);
                        activeTasks.Add(newTask);
                    }
                    else if (newTask != null)
                    {
                        activeTasks.Add(newTask);
                    }

                    activeTasks = SortList(activeTasks, new SortableProperty() { Description = "Sort Order", Name = "SortOrder" });
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

                _appInfo.GlobalQueryCache.UpdateCacheItem(Constants.ActiveTasksCacheItem, activeTasks);
            }
        }

        #endregion // Private Helpers
    }
}