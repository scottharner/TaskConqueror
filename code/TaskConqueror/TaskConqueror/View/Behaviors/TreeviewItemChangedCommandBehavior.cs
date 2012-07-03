using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.ComponentModel;
using System.Windows.Input;
using System;
using System.Windows.Media;

namespace TaskConqueror
{
    public class TreeviewItemChangedCommandBehavior
    {
        #region CommandProperty

        public static DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(TreeviewItemChangedCommandBehavior),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(TreeviewItemChangedCommandBehavior.CommandChanged)));


        public static void SetCommand(DependencyObject target, ICommand value)
        {
            target.SetValue(TreeviewItemChangedCommandBehavior.CommandProperty, value);
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
                    element.Selected += OnTreeViewItemSelected;
                    element.PreviewMouseRightButtonDown += SelectItemOnRightClick;
                }
                else if ((e.NewValue == null) && (e.OldValue != null))
                {
                    element.Selected -= OnTreeViewItemSelected;
                    element.PreviewMouseRightButtonDown -= SelectItemOnRightClick;
                }

            }
        }

        #endregion

        #region InvokeOnRightClick

        public static DependencyProperty InvokeOnRightClickProperty =
            DependencyProperty.RegisterAttached("InvokeOnRightClick", typeof(bool), typeof(TreeviewItemChangedCommandBehavior),
            new FrameworkPropertyMetadata(null));


        public static void SetInvokeOnRightClick(DependencyObject target, bool value)
        {
            target.SetValue(TreeviewItemChangedCommandBehavior.InvokeOnRightClickProperty, value);
        }

        public static bool GetInvokeOnRightClick(DependencyObject target)
        {
            return (bool)target.GetValue(InvokeOnRightClickProperty);
        }

        #endregion

        /// <summary>
        /// Optionaly sets the selected item on right click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void SelectItemOnRightClick(object sender, MouseButtonEventArgs e)
        {
            if ((bool)((TreeViewItem)sender).GetValue(TreeviewItemChangedCommandBehavior.InvokeOnRightClickProperty))
            {
                var obj = e.OriginalSource as DependencyObject;
                while (obj != null)
                {
                    if (obj is TreeViewItem)
                    {
                        ((TreeViewItem)obj).IsSelected = true;
                        e.Handled = true;
                        return;
                    }

                    obj = VisualTreeHelper.GetParent(obj);
                }
            }
        }

        /// <summary>
        /// Executes the command thats attached to the selected item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void OnTreeViewItemSelected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;

            if (item.IsSelected)
            {

                item.BringIntoView();

                ICommand command = (ICommand)item.GetValue(TreeviewItemChangedCommandBehavior.CommandProperty);

                command.Execute(item.DataContext);
            }

            e.Handled = true;

        }

    }
}
