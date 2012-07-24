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

        public virtual string Title
        {
            get { return "Active Tasks"; }
        }

        #endregion

        #region Methods

        public override FlowDocument Build()
        {
            FlowDocument flowDocument = new FlowDocument();

            Table flowTable = new Table();
            flowDocument.Blocks.Add(flowTable);
            flowTable.CellSpacing = 10;
            flowTable.Background = System.Windows.Media.Brushes.White;

            // Create 5 columns and add them to the table's Columns collection.
            int numberOfColumns = 5;
            for (int x = 0; x < numberOfColumns; x++)
            {
                flowTable.Columns.Add(new TableColumn());

                // Set alternating background colors for the middle colums.
                if (x % 2 == 0)
                    flowTable.Columns[x].Background = Brushes.Beige;
                else
                    flowTable.Columns[x].Background = Brushes.LightSteelBlue;
            }

            // Create and add an empty TableRowGroup to hold the table's Rows.
            flowTable.RowGroups.Add(new TableRowGroup());

            // Add the header row.
            flowTable.RowGroups[0].Rows.Add(new TableRow());
            TableRow currentRow = flowTable.RowGroups[0].Rows[0];

            // Global formatting for the header row.
            currentRow.FontSize = 18;
            currentRow.FontWeight = FontWeights.Bold;

            // Add cells with content to the second row.
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Title"))));
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Status"))));
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Priority"))));
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Date Created"))));
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("Date Completed"))));

            // set formatting for content rows.
            currentRow.FontSize = 12;
            currentRow.FontWeight = FontWeights.Normal;
            int currentRowCount = 1;

            // loop through content and output table rows.
            TaskData tData = new TaskData();
            List<Task> activeTasks = tData.GetActiveTasks();

            foreach (Task currentTask in activeTasks)
            {
                flowTable.RowGroups[0].Rows.Add(new TableRow());
                currentRow = flowTable.RowGroups[0].Rows[currentRowCount];
                currentRow.Cells.Add(new TableCell(new Paragraph(new Run(currentTask.Title))));
                currentRow.Cells.Add(new TableCell(new Paragraph(new Run(currentTask.StatusId.ToString()))));
                currentRow.Cells.Add(new TableCell(new Paragraph(new Run(currentTask.PriorityId.ToString()))));
                currentRow.Cells.Add(new TableCell(new Paragraph(new Run(currentTask.CreatedDate.ToShortDateString()))));
                currentRow.Cells.Add(new TableCell(new Paragraph(new Run(currentTask.CompletedDate.HasValue ? currentTask.CompletedDate.Value.ToShortDateString() : ""))));
                currentRowCount++;
            }

            return flowDocument;
        }

        #endregion
    }
}
