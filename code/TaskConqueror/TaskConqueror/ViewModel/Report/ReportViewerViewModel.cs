﻿using System;
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
            // Create a PrintDialog
            PrintDialog printDlg = new PrintDialog();

            // Create IDocumentPaginatorSource from a copy of our flowdocument content
            MemoryStream stream = new MemoryStream();
            TextRange sourceDocument = new TextRange(Content.ContentStart, Content.ContentEnd);
            sourceDocument.Save(stream, DataFormats.Xaml);

            FlowDocument flowDocumentCopy = new FlowDocument();
            TextRange copyDocumentRange = new TextRange(flowDocumentCopy.ContentStart, flowDocumentCopy.ContentEnd);
            copyDocumentRange.Load(stream, DataFormats.Xaml); 
            
            IDocumentPaginatorSource idpSource = flowDocumentCopy;

            try
            {
                // Call PrintDocument method to send document to printer
                printDlg.PrintDocument(idpSource.DocumentPaginator, Title);
                this.OnRequestClose();
            }
            catch (Exception printError)
            {
                MessageBox.Show(Properties.Resources.CannotPrint + " " + printError.Message, Properties.Resources.Error_Encountered);    
            }
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