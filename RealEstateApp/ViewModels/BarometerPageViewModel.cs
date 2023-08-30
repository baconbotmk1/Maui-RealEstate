using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public double CurrentAlt { get => _currentAlt; set {SetProperty(ref _currentAlt, value); } }
        private double _currentPressure;
        public double CurrentPressure { get => _currentPressure; set {SetProperty(ref _currentPressure, value); } }

    }
}
