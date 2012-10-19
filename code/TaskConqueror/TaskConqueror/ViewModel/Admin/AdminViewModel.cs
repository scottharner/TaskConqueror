using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace TaskConqueror
{
    /// <summary>
    /// A UI-friendly wrapper for a Administrative functionality.
    /// </summary>
    public class AdminViewModel : WorkspaceViewModel
    {
        #region Fields

        readonly TaskData _taskData;
        readonly ProjectData _projectData;
        readonly GoalData _goalData;
        readonly List<string> _objectTypeOptions = new List<string>() { ObjectTypes.Any, ObjectTypes.Goal, ObjectTypes.Project, ObjectTypes.Task };
        string _selectedObjectType;
        RelayCommand _purgeCommand;
        bool _includeAbandoned = true;
        bool _includeCompleted = true;

        #endregion // Fields

        #region Constructor

        public AdminViewModel(TaskData taskData, ProjectData projectData, GoalData goalData)
        {
            if (taskData == null)
                throw new ArgumentNullException("taskData");

            if (projectData == null)
                throw new ArgumentNullException("projectData");

            if (goalData == null)
                throw new ArgumentNullException("goalData");

            _taskData = taskData;
            _projectData = projectData;
            _goalData = goalData;
            _selectedObjectType = ObjectTypes.Any;

            base.DisplayName = Properties.Resources.Admin_DisplayName;
            base.DisplayImage = "pack://application:,,,/TaskConqueror;Component/Assets/Images/admin.png";
        }

        #endregion // Constructor

        #region Presentation Properties

        public List<string> ObjectTypeOptions
        {
            get { return _objectTypeOptions; }
        }

        public string SelectedObjectType
        {
            get { return _selectedObjectType; }
            set
            {
                _selectedObjectType = value;
            }
        }

        public bool IncludeAbandoned
        {
            get
            {
                return _includeAbandoned;
            }

            set
            {
                if (value == _includeAbandoned)
                    return;

                _includeAbandoned = value;

                base.OnPropertyChanged("IncludeAbandoned");
            }
        }

        public bool IncludeCompleted
        {
            get
            {
                return _includeCompleted;
            }

            set
            {
                if (value == _includeCompleted)
                    return;

                _includeCompleted = value;

                base.OnPropertyChanged("IncludeCompleted");
            }
        }

        #endregion // Presentation Properties

        #region Public Methods

        /// <summary>
        /// Purge objects from the database.  This method is invoked by the PurgeCommand.
        /// </summary>
        public void Purge()
        {
            if (WPFMessageBox.Show(Properties.Resources.Purge_Confirm, Properties.Resources.Objects_Purge_Confirm, WPFMessageBoxButtons.YesNo, WPFMessageBoxImage.Question) == WPFMessageBoxResult.Yes)
            {
                if (SelectedObjectType == ObjectTypes.Any)
                {
                    if (IncludeAbandoned)
                    {
                        _taskData.PurgeAbandonedTasks();
                        _projectData.PurgeAbandonedProjects(_taskData);
                        _goalData.PurgeAbandonedGoals(_projectData, _taskData);
                    }

                    if (IncludeCompleted)
                    {
                        _taskData.PurgeCompletedTasks();
                        _projectData.PurgeCompletedProjects(_taskData);
                        _goalData.PurgeCompletedGoals(_projectData, _taskData);
                    }
                }
                else if (SelectedObjectType == ObjectTypes.Goal)
                {
                    if (IncludeAbandoned)
                    {
                        _goalData.PurgeAbandonedGoals(_projectData, _taskData);
                    }

                    if (IncludeCompleted)
                    {
                        _goalData.PurgeCompletedGoals(_projectData, _taskData);
                    }
                }
                else if (SelectedObjectType == ObjectTypes.Project)
                {
                    if (IncludeAbandoned)
                    {
                        _projectData.PurgeAbandonedProjects(_taskData);
                    }

                    if (IncludeCompleted)
                    {
                        _projectData.PurgeCompletedProjects(_taskData);
                    }
                }
                else
                {
                    if (IncludeAbandoned)
                    {
                        _taskData.PurgeAbandonedTasks();
                    }

                    if (IncludeCompleted)
                    {
                        _taskData.PurgeCompletedTasks();
                    }
                }

                WPFMessageBox.Show(Properties.Resources.Action_Success, Properties.Resources.Objects_Purge_Success, WPFMessageBoxButtons.OK, WPFMessageBoxImage.Information);
            }
        }

        public override void ViewHelp()
        {
            Help.ShowHelp(null, "TaskConqueror.chm", "html/administrative/purge.htm");
        }

        #endregion // Public Methods

        #region Private Helpers

        /// <summary>
        /// Returns true if the task is valid and can be saved.
        /// </summary>
        protected bool CanPurge
        {
            get { return IncludeAbandoned || IncludeCompleted; }
        }

        #endregion // Private Helpers

        #region Commands

        /// <summary>
        /// Returns a command that purges objects.
        /// </summary>
        public ICommand PurgeCommand
        {
            get
            {
                if (_purgeCommand == null)
                {
                    _purgeCommand = new RelayCommand(
                        param => this.Purge(),
                        param => this.CanPurge
                        );
                }
                return _purgeCommand;
            }
        }

        #endregion
    }
}