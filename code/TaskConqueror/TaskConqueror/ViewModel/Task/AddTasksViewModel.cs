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
        object _selectedNode;
        RootTreeNodeViewModel _root = new RootTreeNodeViewModel();
        ObservableCollection<TaskViewModel> _selectedTasks = new ObservableCollection<TaskViewModel>();
        TaskData _taskData;

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
            SelectedTasks.Add(new TaskViewModel(_taskData.GetTaskByTaskId(taskNodeVM.NodeId), _taskData));
            
            // remove the node and any empty ancestors from the tree
            ITreeNodeContainerViewModel parent = taskNodeVM.Parent;
            if (parent != null)
            {
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

        #endregion // Private Helpers
    }
}