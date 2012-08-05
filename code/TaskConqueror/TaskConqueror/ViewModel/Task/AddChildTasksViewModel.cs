using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Data;
using System.Collections.ObjectModel;

namespace TaskConqueror
{
    /// <summary>
    /// Allows addition of a task to a project.
    /// </summary>
    public class AddChildTasksViewModel : WorkspaceViewModel
    {
        #region Fields

        RelayCommand _addCommand;
        RelayCommand _okCommand;
        RelayCommand _removeCommand;
        ObservableCollection<TaskViewModel> _unassignedTasks = new ObservableCollection<TaskViewModel>();
        ObservableCollection<TaskViewModel> _selectedTasks = new ObservableCollection<TaskViewModel>();
        readonly Project _parentProject;
        readonly ProjectData _projectData;
        readonly TaskData _taskData;

        #endregion // Fields

        #region Constructor

        public AddChildTasksViewModel(TaskData taskData, Project parentProject, ProjectData projectData)
        {
            if (taskData == null)
                throw new ArgumentNullException("taskData");

            if (projectData == null)
                throw new ArgumentNullException("projectData");
            
            if (parentProject == null)
                throw new ArgumentNullException("parentProject");

            List<Task> unassignedTasks = taskData.GetUnassignedTasks();
            foreach (Task unassignedTask in unassignedTasks)
	        {
		        _unassignedTasks.Add(new TaskViewModel(unassignedTask, taskData));
	        }

            _parentProject = parentProject;
            _projectData = projectData;
            _taskData = taskData;

            base.DisplayName = Properties.Resources.Add_Tasks_DisplayName;
            base.DisplayImage = "pack://application:,,,/TaskConqueror;Component/Assets/Images/task.png";
        }

        #endregion // Constructor

        #region Task Properties

        /// <summary>
        /// the tasks that have been selected for addition
        /// </summary>       
        public ObservableCollection<TaskViewModel> SelectedTasks
        {
            get { return _selectedTasks; }
            set
            {
                if (_selectedTasks == value)
                    return;

                _selectedTasks = value;

                this.OnPropertyChanged("SelectedTasks");
            }
        }

        /// <summary>
        /// list of tasks that have not be assigned to a project
        /// </summary>       
        public ObservableCollection<TaskViewModel> UnassignedTasks
        {
            get { return _unassignedTasks; }
            set
            {
                if (_unassignedTasks == value)
                    return;

                _unassignedTasks = value;

                this.OnPropertyChanged("UnassignedTasks");
            }
        }

        #endregion // Properties

        #region Presentation Properties

        /// <summary>
        /// Returns a command that adds the task to the selected list.
        /// </summary>
        public ICommand AddCommand
        {
            get
            {
                if (_addCommand == null)
                {
                    _addCommand = new RelayCommand(
                        param => this.AddTask(),
                        param => this.CanAddTask
                        );
                }
                return _addCommand;
            }
        }

        /// <summary>
        /// Returns a command that adds the selected task to the current project.
        /// </summary>
        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand(
                        param => { this.AddTasksToProject(); this.OnRequestClose(); },
                        param => this.CanAddTasksToProject
                        );
                }
                return _okCommand;
            }
        }

        /// <summary>
        /// Returns a command that removes a task from the selected task list.
        /// </summary>
        public ICommand RemoveCommand
        {
            get
            {
                if (_removeCommand == null)
                {
                    _removeCommand = new RelayCommand(
                        param => this.RemoveTask(),
                        param => this.CanRemoveTask
                        );
                }
                return _removeCommand;
            }
        }

        #endregion // Presentation Properties

        #region Public Methods

        /// <summary>
        /// Adds the task to the selected list.  This method is invoked by the AddCommand.
        /// </summary>
        public void AddTask()
        {
            TaskViewModel selectedTaskVM = _unassignedTasks.FirstOrDefault(t => t.IsSelected == true);
            SelectedTasks.Add(selectedTaskVM);
            UnassignedTasks.Remove(selectedTaskVM);
        }

        /// <summary>
        /// Removes the task from the selected list.  This method is invoked by the RemoveCommand.
        /// </summary>
        public void RemoveTask()
        {
            TaskViewModel selectedTaskVM = SelectedTasks.FirstOrDefault(t => t.IsSelected == true);
            UnassignedTasks.Add(selectedTaskVM);
            SelectedTasks.Remove(selectedTaskVM);
        }

        /// <summary>
        /// Adds the selected tasks to the current project.
        /// </summary>
        public void AddTasksToProject()
        {
            List<int> selectedTaskIds = new List<int>();
            foreach (TaskViewModel selectedTask in SelectedTasks)
            {
                selectedTaskIds.Add(selectedTask.TaskId);
            }
            
            _taskData.AddTasksToProject(_parentProject, selectedTaskIds);
        }

        #endregion // Public Methods

        #region Private Helpers

        /// <summary>
        /// Returns true if the task is valid and can be saved.
        /// </summary>
        bool CanAddTask
        {
            get { return UnassignedTasks.Any(t => t.IsSelected == true); }
        }

        /// <summary>
        /// Returns true if there is a task available for removal from the selected list.
        /// </summary>
        bool CanRemoveTask
        {
            get { return SelectedTasks.Any(t => t.IsSelected == true); }
        }

        /// <summary>
        /// Returns true if there are tasks to be added to the current project.
        /// </summary>
        bool CanAddTasksToProject
        {
            get
            { return SelectedTasks.Count > 0; }
        }

        #endregion // Private Helpers

        #region  Base Class Overrides

        protected override void OnDispose()
        {
            foreach (TaskViewModel taskVM in this.SelectedTasks)
                taskVM.Dispose();

            this.SelectedTasks.Clear();

            foreach (TaskViewModel taskVM in this.UnassignedTasks)
                taskVM.Dispose();

            this.UnassignedTasks.Clear();
        }

        #endregion // Base Class Overrides
    }
}