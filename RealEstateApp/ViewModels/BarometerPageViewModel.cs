using RealEstateApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RealEstateApp.ViewModels
{
    public class BarometerPageViewModel : BaseViewModel
    {
        public BarometerPageViewModel()
        {
            Barometer.ReadingChanged += Barometer_ReadingChanged;
            Barometer.Start(SensorSpeed.UI);
        }

        private void Barometer_ReadingChanged(object sender, BarometerChangedEventArgs e)
        {
            CurrentPressure = e.Reading.PressureInHectopascals;
            CurrentAlt = 44307.694 * (1 - Math.Pow(CurrentPressure / 1013.25, 0.190284));
        }

        private double _currentAlt;
        public double CurrentAlt { get => _currentAlt; set { SetProperty(ref _currentAlt, value); } }
        private double _currentPressure;
        public double CurrentPressure { get => _currentPressure; set { SetProperty(ref _currentPressure, value); } }

        private string _label;
        public string Label { get => _label; set {SetProperty(ref _label, value); } }


        public ObservableCollection<BarometerMeasurement> BarometerCollection { get; } = new();
        private Command saveBarometerCommand;
        public ICommand SaveBarometerCommand => saveBarometerCommand ??= new Command(() => SaveCurrent());
        private void SaveCurrent()
        {
            var newBar = new BarometerMeasurement() { Altitude = CurrentAlt, Pressure = CurrentPressure, Label =  this.Label };
            if (BarometerCollection.Count > 0)
            {
                var prevBar = BarometerCollection.Last();
                newBar.HeightChange = CurrentAlt - prevBar.Altitude;
            }
            BarometerCollection.Add(newBar);
        }

    }
}
