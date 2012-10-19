using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

// Custom messagebox courtesy of http://blogsprajeesh.blogspot.com

namespace TaskConqueror
{
    public class MessageBoxViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Title
        {
            get { return ___Title; }
            set
            {
                if (___Title != value)
                {
                    ___Title = value;
                    NotifyPropertyChange("Title");
                }
            }
        }

        public string Message
        {
            get { return ___Message; }
            set
            {
                if (___Message != value)
                {
                    ___Message = value;
                    NotifyPropertyChange("Message");
                }
            }
        }

        public string InnerMessageDetails
        {
            get { return ___InnerMessageDetails; }
            set
            {
                if (___InnerMessageDetails != value)
                {
                    ___InnerMessageDetails = value;
                    NotifyPropertyChange("InnerMessageDetails");
                }
            }
        }

        public ImageSource MessageImageSource
        {
            get { return ___MessageImageSource; }
            set
            {
                ___MessageImageSource = value;
                NotifyPropertyChange("MessageImageSource");
            }
        }

        public Visibility YesNoVisibility
        {
            get { return ___YesNoVisibility; }
            set
            {
                if (___YesNoVisibility != value)
                {
                    ___YesNoVisibility = value;
                    NotifyPropertyChange("YesNoVisibility");
                }
            }
        }

        public Visibility CancelVisibility
        {
            get { return ___CancelVisibility; }
            set
            {
                if (___CancelVisibility != value)
                {
                    ___CancelVisibility = value;
                    NotifyPropertyChange("CancelVisibility");
                }
            }
        }

        public Visibility OkVisibility
        {
            get { return ___OKVisibility; }
            set
            {
                if (___OKVisibility != value)
                {
                    ___OKVisibility = value;
                    NotifyPropertyChange("OkVisibility");
                }
            }
        }

        public Visibility CloseVisibility
        {
            get { return ___CloseVisibility; }
            set
            {
                if (___CloseVisibility != value)
                {
                    ___CloseVisibility = value;
                    NotifyPropertyChange("CloseVisibility");
                }
            }
        }

        public Visibility ShowDetails
        {
            get { return ___ShowDetails; }
            set
            {
                if (___ShowDetails != value)
                {
                    ___ShowDetails = value;
                    NotifyPropertyChange("ShowDetails");
                }
            }
        }

        public ICommand YesCommand
        {
            get
            {
                if (___YesCommand == null)
                    ___YesCommand = new DelegateCommand(() => 
                        {
                            ___View.Result = WPFMessageBoxResult.Yes;
                            ___View.Close();
                        });
                return ___YesCommand;
            }
        }

        public ICommand NoCommand
        {
            get
            {
                if (___NoCommand == null)
                    ___NoCommand = new DelegateCommand(() => 
                        {
                            ___View.Result = WPFMessageBoxResult.No;
                            ___View.Close();
                        });
                return ___NoCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (___CancelCommand == null)
                    ___CancelCommand = new DelegateCommand(() =>
                        {
                            ___View.Result = WPFMessageBoxResult.Cancel;
                            ___View.Close();
                        });
                return ___CancelCommand;
            }
        }


        public ICommand CloseCommand
        {
            get
            {
                if (___CloseCommand == null)
                    ___CloseCommand = new DelegateCommand(() => 
                        {
                            ___View.Result = WPFMessageBoxResult.Close;
                            ___View.Close();
                        });
                return ___CloseCommand;
            }
        }

        public ICommand OkCommand
        {
            get
            {
                if (___OKCommand == null)
                    ___OKCommand = new DelegateCommand(() => 
                        {
                            ___View.Result = WPFMessageBoxResult.Ok;
                            ___View.Close();
                        });
                return ___OKCommand;
            }
        }

        public MessageBoxViewModel(WPFMessageBox view, 
            string title, string message, string innerMessage,
            WPFMessageBoxButtons buttonOption, WPFMessageBoxImage image)
        {
            Title = title;
            Message = message;
            InnerMessageDetails = innerMessage;
            SetButtonVisibility(buttonOption);
            SetImageSource(image);
            ___View = view;
        }

        void NotifyPropertyChange(string property)
        {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        void SetButtonVisibility(WPFMessageBoxButtons buttonOption)
        {
            switch (buttonOption)
            {
                case WPFMessageBoxButtons.YesNo:
                    OkVisibility = CancelVisibility = CloseVisibility = Visibility.Collapsed;
                    break;
                case WPFMessageBoxButtons.YesNoCancel:
                    OkVisibility = CloseVisibility = Visibility.Collapsed;
                    break;
                case WPFMessageBoxButtons.OK:
                    YesNoVisibility = CancelVisibility = CloseVisibility = Visibility.Collapsed;
                    break;
                case WPFMessageBoxButtons.OKClose:
                    YesNoVisibility = CancelVisibility = Visibility.Collapsed;
                    break;
                default:
                    OkVisibility = CancelVisibility = YesNoVisibility = Visibility.Collapsed;
                    break;
            }
            if (string.IsNullOrEmpty(InnerMessageDetails)) ShowDetails = Visibility.Collapsed;
            else ShowDetails = Visibility.Visible;
        }

        void SetImageSource(WPFMessageBoxImage image)
        {
            string __Source = "pack://application:,,,/TaskConqueror;Component/Assets/Images/MessageBox/default.png";
            switch (image)
            {
                case WPFMessageBoxImage.Alert:
                    __Source = "pack://application:,,,/TaskConqueror;Component/Assets/Images/MessageBox/alert.png";
                    break;
                case WPFMessageBoxImage.Error:
                    __Source = "pack://application:,,,/TaskConqueror;Component/Assets/Images/MessageBox/error.png";
                    break;
                case WPFMessageBoxImage.Information:
                    __Source = "pack://application:,,,/TaskConqueror;Component/Assets/Images/MessageBox/info.png";
                    break;
                case WPFMessageBoxImage.OK:
                    __Source = "pack://application:,,,/TaskConqueror;Component/Assets/Images/MessageBox/ok.png";
                    break;
                case WPFMessageBoxImage.Question:
                    __Source = "pack://application:,,,/TaskConqueror;Component/Assets/Images/MessageBox/help.png";
                    break;
                default:
                    __Source = "pack://application:,,,/TaskConqueror;Component/Assets/Images/MessageBox/default.png";
                    break;

            }
            Uri __ImageUri = new Uri(__Source, UriKind.RelativeOrAbsolute);
            MessageImageSource = new BitmapImage(__ImageUri);
        }

        string ___Title;
        string ___Message;
        string ___InnerMessageDetails;

        Visibility ___YesNoVisibility;
        Visibility ___CancelVisibility;
        Visibility ___OKVisibility;
        Visibility ___CloseVisibility;
        Visibility ___ShowDetails;

        ICommand ___YesCommand;
        ICommand ___NoCommand;
        ICommand ___CancelCommand;
        ICommand ___CloseCommand;
        ICommand ___OKCommand;

        WPFMessageBox ___View;
        ImageSource ___MessageImageSource;
    }
}
