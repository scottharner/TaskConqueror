using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace TaskConqueror
{
    /// <summary>
    /// A UI-friendly wrapper for a GoalProgressReport object.
    /// </summary>
    public class GoalProgressReportViewModel : EditorViewModel, IDataErrorInfo
    {
        #region Fields

        readonly GoalProgressReport _goalProgressReport;
        RelayCommand _runCommand;
        List<Goal> _goalOptions;
        bool? _dialogResult;

        #endregion // Fields

        #region Constructor

        public GoalProgressReportViewModel(GoalProgressReport goalProgressReport)
        {
            if (goalProgressReport == null)
                throw new ArgumentNullException("goalProgressReport");

            _goalProgressReport = goalProgressReport;

            GoalData gData = new GoalData();
            _goalOptions = gData.GetGoals();

            base.DisplayName = goalProgressReport.Title;
            base.DisplayImage = "pack://application:,,,/TaskConqueror;Component/Assets/Images/report.png";
        }

        #endregion // Constructor

        #region Goal Progress Report Properties

        public Goal SelectedGoal
        {
            get { return _goalProgressReport.SelectedGoal; }
            set
            {
                if (value == _goalProgressReport.SelectedGoal)
                    return;

                _goalProgressReport.SelectedGoal = value;

                base.OnPropertyChanged("SelectedGoal");
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
            if (!_goalProgressReport.IsValid)
                throw new InvalidOperationException(Properties.Resources.Exception_CannotRunReport);

            DialogResult = true;

            this.OnRequestClose();
        }

        #endregion // Public Methods

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error
        {
            get { return (_goalProgressReport as IDataErrorInfo).Error; }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                string error = null;

                error = (_goalProgressReport as IDataErrorInfo)[propertyName];

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

        public List<Goal> GoalOptions
        {
            get { return _goalOptions; }
            set
            {
                if (value == _goalOptions)
                    return;

                _goalOptions = value;

                base.OnPropertyChanged("GoalOptions");
            }
        }
        
        #endregion

        #region Private Helpers

        /// <summary>
        /// Returns true if the task is valid and can be saved.
        /// </summary>
        protected bool CanRunReport
        {
            get { return _goalProgressReport.IsValid; }
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