using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace TaskConqueror
{
    /// <summary>
    /// A UI-friendly wrapper for a Task object.
    /// </summary>
    public class CompletedGoalsReportViewModel : EditorViewModel, IDataErrorInfo
    {
        #region Fields

        readonly CompletedGoalsReport _completedGoalsReport;
        RelayCommand _runCommand;
        bool? _dialogResult;

        #endregion // Fields

        #region Constructor

        public CompletedGoalsReportViewModel(CompletedGoalsReport completedGoalsReport)
        {
            if (completedGoalsReport == null)
                throw new ArgumentNullException("completedGoalsReport");

            _completedGoalsReport = completedGoalsReport;

            base.DisplayName = Properties.Resources.CompletedGoalsReport_DisplayName;            
        }

        #endregion // Constructor

        #region Completed Goals Properties

        public DateTime StartDate
        {
            get { return _completedGoalsReport.StartDate; }
            set
            {
                if (value == _completedGoalsReport.StartDate)
                    return;

                _completedGoalsReport.StartDate = value;

                base.OnPropertyChanged("StartDate");
            }
        }

        public DateTime EndDate
        {
            get { return _completedGoalsReport.EndDate; }
            set
            {
                if (value == _completedGoalsReport.EndDate)
                    return;

                _completedGoalsReport.EndDate = value;

                base.OnPropertyChanged("EndDate");
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
            if (!_completedGoalsReport.IsValid)
                throw new InvalidOperationException(Properties.Resources.Exception_CannotRunReport);

            DialogResult = true;

            this.OnRequestClose();
        }

        #endregion // Public Methods

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error
        {
            get { return (_completedGoalsReport as IDataErrorInfo).Error; }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                string error = null;

                error = (_completedGoalsReport as IDataErrorInfo)[propertyName];

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
        
        #endregion

        #region Private Helpers

        /// <summary>
        /// Returns true if the task is valid and can be saved.
        /// </summary>
        protected bool CanRunReport
        {
            get { return _completedGoalsReport.IsValid; }
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