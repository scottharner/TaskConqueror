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
    /// A tree-friendly wrapper for a Goal object.
    /// </summary>
    public class GoalTreeNodeViewModel : ITreeNodeViewModel, ITreeNodeContainerViewModel
    {
        #region Fields

        readonly Goal _goal;
        bool _isSelected = false;
        ObservableCollection<ITreeNodeViewModel> _childNodes = new ObservableCollection<ITreeNodeViewModel>();

        #endregion // Fields

        #region Constructor

        public GoalTreeNodeViewModel(Goal goal, GoalData goalData)
        {
            if (goal == null)
                throw new ArgumentNullException("goal");

            _goal = goal;

            List<Project> childProjects = goalData.GetChildProjectsContainingInactiveTasks(goal.GoalId);
            ProjectData pData = new ProjectData();
            foreach (Project childProject in childProjects)
            {
                _childNodes.Add(new ProjectTreeNodeViewModel(childProject, pData));
            }
        }

        #endregion // Constructor

        #region Goal Properties

        public int NodeId
        {
            get { return _goal.GoalId; }
        }

        public string Title
        {
            get { return _goal.Title; }
        }

        public ObservableCollection<ITreeNodeViewModel> ChildNodes
        {
            get { return _childNodes; }
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