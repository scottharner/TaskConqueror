using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace TaskConqueror
{
    /// <summary>
    /// A UI-friendly wrapper for a ProjectProgressReport object.
    /// </summary>
    public class ProjectProgressReportViewModel : EditorViewModel, IDataErrorInfo
    {
        #region Fields

        readonly ProjectProgressReport _projectProgressReport;
        RelayCommand _runCommand;
        List<Project> _projectOptions;
        bool? _dialogResult;

        #endregion // Fields

        #region Constructor

        public ProjectProgressReportViewModel(ProjectProgressReport projectProgressReport)
        {
            if (projectProgressReport == null)
                throw new ArgumentNullException("projectProgressReport");

            _projectProgressReport = projectProgressReport;

            ProjectData pData = new ProjectData();
            _projectOptions = pData.GetProjects();

            base.DisplayName = projectProgressReport.Title;            
        }

        #endregion // Constructor

        #region Project Progress Report Properties

        public Project SelectedProject
        {
            get { return _projectProgressReport.SelectedProject; }
            set
            {
                if (value == _projectProgressReport.SelectedProject)
                    return;

                _projectProgressReport.SelectedProject = value;

                base.OnPropertyChanged("SelectedProject");
            }
        }

        #endregion // Properties

        #region Public Methods

        public override void Save()
        {
        }

        /// <summary>
        /// Runs the report.  This method is invoked by the RunCommand.
        /// </summary>
        public void RunReport()
        {
            if (!_projectProgressReport.IsValid)
                throw new InvalidOperationException(Properties.Resources.Exception_CannotRunReport);

            DialogResult = true;

            this.OnRequestClose();
        }

        #endregion // Public Methods

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error
        {
            get { return (_projectProgressReport as IDataErrorInfo).Error; }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                string error = null;

                error = (_projectProgressReport as IDataErrorInfo)[propertyName];

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

        #region Presentation Properties

        /// <summary>
        /// Returns a command that changes the completed status of the selected task.
        /// </summary>
        public ICommand RunCommand
        {
            get
            {
                if (_runCommand == null)
                {
                    _runCommand = new RelayCommand(
                        param => this.RunReport(),
                        param => this.CanRunReport
                        );
                }
                return _runCommand;
            }
        }

        public bool? DialogResult
        {
            get { return _dialogResult; }
            set
            {
                if (value == _dialogResult)
                    return;

                _dialogResult = value;

                base.OnPropertyChanged("DialogResult");
            }
        }

        public List<Project> ProjectOptions
        {
            get { return _projectOptions; }
            set
            {
                if (value == _projectOptions)
                    return;

                _projectOptions = value;

                base.OnPropertyChanged("ProjectOptions");
            }
        }
        
        #endregion

        #region Private Helpers

        /// <summary>
        /// Returns true if the report criteria are valid and the report can be run.
        /// </summary>
        protected bool CanRunReport
        {
            get { return _projectProgressReport.IsValid; }
        }

        protected override bool CanSave
        {
            get { return true; }
        }

        protected override bool IsSaved
        {
            get { return true; }
        }
        
        #endregion
    }
}