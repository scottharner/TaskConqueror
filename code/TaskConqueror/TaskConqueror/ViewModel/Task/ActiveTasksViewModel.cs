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
    public class ActiveTasksViewModel : NavigatorViewModel
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
            base.DisplayImage = "pack://application:,,,/TaskConqueror;Component/Assets/Images/task_list.png";

            _taskData = taskData;

            // Subscribe for notifications of when a new task is saved.
            _taskData.TaskAdded += this.OnTaskAdded;
            _taskData.TaskUpdated += this.OnTaskUpdated;
            _taskData.TaskDeleted += this.OnTaskDeleted;

            // Populate the AllTasks collection with TaskViewModels.
            this.ActiveTasks = new ObservableCollection<TaskViewModel>();
            this.ActiveTasks.CollectionChanged += this.OnCollectionChanged;

            this.PageFirst();

            SortColumns.Add(new SortableProperty() { Description = "Title", Name = "Title" });
            SortColumns.Add(new SortableProperty() { Description = "Status", Name = "StatusId" });
            SortColumns.Add(new SortableProperty() { Description = "Priority", Name = "PriorityId" });
            SortColumns.Add(new SortableProperty() { Description = "Project", Name = "ProjectTitle" });
            SortColumns.Add(new SortableProperty() { Description = "Date Created", Name = "CreatedDate" });
            SortColumns.Add(new SortableProperty() { Description = "Date Completed", Name = "CompletedDate" });

            SelectedSortColumn = SortColumns.FirstOrDefault();
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
            if (e.NewTask.IsActive)
            {
                RefreshPage();
            }
        }

        void OnTaskUpdated(object sender, TaskUpdatedEventArgs e)
        {
            RefreshPage();
        }

        void OnTaskDeleted(object sender, TaskDeletedEventArgs e)
        {
            if (e.DeletedTask.IsActive)
            {
                RefreshPage();
            }
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
                        param => this.DeactivateCompletedTasks(),
                        param => this.CanDeactivateCompletedTasks()
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

        /// <summary>
        /// Returns a command that adds tasks to the active task list.
        /// </summary>
        public ICommand AddCommand
        {
            get
            {
                if (_addCommand == null)
                {
                    _addCommand = new RelayCommand(
                        param => this.AddTasks()
                        );
                }
                return _addCommand;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Deactivates the selected task.
        /// </summary>
        public void DeactivateTask()
        {
            TaskViewModel selectedTaskVM = ActiveTasks.FirstOrDefault(t => t.IsSelected == true);

            if (selectedTaskVM != null)
            {
                selectedTaskVM.IsActive = false;
                selectedTaskVM.Save();
            }
        }

        /// <summary>
        /// Deactivates all completed tasks.
        /// </summary>
        public void DeactivateCompletedTasks()
        {
            List<TaskViewModel> completedTaskList = ActiveTasks.Where(t => t.IsCompleted == true).ToList();

            if (completedTaskList != null)
            {
                foreach (TaskViewModel taskVM in completedTaskList)
                {
                    taskVM.IsActive = false;
                    taskVM.Save();
                }
            }
        }

        /// <summary>
        /// Launches the edit task window.
        /// </summary>
        public void EditTask()
        {
            TaskView window = new TaskView();

            TaskViewModel selectedTaskVM = ActiveTasks.FirstOrDefault(t => t.IsSelected == true);

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
            TaskViewModel selectedTaskVM = ActiveTasks.FirstOrDefault(t => t.IsSelected == true);
            
            if (selectedTaskVM != null && MessageBox.Show("Are you sure you want to delete the selected task?", "Confirm Cancel", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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
            AddTasksView window = new AddTasksView();

            using (var viewModel = new AddTasksViewModel(_taskData, new ProjectData(), new GoalData()))
            {
                this.ShowWorkspaceAsDialog(window, viewModel);
            }
        }

        public override void PerformFilter()
        {
            if (FilterTermHasChanged)
            {
                PageFirst();
            }
        }

        public override void SortResults()
        {
            PageFirst();
        }

        public override void GetPagedTasks(int pageNumber)
        {
            for (int i = (ActiveTasks.Count - 1); i >= 0; i--)
            {
                TaskViewModel taskVm = ActiveTasks[i];
                this.ActiveTasks.Remove(taskVm);
                taskVm.Dispose();
            }

            List<TaskViewModel> active =
                (from task in _taskData.GetActiveTasks(FilterTerm, SelectedSortColumn, pageNumber)
                 select new TaskViewModel(task, _taskData)).ToList();

            foreach (TaskViewModel tvm in active)
                tvm.PropertyChanged += this.OnTaskViewModelPropertyChanged;

            for (int i = 0; i < active.Count; i++)
            {
                this.ActiveTasks.Add(active[i]);
            }

            FirstRecordNumber = ActiveTasks.Count > 0 ? (Constants.RecordsPerPage * (pageNumber - 1)) + 1 : 0;
            LastRecordNumber = FirstRecordNumber + ActiveTasks.Count - 1;
            TotalRecordCount = _taskData.GetActiveTasksCount(FilterTerm);
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

        bool CanDeactivateCompletedTasks()
        {
            return ActiveTasks.Count(t => t.IsCompleted == true) > 0;
        }

        #endregion
    }
}