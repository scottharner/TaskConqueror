using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;

namespace TaskConqueror
{
    /// <summary>
    /// A UI-friendly wrapper for a Report list view item.
    /// </summary>
    public class ReportViewModel
    {
        #region Fields

        string _title;
        Type _reportType;
        bool _isSelected;

        #endregion // Fields

        #region Constructor

        public ReportViewModel(string title, Type reportType)
        {
            if (title == null)
                throw new ArgumentNullException("title");

            if (reportType == null)
                throw new ArgumentNullException("reportType");

            _title = title;
            _reportType = reportType;
        }

        #endregion // Constructor

        #region Report Properties

        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title)
                    return;

                _title = value;
            }
        }

        public Type ReportType
        {
            get { return _reportType; }
            set
            {
                if (value == _reportType)
                    return;

                _reportType = value;
            }
        }

        #endregion // Properties

        #region Presentation Properties

        /// <summary>
        /// Gets/sets whether this object is selected in the UI.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value == _isSelected)
                    return;

                _isSelected = value;
            }
        }

        #endregion // Presentation Properties

    }
}