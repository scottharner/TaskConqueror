using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows;

namespace TaskConqueror
{
    public class CompletedTasksReport : DateRangeReportBase
    {
        #region Properties

        public override string Title
        {
            get { return "Completed Tasks"; }
        }

        #endregion

        #region Methods

        public override FlowDocument Build()
        {
            FlowDocument flowDocument = base.Build();

            Dictionary<string, string> columnDefinitions = new Dictionary<string, string>()
            {
                {"Title", "Title"},
                {"PriorityDescription", "Priority"},
                {"CreatedDate", "Date Created"},
                {"CompletedDate", "Date Completed"}
            };

            using (TaskData tData = new TaskData())
            {
                List<Task> completedTasks = tData.GetCompletedTasksByDate(StartDate, EndDate);
                List<TaskViewModel> rowData = new List<TaskViewModel>();
                foreach (Task task in completedTasks)
                {
                    rowData.Add(new TaskViewModel(task, tData));
                }

                flowDocument.Blocks.Add(FlowDocumentHelper.BuildTable<TaskViewModel>(columnDefinitions, rowData));

                foreach (TaskViewModel taskVm in rowData)
                    taskVm.Dispose();
            }

            return flowDocument;
        }

        #endregion
    }
}
