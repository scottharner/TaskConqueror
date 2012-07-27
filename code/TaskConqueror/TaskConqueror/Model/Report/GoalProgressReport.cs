using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows;

namespace TaskConqueror
{
    public class GoalProgressReport : ReportBase
    {        
        #region Fields

        GoalData _gData;

        #endregion

        #region Constructor

        public GoalProgressReport()
        {
            _gData = new GoalData();
            SelectedGoal = _gData.GetGoals().FirstOrDefault();
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

            ProjectData pData = new ProjectData();
            TaskData tData = new TaskData();
            List<Project> childProjects = _gData.GetChildProjects(SelectedGoal.GoalId);
            List<ProjectViewModel> rowData = new List<ProjectViewModel>();
            foreach (Project project in childProjects)
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
