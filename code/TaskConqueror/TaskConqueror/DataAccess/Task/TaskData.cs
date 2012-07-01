using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Data.Objects;
using System.Data.Objects.DataClasses;

namespace TaskConqueror
{
    /// <summary>
    /// Represents a source of tasks in the application.
    /// </summary>
    public class TaskData
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
        public List<Task> GetTasks()
        {
            List<Data.Task> dbTasks = (from t in _appInfo.GcContext.Tasks
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
        /// Returns a shallow-copied list of all active tasks in the repository.
        /// </summary>
        public List<Task> GetActiveTasks()
        {
            List<Data.Task> dbTasks = (from t in _appInfo.GcContext.Tasks
                                       orderby t.Title
                                       where t.IsActive == true
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

        #endregion // Public Interface

        #region Private Helpers

        #endregion // Private Helpers
    }
}