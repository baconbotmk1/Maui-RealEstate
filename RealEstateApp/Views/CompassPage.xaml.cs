using RealEstateApp.ViewModels;

namespace RealEstateApp.Views;

public partial class CompassPage : ContentPage
{
	public CompassPage(CompassPageViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}