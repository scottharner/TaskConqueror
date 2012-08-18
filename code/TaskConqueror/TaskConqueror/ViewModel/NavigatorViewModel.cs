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
        RelayCommand _pageFirstCommand;
        RelayCommand _pageLastCommand;
        RelayCommand _pageNextCommand;
        RelayCommand _pagePreviousCommand;

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

        int _firstRecordNumber;

        public int FirstRecordNumber
        {
            get
            {
                return _firstRecordNumber;
            }

            set
            {
                if (_firstRecordNumber == value)
                    return;

                _firstRecordNumber = value;
                base.OnPropertyChanged("FirstRecordNumber");
            }
        }

        int _lastRecordNumber;

        public int LastRecordNumber
        {
            get
            {
                return _lastRecordNumber;
            }

            set
            {
                if (_lastRecordNumber == value)
                    return;

                _lastRecordNumber = value;
                base.OnPropertyChanged("LastRecordNumber");
            }
        }

        int _totalRecordCount;

        public int TotalRecordCount
        {
            get
            {
                return _totalRecordCount;
            }

            set
            {
                if (_totalRecordCount == value)
                    return;

                _totalRecordCount = value;
                base.OnPropertyChanged("TotalRecordCount");
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

        SortableProperty _selectedSortColumn = null;

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

        public ICommand PageFirstCommand
        {
            get
            {
                if (_pageFirstCommand == null)
                {
                    _pageFirstCommand = new RelayCommand(
                        param => this.PageFirst(),
                        param => this.CanPageFirst()
                            );
                }

                return _pageFirstCommand;
            }
        }

        public ICommand PageLastCommand
        {
            get
            {
                if (_pageLastCommand == null)
                {
                    _pageLastCommand = new RelayCommand(
                        param => this.PageLast(),
                        param => this.CanPageLast()
                            );
                }

                return _pageLastCommand;
            }
        }

        public ICommand PageNextCommand
        {
            get
            {
                if (_pageNextCommand == null)
                {
                    _pageNextCommand = new RelayCommand(
                        param => this.PageNext(),
                        param => this.CanPageNext()
                            );
                }

                return _pageNextCommand;
            }
        }

        public ICommand PagePreviousCommand
        {
            get
            {
                if (_pagePreviousCommand == null)
                {
                    _pagePreviousCommand = new RelayCommand(
                        param => this.PagePrevious(),
                        param => this.CanPagePrevious()
                            );
                }

                return _pagePreviousCommand;
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

        public bool CanPageFirst()
        {
            return FirstRecordNumber > Constants.RecordsPerPage;
        }

        public bool CanPageLast()
        {
            return LastRecordNumber < TotalRecordCount;
        }

        public bool CanPageNext()
        {
            return LastRecordNumber < TotalRecordCount;
        }

        public bool CanPagePrevious()
        {
            return FirstRecordNumber > Constants.RecordsPerPage;
        }

        public void PageFirst()
        {
            GetPagedTasks(1);
        }

        public void PageLast()
        {
            GetPagedTasks((TotalRecordCount - 1) / Constants.RecordsPerPage + 1);
        }

        public void PageNext()
        {
            GetPagedTasks((LastRecordNumber) / Constants.RecordsPerPage + 1);
        }

        public void PagePrevious()
        {
            GetPagedTasks((FirstRecordNumber - 2) / Constants.RecordsPerPage + 1);
        }

        public abstract void GetPagedTasks(int pageNumber);

        #endregion
    }
}