using System;
using System.Windows.Input;
using System.Windows;
using System.ComponentModel;

namespace TaskConqueror
{
    /// <summary>
    /// This ViewModelBase subclass requests to be removed 
    /// from the UI when its CloseCommand executes.
    /// This class is abstract.
    /// </summary>
    public abstract class EditorViewModel : WorkspaceViewModel
    {
        #region Fields

        RelayCommand _cancelCommand;
        bool _isSelected;
        RelayCommand _saveCommand;
        ActionCommand<CancelEventArgs> _closeWindowCommand;

        #endregion // Fields

        #region Properties

        protected abstract bool IsSaved
        {
            get;
        }

        /// <summary>
        /// Gets/sets whether this object is selected in the UI.
        /// </summary>
        public virtual bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value == _isSelected)
                    return;

                _isSelected = value;

                base.OnPropertyChanged("IsSelected");
            }
        }

        protected abstract bool CanSave { get; }

        #endregion

        #region Constructor

        protected EditorViewModel()
        {
        }

        #endregion // Constructor

        #region Commands

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                    _cancelCommand = new RelayCommand(
                        param => { 
                            this.OnRequestClose(); 
                        });

                return _cancelCommand;
            }
        }

        public ActionCommand<CancelEventArgs> CloseWindowCommand
        {
            get
            {
                if (_closeWindowCommand == null)
                    _closeWindowCommand = new ActionCommand<CancelEventArgs>(OnClosingWindow);

                return _closeWindowCommand;
            }
        }

        protected void OnClosingWindow(CancelEventArgs e)
        {
            CancelArgs = e;
            this.OnRequestClose();
        }

        /// <summary>
        /// Returns a command that saves the object.
        /// </summary>
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(
                        param => this.Save(),
                        param => this.CanSave
                        );
                }
                return _saveCommand;
            }
        }

        #endregion // Commands

        #region Public Methods

        public abstract void Save();

        #endregion

        #region Overrides

        protected override bool ShouldClose()
        {
            return IsSaved || WPFMessageBox.Show("Confirm Cancel", "Cancel without saving changes?", WPFMessageBoxButtons.YesNo, WPFMessageBoxImage.Question) == WPFMessageBoxResult.Yes;
        }

        #endregion
    }
}