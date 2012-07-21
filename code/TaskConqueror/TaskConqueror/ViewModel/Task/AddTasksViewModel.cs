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
    /// Allows addition of a task to the active tasks list.
    /// </summary>
    public class AddTasksViewModel : WorkspaceViewModel
    {
        #region Fields

        RelayCommand _addCommand;
        RelayCommand<object> _selectNodeCommand;
        RelayCommand _okCommand;
        RelayCommand _removeCommand;
        object _selectedNode;
        RootTreeNodeViewModel _root = new RootTreeNodeViewModel();
        ObservableCollection<TaskViewModel> _selectedTasks = new ObservableCollection<TaskViewModel>();
        TaskData _taskData;
        Dictionary<TaskViewModel, ITreeNodeViewModel> _removedNodes = new Dictionary<TaskViewModel,ITreeNodeViewModel>();

        #endregion // Fields

        #region Constructor

        public AddTasksViewModel(TaskData taskData, ProjectData projectData, GoalData goalData)
        {
            if (taskData == null)
                throw new ArgumentNullException("taskData");

            if (projectData == null)
                throw new ArgumentNullException("projectData");

            if (goalData == null)
                throw new ArgumentNullException("goalData");

            List<Goal> allGoals = goalData.GetGoalsContainingInactiveTasks();
            foreach (Goal goalObj in allGoals)
            {
                _root.ChildNodes.Add(new GoalTreeNodeViewModel(goalObj, goalData, _root));
            }

            UnassignedTreeNodeViewModel unassigned = new UnassignedTreeNodeViewModel(taskData, projectData, _root);
            if (unassigned.ChildNodes.Count > 0)
            {
                _root.ChildNodes.Add(unassigned);
            }

            _taskData = taskData;

            base.DisplayName = Properties.Resources.Add_Tasks_DisplayName;            
        }

        #endregion // Constructor

        #region Task Properties

        public ObservableCollection<ITreeNodeViewModel> InactiveTasksByGoal
        {
            get { return _root.ChildNodes; }
        }

        #endregion // Properties

        #region Presentation Properties

        #region SelectNode

        /// <summary>
        /// Command to select a node in the tree.
        /// </summary>
        public RelayCommand<object> SelectNodeCommand
        {
            get
            {
                if (_selectNodeCommand == null)
                    _selectNodeCommand = new RelayCommand<object>((item) => this.SelectedNode = item, (item) => item != null);

                return _selectNodeCommand;
            }
        }

        #endregion

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
        /// Returns a command that sets the selected tasks to active.
        /// </summary>
        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand(
                        param => { this.ActivateTasks(); this.OnRequestClose(); },
                        param => this.CanActivateTasks
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

        /// <summary>
        /// the node currently selected in the ui,
        /// </summary>       
        public object SelectedNode
        {
            get { return _selectedNode; }
            set
            {
                if (_selectedNode == value)
                    return;

                _selectedNode = value;

                this.OnPropertyChanged("SelectedNode");
            }
        }

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

        #endregion // Presentation Properties

        #region Public Methods

        /// <summary>
        /// Adds the task to the selected list.  This method is invoked by the AddCommand.
        /// </summary>
        public void AddTask()
        {
            // add the selected task to the collection of selected tasks
            TaskTreeNodeViewModel taskNodeVM = SelectedNode as TaskTreeNodeViewModel;
            TaskViewModel taskVM = new TaskViewModel(_taskData.GetTaskByTaskId(taskNodeVM.NodeId), _taskData);
            SelectedTasks.Add(taskVM);
            
            // remove the node and any empty ancestors from the tree
            ITreeNodeContainerViewModel parent = taskNodeVM.Parent;
            if (parent != null)
            {
                _removedNodes.Add(taskVM, taskNodeVM);
                parent.ChildNodes.Remove(taskNodeVM);
                while (parent != null && parent.ChildNodes.Count == 0)
                {
                    ITreeNodeContainerViewModel child = parent;
                    parent = child.Parent;
                    if (parent != null)
                    {
                        parent.ChildNodes.Remove(child);
                    }
                }
            }

            // clear the selected node
            SelectedNode = null;
        }

        /// <summary>
        /// Activates the tasks in the selected list.  This method is invoked by the OkCommand.
        /// </summary>
        public void ActivateTasks()
        {
            foreach (TaskViewModel selectedTaskVM in SelectedTasks)
            {
                selectedTaskVM.IsActive = true;
                selectedTaskVM.Save();
            }
        }

        /// <summary>
        /// Removes the task from the selected list.  This method is invoked by the RemoveCommand.
        /// </summary>
        public void RemoveTask()
        {
            using (TaskViewModel selectedTaskVM = SelectedTasks.FirstOrDefault(t => t.IsSelected == true))
            {
                // add the task and missing ancestors back into the tree
                ITreeNodeViewModel child = _removedNodes[selectedTaskVM];
                ITreeNodeContainerViewModel parent = child.Parent;
                bool inParentCollection = parent.ChildNodes.Contains(child);

                while (parent != null && !inParentCollection)
                {
                    parent.ChildNodes.Add(child);
                    parent.ChildNodes.OrderBy(n => n.Title);
                    SelectedNode = child;
                    child = parent;
                    parent = child.Parent;
                    if (parent != null)
                    {
                        inParentCollection = parent.ChildNodes.Contains(child);
                    }
                }

                SelectedTasks.Remove(selectedTaskVM);
            }
        }

        #endregion // Public Methods

        #region Private Helpers

        /// <summary>
        /// Returns true if the task is valid and can be saved.
        /// </summary>
        bool CanAddTask
        {
            get {
                if (SelectedNode == null)
                    return false;

                Type nodeType = SelectedNode.GetType();
                return nodeType != null && nodeType.Name == "TaskTreeNodeViewModel";
            }
        }

        /// <summary>
        /// Returns true if there is a task available for removal from the selected list.
        /// </summary>
        bool CanRemoveTask
        {
            get { return SelectedTasks.Any(t => t.IsSelected == true); }
        }

        /// <summary>
        /// Returns true if there are tasks to be activated.
        /// </summary>
        bool CanActivateTasks
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
        }

        #endregion // Base Class Overrides
    }
}