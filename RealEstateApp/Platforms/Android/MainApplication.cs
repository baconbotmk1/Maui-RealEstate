using Android.App;
using Android.Runtime;
// Opgave 3.1
//[assembly: UsesPermission(Android.Manifest.Permission.AccessCoarseLocation)]
//[assembly: UsesPermission(Android.Manifest.Permission.AccessFineLocation)]
//[assembly: UsesFeature("android.hardware.location", Required = false)]
//[assembly: UsesFeature("android.hardware.location.gps", Required = false)]
//[assembly: UsesFeature("android.hardware.location.network", Required = false)]
//
//Because above api 29 VV
//[assembly: UsesPermission(Manifest.Permission.AccessBackgroundLocation)]
// Opgave 3.1

namespace RealEstateApp;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}
    
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
