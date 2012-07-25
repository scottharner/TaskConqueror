using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows;

namespace TaskConqueror
{
    public class FlowDocumentHelper
    {
        public static Table BuildTable<T>(Dictionary<string, string> columnDefinitions, List<T> rowRecords)
        {
            Table flowTable = new Table();
            flowTable.CellSpacing = 10;
            flowTable.Background = System.Windows.Media.Brushes.White;

            // Create 5 columns and add them to the table's Columns collection.
            int numberOfColumns = columnDefinitions.Count;
            for (int x = 0; x < numberOfColumns; x++)
            {
                flowTable.Columns.Add(new TableColumn());
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
            foreach (string propertyName in columnDefinitions.Keys)
            {
                currentRow.Cells.Add(new TableCell(new Paragraph(new Run(columnDefinitions[propertyName]))));
            }

            int currentRowCount = 1;

            // loop through content and output table rows.
            foreach (T rowRecord in rowRecords)
            {
                flowTable.RowGroups[0].Rows.Add(new TableRow());
                
                currentRow = flowTable.RowGroups[0].Rows[currentRowCount];
                currentRow.FontSize = 12;
                currentRow.FontWeight = FontWeights.Normal;

                foreach (string propertyName in columnDefinitions.Keys)
                {
                    object propertyValue = rowRecord.GetType().GetProperty(propertyName).GetValue(rowRecord, null);
                    string convertedValue = propertyValue == null ? "" : propertyValue.ToString();
                    currentRow.Cells.Add(new TableCell(new Paragraph(new Run(convertedValue))));
                }

                currentRowCount++;
            }

            return flowTable;
        }
    }
}
