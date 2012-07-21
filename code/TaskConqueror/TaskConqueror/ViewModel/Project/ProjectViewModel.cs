using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;

namespace TaskConqueror
{
    /// <summary>
    /// A UI-friendly wrapper for a Project object.
    /// </summary>
    public class ProjectViewModel : EditorViewModel, IDataErrorInfo
    {
        #region Fields

        readonly Project _project;
        readonly ProjectData _projectData;
        readonly TaskData _taskData;
        List<Data.Status> _statusOptions;
        Data.Status _selectedStatus;
        bool _isSaved;
        string _goalTitle;
        string _statusDescription;
        ObservableCollection<TaskViewModel> _childTaskVMs = new ObservableCollection<TaskViewModel>();
        RelayCommand _newTaskCommand;
        RelayCommand _editTaskCommand;
        RelayCommand _deleteTaskCommand;
        RelayCommand _addTasksCommand;

        #endregion // Fields

        #region Constructor

        public ProjectViewModel(Project project, ProjectData projectData, TaskData taskData)
        {
            if (project == null)
                throw new ArgumentNullException("project");

            if (projectData == null)
                throw new ArgumentNullException("projectData");

            _project = project;
            _projectData = projectData;
            _taskData = taskData;
            _statusOptions = AppInfo.Instance.StatusList;
            _selectedStatus = _statusOptions.FirstOrDefault(s => s.StatusID == this.StatusId);
            _isSaved = true;
            _goalTitle = _project.ParentGoal == null ? null : _project.ParentGoal.Title;
            _statusDescription = _statusOptions.FirstOrDefault(s => s.StatusID == this.StatusId).Description;
            List<Task> childTasks = _projectData.GetChildTasks(_project.ProjectId);
            foreach (Task childTask in childTasks)
            {
                _childTaskVMs.Add(new TaskViewModel(childTask, taskData));
            }

            // Subscribe for notifications of when a new task is saved.
            _taskData.TaskAdded += this.OnTaskAdded;
            _taskData.TaskUpdated += this.OnTaskUpdated;
            _taskData.TaskDeleted += this.OnTaskDeleted;

            base.DisplayName = Properties.Resources.Edit_Project_DisplayName;            
        }

        #endregion // Constructor

        #region Project Properties

        public int ProjectId
        {
            get { return _project.ProjectId; }
            set
            {
                if (value == _project.ProjectId)
                    return;

                _project.ProjectId = value;

                base.OnPropertyChanged("ProjectId");
            }
        }

        public int StatusId
        {
            get { return _project.StatusId; }
            set
            {
                if (value == _project.StatusId)
                    return;

                _project.StatusId = value;
                _isSaved = false;
                _statusDescription = _statusOptions.FirstOrDefault(s => s.StatusID == value).Description;

                base.OnPropertyChanged("StatusId");
            }
        }

        public decimal? EstimatedCost
        {
            get { return _project.EstimatedCost; }
            set
            {
                if (value == _project.EstimatedCost)
                    return;

                _project.EstimatedCost = value;
                _isSaved = false;

                base.OnPropertyChanged("EstimatedCost");
            }
        }

        public DateTime CreatedDate
        {
            get { return _project.CreatedDate; }
            set
            {
                if (value == _project.CreatedDate)
                    return;

                _project.CreatedDate = value;

                base.OnPropertyChanged("CreatedDate");
            }
        }

        public DateTime? CompletedDate
        {
            get { return _project.CompletedDate; }
            set
            {
                if (value == _project.CompletedDate)
                    return;

                _project.CompletedDate = value;

                base.OnPropertyChanged("CompletedDate");
            }
        }

        public string Title
        {
            get { return _project.Title; }
            set
            {
                if (value == _project.Title)
                    return;

                _project.Title = value;
                _isSaved = false;

                base.OnPropertyChanged("Title");
            }
        }

        public Goal ParentGoal
        {
            get { return _project.ParentGoal; }
        }

        public ObservableCollection<TaskViewModel> ChildTasks
        {
            get { return _childTaskVMs; }
        }

        #endregion // Properties

        #region Presentation Properties

        public List<Data.Status> StatusOptions
        {
            get { return _statusOptions; }
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

        public string GoalTitle
        {
            get { return _goalTitle; }
        }

        public string StatusDescription
        {
            get { return _statusDescription; }
        }

        #endregion // Presentation Properties

        #region Public Methods

        /// <summary>
        /// Saves the project to the database.  This method is invoked by the SaveCommand.
        /// </summary>
        public override void Save()
        {
            if (!_project.IsValid)
                throw new InvalidOperationException(Properties.Resources.ProjectViewModel_Exception_CannotSave);

            if (this.IsNewProject)
            {
                CreatedDate = DateTime.Now;
                ProjectId = _projectData.AddProject(_project);
            }
            else
            {
                _projectData.UpdateProject(_project);
            }

            _isSaved = true;
        }

        /// <summary>
        /// Launches the new task window.
        /// </summary>
        public void CreateTask()
        {
            TaskView window = new TaskView();

            using (var viewModel = new TaskViewModel(Task.CreateNewTask(this.ProjectId), _taskData))
            {
                this.ShowWorkspaceAsDialog(window, viewModel);
            }
        }

        /// <summary>
        /// Launches the edit task window.
        /// </summary>
        public void EditTask()
        {
            TaskView window = new TaskView();
            TaskViewModel selectedTaskVM = ChildTasks.FirstOrDefault(t => t.IsSelected == true);

            using (var viewModel = new TaskViewModel(_taskData.GetTaskByTaskId(selectedTaskVM.TaskId), _taskData))
            {
                this.ShowWorkspaceAsDialog(window, viewModel);
            }
        }

        /// <summary>
        /// Launches the delete task window.
        /// </summary>
        public void DeleteTask()
        {
            TaskViewModel selectedTaskVM = ChildTasks.FirstOrDefault(t => t.IsSelected == true);

            if (selectedTaskVM != null && MessageBox.Show(Properties.Resources.Tasks_Delete_Confirm, Properties.Resources.Delete_Confirm, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _taskData.DeleteTask(_taskData.GetTaskByTaskId(selectedTaskVM.TaskId));
                selectedTaskVM.Dispose();
            }
        }

        /// <summary>
        /// Launches the add tasks window.
        /// </summary>
        public void AddTasks()
        {
            AddChildTasksView window = new AddChildTasksView();

            using (var viewModel = new AddChildTasksViewModel(_taskData, _project, new ProjectData()))
            {
                this.ShowWorkspaceAsDialog(window, viewModel);
            }
        }

        #endregion // Public Methods

        #region Private Helpers

        /// <summary>
        /// Returns true if this project was created by the user and it has not yet
        /// been saved to the db.
        /// </summary>
        bool IsNewProject
        {
            get { return ProjectId == 0; }
        }

        /// <summary>
        /// Returns true if no changes have been made since the last save.
        /// </summary>
        protected override bool IsSaved
        {
            get { return _isSaved; }
        }

        /// <summary>
        /// Returns true if the project is valid and can be saved.
        /// </summary>
        protected override bool CanSave
        {
            get { return _project.IsValid && (IsNewProject || !IsSaved); }
        }

        bool CanEditTask()
        {
            return ChildTasks.Count(t => t.IsSelected == true) == 1;
        }

        bool CanDeleteTask()
        {
            return ChildTasks.Count(t => t.IsSelected == true) == 1;
        }

        #endregion // Private Helpers

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error
        {
            get { return (_project as IDataErrorInfo).Error; }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                string error = null;

                error = (_project as IDataErrorInfo)[propertyName];

                // Dirty the commands registered with CommandManager,
                // such as our Save command, so that they are queried
                // to see if they can execute now.
                CommandManager.InvalidateRequerySuggested();

                return error;
            }
        }

        #endregion // IDataErrorInfo Members

        #region Event Handling Methods

        void OnTaskAdded(object sender, TaskAddedEventArgs e)
        {
            var viewModel = new TaskViewModel(e.NewTask, _taskData);
            this.ChildTasks.Add(viewModel);
        }

        void OnTaskUpdated(object sender, TaskUpdatedEventArgs e)
        {
            this.ChildTasks.Remove(this.ChildTasks.FirstOrDefault(t => t.TaskId == e.UpdatedTask.TaskId));
            var viewModel = new TaskViewModel(e.UpdatedTask, _taskData);
            this.ChildTasks.Add(viewModel);
        }

        void OnTaskDeleted(object sender, TaskDeletedEventArgs e)
        {
            using (var taskVM = this.ChildTasks.FirstOrDefault(t => t.TaskId == e.DeletedTask.TaskId))
            {
                this.ChildTasks.Remove(taskVM);
            }
        }

        #endregion

        #region Commands
        
        /// <summary>
        /// Returns a command that creates a new task.
        /// </summary>
        public ICommand NewTaskCommand
        {
            get
            {
                if (_newTaskCommand == null)
                {
                    _newTaskCommand = new RelayCommand(
                        param => this.CreateTask()
                        );
                }
                return _newTaskCommand;
            }
        }

        /// <summary>
        /// Returns a command that edits an existing task.
        /// </summary>
        public ICommand EditCommand
        {
            get
            {
                if (_editTaskCommand == null)
                {
                    _editTaskCommand = new RelayCommand(
                        param => this.EditTask(),
                        param => this.CanEditTask()
                        );
                }
                return _editTaskCommand;
            }
        }

        /// <summary>
        /// Returns a command that deletes an existing task.
        /// </summary>
        public ICommand DeleteTaskCommand
        {
            get
            {
                if (_deleteTaskCommand == null)
                {
                    _deleteTaskCommand = new RelayCommand(
                        param => this.DeleteTask(),
                        param => this.CanDeleteTask()
                        );
                }
                return _deleteTaskCommand;
            }
        }

        /// <summary>
        /// Returns a command that adds tasks to the project.
        /// </summary>
        public ICommand AddTasksCommand
        {
            get
            {
                if (_addTasksCommand == null)
                {
                    _addTasksCommand = new RelayCommand(
                        param => this.AddTasks()
                        );
                }
                return _addTasksCommand;
            }
        }

        #endregion

        #region  Base Class Overrides

        protected override void OnDispose()
        {
            foreach (TaskViewModel taskVM in this.ChildTasks)
                taskVM.Dispose();

            this.ChildTasks.Clear();

            _taskData.TaskAdded -= this.OnTaskAdded;
            _taskData.TaskUpdated -= this.OnTaskUpdated;
            _taskData.TaskDeleted -= this.OnTaskDeleted;
        }

        #endregion // Base Class Overrides

    }
}