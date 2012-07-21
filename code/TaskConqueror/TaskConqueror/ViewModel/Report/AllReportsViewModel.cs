using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows;

namespace TaskConqueror
{
    /// <summary>
    /// Represents a container of Report objects
    /// </summary>
    public class AllReportsViewModel : WorkspaceViewModel
    {
        #region Fields

        RelayCommand _runCommand;

        #endregion // Fields

        #region Constructor

        public AllReportsViewModel()
        {
            // Populate the AllReports collection with ReportViewModels.
            this.CreateAllReports();              
        }

        void CreateAllReports()
        {
            List<ReportViewModel> all =
                (from report in Constants.Reports
                 select new ReportViewModel(report.Item1, report.Item2)).ToList();

            this.AllReports = new ObservableCollection<ReportViewModel>(all);

            base.DisplayName = Properties.Resources.Reports_DisplayName;
            base.DisplayImage = "pack://application:,,,/TaskConqueror;Component/Assets/Images/report.png";
        }

        #endregion // Constructor

        #region Public Interface

        /// <summary>
        /// Returns a collection of all the Report objects.
        /// </summary>
        public ObservableCollection<ReportViewModel> AllReports { get; private set; }

        #endregion // Public Interface

        #region  Base Class Overrides

        protected override void OnDispose()
        {
            this.AllReports.Clear();
        }

        #endregion // Base Class Overrides

        #region Commands

        /// <summary>
        /// Returns a command that runs a report.
        /// </summary>
        public ICommand RunCommand
        {
            get
            {
                if (_runCommand == null)
                {
                    _runCommand = new RelayCommand(
                        param => this.RunReport(),
                        param => this.CanRunReport()
                        );
                }
                return _runCommand;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Runs the selected report.
        /// </summary>
        public void RunReport()
        {
            ReportViewModel selectedReportVM = AllReports.FirstOrDefault(r => r.IsSelected == true);

            // todo - instantiate an object based on the selected report vm's type
            MessageBox.Show(selectedReportVM.Title);
        }

        #endregion // Public Methods

        #region Private Helpers

        bool CanRunReport()
        {
            return AllReports.Count(r => r.IsSelected == true) == 1; 
        }

        #endregion
    }
}