
using NAudio.CoreAudioApi;
using System;
using System.Windows;
using System.Windows.Controls;

namespace fermiac
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class FermiacSettings : Window
    {
        public FermiacSettings()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
                var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
                audioSources.ItemsSource = devices;
                audioSources.SelectionChanged += audioSourcesChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Bad thing happened");
            }
        }

        private void audioSourcesChanged(object sender, SelectionChangedEventArgs e)
        {
            var text = e.AddedItems[0];
        }
    }
}
