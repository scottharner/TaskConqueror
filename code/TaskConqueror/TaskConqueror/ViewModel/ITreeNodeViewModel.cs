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
    /// A tree-friendly wrapper for a Goal object.
    /// </summary>
    public interface ITreeNodeViewModel
    {
        #region Goal Properties

        int NodeId { get; }

        string Title { get; }

        ITreeNodeContainerViewModel Parent { get; }

        #endregion // Properties

        #region Presentation Properties

        bool IsSelected { get; set; }

        #endregion // Presentation Properties

    }
}