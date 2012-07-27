using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows;

namespace TaskConqueror
{
    public class ProjectProgressReport : ReportBase
    {
        #region Fields

        ProjectData _pData;

        #endregion

        #region Constructor

        public ProjectProgressReport()
        {
            _pData = new ProjectData();
            SelectedProject = _pData.GetProjects().FirstOrDefault();
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

        #endregion
    }
}
