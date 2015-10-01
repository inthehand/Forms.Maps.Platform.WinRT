# Forms.Maps.Platform.WinRT
Xamarin Forms Maps for Windows Phone 8.1

Full support for Maps and Geocoder on Windows Phone 8.1 Apps.

Available on NuGet - https://www.nuget.org/packages/InTheHand.Forms.Maps.Platform.WinRT/

Once you've added the NuGet package to your Windows Phone 8.1 Xamarin Forms app you need to add just one line of code to set this up in your App.xaml.cs OnLaunched method:-

InTheHand.FormsMaps.Init(<your maps key here>);

You can then use the Geocoder or Map control in your Portable/shared Xamarin Forms project as you would on Android, iOS or Windows Phone Silverlight. For debugging you can pass null for the map key and the map will display a banner showing that no license key is supplied. You can get a Maps key for your app from the Windows Dev Center.
