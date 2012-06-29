using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Data;

namespace TaskConqueror
{
    /// <summary>
    /// A UI-friendly wrapper for a Task object.
    /// </summary>
    public class AddTasksViewModel : EditorViewModel, IDataErrorInfo
    {
        #region Fields

        readonly Task _task;
        readonly TaskData _taskData;
        bool _isSelected;
        RelayCommand _saveCommand;
        List<Data.Status> _statusOptions;
        List<Data.TaskPriority> _priorityOptions;
        Data.Status _selectedStatus;
        Data.TaskPriority _selectedPriority;
        bool _isSaved;
        string _projectTitle;
        string _statusDescription;
        string _priorityDescription;
        RelayCommand _toggleCompleteCommand;
        DataSet _inactiveTasksByGoals;

        #endregion // Fields

        #region Constructor

        public AddTasksViewModel(TaskData taskData)
        {
            if (taskData == null)
                throw new ArgumentNullException("taskData");

            _taskData = taskData;
            _inactiveTasksByGoals = CreateDataSet();

            base.DisplayName = Properties.Resources.Add_Tasks_DisplayName;            
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

        public DataSet InactiveTasksByGoal
        {
            get { return _inactiveTasksByGoals; }
        }

        #endregion // Properties

        #region Presentation Properties

        /// <summary>
        /// Gets/sets whether this customer is selected in the UI.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value == _isSelected)
                    return;

                _isSelected = value;

                base.OnPropertyChanged("IsSelected");
            }
        }

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

        /// <summary>
        /// Returns a command that saves the customer.
        /// </summary>
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(
                        param => this.Save(),
                        param => this.CanSave
                        );
                }
                return _saveCommand;
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
        public void Save()
        {
            if (!_task.IsValid)
                throw new InvalidOperationException(Properties.Resources.TaskViewModel_Exception_CannotSave);

            if (this.IsNewTask)
            {
                CreatedDate = DateTime.Now;
                TaskId = _taskData.AddTask(_task);
            }
            else
            {
                _taskData.UpdateTask(_task);
            }

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

        private DataSet CreateDataSet()
        {
            DataSet ds = new DataSet();

            DataTable tbl = new DataTable("Master");
            tbl.Columns.Add("ID", typeof(int));
            tbl.Columns.Add("Name");
            for (int i = 0; i < 3; ++i)
            {
                DataRow row = tbl.NewRow();
                row["ID"] = i;
                row["Name"] = "Master #" + i;
                tbl.Rows.Add(row);
            }

            ds.Tables.Add(tbl);

            tbl = new DataTable("Detail");
            tbl.Columns.Add("MasterID", typeof(int));
            tbl.Columns.Add("Info");
            for (int i = 0; i < 9; ++i)
            {
                DataRow row = tbl.NewRow();
                row["MasterID"] = i % 3;
                row["Info"] = String.Format("Detail Info #{0} for Master #{1}",
                    (i / 3), (i % 3));
                tbl.Rows.Add(row);
            }

            ds.Tables.Add(tbl);

            ds.Relations.Add(
                "Master2Detail",
                ds.Tables["Master"].Columns["ID"],
                ds.Tables["Detail"].Columns["MasterID"]);

            return ds;
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
        bool CanSave
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