using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RealEstateApp.Models;
public class PropertyListItem : INotifyPropertyChanged
{
    public PropertyListItem(Property property)
    {
        Property = property;
    }

    private double _distance;
    public double Distance { get => _distance; set { _distance = value; OnPropertyChanged(); } }

    private Property _property;

    public Property Property
    {
        get => _property;
        set
        {
            _property = value;
            OnPropertyChanged();
        }
    }


    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
