namespace Simplifier.Core.Models;

public class SimplifyRequest
{
	public string SimplifyUrl { get; set; }
	public string? TargetSpeakerLevel { get; set; }
	public string? TargetLanguageCode { get; set; }
}