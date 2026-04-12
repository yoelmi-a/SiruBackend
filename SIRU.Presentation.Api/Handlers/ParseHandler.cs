namespace SIRU.Presentation.Api.Handlers
{
    public static class ParseHandler
    {
        public static string ParseUserAgent(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent)) return "Unknown";

            // Truncar por seguridad
            var ua = userAgent.Length > 500 ? userAgent[..500] : userAgent;

            // Detectar OS
            var os = ua switch
            {
                _ when ua.Contains("Windows NT 10") => "Windows 11/10",
                _ when ua.Contains("Windows NT 6") => "Windows 7/8",
                _ when ua.Contains("Mac OS X") => "MacOS",
                _ when ua.Contains("Android") => "Android",
                _ when ua.Contains("iPhone") || ua.Contains("iPad") => "IOS",
                _ when ua.Contains("Linux") => "Linux",
                _ => "Unknown OS"
            };

            // Detectar navegador
            var browser = ua switch
            {
                _ when ua.Contains("Edg/") => "Edge",
                _ when ua.Contains("Chrome/") => "Chrome",
                _ when ua.Contains("Firefox/") => "Firefox",
                _ when ua.Contains("Safari/") && !ua.Contains("Chrome") => "Safari",
                _ when ua.Contains("curl/") => "curl ⚠️",
                _ => "Unknown Browser"
            };

            return $"{browser} / {os}";
        }
    }
}
