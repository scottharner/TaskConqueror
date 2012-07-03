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
    public class AddTasksViewModel : ViewModelBase
    {
        #region Fields

        RelayCommand _addCommand;
        RelayCommand<object> _selectNodeCommand;
        object _selectedNode;
        ObservableCollection<ITreeNodeContainerViewModel> _goals = new ObservableCollection<ITreeNodeContainerViewModel>();

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
                _goals.Add(new GoalTreeNodeViewModel(goalObj, goalData));
            }

            UnassignedTreeNodeViewModel unassigned = new UnassignedTreeNodeViewModel(taskData, projectData);
            if (unassigned.ChildNodes.Count > 0)
            {
                _goals.Add(unassigned);
            }

            base.DisplayName = Properties.Resources.Add_Tasks_DisplayName;            
        }

        #endregion // Constructor

        #region Task Properties

        public ObservableCollection<ITreeNodeContainerViewModel> InactiveTasksByGoal
        {
            get { return _goals; }
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

        #endregion // Presentation Properties

        #region Public Methods

        /// <summary>
        /// Adds the task to the selected list.  This method is invoked by the AddCommand.
        /// </summary>
        public void AddTask()
        {
            MessageBox.Show("task added");
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