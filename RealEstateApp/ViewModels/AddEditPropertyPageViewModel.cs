using RealEstateApp.Models;
using RealEstateApp.Services;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows.Input;

namespace RealEstateApp.ViewModels;

[QueryProperty(nameof(Mode), "mode")]
[QueryProperty(nameof(Property), "MyProperty")]
public class AddEditPropertyPageViewModel : BaseViewModel
{
    readonly IPropertyService service;

    public AddEditPropertyPageViewModel(IPropertyService service)
    {
        this.service = service;
        Agents = new ObservableCollection<Agent>(service.GetAgents());
        Battery.Default.BatteryInfoChanged += BatChanged;
    }

    private void BatChanged(object sender, BatteryInfoChangedEventArgs e)
    {
        CheckBat();
    }

    public string Mode { get; set; }
    public bool HasNetworkAccess { get; set; }

    #region PROPERTIES
    public ObservableCollection<Agent> Agents { get; }

    private Property _property;
    public Property Property
    {
        get => _property;
        set
        {
            SetProperty(ref _property, value);
            Title = Mode == "newproperty" ? "Add Property" : "Edit Property";

            if (_property.AgentId != null)
            {
                SelectedAgent = Agents.FirstOrDefault(x => x.Id == _property?.AgentId);
            }
        }
    }

    private Agent _selectedAgent;
    public Agent SelectedAgent
    {
        get => _selectedAgent;
        set
        {
            if (Property != null)
            {
                _selectedAgent = value;
                Property.AgentId = _selectedAgent?.Id;
            }
        }
    }

    string statusMessage;
    public string StatusMessage
    {
        get { return statusMessage; }
        set { SetProperty(ref statusMessage, value); }
    }

    Color statusColor;
    public Color StatusColor
    {
        get { return statusColor; }
        set { SetProperty(ref statusColor, value); }
    }


    string batMsg;
    public string BatMsg
    {
        get { return batMsg; }
        set { SetProperty(ref batMsg, value); }
    }

    Color batColor;
    public Color BatColor
    {
        get { return batColor; }
        set { SetProperty(ref batColor, value); }
    }
    #endregion

    private void CheckBat()
    {
        if (Battery.EnergySaverStatus == EnergySaverStatus.On)
        {
            BatColor = Color.FromArgb("00ff00");
        }
        else if (Battery.State == BatteryState.Charging)
        {
            BatColor = Color.FromArgb("ffff00");
        }
        else if (Battery.ChargeLevel <= 0.2) // Rød baggrund
        {
            BatColor = Color.FromArgb("ff0000");
        }
        else
        {
            BatColor = Color.FromArgb("daffda");
        }
        BatMsg = $"{(int)(Battery.ChargeLevel * 100)}%";
    }

    //Opgave 3.1
    private Command getCurrentLocation;
    public ICommand GetCurrentLocationCommand => getCurrentLocation ??= new Command(async () => await GetCurrentLocationAsync());

    private async Task GetCurrentLocationAsync()
    {
        await CheckNetworkAccess();
        if (HasNetworkAccess)
        {
            GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));

            var location = await Geolocation.Default.GetLocationAsync(request);
            Property.Longitude = location.Longitude;
            Property.Latitude = location.Latitude;

            var marker = await new LocationTool().GetGeocodeReverseData(location.Latitude, location.Longitude);
            Property.Address = $"{marker.Thoroughfare} {marker.SubThoroughfare}, {marker.PostalCode} {marker.Locality}, {marker.CountryName}";
        }
    }
    //Opgave 3.1

    //Opgave 3.3 part 2
    private Command getCoordsCommand;
    public ICommand GetCoordsCommand => getCoordsCommand ??= new Command(async () => await GetCoordsFromAddress());
    private async Task GetCoordsFromAddress()
    {
        await CheckNetworkAccess();
        if (HasNetworkAccess)
        {
            var address = Property.Address;
            if (address != null)
            {
                IEnumerable<Location> locations = await Geocoding.Default.GetLocationsAsync(address);

                var Coords = locations.FirstOrDefault();
                Property.Longitude = Coords.Longitude;
                Property.Latitude = Coords.Latitude;
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error happened", "Address field empty", "Ok");
            }
        }
    }
    //Opgave 3.3 part 2


    private Command savePropertyCommand;
    public ICommand SavePropertyCommand => savePropertyCommand ??= new Command(async () => await SaveProperty());
    private async Task SaveProperty()
    {
        if (IsValid() == false)
        {
            StatusMessage = "Please fill in all required fields";
            StatusColor = Colors.Red;
        }
        else
        {
            service.SaveProperty(Property);
            await Shell.Current.GoToAsync("///propertylist");
        }
    }

    public bool IsValid()
    {
        if (string.IsNullOrEmpty(Property.Address)
            || Property.Beds == null
            || Property.Price == null
            || Property.AgentId == null)
        {
            StartVibrate(5.0);
            return false;

        }
        return true;
    }
    private Command cancelSaveCommand;
    public ICommand CancelSaveCommand => cancelSaveCommand ??= new Command(async () => { StopVibrate(); await Shell.Current.GoToAsync(".."); });


    //Opgave 3.5

    public void StartVibrate(double secondsToVibrate)
    {
        Vibration.Default.Vibrate(secondsToVibrate * 1000);
    }
    public void StopVibrate()
    {
        Vibration.Default.Cancel();
    }
    //Opgave 3.5

    public async Task CheckNetworkAccess()
    {
        NetworkAccess accessType = Connectivity.Current.NetworkAccess;

        if (accessType == NetworkAccess.Internet)
        {
            HasNetworkAccess = true;
            OnPropertyChanged(nameof(HasNetworkAccess));

            return;
        }
        HasNetworkAccess = false;
        await Application.Current.MainPage.DisplayAlert("Error", "Connection to internet is not available!", "OK");

        OnPropertyChanged(nameof(HasNetworkAccess));
    }

    private bool flashToggled;
    public bool FlashToggled { get => flashToggled; set { SetProperty(ref flashToggled, value); } }


    private Command flashToggleCommand;
    public ICommand FlashToggleCommand => flashToggleCommand ??= new Command(async () => await ToggleFlash());
    async Task ToggleFlash()
    {
        if (!FlashToggled)
            await Flashlight.Default.TurnOnAsync();
        else
            await Flashlight.Default.TurnOffAsync();
        FlashToggled = !FlashToggled;
    }

}

