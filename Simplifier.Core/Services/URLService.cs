using Microsoft.Extensions.Logging;

namespace Simplifier.Core.Services;

public class URLService
{
	private readonly ILogger<URLService> _logger;
    
	public URLService(ILogger<URLService> logger)
	{
		_logger = logger;
	}
    
	public string SanitizeUrl(string articleUrl)
	{
		var decodedUrl = System.Net.WebUtility.UrlDecode(articleUrl);
		
		// Trim whitespaces
		var trimmedUrl = decodedUrl.Trim();
        
		// Convert HTTP to HTTPS
		var secureUrl = trimmedUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) 
			? "https" + trimmedUrl.Substring(4) 
			: trimmedUrl;
        
		try
		{
			var uri = new Uri(secureUrl);
			// Remove URL parameters
			var sanitizedUrl = uri.GetLeftPart(UriPartial.Path);
            
			// Ensure the scheme is HTTPS
			if (!sanitizedUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
				throw new UriFormatException("URL must use HTTPS.");
            
			return sanitizedUrl;
		}
		catch (UriFormatException ex)
		{
			_logger.LogError(ex, "Invalid URL format.");
			throw;
		}
	}
}
