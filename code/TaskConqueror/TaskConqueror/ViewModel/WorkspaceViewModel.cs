using System;
using System.Windows.Input;
using System.Windows;
using System.Windows.Forms;
using System.ComponentModel;

namespace TaskConqueror
{
    /// <summary>
    /// This ViewModelBase subclass requests to be removed 
    /// from the UI when its CloseCommand executes.
    /// This class is abstract.
    /// </summary>
    public abstract class WorkspaceViewModel : ViewModelBase
    {
        #region Fields

        RelayCommand _closeCommand;
        RelayCommand _viewHelpCommand;

        #endregion // Fields

        #region Properties

        protected CancelEventArgs CancelArgs { get; set; }

        #endregion

        #region Constructor

        protected WorkspaceViewModel()
        {
        }

        #endregion // Constructor

        #region Commands

        /// <summary>
        /// Returns the command that, when invoked, attempts
        /// to remove this workspace from the user interface.
        /// </summary>
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                    _closeCommand = new RelayCommand(param => this.OnRequestClose());

                return _closeCommand;
            }
        }

        /// <summary>
        /// Returns the command that, when invoked, attempts
        /// to display help.
        /// </summary>
        public ICommand ViewHelpCommand
        {
            get
            {
                if (_viewHelpCommand == null)
                    _viewHelpCommand = new RelayCommand(param => this.ViewHelp());

                return _viewHelpCommand;
            }
        }

        #endregion // Commands

        #region Public Methods

        public virtual void ViewHelp()
        {
            Help.ShowHelp(null, "TaskConqueror.chm");
        }

        #endregion

        #region RequestClose [event]

        /// <summary>
        /// Raised when this workspace should be removed from the UI.
        /// </summary>
        public virtual event EventHandler RequestClose;

        protected void OnRequestClose()
        {
            EventHandler handler = this.RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion // RequestClose [event]

        protected virtual void ShowWorkspaceAsDialog(Window view, WorkspaceViewModel viewModel)
        {
            // When the ViewModel asks to be closed, 
            // close the window.
            EventHandler handler = null;
            handler = delegate
            {
                try
                {
                    if (viewModel.ShouldClose())
                    {
                            viewModel.RequestClose -= handler;
                            view.Close();
                    }
                    else
                    {
                        if (viewModel.CancelArgs != null)
                            viewModel.CancelArgs.Cancel = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            };
            viewModel.RequestClose += handler;

            view.DataContext = viewModel;

            view.ShowDialog();
        }

        protected virtual bool ShouldClose()
        {
            return true;
        }

    }
}