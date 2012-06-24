using System;
using System.Windows.Input;
using System.Windows;

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

        #endregion // Fields

        #region Properties

        protected abstract bool IsSaved
        {
            get;
        }

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
                        param => { if (IsSaved || MessageBox.Show("Cancel without saving changes?", "Confirm Cancel", MessageBoxButton.YesNo) == MessageBoxResult.Yes) this.OnRequestClose(); });

                return _cancelCommand;
            }
        }

        #endregion // Commands


    }
}