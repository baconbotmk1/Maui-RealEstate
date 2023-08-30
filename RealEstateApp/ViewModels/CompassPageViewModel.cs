using RealEstateApp.Models;
using RealEstateApp.Services;
using RealEstateApp.Views;
using System.Windows.Input;

namespace RealEstateApp.ViewModels
{

    [QueryProperty(nameof(Property), "property")]
    public class CompassPageViewModel : BaseViewModel
    {
        private readonly IPropertyService service;
        public CompassPageViewModel(IPropertyService service)
        {
            this.service = service;
            Compass.ReadingChanged += Compass_ReadingChanged;
        }

        private Command toggleCompass;
        public ICommand ToggleCompass => toggleCompass ??= new Command(() => { if (CompassState) Compass.Stop(); else Compass.Start(SensorSpeed.UI); CompassState = !CompassState; });
        private bool _compassState;
        public bool CompassState { get => _compassState; set { SetProperty(ref _compassState, value); } }


        private void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            CurrentHeading = e.Reading.HeadingMagneticNorth;
            RotationAngle = e.Reading.HeadingMagneticNorth;
            Property.Aspect = CalcAspect();
        }


        double _currentHeading;
        public double CurrentHeading { get => _currentHeading; set { SetProperty(ref _currentHeading, value); } }


        double _rotationAngle;
        public double RotationAngle { get => (360 - _rotationAngle); set { SetProperty(ref _rotationAngle, value); } }


        private string CalcAspect()
        {
            if (_rotationAngle >= 0 && _rotationAngle < 45 || _rotationAngle >= 315 && _rotationAngle <= 360)
            {
                return "North";
            }
            else if (_rotationAngle >= 45 && _rotationAngle < 135)
            {
                return "East";
            }
            else if (_rotationAngle >= 135 && _rotationAngle < 225)
            {
                return "South";
            }
            else if (_rotationAngle >= 225 && _rotationAngle < 315)
            {
                return "West";
            }
            else
            {
                return "Error";
            }

        }

        Property property;
        public Property Property { get => property; set { SetProperty(ref property, value); } }

        PropertyListItem propertyListItem;
        public PropertyListItem PropertyListItem
        {
            set
            {
                SetProperty(ref propertyListItem, value);

                Property = propertyListItem.Property;
            }
        }

    }
}
