using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using OpenNEL.LocalApi;
using OpenNEL.UI.Bridge;
using Serilog;
using Serilog.Events;
using WinRT.Interop;

namespace OpenNEL.WinUI;

public partial class App : Application
{
	private WebApplication? _webApp;

	private Window? m_window;

	public App()
	{
		InitializeComponent();
		base.UnhandledException += delegate(object s, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
		{
			try
			{
				Log.Fatal(e.Exception, "[WinUI] Unhandled exception");
				Log.CloseAndFlush();
			}
			catch
			{
			}
			e.Handled = true;
		};
		AppDomain.CurrentDomain.UnhandledException += delegate(object s, System.UnhandledExceptionEventArgs e)
		{
			try
			{
				Log.Fatal(e.ExceptionObject as Exception, "[WinUI] AppDomain unhandled");
				Log.CloseAndFlush();
			}
			catch
			{
			}
		};
	}

	protected override async void OnLaunched(LaunchActivatedEventArgs args)
	{
		try
		{
			string text = Path.Combine(AppContext.BaseDirectory, "data", "logs");
			Directory.CreateDirectory(text);
			Log.Logger = new LoggerConfiguration().MinimumLevel.Information().MinimumLevel.Override("Microsoft", LogEventLevel.Warning).WriteTo.Console().WriteTo.File(Path.Combine(text, "OpenNEL.WinUI-.log"), LogEventLevel.Verbose, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", null, retainedFileCountLimit: 7, flushToDiskInterval: TimeSpan.FromSeconds(1L), fileSizeLimitBytes: 1073741824L, levelSwitch: null, buffered: false, shared: true, rollingInterval: RollingInterval.Day).CreateLogger();
			Log.Information("[WinUI] OnLaunched start");
			Backend.Initialize();
			Log.Information("[WinUI] Backend initialized");
			if (!IsPortInUse(39719))
			{
				_ = Task.Run(() => StartApiHostAsync());
			}
			else
			{
				Log.Information("[WinUI] Port 39719 already in use, skipping in-process API host (using external LocalApi)");
			}
			Log.Information("[WinUI] Creating MainWindow");
			m_window = new MainWindow();
			Log.Information("[WinUI] Activating MainWindow");
			m_window.Activate();
			Log.Information("[WinUI] MainWindow activated, handle={Handle}", WindowNative.GetWindowHandle(m_window));
		}
		catch (Exception ex)
		{
			try
			{
				Log.Fatal(ex, "[WinUI] OnLaunched failed");
				Log.CloseAndFlush();
			}
			catch
			{
			}
			try
			{
				Process.Start("cmd", "/c echo " + ex.Message + " > \"%TEMP%\\OpenNEL.WinUI.crash.log\"");
			}
			catch
			{
			}
			throw;
		}
	}

	private static bool IsPortInUse(int port)
	{
		try
		{
			using TcpClient tcpClient = new TcpClient();
			IAsyncResult asyncResult = tcpClient.BeginConnect(IPAddress.Loopback, port, null, null);
			if (!asyncResult.AsyncWaitHandle.WaitOne(150))
			{
				return false;
			}
			try
			{
				tcpClient.EndConnect(asyncResult);
				return true;
			}
			catch
			{
				return false;
			}
		}
		catch
		{
			return false;
		}
	}

	private async Task StartApiHostAsync()
	{
		try
		{
			WebApplicationBuilder webApplicationBuilder = WebApplication.CreateBuilder();
			webApplicationBuilder.WebHost.UseUrls("http://127.0.0.1:39719");
			webApplicationBuilder.Services.AddCors(delegate(CorsOptions o)
			{
				o.AddDefaultPolicy(delegate(CorsPolicyBuilder p)
				{
					p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
				});
			});
			WebApplication webApplication = (_webApp = webApplicationBuilder.Build());
			webApplication.UseCors();
			webApplication.MapGet("/api/health", (Func<Task<IResult>>)async delegate
			{
				await Backend.WaitForInitAsync();
				return Results.Json(new
				{
					success = true,
					name = "OpenNEL Local API (WinUI-hosted)",
					version = "winui-1.0"
				});
			});
			webApplication.MapPost("/api/bridge", (Func<JsonElement, Task<IResult>>)async delegate(JsonElement body)
			{
				string action = (body.TryGetProperty("action", out var value) ? (value.GetString() ?? "") : "");
				string requestId = (body.TryGetProperty("requestId", out var value2) ? (value2.GetString() ?? Guid.NewGuid().ToString("N")) : Guid.NewGuid().ToString("N"));
				JsonElement? data = (body.TryGetProperty("data", out var value3) ? new JsonElement?(value3.Clone()) : ((JsonElement?)null));
				return Results.Json(await BridgeDispatcher.InvokeAsync(new BridgeRequest
				{
					Action = action,
					RequestId = requestId,
					Data = data
				}));
			});
			webApplication.MapGet("/api/settings", (Func<Task<IResult>>)(async () => Results.Json(await BridgeDispatcher.InvokeAsync(new BridgeRequest
			{
				Action = "settings:get",
				RequestId = Guid.NewGuid().ToString("N")
			}))));
			webApplication.MapPost("/api/settings", (Func<JsonElement, Task<IResult>>)(async (JsonElement body) => Results.Json(await BridgeDispatcher.InvokeAsync(new BridgeRequest
			{
				Action = "settings:set",
				RequestId = Guid.NewGuid().ToString("N"),
				Data = body.Clone()
			}))));
			Log.Information("[WinUI] In-process API host listening on 127.0.0.1:39719");
			await webApplication.RunAsync();
		}
		catch (Exception exception)
		{
			Log.Error(exception, "[WinUI] API host failed");
		}
	}

}
