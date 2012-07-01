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
    /// A tree-friendly wrapper for a Project object.
    /// </summary>
    public class ProjectTreeNodeViewModel : ITreeNodeContainerViewModel, ITreeNodeViewModel
    {
        #region Fields

        readonly Project _project;
        bool _isSelected;
        ObservableCollection<ITreeNodeViewModel> _childNodes = new ObservableCollection<ITreeNodeViewModel>();

        #endregion // Fields

        #region Constructor

        public ProjectTreeNodeViewModel(Project project, ProjectData projectData)
        {
            if (project == null)
                throw new ArgumentNullException("project");

            _project = project;

            List<Task> childTasks = projectData.GetChildTasks(project.ProjectId);
            foreach (Task childTask in childTasks)
            {
                _childNodes.Add(new TaskTreeNodeViewModel(childTask));
            }
        }

        #endregion // Constructor

        #region Project Properties

        public int NodeId
        {
            get { return _project.ProjectId; }
        }

        public string Title
        {
            get { return _project.Title; }
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
        }

        #endregion // Presentation Properties

    }
}