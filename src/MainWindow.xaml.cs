using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace fermiac
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BotManager Bot;
        private readonly IConfiguration _config;
        private ObservableCollection<LogMsg> messages { get; set; }

        public MainWindow(IConfiguration config)
        {
            _config = config;
            messages = new ObservableCollection<LogMsg>();
            InitializeComponent();
            MessageListView.ItemsSource = messages;
            messages.CollectionChanged += Messages_CollectionChanged;
        }

        private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            FrameworkElement border = (FrameworkElement)VisualTreeHelper.GetChild(MessageListView, 0);
            ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
            scrollViewer.ScrollToBottom();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Bot = new BotManager();
            Bot.logger = ((msg) => {
                Dispatcher.BeginInvoke((Action) (() =>
                {
                    messages.Add(msg);
                }), null);
            });
            Bot.connect(
                _config["twitch:username"],
                _config["twitch:accesstoken"],
                _config["twitch:channel"],
                _config["azure:speech:key"],
                _config["azure:speech:region"]);
            (new FermiacCam(Bot)).Show();
            (new FermiacSettings()).Show();
        }
    }
}
