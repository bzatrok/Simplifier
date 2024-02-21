using Simplifier.Core.Enums;

namespace Simplifier.Models;

public class SubmitUrlForm
{
	public string EnteredUrl { get; set; }
	public LanguageSpeakerLevel TargetSpeakerLevel { get; set; } = LanguageSpeakerLevel.Beginner;
	public string TargetLanguageCode { get; set; } = "en";
}