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
    public class CompletedGoalsReport : DateRangeReportBase
    {
        #region Properties

        public override string Title
        {
            get { return "Completed Goals"; }
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
                {"CategoryDescription", "Category"},
                {"CreatedDate", "Date Created"},
                {"CompletedDate", "Date Completed"}
            };

            GoalData gData = new GoalData();
            ProjectData pData = new ProjectData();
            TaskData tData = new TaskData();
            List<Goal> completedGoals = gData.GetCompletedGoalsByDate(StartDate, EndDate);
            List<GoalViewModel> rowData = new List<GoalViewModel>();
            foreach (Goal goal in completedGoals)
            {
                rowData.Add(new GoalViewModel(goal, gData, pData, tData));
            }

            flowDocument.Blocks.Add(FlowDocumentHelper.BuildTable<GoalViewModel>(columnDefinitions, rowData));

            foreach (GoalViewModel goalVm in rowData)
                goalVm.Dispose();

            return flowDocument;
        }

        #endregion

    }
}
