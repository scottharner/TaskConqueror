﻿using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace TaskConqueror
{
    /// <summary>
    /// A UI-friendly wrapper for a Task object.
    /// </summary>
    public class TaskViewModel : EditorViewModel, IDataErrorInfo
    {
        #region Fields

        readonly Task _task;
        readonly TaskData _taskData;
        List<Data.Status> _statusOptions;
        List<Data.TaskPriority> _priorityOptions;
        Data.Status _selectedStatus;
        Data.TaskPriority _selectedPriority;
        bool _isSaved;
        string _projectTitle;
        string _statusDescription;
        string _priorityDescription;
        RelayCommand _toggleCompleteCommand;
        int _originalStatusId;
        bool _originalIsActive;

        #endregion // Fields

        #region Constructor

        public TaskViewModel(Task task, TaskData taskData)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            if (taskData == null)
                throw new ArgumentNullException("taskData");

            _task = task;
            _taskData = taskData;
            _statusOptions = AppInfo.Instance.StatusList;
            _priorityOptions = AppInfo.Instance.PriorityList;
            _selectedStatus = _statusOptions.FirstOrDefault(s => s.StatusID == this.StatusId);
            _selectedPriority = _priorityOptions.FirstOrDefault(p => p.PriorityID == this.PriorityId);
            _isSaved = true;
            _projectTitle = _task.ParentProject == null ? null : _task.ParentProject.Title;
            _statusDescription = _statusOptions.FirstOrDefault(s => s.StatusID == this.StatusId).Description;
            _priorityDescription = _priorityOptions.FirstOrDefault(p => p.PriorityID == this.PriorityId).Description;
            _originalStatusId = StatusId;
            _originalIsActive = IsActive;

            base.DisplayName = Properties.Resources.Edit_Task_DisplayName;
            base.DisplayImage = "pack://application:,,,/TaskConqueror;Component/Assets/Images/task.png";
        }

        #endregion // Constructor

        #region Task Properties

        public int TaskId
        {
            get { return _task.TaskId; }
            set
            {
                if (value == _task.TaskId)
                    return;

                _task.TaskId = value;

                base.OnPropertyChanged("TaskId");
            }
        }

        public int StatusId
        {
            get { return _task.StatusId; }
            set
            {
                if (value == _task.StatusId)
                    return;

                _task.StatusId = value;
                _isSaved = false;
                _statusDescription = _statusOptions.FirstOrDefault(s => s.StatusID == value).Description;

                base.OnPropertyChanged("StatusId");
            }
        }

        public int PriorityId
        {
            get { return _task.PriorityId; }
            set
            {
                if (value == _task.PriorityId)
                    return;

                _task.PriorityId = value;
                _isSaved = false;
                _priorityDescription = _priorityDescription = _priorityOptions.FirstOrDefault(p => p.PriorityID == this.PriorityId).Description;

                base.OnPropertyChanged("PriorityId");
            }
        }

        public bool IsActive
        {
            get { return _task.IsActive; }
            set
            {
                if (value == _task.IsActive)
                    return;

                _task.IsActive = value;
                _isSaved = false;

                base.OnPropertyChanged("IsActive");
            }
        }

        public DateTime CreatedDate
        {
            get { return _task.CreatedDate; }
            set
            {
                if (value == _task.CreatedDate)
                    return;

                _task.CreatedDate = value;

                base.OnPropertyChanged("CreatedDate");
            }
        }

        public DateTime? CompletedDate
        {
            get { return _task.CompletedDate; }
            set
            {
                if (value == _task.CompletedDate)
                    return;

                _task.CompletedDate = value;

                base.OnPropertyChanged("CompletedDate");
            }
        }

        public string Title
        {
            get { return _task.Title; }
            set
            {
                if (value == _task.Title)
                    return;

                _task.Title = value;
                _isSaved = false;

                base.OnPropertyChanged("Title");
            }
        }

        public Project ParentProject
        {
            get { return _task.ParentProject; }
        }

        public int? SortOrder
        {
            get { return _task.SortOrder; }
            set
            {
                if (value == _task.SortOrder)
                    return;

                _task.SortOrder = value;

                base.OnPropertyChanged("SortOrder");
            }
        }

        #endregion // Properties

        #region Presentation Properties

        /// <summary>
        /// Gets/sets whether the task is completed.
        /// </summary>
        public bool IsCompleted
        {
            get { return this.StatusId == Statuses.Completed; }
            set
            {
                if ((value == true && this.StatusId == Statuses.Completed) ||
                    value == false && this.StatusId != Statuses.Completed)
                    return;

                if (value == true)
                    this.StatusId = Statuses.Completed;
                else
                    this.StatusId = Statuses.InProgress;

                base.OnPropertyChanged("IsCompleted");
                base.OnPropertyChanged("StatusId");
                base.OnPropertyChanged("StatusDescription");
            }
        }

        /// <summary>
        /// Returns a command that changes the completed status of the selected task.
        /// </summary>
        public ICommand ToggleCompleteCommand
        {
            get
            {
                if (_toggleCompleteCommand == null)
                {
                    _toggleCompleteCommand = new RelayCommand(
                        param => this.ToggleCompleteTask()
                        );
                }
                return _toggleCompleteCommand;
            }
        }

        public List<Data.Status> StatusOptions
        {
            get { return _statusOptions; }
        }

        public List<Data.TaskPriority> PriorityOptions
        {
            get { return _priorityOptions; }
        }

        public Data.Status SelectedStatus
        {
            get { return _selectedStatus; }
            set
            {
                _selectedStatus = value;
                StatusId = _selectedStatus.StatusID;
            }
        }

        public Data.TaskPriority SelectedPriority
        {
            get { return _selectedPriority; }
            set
            {
                _selectedPriority = value;
                PriorityId = _selectedPriority.PriorityID;
            }
        }

        public string ProjectTitle
        {
            get { return _projectTitle; }
        }

        public string StatusDescription
        {
            get { return _statusDescription; }
        }

        public string PriorityDescription
        {
            get { return _priorityDescription; }
        }

        #endregion // Presentation Properties

        #region Public Methods

        /// <summary>
        /// Saves the task to the database.  This method is invoked by the SaveCommand.
        /// </summary>
        public override void Save()
        {
            if (!_task.IsValid)
                throw new InvalidOperationException(Properties.Resources.TaskViewModel_Exception_CannotSave);

            if (this.IsNewTask)
            {
                CreatedDate = DateTime.Now;
                
                if (StatusId == Statuses.Completed)
                    CompletedDate = DateTime.Now;

                if (this.IsActive)
                    SortOrder = _taskData.GetNextAvailableSortOrder();
                
                TaskId = _taskData.AddTask(_task);
            }
            else
            {
                if (StatusId != _originalStatusId && StatusId == Statuses.Completed)
                {
                    CompletedDate = DateTime.Now;
                }
                else if (StatusId != Statuses.Completed && _originalStatusId == Statuses.Completed)
                {
                    CompletedDate = null;
                }

                if (IsActive != _originalIsActive && IsActive == true)
                {
                    SortOrder = _taskData.GetNextAvailableSortOrder();
                }
                else if (IsActive != _originalIsActive)
                {
                    SortOrder = null;
                }
                
                _taskData.UpdateTask(_task);
            }

            _originalStatusId = StatusId;
            _originalIsActive = IsActive;
            _isSaved = true;
        }

        /// <summary>
        /// Changes the completed status of the selected task.
        /// </summary>
        public void ToggleCompleteTask()
        {
            this.IsCompleted = !this.IsCompleted;

            // auto save since this will be called from active task list
            this.Save();
        }

        public override void ViewHelp()
        {
            Help.ShowHelp(null, "TaskConqueror.chm", "html/tasks/edit.htm");
        }

        #endregion // Public Methods

        #region Private Helpers

        /// <summary>
        /// Returns true if this task was created by the user and it has not yet
        /// been saved to the db.
        /// </summary>
        bool IsNewTask
        {
            get { return TaskId == 0; }
        }

        /// <summary>
        /// Returns true if no changes have been made since the last save.
        /// </summary>
        protected override bool IsSaved
        {
            get { return _isSaved; }
        }

        /// <summary>
        /// Returns true if the task is valid and can be saved.
        /// </summary>
        protected override bool CanSave
        {
            get { return _task.IsValid && (IsNewTask || !IsSaved); }
        }

        #endregion // Private Helpers

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error
        {
            get { return (_task as IDataErrorInfo).Error; }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                string error = null;

                error = (_task as IDataErrorInfo)[propertyName];

                // Dirty the commands registered with CommandManager,
                // such as our Save command, so that they are queried
                // to see if they can execute now.
                CommandManager.InvalidateRequerySuggested();

                return error;
            }
        }

        #endregion // IDataErrorInfo Members

        #region Event Handling Methods

        #endregion
    }
}