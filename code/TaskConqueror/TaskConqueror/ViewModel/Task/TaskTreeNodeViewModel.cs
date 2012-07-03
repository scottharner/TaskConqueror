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

        #endregion // Fields

        #region Constructor

        public TaskTreeNodeViewModel(Task task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            _task = task;
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