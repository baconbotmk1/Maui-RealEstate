using RealEstateApp.Models;
using RealEstateApp.Services;
using RealEstateApp.Views;
using System.Windows.Input;

namespace RealEstateApp.ViewModels;

[QueryProperty(nameof(PropertyListItem), "MyPropertyListItem")]
public class PropertyDetailPageViewModel : BaseViewModel
{
    private readonly IPropertyService service;
    public PropertyDetailPageViewModel(IPropertyService service)
    {
        this.service = service;
    }

    Property property;
    public Property Property { get => property; set { SetProperty(ref property, value); } }


    Agent agent;
    public Agent Agent { get => agent; set { SetProperty(ref agent, value); } }


    PropertyListItem propertyListItem;
    public PropertyListItem PropertyListItem
    {
        set
        {
            SetProperty(ref propertyListItem, value);

            Property = propertyListItem.Property;
            Agent = service.GetAgents().FirstOrDefault(x => x.Id == Property.AgentId);
        }
    }

    private Command editPropertyCommand;
    public ICommand EditPropertyCommand => editPropertyCommand ??= new Command(async () => await GotoEditProperty());
    async Task GotoEditProperty()
    {
        await Shell.Current.GoToAsync($"{nameof(AddEditPropertyPage)}?mode=MyProperty", true, new Dictionary<string, object>
        {
            {"MyProperty", property }
        });
    }

    private bool viewPlay = true;
    public bool ViewPlay { get => viewPlay; set { SetProperty(ref viewPlay, value); } }
    private bool viewStop = false;
    public bool ViewStop { get => viewStop; set { SetProperty(ref viewStop, value); } }

    CancellationTokenSource cts;

    private Command playTTSComand;
    public ICommand PlayTTSComand => playTTSComand ??= new Command(async () => await PlayTTS());
    public async Task PlayTTS()
    {
        ViewPlay = false;
        ViewStop = true;
        cts = new CancellationTokenSource();
        await TextToSpeech.Default.SpeakAsync(Property.Description, cancelToken: cts.Token);
        ViewStop = !ViewStop;
        ViewPlay = !ViewPlay;
    }

    private Command stopTTSComand;
    public ICommand StopTTSComand => stopTTSComand ??= new Command(() => StopTTS());
    public void StopTTS()
    {
        if (cts?.IsCancellationRequested ?? true)
            return;

        cts.Cancel();
        ViewStop = false;
        ViewPlay = true;
    }

}
