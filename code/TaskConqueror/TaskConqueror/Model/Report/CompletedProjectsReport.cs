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

            Dictionary<string, Tuple<string, string>> columnDefinitions = new Dictionary<string, Tuple<string, string>>()
            {
                {"Title", new Tuple<string, string>("Title", null)},
                {"StatusDescription", new Tuple<string, string>("Status", null)},
                {"GoalTitle", new Tuple<string, string>("Goal", null)},
                {"EstimatedCost", new Tuple<string, string>("Est. Cost", "{0:C}")},
                {"CreatedDate", new Tuple<string, string>("Date Created", null)},
                {"CompletedDate", new Tuple<string, string>("Date Completed", null)}
            };

            using (ProjectData pData = new ProjectData())
            {
                using (TaskData tData = new TaskData())
                {
                    List<Project> completedProjects = pData.GetCompletedProjectsByDate(StartDate, EndDate);
                    List<ProjectViewModel> rowData = new List<ProjectViewModel>();
                    foreach (Project project in completedProjects)
                    {
                        rowData.Add(new ProjectViewModel(project, pData, tData));
                    }

                    flowDocument.Blocks.Add(FlowDocumentHelper.BuildTable<ProjectViewModel>(columnDefinitions, rowData));

                    foreach (ProjectViewModel projectVm in rowData)
                        projectVm.Dispose();
                }
            }

            return flowDocument;
        }

        #endregion
    }
}
