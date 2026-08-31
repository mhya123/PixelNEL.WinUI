using System;
using System.Text.Json;
using System.Threading.Tasks;
using OpenNEL.LocalApi;
using OpenNEL.UI.Bridge;

namespace OpenNEL.WinUI.Services;

public static class BridgeService
{
	public static async Task<JsonElement> InvokeAsync(string action, object? data = null)
	{
		JsonElement? data2 = null;
		if (data != null)
		{
			using JsonDocument jsonDocument = JsonDocument.Parse(JsonSerializer.Serialize(data));
			data2 = jsonDocument.RootElement.Clone();
		}
		BridgeResponse bridgeResponse = await BridgeDispatcher.InvokeAsync(new BridgeRequest
		{
			Action = action,
			RequestId = Guid.NewGuid().ToString("N"),
			Data = data2
		});
		if (!bridgeResponse.Success)
		{
			string text = "请求失败";
			if (bridgeResponse.Data.HasValue && bridgeResponse.Data.Value.TryGetProperty("message", out var value) && value.ValueKind == JsonValueKind.String)
			{
				text = value.GetString() ?? text;
			}
			throw new InvalidOperationException(text);
		}
		if (bridgeResponse.Data.HasValue && bridgeResponse.Data.Value.ValueKind != JsonValueKind.Undefined)
		{
			return bridgeResponse.Data.Value.Clone();
		}
		return JsonDocument.Parse("{}").RootElement.Clone();
	}

	public static async Task<JsonElement> InvokeWithDataAsync(string action, JsonElement? data)
	{
		BridgeResponse bridgeResponse = await BridgeDispatcher.InvokeAsync(new BridgeRequest
		{
			Action = action,
			RequestId = Guid.NewGuid().ToString("N"),
			Data = data
		});
		if (!bridgeResponse.Success)
		{
			string text = "请求失败";
			if (bridgeResponse.Data.HasValue && bridgeResponse.Data.Value.TryGetProperty("message", out var value) && value.ValueKind == JsonValueKind.String)
			{
				text = value.GetString() ?? text;
			}
			throw new InvalidOperationException(text);
		}
		if (bridgeResponse.Data.HasValue)
		{
			return bridgeResponse.Data.Value.Clone();
		}
		return JsonDocument.Parse("{}").RootElement.Clone();
	}
}
