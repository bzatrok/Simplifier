namespace Simplifier.Core.DTOs;

public class OpenAiRequestDto
{
	public string model { get; set; }
	public List<OpenAiMessageDto> messages { get; set; }
}