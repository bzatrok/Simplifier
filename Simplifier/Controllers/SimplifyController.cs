using System.Diagnostics;
using System.Globalization;
using Simplifier.Core.Enums;
using Simplifier.Core.Extensions;
using Microsoft.AspNetCore.Mvc;
using Simplifier.Core.Models;
using Simplifier.Core.Services;

namespace Simplifier.Controllers;

[ApiController]
[Route("api")]
public class SimplifyController : ControllerBase
{
	#region Variables
	
	private readonly ILogger<SimplifyController> _logger;
	private readonly URLService _urlService;
	private readonly SimplifierService _simplifierService;
	
	#endregion
	#region Initialization

	public SimplifyController(
		ILogger<SimplifyController> logger,
		URLService urlService,
			SimplifierService simplifierService
	)
	{
		_logger = logger;
		_urlService = urlService;
		_simplifierService = simplifierService;
	}
	
	#endregion

	[HttpPost]
	[Route("simplify")]
	public async Task<ActionResult<SimplifiedArticleResponse>> SimplifyUrl([FromBody] SimplifyRequest simplifyRequest)
	{
		_logger.LogInformation($"Simplification request received for URL: {simplifyRequest.SimplifyUrl}");

		try
		{
			LanguageSpeakerLevel languageSpeakerLevel = LanguageSpeakerLevel.Beginner;
			string targetLanguageCode = null;

			try
			{
				if (!string.IsNullOrWhiteSpace(simplifyRequest.TargetSpeakerLevel))
					languageSpeakerLevel = EnumExtensions.GetEnumValueFromDescription<LanguageSpeakerLevel>(simplifyRequest.TargetSpeakerLevel.Trim());

				if (!string.IsNullOrWhiteSpace(simplifyRequest.TargetLanguageCode))
				{
					CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
					CultureInfo matchingTargetCulture = cultures.FirstOrDefault(lang => lang.Name.ToLower() == simplifyRequest.TargetLanguageCode.ToLower().Trim());
					targetLanguageCode = matchingTargetCulture.Name;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error parsing target language or speaker level");
			}

			var sanitizedUrl = _urlService.SanitizeUrl(simplifyRequest.SimplifyUrl);
			
			_logger.LogInformation($"Simplification url sanitized to: {sanitizedUrl}");
			
			var simplifiedResponse = await _simplifierService.SimplifyArticleByUrl(
				sanitizedUrl,
				languageSpeakerLevel,
				targetLanguageCode);

			_logger.LogInformation($"Simplification response received for URL: {simplifyRequest.SimplifyUrl}");
			
			return simplifiedResponse;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"Error simplifying URL: {simplifyRequest.SimplifyUrl}");
			throw;
		}
	}
}
