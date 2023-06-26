using KspWalkAbout.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace LLAtoGC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WorldCoordinates _flagPole;

        public MainWindow()
        {
            InitializeComponent();
            _flagPole = new WorldCoordinates(-0.094169025159008, -74.6535078892799, 64.3840068761492, 600000);
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var latitude = Regex.Match(textBox.Text, @"(?<=Latitude\s=\s)[-\.\d]+").Value;
            var longitude = Regex.Match(textBox.Text, @"(?<=Longitude\s=\s)[-\.\d]+").Value;
            var altitude = Regex.Match(textBox.Text, @"(?<=Altitude\s=\s)[-\.\d]+").Value;

            if ((latitude != string.Empty) && (longitude != string.Empty) && (altitude != string.Empty))
            {
                var coordinates =
                    new WorldCoordinates(
                        double.Parse(latitude),
                        double.Parse(longitude),
                        double.Parse(altitude),
                        600000);
                var gc = new GreatCircle(_flagPole, coordinates);
                var output = $"        ForwardAzimuth = {gc.ForwardAzimuth}\n        Distance = {gc.DistanceAtOrigAlt} \n        DeltaAltitude = {gc.DeltaASL}";
                Clipboard.SetText(output);
                outputBlock1.Text = $"copied to clipboard:\n{output}";
            }
            else
            {
                outputBlock1.Text = "Could not extract Latitude, Longitude, and Altitude";
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            textBox.Text = Clipboard.GetText();
        }
    }
}
