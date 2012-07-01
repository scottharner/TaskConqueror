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
    /// A tree-friendly wrapper for a tree node container object.
    /// </summary>
    public interface ITreeNodeContainerViewModel
    {
        #region Properties

        ObservableCollection<ITreeNodeViewModel> ChildNodes { get; }

        #endregion // Properties
    }
}