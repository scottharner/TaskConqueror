using System;
using System.ComponentModel;
using System.Diagnostics;
using TaskConqueror.Properties;
using System.Linq;

namespace TaskConqueror
{
    /// <summary>
    /// Represents a task.  This class
    /// has built-in validation logic. It is wrapped
    /// by the TaskViewModel class, which enables it to
    /// be easily displayed and edited by a WPF user interface.
    /// </summary>
    public class Task : IDataErrorInfo
    {
        #region Creation

        public static Task CreateNewTask(int? projectId = null)
        {
            Task newTask = new Task()
            {
                IsActive = false,
                StatusId = (int)Statuses.New,
                PriorityId = (int)TaskPriorities.Medium
            };

            if (projectId.HasValue)
            {
                ProjectData projectData = new ProjectData();
                newTask.ParentProject = projectData.GetProjectByProjectId(projectId.Value);
            }

            return newTask;
        }

        public static Task CreateTask(Data.Task dbTask)
        {
            Data.Project parentProject = (from pt in dbTask.Projects
                                            select pt).FirstOrDefault();

            return new Task
            {
                TaskId = dbTask.TaskID,
                StatusId = dbTask.StatusID,
                PriorityId = dbTask.PriorityID,
                IsActive = dbTask.IsActive,
                CreatedDate = dbTask.CreatedDate,
                CompletedDate = dbTask.CompletedDate,
                Title = dbTask.Title,
                ParentProject = parentProject == null ? null : Project.CreateProject(parentProject)
            };
        }

        protected Task()
        {
        }

        #endregion // Creation

        #region State Properties

        /// <summary>
        /// Gets/sets the task id for the task.
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// Gets/sets the task's status id.
        /// </summary>
        public int StatusId { get; set; }

        /// <summary>
        /// Gets/sets the task's priority id.
        /// </summary>
        public int PriorityId { get; set; }

        /// <summary>
        /// Gets/sets whether the task is active.
        /// The default value is false.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets/sets the task's date of creation.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets/sets the task's date of completion.
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// Gets/sets the task's title.
        /// </summary>
        public string Title { get; set; }

        public Project ParentProject { get; set; }

        #endregion // State Properties

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error { get { return null; } }

        string IDataErrorInfo.this[string propertyName]
        {
            get { return this.GetValidationError(propertyName); }
        }

        #endregion // IDataErrorInfo Members

        #region Validation

        /// <summary>
        /// Returns true if this object has no validation errors.
        /// </summary>
        public bool IsValid
        {
            get
            {
                foreach (string property in ValidatedProperties)
                    if (GetValidationError(property) != null)
                        return false;

                return true;
            }
        }

        static readonly string[] ValidatedProperties = 
        { 
            "Title",
            "StatusId",
            "PriorityId"
        };

        string GetValidationError(string propertyName)
        {
            if (Array.IndexOf(ValidatedProperties, propertyName) < 0)
                return null;

            string error = null;

            switch (propertyName)
            {
                case "Title":
                    error = this.ValidateTitle();
                    break;

                case "StatusId":
                    error = this.ValidateStatusId();
                    break;

                case "PriorityId":
                    error = this.ValidatePriorityId();
                    break;

                default:
                    Debug.Fail("Unexpected property being validated on Task: " + propertyName);
                    break;
            }

            return error;
        }

        string ValidateTitle()
        {
            if (IsStringMissing(this.Title))
            {
                return Properties.Resources.Error_MissingTitle;
            }
            return null;
        }

        string ValidateStatusId()
        {
            AppInfo appInfo = AppInfo.Instance;
            if (appInfo.StatusList.FirstOrDefault(s => s.StatusID == this.StatusId) == null)
            {
                return Properties.Resources.Error_InvalidStatus;
            }

            return null;
        }

        string ValidatePriorityId()
        {
            AppInfo appInfo = AppInfo.Instance;
            if (appInfo.PriorityList.FirstOrDefault(p => p.PriorityID == this.PriorityId) == null)
            {
                return Properties.Resources.Task_Error_InvalidPriority;
            }

            return null;
        }

        static bool IsStringMissing(string value)
        {
            return
                String.IsNullOrEmpty(value) ||
                value.Trim() == String.Empty;
        }

        #endregion // Validation
    }
}