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
    public class ProjectProgressReport : ReportBase, IDataErrorInfo
    {
        #region Fields

        ProjectData _pData;

        #endregion

        #region Constructor

        public ProjectProgressReport()
        {
            _pData = new ProjectData();
        }

        #endregion

        #region Properties

        public override string Title
        {
            get { return "Project Progress"; }
        }

        public Project SelectedProject
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
            titleValueSpan.Inlines.Add(SelectedProject.Title);
            Span dateTitleSpan = new Span();
            dateTitleSpan.Inlines.Add("     Date Created: ");
            dateTitleSpan.FontWeight = FontWeights.Bold;
            Span dateValueSpan = new Span();
            dateValueSpan.Inlines.Add(SelectedProject.CreatedDate.ToShortDateString());
            criteriaPara.Inlines.Add(titleSpan);
            criteriaPara.Inlines.Add(titleValueSpan);
            criteriaPara.Inlines.Add(dateTitleSpan);
            criteriaPara.Inlines.Add(dateValueSpan);
            flowDocument.Blocks.Add(criteriaPara);

            Dictionary<string, string> columnDefinitions = new Dictionary<string, string>()
            {
                {"Title", "Task Title"},
                {"StatusDescription", "Status"},
                {"PriorityDescription", "Priority"},
                {"CreatedDate", "Date Created"},
                {"CompletedDate", "Date Completed"}
            };

            TaskData tData = new TaskData();
            List<Task> childTasks = _pData.GetChildTasks(SelectedProject.ProjectId);
            List<TaskViewModel> rowData = new List<TaskViewModel>();
            foreach (Task task in childTasks)
            {
                rowData.Add(new TaskViewModel(task, tData));
            }

            flowDocument.Blocks.Add(FlowDocumentHelper.BuildTable<TaskViewModel>(columnDefinitions, rowData));

            foreach (TaskViewModel taskVm in rowData)
                taskVm.Dispose();

            return flowDocument;
        }

        public override bool? GatherCriteria()
        {
            bool? criteriaResult = false;

            ProjectProgressReportView window = new ProjectProgressReportView();

            using (var viewModel = new ProjectProgressReportViewModel(this))
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
            "SelectedProject"
        };

        string GetValidationError(string propertyName)
        {
            if (Array.IndexOf(ValidatedProperties, propertyName) < 0)
                return null;

            string error = null;

            switch (propertyName)
            {
                case "SelectedProject":
                    error = this.ValidateSelectedProject();
                    break;

                default:
                    Debug.Fail("Unexpected property being validated on Report: " + propertyName);
                    break;
            }

            return error;
        }

        string ValidateSelectedProject()
        {
            if (this.SelectedProject == null)
            {
                return Properties.Resources.Error_MissingSelectedProject;
            }

            return null;
        }

        #endregion // Validation
    }
}
