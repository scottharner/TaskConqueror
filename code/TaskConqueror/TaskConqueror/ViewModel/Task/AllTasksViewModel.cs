﻿using System;
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
    public class AllTasksViewModel : WorkspaceViewModel
    {
        #region Fields

        readonly TaskData _taskData;
        RelayCommand _newCommand;
        RelayCommand _editCommand;
        RelayCommand _deleteCommand;
        RelayCommand _filterResultsCommand;
        bool _filterTermHasChanged = false;

        #endregion // Fields

        #region Constructor

        public AllTasksViewModel(TaskData taskData)
        {
            if (taskData == null)
                throw new ArgumentNullException("taskData");

            base.DisplayName = Properties.Resources.Tasks_DisplayName;
            base.DisplayImage = "pack://application:,,,/TaskConqueror;Component/Assets/Images/task.png";

            _taskData = taskData;

            // Subscribe for notifications of when a new task is saved.
            _taskData.TaskAdded += this.OnTaskAdded;
            _taskData.TaskUpdated += this.OnTaskUpdated;
            _taskData.TaskDeleted += this.OnTaskDeleted;

            // Populate the AllTasks collection with TaskViewModels.
            this.CreateAllTasks();              
        }

        void CreateAllTasks()
        {
            List<TaskViewModel> all =
                (from task in _taskData.GetTasks(FilterTerm)
                 select new TaskViewModel(task, _taskData)).ToList();

            foreach (TaskViewModel tvm in all)
                tvm.PropertyChanged += this.OnTaskViewModelPropertyChanged;

            this.AllTasks = new ObservableCollection<TaskViewModel>(all);
            this.AllTasks.CollectionChanged += this.OnCollectionChanged;
        }

        void ClearAllTasks()
        {
            foreach (TaskViewModel taskVM in this.AllTasks)
                taskVM.Dispose();

            this.AllTasks.Clear();
            this.AllTasks.CollectionChanged -= this.OnCollectionChanged;
        }

        #endregion // Constructor

        #region Public Interface

        /// <summary>
        /// Returns a collection of all the TaskViewModel objects.
        /// </summary>
        public ObservableCollection<TaskViewModel> AllTasks { get; private set; }

        private string _filterTerm = "";

        public string FilterTerm
        {
            get
            {
                return _filterTerm;
            }

            set
            {
                if (_filterTerm == value)
                    return;

                _filterTerm = value;
                _filterTermHasChanged = true;
                base.OnPropertyChanged("FilterTerm");
            }
        }

        #endregion // Public Interface

        #region  Base Class Overrides

        protected override void OnDispose()
        {
            ClearAllTasks();

            _taskData.TaskAdded -= this.OnTaskAdded;
            _taskData.TaskUpdated -= this.OnTaskUpdated;
            _taskData.TaskDeleted -= this.OnTaskDeleted;
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
            var viewModel = new TaskViewModel(e.NewTask, _taskData);
            this.AllTasks.Add(viewModel);
        }

        void OnTaskUpdated(object sender, TaskUpdatedEventArgs e)
        {
            this.AllTasks.Remove(this.AllTasks.FirstOrDefault(t => t.TaskId == e.UpdatedTask.TaskId));
            var viewModel = new TaskViewModel(e.UpdatedTask, _taskData);
            this.AllTasks.Add(viewModel);
        }

        void OnTaskDeleted(object sender, TaskDeletedEventArgs e)
        {
            TaskViewModel taskVM = this.AllTasks.FirstOrDefault(t => t.TaskId == e.DeletedTask.TaskId);
            this.AllTasks.Remove(taskVM);
            taskVM.Dispose();
        }

        #endregion // Event Handling Methods

        #region Commands

        /// <summary>
        /// Returns a command that creates a new task.
        /// </summary>
        public ICommand NewCommand
        {
            get
            {
                if (_newCommand == null)
                {
                    _newCommand = new RelayCommand(
                        param => this.CreateTask()
                        );
                }
                return _newCommand;
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

        /// <summary>
        /// Returns a command that deletes an existing task.
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new RelayCommand(
                        param => this.DeleteTask(),
                        param => this.CanDeleteTask()
                        );
                }
                return _deleteCommand;
            }
        }

        public ICommand FilterResultsCommand
        {
            get
            {
                if (_filterResultsCommand == null)
                {
                    _filterResultsCommand = new RelayCommand(
                        param => this.FilterResults()
                        );
                }

                return _filterResultsCommand;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Launches the new task window.
        /// </summary>
        public void CreateTask()
        {
            TaskView window = new TaskView();

            using (var viewModel = new TaskViewModel(Task.CreateNewTask(), _taskData))
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

            TaskViewModel selectedTaskVM = AllTasks.FirstOrDefault(t => t.IsSelected == true);

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
            using (TaskViewModel selectedTaskVM = AllTasks.FirstOrDefault(t => t.IsSelected == true))
            {
                if (selectedTaskVM != null && MessageBox.Show(Properties.Resources.Tasks_Delete_Confirm, Properties.Resources.Delete_Confirm, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _taskData.DeleteTask(_taskData.GetTaskByTaskId(selectedTaskVM.TaskId));
                }
            }
        }

        public void FilterResults()
        {
            if (_filterTermHasChanged)
            {
                for (int i = (AllTasks.Count - 1); i >= 0; i--)
                {
                    TaskViewModel taskVm = AllTasks[i];
                    this.AllTasks.Remove(taskVm);
                    taskVm.Dispose();
                }

                List<TaskViewModel> all =
                    (from task in _taskData.GetTasks(FilterTerm)
                     select new TaskViewModel(task, _taskData)).ToList();

                foreach (TaskViewModel tvm in all)
                    tvm.PropertyChanged += this.OnTaskViewModelPropertyChanged;

                for (int i = 0; i < all.Count; i++)
                {
                    this.AllTasks.Add(all[i]);
                }

                _filterTermHasChanged = false;
            }
        }

        #endregion // Public Methods

        #region Private Helpers

        bool CanEditTask()
        {
            return AllTasks.Count(t => t.IsSelected == true) == 1; 
        }

        bool CanDeleteTask()
        {
            return AllTasks.Count(t => t.IsSelected == true) == 1;
        }

        #endregion
    }
}