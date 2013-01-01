using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using System.Windows.Forms;

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
        RelayCommand _moveUpCommand;
        RelayCommand _moveDownCommand;

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

            GetActiveTasks();
            
            // select the first task
            TaskViewModel firstTask = ActiveTasks.FirstOrDefault();
            if (firstTask != null)
            {
                firstTask.IsSelected = true;
            }
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
                RefreshTasksAfterModification();
            }
        }

        void OnTaskUpdated(object sender, TaskUpdatedEventArgs e)
        {
            RefreshTasksAfterModification();
        }

        void OnTaskDeleted(object sender, TaskDeletedEventArgs e)
        {
            if (e.DeletedTask.IsActive)
            {
                RefreshTasksAfterModification();
            }
        }

        void RefreshTasksAfterModification()
        {
            bool taskSelected = false;

            TaskViewModel origSelectedTask = ActiveTasks.Where(t => t.IsSelected == true).FirstOrDefault();
            GetActiveTasks();
            if (origSelectedTask != null)
            {
                TaskViewModel newSelectedTask = ActiveTasks.Where(t => t.TaskId == origSelectedTask.TaskId).FirstOrDefault();
                if (newSelectedTask != null)
                {
                    newSelectedTask.IsSelected = true;
                    taskSelected = true;
                }
            }

            if (!taskSelected)
            {
                TaskViewModel firstTask = ActiveTasks.FirstOrDefault();
                if (firstTask != null)
                    firstTask.IsSelected = true;
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
        /// Returns a command that moves the selected task up in order within the active task list.
        /// </summary>
        public ICommand MoveUpCommand
        {
            get
            {
                if (_moveUpCommand == null)
                {
                    _moveUpCommand = new RelayCommand(
                        param => this.MoveUpTask(),
                        param => this.CanMoveUpTask()
                        );
                }
                return _moveUpCommand;
            }
        }

        /// <summary>
        /// Returns a command that moves the selected task down in order within the active task list.
        /// </summary>
        public ICommand MoveDownCommand
        {
            get
            {
                if (_moveDownCommand == null)
                {
                    _moveDownCommand = new RelayCommand(
                        param => this.MoveDownTask(),
                        param => this.CanMoveDownTask()
                        );
                }
                return _moveDownCommand;
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
        /// Moves the selected task up in order within active task list.
        /// </summary>
        public void MoveUpTask()
        {
            TaskViewModel selectedTaskVM = ActiveTasks.FirstOrDefault(t => t.IsSelected == true);
            TaskViewModel previousTaskVM = ActiveTasks[ActiveTasks.IndexOf(selectedTaskVM)-1];

            if (selectedTaskVM != null && previousTaskVM != null)
            {
                int selectedSortOrder = selectedTaskVM.SortOrder.Value;
                int previousSortorder = previousTaskVM.SortOrder.Value;
                selectedTaskVM.SortOrder = previousSortorder;
                selectedTaskVM.Save();
                previousTaskVM.SortOrder = selectedSortOrder;
                previousTaskVM.Save();
            }
        }

        /// <summary>
        /// Moves the selected task down in order within active task list.
        /// </summary>
        public void MoveDownTask()
        {
            TaskViewModel selectedTaskVM = ActiveTasks.FirstOrDefault(t => t.IsSelected == true);
            TaskViewModel nextTaskVM = ActiveTasks[ActiveTasks.IndexOf(selectedTaskVM)+1];

            if (selectedTaskVM != null && nextTaskVM != null)
            {
                int selectedSortOrder = selectedTaskVM.SortOrder.Value;
                int nextSortOrder = nextTaskVM.SortOrder.Value;
                selectedTaskVM.SortOrder = nextSortOrder;
                selectedTaskVM.Save();
                nextTaskVM.SortOrder = selectedSortOrder;
                nextTaskVM.Save();
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

            if (selectedTaskVM != null && WPFMessageBox.Show(Properties.Resources.Delete_Confirm, Properties.Resources.Tasks_Delete_Confirm, WPFMessageBoxButtons.YesNo, WPFMessageBoxImage.Question) == WPFMessageBoxResult.Yes)
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

            using (ProjectData pdata = new ProjectData())
            {
                using (GoalData gData = new GoalData())
                {
                    using (var viewModel = new AddTasksViewModel(_taskData, pdata, gData))
                    {
                        this.ShowWorkspaceAsDialog(window, viewModel);
                    }
                }
            }
        }

        public void GetActiveTasks()
        {
            for (int i = (ActiveTasks.Count - 1); i >= 0; i--)
            {
                TaskViewModel taskVm = ActiveTasks[i];
                this.ActiveTasks.Remove(taskVm);
                taskVm.Dispose();
            }

            List<TaskViewModel> active =
                (from task in _taskData.GetActiveTasks()
                 select new TaskViewModel(task, _taskData)).ToList();

            foreach (TaskViewModel tvm in active)
                tvm.PropertyChanged += this.OnTaskViewModelPropertyChanged;

            for (int i = 0; i < active.Count; i++)
            {
                this.ActiveTasks.Add(active[i]);
            }
        }

        public override void ViewHelp()
        {
            Help.ShowHelp(null, "TaskConqueror.chm", "html/active_tasks/search.htm");
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

        bool CanMoveUpTask()
        {
            TaskViewModel selectedTask = ActiveTasks.Where(t => t.IsSelected == true).FirstOrDefault();
            return selectedTask != null && ActiveTasks.IndexOf(selectedTask) > 0 && ActiveTasks.Count(t => t.IsSelected == true) == 1;
        }

        bool CanMoveDownTask()
        {
            TaskViewModel selectedTask = ActiveTasks.Where(t => t.IsSelected == true).FirstOrDefault();
            return selectedTask != null && (ActiveTasks.IndexOf(selectedTask) < (ActiveTasks.Count - 1)) && ActiveTasks.Count(t => t.IsSelected == true) == 1;
        }

        #endregion
    }
}