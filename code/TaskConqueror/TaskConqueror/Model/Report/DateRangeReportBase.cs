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
    public abstract class DateRangeReportBase : ReportBase, IDataErrorInfo
    {
        #region Constructor

        public DateRangeReportBase()
        {
            StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            EndDate = new DateTime(DateTime.Now.Year, 12, 31, 23, 59, 59);
        }

        #endregion

        #region Properties

        public DateTime StartDate
        { get; set; }

        public DateTime EndDate
        { get; set; }

        #endregion

        #region Methods

        public override bool? GatherCriteria()
        {
            bool? criteriaResult = false;

            DateRangeReportView window = new DateRangeReportView();

            using (var viewModel = new DateRangeReportViewModel(this))
            {
                // When the ViewModel asks to be closed, 
                // close the window.
                EventHandler handler = null;
                handler = delegate
                {
                    viewModel.RequestClose -= handler;
                    window.Close();
                };
                viewModel.RequestClose += handler;

                window.DataContext = viewModel;

                criteriaResult = window.ShowDialog();
            }

            return criteriaResult;
        }

        public override FlowDocument Build()
        {
            FlowDocument flowDocument = new FlowDocument();

            flowDocument.Blocks.Add(FlowDocumentHelper.BuildTitle(Title));

            Paragraph criteriaPara = new Paragraph();
            Span startTitleSpan = new Span();
            startTitleSpan.Inlines.Add("Start Date: ");
            startTitleSpan.FontWeight = FontWeights.Bold;
            Span startValueSpan = new Span();
            startValueSpan.Inlines.Add(StartDate.ToShortDateString());
            Span endTitleSpan = new Span();
            endTitleSpan.Inlines.Add("     End Date: ");
            endTitleSpan.FontWeight = FontWeights.Bold;
            Span endValueSpan = new Span();
            endValueSpan.Inlines.Add(EndDate.ToShortDateString());
            criteriaPara.Inlines.Add(startTitleSpan);
            criteriaPara.Inlines.Add(startValueSpan);
            criteriaPara.Inlines.Add(endTitleSpan);
            criteriaPara.Inlines.Add(endValueSpan);
            flowDocument.Blocks.Add(criteriaPara);

            return flowDocument;
        }

        #endregion

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error { get { return null; } }

        string IDataErrorInfo.this[string propertyName]
        {
            get { return this.GetValidationError(propertyName); }
        }

        #endregion // IDataErrorInfo Members

        #region Validation

        /// <summary>
        /// Returns true if this object has no validation errors.
        /// </summary>
        public bool IsValid
        {
            get
            {
                foreach (string property in ValidatedProperties)
                    if (GetValidationError(property) != null)
                        return false;

                return true;
            }
        }

        static readonly string[] ValidatedProperties = 
        { 
            "StartDate",
            "EndDate"
        };

        string GetValidationError(string propertyName)
        {
            if (Array.IndexOf(ValidatedProperties, propertyName) < 0)
                return null;

            string error = null;

            switch (propertyName)
            {
                case "StartDate":
                    error = this.ValidateStartDate();
                    break;

                case "EndDate":
                    error = this.ValidateEndDate();
                    break;

                default:
                    Debug.Fail("Unexpected property being validated on Report: " + propertyName);
                    break;
            }

            return error;
        }

        string ValidateStartDate()
        {
            if (this.StartDate == null)
            {
                return Properties.Resources.Error_MissingStartDate;
            }
            else if (this.StartDate > DateTime.Now)
            {
                return Properties.Resources.Error_FutureStartDate;
            }

            return null;
        }

        string ValidateEndDate()
        {
            if (this.EndDate == null)
            {
                return Properties.Resources.Error_MissingEndDate;
            }
            else if (this.StartDate >= this.EndDate)
            {
                return Properties.Resources.Error_EarlyEndDate;
            }

            return null;
        }

        #endregion // Validation

    }
}
