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
        public List<Task> GetTasks(string filterTerm = "", SortableProperty sortColumn = null)
        {
            List<Data.Task> dbTasks;

            if (string.IsNullOrEmpty(filterTerm))
            {
                dbTasks = (from t in _appInfo.GcContext.Tasks
                           select t).ToList();
            }
            else
            {
                dbTasks = (from t in _appInfo.GcContext.Tasks
                           where t.Title.Contains(filterTerm)
                           select t).ToList();
            }

            List<Task> tasks = new List<Task>();

            foreach (Data.Task dbTask in dbTasks)
            {
                tasks.Add(Task.CreateTask(dbTask));
            }

            tasks = SortTasks(tasks, sortColumn);

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
        public List<Task> GetActiveTasks(string filterTerm = "", SortableProperty sortColumn = null)
        {
            List<Data.Task> dbTasks;

            if (string.IsNullOrEmpty(filterTerm))
            {
                dbTasks = (from t in _appInfo.GcContext.Tasks
                           where t.IsActive == true
                           select t).ToList();
            }
            else
            {
                dbTasks = (from t in _appInfo.GcContext.Tasks
                           where t.Title.Contains(filterTerm) && t.IsActive == true
                           select t).ToList();
            }

            List<Task> tasks = new List<Task>();

            foreach (Data.Task dbTask in dbTasks)
            {
                tasks.Add(Task.CreateTask(dbTask));
            }

            tasks = SortTasks(tasks, sortColumn);

            return tasks;
        }

        private List<Task> SortTasks(List<Task> tasks, SortableProperty sortColumn = null)
        {
            if (sortColumn == null)
            {
                tasks = tasks.OrderBy(t => t.Title).ToList();
            }
            else
            {
                switch (sortColumn.Name)
                {
                    case "StatusId":
                        tasks = tasks.OrderBy(t => t.StatusId).ToList();
                        break;
                    case "PriorityId":
                        tasks = tasks.OrderByDescending(t => t.PriorityId).ToList();
                        break;
                    case "ProjectTitle":
                        tasks = tasks.OrderBy(t => t.ParentProject == null ? "" : t.ParentProject.Title).ToList();
                        break;
                    case "CreatedDate":
                        tasks = tasks.OrderBy(t => t.CreatedDate).ToList();
                        break;
                    case "CompletedDate":
                        tasks = tasks.OrderBy(t => t.CompletedDate).ToList();
                        break;
                    default:
                        tasks = tasks.OrderBy(t => t.Title).ToList();
                        break;
                }
            }

            return tasks;
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

        #endregion // Public Interface

        #region Private Helpers

        #endregion // Private Helpers
    }
}