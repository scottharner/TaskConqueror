using System.Windows.Controls;
using System.Windows;

namespace TaskConqueror
{
    /// <summary>
    /// Interaction logic for CompletableTextBlock.xaml
    /// </summary>
    public partial class CompletableTextBlock : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
          "Text",
          typeof(string),
          typeof(CompletableTextBlock)
        );

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public CompletableTextBlock()
        {
            InitializeComponent();
        }
    }
}
