using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows;
using System.ComponentModel;
using System.Diagnostics;

namespace TaskConqueror
{
    public class GoalProgressReport : ReportBase, IDataErrorInfo
    {        
        #region Fields

        #endregion

        #region Constructor

        public GoalProgressReport()
        {
        }

        #endregion

        #region Properties

        public override string Title
        {
            get { return "Goal Progress"; }
        }

        public Goal SelectedGoal
        { get; set; }

        #endregion

        #region Methods

        public override FlowDocument Build()
        {
            FlowDocument flowDocument = new FlowDocument();

            flowDocument.Blocks.Add(FlowDocumentHelper.BuildTitle(Title));

            Paragraph criteriaPara = new Paragraph();
            Span titleSpan = new Span();
            titleSpan.Inlines.Add("Title: ");
            titleSpan.FontWeight = FontWeights.Bold;
            Span titleValueSpan = new Span();
            titleValueSpan.Inlines.Add(SelectedGoal.Title);
            Span dateTitleSpan = new Span();
            dateTitleSpan.Inlines.Add("     Date Created: ");
            dateTitleSpan.FontWeight = FontWeights.Bold;
            Span dateValueSpan = new Span();
            dateValueSpan.Inlines.Add(SelectedGoal.CreatedDate.ToShortDateString());
            criteriaPara.Inlines.Add(titleSpan);
            criteriaPara.Inlines.Add(titleValueSpan);
            criteriaPara.Inlines.Add(dateTitleSpan);
            criteriaPara.Inlines.Add(dateValueSpan);
            flowDocument.Blocks.Add(criteriaPara);

            Dictionary<string, string> columnDefinitions = new Dictionary<string, string>()
            {
                {"Title", "Project Title"},
                {"StatusDescription", "Status"},
                {"EstimatedCost", "Estimated Cost"},
                {"CreatedDate", "Date Created"},
                {"CompletedDate", "Date Completed"}
            };

            using (ProjectData pData = new ProjectData())
            {
                using (TaskData tData = new TaskData())
                {
                    using (GoalData gData = new GoalData())
                    {
                        List<Project> childProjects = gData.GetChildProjects(SelectedGoal.GoalId);
                        List<ProjectViewModel> rowData = new List<ProjectViewModel>();
                        foreach (Project project in childProjects)
                        {
                            rowData.Add(new ProjectViewModel(project, pData, tData));
                        }

                        flowDocument.Blocks.Add(FlowDocumentHelper.BuildTable<ProjectViewModel>(columnDefinitions, rowData));

                        foreach (ProjectViewModel projectVm in rowData)
                            projectVm.Dispose();
                    }
                }
            }

            return flowDocument;
        }

        public override bool? GatherCriteria()
        {
            bool? criteriaResult = false;

            GoalProgressReportView window = new GoalProgressReportView();

            using (var viewModel = new GoalProgressReportViewModel(this))
            {
                // When the ViewModel asks to be closed, 
                // close the window.
                EventHandler handler = null;
                handler = delegate
                {
                    viewModel.RequestClose -= handler;
                    window.Close();
                };
                viewModel.RequestClose += handler;

                window.DataContext = viewModel;

                criteriaResult = window.ShowDialog();
            }

            return criteriaResult;
        }

        #endregion

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error { get { return null; } }

        string IDataErrorInfo.this[string propertyName]
        {
            get { return this.GetValidationError(propertyName); }
        }

        #endregion // IDataErrorInfo Members

        #region Validation

        /// <summary>
        /// Returns true if this object has no validation errors.
        /// </summary>
        public bool IsValid
        {
            get
            {
                foreach (string property in ValidatedProperties)
                    if (GetValidationError(property) != null)
                        return false;

                return true;
            }
        }

        static readonly string[] ValidatedProperties = 
        { 
            "SelectedGoal"
        };

        string GetValidationError(string propertyName)
        {
            if (Array.IndexOf(ValidatedProperties, propertyName) < 0)
                return null;

            string error = null;

            switch (propertyName)
            {
                case "SelectedGoal":
                    error = this.ValidateSelectedGoal();
                    break;

                default:
                    Debug.Fail("Unexpected property being validated on Report: " + propertyName);
                    break;
            }

            return error;
        }

        string ValidateSelectedGoal()
        {
            if (this.SelectedGoal == null)
            {
                return Properties.Resources.Error_MissingSelectedGoal;
            }

            return null;
        }

        #endregion // Validation
    }
}
