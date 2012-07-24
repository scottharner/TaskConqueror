using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;

namespace TaskConqueror
{
    public interface IReport
    {
        FlowDocument Content
        {
            get;
            set;
        }

        string Title
        {
            get;
        }
        
        void Run();
    }
}
