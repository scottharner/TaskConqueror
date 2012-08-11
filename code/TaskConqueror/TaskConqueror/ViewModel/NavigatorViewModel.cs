using System;
using System.Windows.Input;
using System.Windows;

namespace TaskConqueror
{
    /// <summary>
    /// This WorkspaceViewModel subclass provides search, paging, and sorting functionality.
    /// </summary>
    public abstract class NavigatorViewModel : WorkspaceViewModel
    {
        #region Fields

        RelayCommand _filterResultsCommand;

        #endregion // Fields

        #region Properties

        private string _filterTerm = "";

        public string FilterTerm
        {
            get
            {
                return _filterTerm;
            }

            set
            {
                if (_filterTerm == value)
                    return;

                _filterTerm = value;
                _filterTermHasChanged = true;
                base.OnPropertyChanged("FilterTerm");
            }
        }

        bool _filterTermHasChanged = false;

        public bool FilterTermHasChanged
        {
            get
            {
                return _filterTermHasChanged;
            }
        }

        #endregion

        #region Constructor

        #endregion // Constructor

        #region Commands

        public ICommand FilterResultsCommand
        {
            get
            {
                if (_filterResultsCommand == null)
                {
                    _filterResultsCommand = new RelayCommand(
                        param => this.FilterResults()
                        );
                }

                return _filterResultsCommand;
            }
        }

        #endregion // Commands

        #region Public Methods

        public void FilterResults()
        {
            PerformFilter();

            _filterTermHasChanged = false;
        }

        public abstract void PerformFilter();

        #endregion
    }
}