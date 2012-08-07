using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.ComponentModel;
using System.Windows.Input;
using System;
using System.Windows.Media;

namespace TaskConqueror
{
    public class TreeNodeChangedBehavior
    {
        #region CommandProperty

        public static DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(TreeNodeChangedBehavior),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(TreeNodeChangedBehavior.CommandChanged)));


        public static void SetCommand(DependencyObject target, ICommand value)
        {
            target.SetValue(TreeNodeChangedBehavior.CommandProperty, value);
        }

        public static ICommand GetCommand(DependencyObject target)
        {
            return (ICommand)target.GetValue(CommandProperty);
        }

        private static void CommandChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TreeViewItem element = target as TreeViewItem;

            if (element != null)
            {
                if ((e.NewValue != null) && (e.OldValue == null))
                {
                    element.Selected += OnTreeNodeSelected;
                }
                else if ((e.NewValue == null) && (e.OldValue != null))
                {
                    element.Selected -= OnTreeNodeSelected;
                }

            }
        }

        #endregion

        /// <summary>
        /// Executes the command thats attached to the selected item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void OnTreeNodeSelected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;

            if (item.IsSelected)
            {

                item.BringIntoView();

                ICommand command = (ICommand)item.GetValue(TreeNodeChangedBehavior.CommandProperty);

                command.Execute(item.DataContext);
            }

            e.Handled = true;

        }

    }
}
