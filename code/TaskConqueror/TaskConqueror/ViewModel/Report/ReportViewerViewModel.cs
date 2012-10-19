using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Data;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using System.Windows.Controls;
using System.IO;
using System.Windows.Forms;

namespace TaskConqueror
{
    /// <summary>
    /// Allows viewing of a report object.
    /// </summary>
    public class ReportViewerViewModel : WorkspaceViewModel
    {
        #region Fields

        RelayCommand _printCommand;
        IReport _report;

        #endregion // Fields

        #region Constructor

        public ReportViewerViewModel(IReport report)
        {
            if (report == null)
                throw new ArgumentNullException("report");

            _report = report;

            base.DisplayName = Title;
            base.DisplayImage = "pack://application:,,,/TaskConqueror;Component/Assets/Images/report.png";
        }

        #endregion // Constructor

        #region Report Properties

        public FlowDocument Content
        {
            get { return _report.Content; }
        }

        public string Title
        {
            get { return _report.Title; }
        }

        #endregion // Properties

        #region Presentation Properties

        /// <summary>
        /// Returns a command that prints the selected list.
        /// </summary>
        public ICommand PrintCommand
        {
            get
            {
                if (_printCommand == null)
                {
                    _printCommand = new RelayCommand(
                        param => this.Print()
                        );
                }
                return _printCommand;
            }
        }

        #endregion // Presentation Properties

        #region Public Methods

        /// <summary>
        /// Prints the report content.  This method is invoked by the PrintCommand.
        /// </summary>
        public void Print()
        {
            const double Inch = 96;

            // Create a PrintDialog
            System.Windows.Controls.PrintDialog printDlg = new System.Windows.Controls.PrintDialog();

            // Create IDocumentPaginatorSource from a copy of our flowdocument content
            MemoryStream stream = new MemoryStream();
            TextRange sourceDocument = new TextRange(Content.ContentStart, Content.ContentEnd);
            sourceDocument.Save(stream, System.Windows.DataFormats.Xaml);

            FlowDocument flowDocumentCopy = new FlowDocument();
            TextRange copyDocumentRange = new TextRange(flowDocumentCopy.ContentStart, flowDocumentCopy.ContentEnd);
            copyDocumentRange.Load(stream, System.Windows.DataFormats.Xaml);
            
            double xMargin = (1.25 * Inch);
            double yMargin = (1 * Inch);

            // Set the page padding
            flowDocumentCopy.PagePadding = new Thickness(yMargin, xMargin, xMargin, yMargin);            

            IDocumentPaginatorSource idpSource = flowDocumentCopy;

            try
            {
                // Call PrintDocument method to send document to printer
                printDlg.PageRangeSelection = PageRangeSelection.AllPages;
                printDlg.UserPageRangeEnabled = true;
                if (printDlg.ShowDialog() == true)
                {
                    printDlg.PrintDocument(idpSource.DocumentPaginator, Title);
                    this.OnRequestClose();
                }
            }
            catch (Exception printError)
            {
                WPFMessageBox.Show(Properties.Resources.Error_Encountered, Properties.Resources.CannotPrint + " " + printError.Message, WPFMessageBoxButtons.OK, WPFMessageBoxImage.Error);
            }
        }

        public override void ViewHelp()
        {
            Help.ShowHelp(null, "TaskConqueror.chm", "html/reports/print.htm");
        }

        #endregion // Public Methods

        #region Private Helpers

        #endregion // Private Helpers

        #region  Base Class Overrides

        protected override void OnDispose()
        {
        }

        #endregion // Base Class Overrides
    }
}