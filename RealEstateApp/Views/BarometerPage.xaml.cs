using RealEstateApp.ViewModels;

namespace RealEstateApp.Views;

public partial class BarometerPage : ContentPage
{
	public BarometerPage(BarometerPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;

    }
}