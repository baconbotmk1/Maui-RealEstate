using Microsoft.Maui.Devices.Sensors;
using RealEstateApp.Models;
using RealEstateApp.Services;
using RealEstateApp.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace RealEstateApp.ViewModels;
public class PropertyListPageViewModel : BaseViewModel
{
    public Position MyPosition { get; set; }
    public bool SortOnLocation { get; set; }
    public ObservableCollection<PropertyListItem> PropertiesCollection { get; } = new();

    private readonly IPropertyService service;

    public PropertyListPageViewModel(IPropertyService service)
    {
        Title = "Property List";
        this.service = service;
    }

    bool isRefreshing;
    public bool IsRefreshing
    {
        get => isRefreshing;
        set => SetProperty(ref isRefreshing, value);
    }

    private Command getPropertiesCommand;
    public ICommand GetPropertiesCommand => getPropertiesCommand ??= new Command(async () => await GetPropertiesAsync());

    async Task GetPropertiesAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;

            var location = await new LocationGrabber().GetCurrentLocationAsync();
            MyPosition = new Position { Lat = location.Latitude, Long = location.Longitude };

            Location myLocation = new Location(MyPosition.Lat, MyPosition.Long);

            List<Property> properties = service.GetProperties();

            if (PropertiesCollection.Count != 0)
                PropertiesCollection.Clear();

            List<PropertyListItem> tempList = new List<PropertyListItem>();

            foreach (Property property in properties)
            {
                var propListItem = new PropertyListItem(property);

                var distance = Location.CalculateDistance(new Location((double)property.Latitude, (double)property.Longitude), myLocation, DistanceUnits.Kilometers);

                propListItem.Distance = distance;

                if (SortOnLocation)
                    tempList.Add(propListItem);
                else
                    PropertiesCollection.Add(propListItem);

            }

            if (this.SortOnLocation)
            {
                tempList = tempList.OrderBy(p => p.Distance).ToList();
                foreach (var propertyListItem in tempList)
                {
                    PropertiesCollection.Add(propertyListItem);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to get monkeys: {ex.Message}");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }


    private Command sortAsyncCommand;
    public ICommand SortAsyncCommand => sortAsyncCommand ??= new Command(async () => await SortAsync());

    async Task SortAsync()
    {
        this.SortOnLocation = !SortOnLocation;
        await GetPropertiesAsync();
    }



    private Command goToDetailsCommand;
    public ICommand GoToDetailsCommand => goToDetailsCommand ??= new Command<PropertyListItem>(async (propertyListItem) => await GoToDetails(propertyListItem));
    async Task GoToDetails(PropertyListItem propertyListItem)
    {
        if (propertyListItem == null)
            return;

        await Shell.Current.GoToAsync(nameof(PropertyDetailPage), true, new Dictionary<string, object>
        {
            {"MyPropertyListItem", propertyListItem }
        });
    }

    private Command goToAddPropertyCommand;
    public ICommand GoToAddPropertyCommand => goToAddPropertyCommand ??= new Command(async () => await GotoAddProperty());
    async Task GotoAddProperty()
    {
        await Shell.Current.GoToAsync($"{nameof(AddEditPropertyPage)}?mode=newproperty", true, new Dictionary<string, object>
        {
            {"MyProperty", new Property() }
        });
    }


}
