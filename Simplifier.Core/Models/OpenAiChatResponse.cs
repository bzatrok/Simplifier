namespace Simplifier.Core.Models;

public class OpenAiChatResponse
{
	public string id { get; set; }
	public string @object { get; set; }
	public int created { get; set; }
	public string model { get; set; }
	public List<Choice> choices { get; set; }
	public Usage usage { get; set; }

	public class Choice
	{
		public string text { get; set; }
		public int index { get; set; }
		public string finish_reason { get; set; }
		public ChoiceMessage message { get; set; }
	}
    
	public class ChoiceMessage
	{
		public string role { get; set; }
		public string content { get; set; }
	}

	public class Usage
	{
		public int prompt_tokens { get; set; }
		public int completion_tokens { get; set; }
		public int total_tokens { get; set; }
	}
}
