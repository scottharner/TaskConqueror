using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows;

namespace TaskConqueror
{
    public class CompletedProjectsReport : ReportBase
    {
        #region Constructor

        public CompletedProjectsReport()
        {
            StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            EndDate = new DateTime(DateTime.Now.Year, 12, 31, 23, 59, 59);
        }

        #endregion

        #region Properties

        public override string Title
        {
            get { return "Completed Projects"; }
        }

        public DateTime StartDate
        { get; set; }

        public DateTime EndDate
        { get; set; }

        #endregion

        #region Methods

        public override FlowDocument Build()
        {
            FlowDocument flowDocument = new FlowDocument();

            flowDocument.Blocks.Add(FlowDocumentHelper.BuildTitle(Title));

            Paragraph criteriaPara = new Paragraph();
            Span startTitleSpan = new Span();
            startTitleSpan.Inlines.Add("Start Date: ");
            startTitleSpan.FontWeight = FontWeights.Bold;
            Span startValueSpan = new Span();
            startValueSpan.Inlines.Add(StartDate.ToShortDateString());
            Span endTitleSpan = new Span();
            endTitleSpan.Inlines.Add("     End Date: ");
            endTitleSpan.FontWeight = FontWeights.Bold;
            Span endValueSpan = new Span();
            endValueSpan.Inlines.Add(EndDate.ToShortDateString());
            criteriaPara.Inlines.Add(startTitleSpan);
            criteriaPara.Inlines.Add(startValueSpan);
            criteriaPara.Inlines.Add(endTitleSpan);
            criteriaPara.Inlines.Add(endValueSpan);
            flowDocument.Blocks.Add(criteriaPara);

            Dictionary<string, string> columnDefinitions = new Dictionary<string, string>()
            {
                {"Title", "Title"},
                {"StatusDescription", "Status"},
                {"GoalTitle", "Goal"},
                {"EstimatedCost", "Est. Cost"},
                {"CreatedDate", "Date Created"},
                {"CompletedDate", "Date Completed"}
            };

            ProjectData pData = new ProjectData();
            TaskData tData = new TaskData();
            List<Project> completedProjects = pData.GetCompletedProjectsByDate(StartDate, EndDate);
            List<ProjectViewModel> rowData = new List<ProjectViewModel>();
            foreach (Project project in completedProjects)
            {
                rowData.Add(new ProjectViewModel(project, pData, tData));
            }

            flowDocument.Blocks.Add(FlowDocumentHelper.BuildTable<ProjectViewModel>(columnDefinitions, rowData));

            foreach (ProjectViewModel projectVm in rowData)
                projectVm.Dispose();

            return flowDocument;
        }

        #endregion
    }
}
