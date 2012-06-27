using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows;

namespace TaskConqueror
{
    /// <summary>
    /// Represents a container of TaskViewModel objects
    /// that has support for staying synchronized with the
    /// database.  This class also provides information
    /// related to multiple selected customers.
    /// </summary>
    public class ActiveTasksViewModel : WorkspaceViewModel
    {
        #region Fields

        readonly TaskData _taskData;
        RelayCommand _addCommand;
        RelayCommand _editCommand;
        RelayCommand _deactivateCompletedCommand;
        RelayCommand _deactivateCommand;

        #endregion // Fields

        #region Constructor

        public ActiveTasksViewModel(TaskData taskData)
        {
            if (taskData == null)
                throw new ArgumentNullException("taskData");

            base.DisplayName = Properties.Resources.Active_Tasks_DisplayName;            

            _taskData = taskData;

            // Subscribe for notifications of when a new task is saved.
            _taskData.TaskAdded += this.OnTaskAdded;
            _taskData.TaskUpdated += this.OnTaskUpdated;
            _taskData.TaskDeleted += this.OnTaskDeleted;

            // Populate the ActiveTasks collection with TaskViewModels.
            this.CreateActiveTasks();              
        }

        void CreateActiveTasks()
        {
            List<TaskViewModel> allActive =
                (from task in _taskData.GetActiveTasks()
                 select new TaskViewModel(task, _taskData)).ToList();

            foreach (TaskViewModel tvm in allActive)
                tvm.PropertyChanged += this.OnTaskViewModelPropertyChanged;

            this.ActiveTasks = new ObservableCollection<TaskViewModel>(allActive);
            this.ActiveTasks.CollectionChanged += this.OnCollectionChanged;
        }

        #endregion // Constructor

        #region Public Interface

        /// <summary>
        /// Returns a collection of all the active TaskViewModel objects.
        /// </summary>
        public ObservableCollection<TaskViewModel> ActiveTasks { get; private set; }

        #endregion // Public Interface

        #region  Base Class Overrides

        protected override void OnDispose()
        {
            foreach (TaskViewModel taskVM in this.ActiveTasks)
                taskVM.Dispose();

            this.ActiveTasks.Clear();
            this.ActiveTasks.CollectionChanged -= this.OnCollectionChanged;

            _taskData.TaskAdded -= this.OnTaskAdded;
        }

        #endregion // Base Class Overrides

        #region Event Handling Methods

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (TaskViewModel taskVM in e.NewItems)
                    taskVM.PropertyChanged += this.OnTaskViewModelPropertyChanged;

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (TaskViewModel taskVM in e.OldItems)
                    taskVM.PropertyChanged -= this.OnTaskViewModelPropertyChanged;
        }

        void OnTaskViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string IsSelected = "IsSelected";

            // Make sure that the property name we're referencing is valid.
            // This is a debugging technique, and does not execute in a Release build.
            (sender as TaskViewModel).VerifyPropertyName(IsSelected);

        }

        void OnTaskAdded(object sender, TaskAddedEventArgs e)
        {
            if (e.NewTask.IsActive)
            {
                var viewModel = new TaskViewModel(e.NewTask, _taskData);
                this.ActiveTasks.Add(viewModel);
            }
        }

        void OnTaskUpdated(object sender, TaskUpdatedEventArgs e)
        {
            this.ActiveTasks.Remove(this.ActiveTasks.FirstOrDefault(t => t.TaskId == e.UpdatedTask.TaskId));
            if (e.UpdatedTask.IsActive)
            {
                var viewModel = new TaskViewModel(e.UpdatedTask, _taskData);
                this.ActiveTasks.Add(viewModel);
            }
        }

        void OnTaskDeleted(object sender, TaskDeletedEventArgs e)
        {
            this.ActiveTasks.Remove(this.ActiveTasks.FirstOrDefault(t => t.TaskId == e.DeletedTask.TaskId));
        }

        #endregion // Event Handling Methods

        #region Commands

        /// <summary>
        /// Returns a command that deactivates the selected task.
        /// </summary>
        public ICommand DeactivateCommand
        {
            get
            {
                if (_deactivateCommand == null)
                {
                    _deactivateCommand = new RelayCommand(
                        param => this.DeactivateTask(),
                        param => this.CanDeactivateTask()
                        );
                }
                return _deactivateCommand;
            }
        }

        /// <summary>
        /// Returns a command that deactivates all the completed tasks.
        /// </summary>
        public ICommand DeactivateCompletedCommand
        {
            get
            {
                if (_deactivateCompletedCommand == null)
                {
                    _deactivateCompletedCommand = new RelayCommand(
                        param => this.DeactivateCompletedTasks()
                        );
                }
                return _deactivateCompletedCommand;
            }
        }

        /// <summary>
        /// Returns a command that edits an existing task.
        /// </summary>
        public ICommand EditCommand
        {
            get
            {
                if (_editCommand == null)
                {
                    _editCommand = new RelayCommand(
                        param => this.EditTask(),
                        param => this.CanEditTask()
                        );
                }
                return _editCommand;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Deactivates the selected task.
        /// </summary>
        public void DeactivateTask()
        {
            TaskView window = new TaskView();

            TaskViewModel selectedTaskVM = ActiveTasks.FirstOrDefault(t => t.IsSelected == true);

            var viewModel = new TaskViewModel(_taskData.GetTaskByTaskId(selectedTaskVM.TaskId), _taskData);

            viewModel.IsActive = false;

            viewModel.Save();
        }

        /// <summary>
        /// Deactivates all completed tasks.
        /// </summary>
        public void DeactivateCompletedTasks()
        {
            TaskView window = new TaskView();

            List<TaskViewModel> completedTaskList = ActiveTasks.Where(t => t.IsCompleted == true).ToList();

            foreach (TaskViewModel taskVM in completedTaskList)
            {
                var viewModel = new TaskViewModel(_taskData.GetTaskByTaskId(taskVM.TaskId), _taskData);

                viewModel.IsActive = false;

                viewModel.Save();
            }
        }

        /// <summary>
        /// Launches the edit task window.
        /// </summary>
        public void EditTask()
        {
            TaskView window = new TaskView();

            TaskViewModel selectedTaskVM = ActiveTasks.FirstOrDefault(t => t.IsSelected == true);
            
            var viewModel = new TaskViewModel(_taskData.GetTaskByTaskId(selectedTaskVM.TaskId), _taskData);

            // When the ViewModel asks to be closed, 
            // close the window.
            EventHandler handler = null;
            handler = delegate
            {
                viewModel.RequestClose -= handler;
                window.Close();
            };
            viewModel.RequestClose += handler;

            window.DataContext = viewModel;

            window.ShowDialog();
        }

        /// <summary>
        /// Launches the delete task window.
        /// </summary>
        public void DeleteTask()
        {
            TaskViewModel selectedTaskVM = ActiveTasks.FirstOrDefault(t => t.IsSelected == true);
            
            if (selectedTaskVM != null && MessageBox.Show("Are you sure you want to delete the selected task?", "Confirm Cancel", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _taskData.DeleteTask(_taskData.GetTaskByTaskId(selectedTaskVM.TaskId));
            }
        }

        #endregion // Public Methods

        #region Private Helpers

        bool CanEditTask()
        {
            return ActiveTasks.Count(t => t.IsSelected == true) == 1; 
        }

        bool CanDeleteTask()
        {
            return ActiveTasks.Count(t => t.IsSelected == true) == 1;
        }

        bool CanDeactivateTask()
        {
            return ActiveTasks.Count(t => t.IsSelected == true) == 1;
        }

        #endregion
    }
}