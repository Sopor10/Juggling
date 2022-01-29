using MauiRuntime.Data;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Siteswaps.Generator.DependencyInjection;

namespace MauiRuntime
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.RegisterBlazorMauiWebView()
				.UseMauiApp<App>()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				});

			builder.Services.AddBlazorWebView();
			builder.Services.AddSingleton<WeatherForecastService>();
			builder.Services.InstallGenerator();

			return builder.Build();
		}
	}
}