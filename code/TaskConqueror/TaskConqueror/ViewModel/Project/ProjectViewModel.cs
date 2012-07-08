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
    /// A UI-friendly wrapper for a Project object.
    /// </summary>
    public class ProjectViewModel : EditorViewModel, IDataErrorInfo
    {
        #region Fields

        readonly Project _project;
        readonly ProjectData _projectData;
        List<Data.Status> _statusOptions;
        Data.Status _selectedStatus;
        bool _isSaved;
        string _goalTitle;
        string _statusDescription;
        ObservableCollection<Task> _childTasks = new ObservableCollection<Task>();

        #endregion // Fields

        #region Constructor

        public ProjectViewModel(Project project, ProjectData projectData)
        {
            if (project == null)
                throw new ArgumentNullException("project");

            if (projectData == null)
                throw new ArgumentNullException("projectData");

            _project = project;
            _projectData = projectData;
            _statusOptions = AppInfo.Instance.StatusList;
            _selectedStatus = _statusOptions.FirstOrDefault(s => s.StatusID == this.StatusId);
            _isSaved = true;
            _goalTitle = _project.ParentGoal == null ? null : _project.ParentGoal.Title;
            _statusDescription = _statusOptions.FirstOrDefault(s => s.StatusID == this.StatusId).Description;
            _childTasks = new ObservableCollection<Task>(_projectData.GetChildTasks(_project.ProjectId));
            
            base.DisplayName = Properties.Resources.Edit_Project_DisplayName;            
        }

        #endregion // Constructor

        #region Project Properties

        public int ProjectId
        {
            get { return _project.ProjectId; }
            set
            {
                if (value == _project.ProjectId)
                    return;

                _project.ProjectId = value;

                base.OnPropertyChanged("ProjectId");
            }
        }

        public int StatusId
        {
            get { return _project.StatusId; }
            set
            {
                if (value == _project.StatusId)
                    return;

                _project.StatusId = value;
                _isSaved = false;
                _statusDescription = _statusOptions.FirstOrDefault(s => s.StatusID == value).Description;

                base.OnPropertyChanged("StatusId");
            }
        }

        public decimal? EstimatedCost
        {
            get { return _project.EstimatedCost; }
            set
            {
                if (value == _project.EstimatedCost)
                    return;

                _project.EstimatedCost = value;
                _isSaved = false;

                base.OnPropertyChanged("EstimatedCost");
            }
        }

        public DateTime CreatedDate
        {
            get { return _project.CreatedDate; }
            set
            {
                if (value == _project.CreatedDate)
                    return;

                _project.CreatedDate = value;

                base.OnPropertyChanged("CreatedDate");
            }
        }

        public DateTime? CompletedDate
        {
            get { return _project.CompletedDate; }
            set
            {
                if (value == _project.CompletedDate)
                    return;

                _project.CompletedDate = value;

                base.OnPropertyChanged("CompletedDate");
            }
        }

        public string Title
        {
            get { return _project.Title; }
            set
            {
                if (value == _project.Title)
                    return;

                _project.Title = value;
                _isSaved = false;

                base.OnPropertyChanged("Title");
            }
        }

        public Goal ParentGoal
        {
            get { return _project.ParentGoal; }
        }

        public ObservableCollection<Task> ChildTasks
        {
            get { return _childTasks; }
        }

        #endregion // Properties

        #region Presentation Properties

        public List<Data.Status> StatusOptions
        {
            get { return _statusOptions; }
        }

        public Data.Status SelectedStatus
        {
            get { return _selectedStatus; }
            set
            {
                _selectedStatus = value;
                StatusId = _selectedStatus.StatusID;
            }
        }

        public string GoalTitle
        {
            get { return _goalTitle; }
        }

        public string StatusDescription
        {
            get { return _statusDescription; }
        }

        #endregion // Presentation Properties

        #region Public Methods

        /// <summary>
        /// Saves the project to the database.  This method is invoked by the SaveCommand.
        /// </summary>
        public override void Save()
        {
            if (!_project.IsValid)
                throw new InvalidOperationException(Properties.Resources.ProjectViewModel_Exception_CannotSave);

            if (this.IsNewProject)
            {
                CreatedDate = DateTime.Now;
                ProjectId = _projectData.AddProject(_project);
            }
            else
            {
                _projectData.UpdateProject(_project);
            }

            _isSaved = true;
        }

        #endregion // Public Methods

        #region Private Helpers

        /// <summary>
        /// Returns true if this project was created by the user and it has not yet
        /// been saved to the db.
        /// </summary>
        bool IsNewProject
        {
            get { return ProjectId == 0; }
        }

        /// <summary>
        /// Returns true if no changes have been made since the last save.
        /// </summary>
        protected override bool IsSaved
        {
            get { return _isSaved; }
        }

        /// <summary>
        /// Returns true if the project is valid and can be saved.
        /// </summary>
        protected override bool CanSave
        {
            get { return _project.IsValid && (IsNewProject || !IsSaved); }
        }

        #endregion // Private Helpers

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error
        {
            get { return (_project as IDataErrorInfo).Error; }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                string error = null;

                error = (_project as IDataErrorInfo)[propertyName];

                // Dirty the commands registered with CommandManager,
                // such as our Save command, so that they are queried
                // to see if they can execute now.
                CommandManager.InvalidateRequerySuggested();

                return error;
            }
        }

        #endregion // IDataErrorInfo Members

        #region Event Handling Methods

        #endregion
    }
}