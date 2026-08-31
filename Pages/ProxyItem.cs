using System;
using System.Text.Json;

namespace OpenNEL.WinUI.Pages;

public sealed class ProxyItem
{
	public string Id { get; set; } = string.Empty;

	public string Mode { get; set; } = string.Empty;

	public string ListenEndpoint { get; set; } = string.Empty;

	public string RemoteEndpoint { get; set; } = string.Empty;

	public string ClientText { get; set; } = string.Empty;

	public string StatusText { get; set; } = string.Empty;

	public string DetailText { get; set; } = string.Empty;

	public string StartedAtText { get; set; } = string.Empty;

	public string CaptureFilePath { get; set; } = string.Empty;

	public static ProxyItem FromJson(JsonElement item)
	{
		string text = GetString(item, "listenAddress");
		int value = GetInt(item, "localPort");
		string value2 = GetString(item, "remoteAddress");
		int value3 = GetInt(item, "remotePort");
		bool flag = GetBool(item, "hasClient");
		string value4 = GetString(item, "clientAddress");
		int? value5 = ((item.TryGetProperty("clientPort", out var value6) && value6.TryGetInt32(out var value7)) ? new int?(value7) : ((int?)null));
		bool flag2 = GetBool(item, "mitmEnabled");
		bool flag3 = GetBool(item, "allowLan");
		string startedAtText = (DateTimeOffset.TryParse(GetString(item, "startedAt"), out var result) ? $"启动于 {result.ToLocalTime():yyyy-MM-dd HH:mm:ss}" : "启动时间未知");
		return new ProxyItem
		{
			Id = GetString(item, "id"),
			Mode = GetString(item, "mode"),
			ListenEndpoint = $"{((text == "0.0.0.0") ? "127.0.0.1" : text)}:{value}",
			RemoteEndpoint = $"{value2}:{value3}",
			ClientText = (flag ? $"{value4}:{value5}" : "等待连接"),
			StatusText = (flag ? "已连接" : "监听中"),
			DetailText = "MITM：" + (flag2 ? "开启" : "关闭") + "\u3000局域网监听：" + (flag3 ? "开启" : "关闭"),
			StartedAtText = startedAtText,
			CaptureFilePath = GetString(item, "captureFilePath")
		};
	}

	private static string GetString(JsonElement item, string name)
	{
		if (!item.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
		{
			return string.Empty;
		}
		return value.GetString() ?? string.Empty;
	}

	private static int GetInt(JsonElement item, string name)
	{
		if (!item.TryGetProperty(name, out var value) || !value.TryGetInt32(out var value2))
		{
			return 0;
		}
		return value2;
	}

	private static bool GetBool(JsonElement item, string name)
	{
		if (item.TryGetProperty(name, out var value))
		{
			return value.ValueKind == JsonValueKind.True;
		}
		return false;
	}
}
