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
    /// A UI-friendly wrapper for a DateRangeReportBase object.
    /// </summary>
    public class DateRangeReportViewModel : EditorViewModel, IDataErrorInfo
    {
        #region Fields

        readonly DateRangeReportBase _dateRangeReport;
        RelayCommand _runCommand;
        bool? _dialogResult;

        #endregion // Fields

        #region Constructor

        public DateRangeReportViewModel(DateRangeReportBase dateRangeReport)
        {
            if (dateRangeReport == null)
                throw new ArgumentNullException("dateRangeReport");

            _dateRangeReport = dateRangeReport;

            base.DisplayName = dateRangeReport.Title;
            base.DisplayImage = "pack://application:,,,/TaskConqueror;Component/Assets/Images/report.png";
        }

        #endregion // Constructor

        #region Date Range Report Properties

        public DateTime StartDate
        {
            get { return _dateRangeReport.StartDate; }
            set
            {
                if (value == _dateRangeReport.StartDate)
                    return;

                _dateRangeReport.StartDate = value;

                base.OnPropertyChanged("StartDate");
            }
        }

        public DateTime EndDate
        {
            get { return _dateRangeReport.EndDate; }
            set
            {
                if (value == _dateRangeReport.EndDate)
                    return;

                _dateRangeReport.EndDate = value;

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
            if (!_dateRangeReport.IsValid)
                throw new InvalidOperationException(Properties.Resources.Exception_CannotRunReport);

            DialogResult = true;

            this.OnRequestClose();
        }

        public override void ViewHelp()
        {
            if ((_dateRangeReport as CompletedGoalsReport) != null)
            {
                Help.ShowHelp(null, "TaskConqueror.chm", "html/reports/completed_goals.htm");
            }
            else if ((_dateRangeReport as CompletedProjectsReport) != null)
            {
                Help.ShowHelp(null, "TaskConqueror.chm", "html/reports/completed_projects.htm");
            }
            else
            {
                Help.ShowHelp(null, "TaskConqueror.chm", "html/reports/completed_tasks.htm");
            }
        }

        #endregion // Public Methods

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error
        {
            get { return (_dateRangeReport as IDataErrorInfo).Error; }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                string error = null;

                error = (_dateRangeReport as IDataErrorInfo)[propertyName];

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
            get { return _dateRangeReport.IsValid; }
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