using System;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;

namespace TaskConqueror
{
    /// <summary>
    /// This WorkspaceViewModel subclass provides search, paging, and sorting functionality.
    /// </summary>
    public abstract class NavigatorViewModel : WorkspaceViewModel
    {
        #region Fields

        RelayCommand _filterResultsCommand;
        RelayCommand _sortResultsCommand;

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

        ObservableCollection<SortableProperty> _sortColumns = new ObservableCollection<SortableProperty>();

        public ObservableCollection<SortableProperty> SortColumns
        {
            get
            {
                return _sortColumns;
            }
        }

        SortableProperty _selectedSortColumn;

        public SortableProperty SelectedSortColumn
        {
            get
            {
                return _selectedSortColumn;
            }

            set
            {
                if (_selectedSortColumn == value)
                    return;

                _selectedSortColumn = value;

                base.OnPropertyChanged("SelectedSortColumn");
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

        public ICommand SortResultsCommand
        {
            get
            {
                if (_sortResultsCommand == null)
                {
                    _sortResultsCommand = new RelayCommand(
                        param => this.SortResults(),
                        param => this.CanSortResults()
                            );
                }

                return _sortResultsCommand;
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

        public abstract void SortResults();

        public bool CanSortResults()
        {
            return SelectedSortColumn != null;
        }

        #endregion
    }
}