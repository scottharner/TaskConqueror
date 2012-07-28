using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows;

namespace TaskConqueror
{
    public class CompletedProjectsReport : DateRangeReportBase
    {
        #region Properties

        public override string Title
        {
            get { return "Completed Projects"; }
        }

        #endregion

        #region Methods

        public override FlowDocument Build()
        {
            FlowDocument flowDocument = base.Build();

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
