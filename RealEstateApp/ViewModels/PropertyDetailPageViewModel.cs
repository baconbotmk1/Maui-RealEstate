using RealEstateApp.Models;
using RealEstateApp.Services;
using RealEstateApp.Views;
using System.Text.Json;
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

    private Command phoneClickedComand;
    public ICommand PhoneClickedComand => phoneClickedComand ??= new Command(async () => await HandlePhoneClicked());
    public async Task HandlePhoneClicked()
    {
        string action = await App.Current.MainPage.DisplayActionSheet(Property.Vendor.Phone, "Cancel", null, "Call", "Sms");

        switch (action)
        {
            case "Call":
                CallActionHandler();
                break;
            case "Sms":
                await SmsActionHandler();
                break;
        }
    }
    private async Task SmsActionHandler()
    {
        if (Sms.Default.IsComposeSupported)
        {
            string[] recipients = new[] { Property.Vendor.Phone };
            string text = $"Hello {Property.Vendor.FirstName}, I'm writing regarting {Property.Address}...";

            var message = new SmsMessage(text, recipients);

            await Sms.Default.ComposeAsync(message);
        }
    }
    private void CallActionHandler()
    {
        if (PhoneDialer.Default.IsSupported)
        {
            PhoneDialer.Default.Open(Property.Vendor.Phone);
        }
    }


    private Command emailClickedCommand;
    public ICommand EmailClickedCommand => emailClickedCommand ??= new Command(async () => await HandleEmailClicked());
    public async Task HandleEmailClicked()
    {
        if (Email.Default.IsComposeSupported)
        {

            string subject = $"Hello {Property.Vendor.FullName}!";
            string body = $"Regarding {Property.Address}...";
            string[] recipients = new[] { Property.Vendor.Email };

            var message = new EmailMessage
            {
                Subject = subject,
                Body = body,
                BodyFormat = EmailBodyFormat.PlainText,
                To = new List<string>(recipients)
            };

            var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var attachmentFilePath = Path.Combine(folder, "Prop.txt");
            File.WriteAllText(attachmentFilePath, Property.Address);

            message.Attachments.Add(new EmailAttachment(attachmentFilePath));

            await Email.Default.ComposeAsync(message);
        }
    }

    private Command openMapCommand;
    public ICommand OpenMapCommand => openMapCommand ??= new Command(async () => await HandleOpenMapCommand());
    private async Task HandleOpenMapCommand()
    {
        var location = new Location((double)Property.Latitude, (double)Property.Longitude);

        try
        {
            await Map.Default.OpenAsync(location);
        }
        catch (Exception ex)
        {
            // No map application available to open
        }
    }
    private Command openMapDirectionCommand;
    public ICommand OpenMapDirectionCommand => openMapDirectionCommand ??= new Command(async () => await HandleOpenMapDirectionCommand());
    private async Task HandleOpenMapDirectionCommand()
    {
        var marker = await new LocationTool().GetGeocodeReverseData((double)Property.Latitude, (double)Property.Longitude);

        var options = new MapLaunchOptions
        {
            NavigationMode = NavigationMode.Driving
        };

        try
        {
            await Map.Default.OpenAsync(marker, options);
        }
        catch (Exception ex)
        {
            // No map application available to open
        }
    }
    private Command openLinkCommand;
    public ICommand OpenLinkCommand => openLinkCommand ??= new Command(async () => await HandleOpenLinkCommand());
    private async Task HandleOpenLinkCommand()
    {
        Uri uri = new Uri(Property.NeighbourhoodUrl);
        await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
    }
    private Command shareCommand;
    public ICommand ShareCommand => shareCommand ??= new Command(async () => await HandleShareCommand());
    private async Task HandleShareCommand()
    {
        var pUrl = Property.NeighbourhoodUrl;
        var pSubject = "A property you may be interested in";
        var pText = $"The address is: {Property.Address}, there are {Property.Beds} bedrooms and it's only ${Property.Price}";
        var pTitle = "Share property";

        var newShareReq = new ShareTextRequest() { Uri = pUrl, Subject = pSubject, Text = pText, Title = pTitle };
        await Share.Default.RequestAsync(newShareReq);
    }
    private Command shareFileCommand;
    public ICommand ShareFileCommand => shareFileCommand ??= new Command(async () => await HandleShareFileCommand());
    private async Task HandleShareFileCommand()
    {
        var pTitle = "Share property";

        var envPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var pFilePath = Path.Combine(envPath, "IS-tema.pdf");

        var newShareReq = new ShareFileRequest() { Title = pTitle, File = new ShareFile(pFilePath) };
        await Share.Default.RequestAsync(newShareReq);
    }

    private Command copyToClipCommand;
    public ICommand CopyToClipCommand => copyToClipCommand ??= new Command(async () => await HandleCopyToClipboard());
    private async Task HandleCopyToClipboard()
    {
        string jsonString = JsonSerializer.Serialize(Property);

          

        await Clipboard.Default.SetTextAsync(jsonString);
    }
}
