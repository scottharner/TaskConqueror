using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace TaskConqueror
{
    /// <summary>
    /// A tree-friendly wrapper for a Task object.
    /// </summary>
    public class TaskTreeNodeViewModel : ITreeNodeViewModel
    {
        #region Fields

        readonly Task _task;
        bool _isSelected;
        ITreeNodeContainerViewModel _parent;

        #endregion // Fields

        #region Constructor

        public TaskTreeNodeViewModel(Task task, ITreeNodeContainerViewModel parent)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            _task = task;
            _parent = parent;
        }

        #endregion // Constructor

        #region Task Properties

        public int NodeId
        {
            get { return _task.TaskId; }
        }

        public string Title
        {
            get { return _task.Title; }
        }

        public ITreeNodeContainerViewModel Parent
        {
            get { return _parent; }
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