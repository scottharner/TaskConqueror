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
    /// A tree-friendly wrapper for an unassigned task container.
    /// </summary>
    public class UnassignedTreeNodeViewModel : ITreeNodeViewModel, ITreeNodeContainerViewModel
    {
        #region Fields

        bool _isSelected = false;
        ObservableCollection<ITreeNodeViewModel> _childNodes = new ObservableCollection<ITreeNodeViewModel>();
        ITreeNodeContainerViewModel _parent;

        #endregion // Fields

        #region Constructor

        public UnassignedTreeNodeViewModel(TaskData taskData, ProjectData projectData, ITreeNodeContainerViewModel parent)
        {
            if (taskData == null)
                throw new ArgumentNullException("task data");

            List<Project> unassignedProjects = projectData.GetUnassignedProjectsContainingInactiveTasks();

            foreach (Project unassignedProject in unassignedProjects)
            {
                _childNodes.Add(new ProjectTreeNodeViewModel(unassignedProject, projectData, this));
            }
            
            List<Task> unassignedTasks = taskData.GetUnassignedInactiveTasks();

            foreach (Task unassignedTask in unassignedTasks)
            {
                _childNodes.Add(new TaskTreeNodeViewModel(unassignedTask, this));
            }

            _parent = parent;
        }

        #endregion // Constructor

        #region Unassigned Properties

        public int NodeId
        {
            get { return 0; }
        }

        public string Title
        {
            get { return "Unassigned"; }
        }

        public ObservableCollection<ITreeNodeViewModel> ChildNodes
        {
            get { return _childNodes; }
        }

        public ITreeNodeContainerViewModel Parent
        {
            get { return _parent; }
        }

        public int SortWeight
        {
            get { return SortWeights.Unassigned; }
        }

        #endregion // Properties

        #region Presentation Properties

        /// <summary>
        /// Gets/sets whether this customer is selected in the UI.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; }
        }

        #endregion // Presentation Properties

    }
}