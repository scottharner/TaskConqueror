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

            Dictionary<string, Tuple<string, string>> columnDefinitions = new Dictionary<string, Tuple<string, string>>()
            {
                {"Title", new Tuple<string, string>("Title", null)},
                {"StatusDescription", new Tuple<string, string>("Status", null)},
                {"CategoryDescription", new Tuple<string, string>("Category", null)},
                {"CreatedDate", new Tuple<string, string>("Date Created", null)},
                {"CompletedDate", new Tuple<string, string>("Date Completed", null)}
            };

            using (GoalData gData = new GoalData())
            {
                using (ProjectData pData = new ProjectData())
                {
                    using (TaskData tData = new TaskData())
                    {
                        List<Goal> completedGoals = gData.GetCompletedGoalsByDate(StartDate, EndDate);
                        List<GoalViewModel> rowData = new List<GoalViewModel>();
                        foreach (Goal goal in completedGoals)
                        {
                            rowData.Add(new GoalViewModel(goal, gData, pData, tData));
                        }

                        flowDocument.Blocks.Add(FlowDocumentHelper.BuildTable<GoalViewModel>(columnDefinitions, rowData));

                        foreach (GoalViewModel goalVm in rowData)
                            goalVm.Dispose();
                    }
                }
            }

            return flowDocument;
        }

        #endregion

    }
}
