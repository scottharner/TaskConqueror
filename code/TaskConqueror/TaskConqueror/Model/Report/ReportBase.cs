using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;

namespace TaskConqueror
{
    public abstract class ReportBase : IReport
    {
        #region Properties

        FlowDocument _content;

        public FlowDocument Content
        {
            get { return _content; }
            set { _content = value; }
        }

        public virtual string Title
        {
            get { return "Report"; }
        }

        #endregion

        #region Methods

        public virtual void Run()
        {
            Content = Build();

            Display();
        }

        public virtual FlowDocument Build()
        {
            FlowDocument flowDocument = new FlowDocument();
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add("Report not implemented.");
            flowDocument.Blocks.Add(paragraph);

            flowDocument.Blocks.Add(paragraph);

            return flowDocument;
        }

        public void Display()
        {
            ReportViewerView window = new ReportViewerView();

            using (var viewModel = new ReportViewerViewModel(this))
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

                window.ShowDialog();
            }
        }

        #endregion
    }
}
