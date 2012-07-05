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
    /// A tree-friendly wrapper for a root tree node task container.
    /// </summary>
    public class RootTreeNodeViewModel : ITreeNodeContainerViewModel
    {
        #region Fields

        bool _isSelected = false;
        ObservableCollection<ITreeNodeViewModel> _childNodes = new ObservableCollection<ITreeNodeViewModel>();

        #endregion // Fields

        #region Constructor

        public RootTreeNodeViewModel()
        {
        }

        #endregion // Constructor

        #region Root Properties

        public int NodeId
        {
            get { return -1; }
        }

        public string Title
        {
            get { return "Root"; }
        }

        public ObservableCollection<ITreeNodeViewModel> ChildNodes
        {
            get { return _childNodes; }
        }

        public ITreeNodeContainerViewModel Parent
        {
            get { return null; }
        }

        public int SortWeight
        {
            get { return SortWeights.Root; }
        }

        #endregion // Properties

        #region Presentation Properties

        /// <summary>
        /// Gets/sets whether this customer is selected in the UI.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; }
        }

        #endregion // Presentation Properties

    }
}