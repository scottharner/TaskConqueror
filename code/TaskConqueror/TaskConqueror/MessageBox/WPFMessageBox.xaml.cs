using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Runtime.InteropServices;

namespace TaskConqueror
{
    /// <summary>
    /// Interaction logic for WPFMessageBox.xaml
    /// </summary>
    public partial class WPFMessageBox : Window 
    {
        public WPFMessageBox()
        {
            InitializeComponent();
        }

        public WPFMessageBoxResult Result { 
            get; 
            set; 
        }

        public static WPFMessageBoxResult Show(string message)
        {
            return Show(string.Empty, message, string.Empty, WPFMessageBoxButtons.OK, WPFMessageBoxImage.Default);
        }

        public static WPFMessageBoxResult Show(string title, string message)
        {
            return Show(title, message, string.Empty, WPFMessageBoxButtons.OK, WPFMessageBoxImage.Default);
        }

        public static WPFMessageBoxResult Show(string title, string message, string details)
        {
            return Show(title, message, details, WPFMessageBoxButtons.OK, WPFMessageBoxImage.Default);
        }

        public static WPFMessageBoxResult Show(string title, string message, WPFMessageBoxButtons buttonOption)
        {
            return Show(title, message, string.Empty, buttonOption, WPFMessageBoxImage.Default);
        }

        public static WPFMessageBoxResult Show(string title, string message, string details, WPFMessageBoxButtons buttonOption)
        {
            return Show(title, message, details, buttonOption, WPFMessageBoxImage.Default);
        }

        public static WPFMessageBoxResult Show(string title, string message, WPFMessageBoxImage image)
        {
            return Show(title, message, string.Empty, WPFMessageBoxButtons.OK, image);
        }

        public static WPFMessageBoxResult Show(string title, string message, string details, WPFMessageBoxImage image)
        {
            return Show(title, message, details, WPFMessageBoxButtons.OK, image);
        }

        public static WPFMessageBoxResult Show(string title, string message, WPFMessageBoxButtons buttonOption, WPFMessageBoxImage image)
        {
            return Show(title, message, string.Empty, buttonOption, image);
        }

        public static WPFMessageBoxResult Show(string title, string message, string details, WPFMessageBoxButtons buttonOption, WPFMessageBoxImage image)
        {
            ___MessageBox = new WPFMessageBox();
            MessageBoxViewModel __ViewModel = new MessageBoxViewModel(___MessageBox, title, message, details, buttonOption, image);
            ___MessageBox.DataContext = __ViewModel;
            ___MessageBox.ShowDialog();
            return ___MessageBox.Result;
        }

        [ThreadStatic]
        static WPFMessageBox ___MessageBox;                

        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }

    }
}
