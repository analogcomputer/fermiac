using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace fermiac
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class FermiacCam : Window
    {
        private BotManager _bot;

        public FermiacCam(BotManager bm)
        {
            InitializeComponent();
            _bot = bm;
            _bot.FermiacTalkStart += _bot_FermiacTalkStart;
            _bot.FermiacTalkEnd += _bot_FermiacTalkEnd;
            CamImage.Visibility = Visibility.Hidden;
        }

        private void _bot_FermiacTalkEnd()
        {
            Dispatcher.BeginInvoke((Action) (() => {
                CamImage.Visibility = Visibility.Hidden;
            }));
        }

        private void _bot_FermiacTalkStart()
        {
            Dispatcher.BeginInvoke((Action)(() => {
                CamImage.Visibility = Visibility.Visible;
                this.UpdateLayout();
            }));
        }
    }
}
