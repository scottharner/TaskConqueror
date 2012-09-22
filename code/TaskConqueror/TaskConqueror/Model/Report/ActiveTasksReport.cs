using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace TaskConqueror
{
    public class ActiveTasksReport : ReportBase
    {
        #region Properties

        public override string Title
        {
            get { return "Active Tasks"; }
        }

        #endregion

        #region Methods

        public override FlowDocument Build()
        {
            FlowDocument flowDocument = new FlowDocument();

            flowDocument.Blocks.Add(FlowDocumentHelper.BuildTitle(Title));
            
            Dictionary<string, Tuple<string, string>> columnDefinitions = new Dictionary<string,Tuple<string, string>>()
            {
                {"Title", new Tuple<string, string>("Title", null)},
                {"StatusDescription", new Tuple<string, string>("Status", null)},
                {"PriorityDescription", new Tuple<string, string>("Priority", null)},
                {"CreatedDate", new Tuple<string, string>("Date Created", null)},
                {"CompletedDate", new Tuple<string, string>("Date Completed", null)}
            };

            using (TaskData tData = new TaskData())
            {
                List<Task> activeTasks = tData.GetActiveTasks();
                List<TaskViewModel> rowData = new List<TaskViewModel>();
                foreach (Task task in activeTasks)
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
