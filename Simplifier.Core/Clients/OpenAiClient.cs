using System.Diagnostics;
using Simplifier.Core.DTOs;
using Simplifier.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Simplifier.Core.Clients;

public class OpenAiClient
{
	private readonly ILogger<OpenAiClient> _logger;
	
	public OpenAiClient(ILogger<OpenAiClient> logger)
	{
		_logger = logger;
	}
	
	public async Task<string> GenerateSingleAssistantResponse(string systemPrompt, string request)
	{
		try
		{
			var requestContent = new OpenAiRequestDto
			{
				model = Environment.GetEnvironmentVariable("OPEN_AI_MODEL"),
				messages = new List<OpenAiMessageDto>
				{
					new OpenAiMessageDto()
					{
						role = "system",
						content = systemPrompt
					},
					new OpenAiMessageDto()
					{
						role = "user",
						content = request
					}
				}
			};

			return await OpenAiCompletionRequestWithContent(requestContent);
		} 
		catch (Exception ex)
		{
			_logger.LogError(ex, ex.Message);
			// Handle exceptions
			//throw;
		}

		return null;
	}
	
	private async Task<string> OpenAiCompletionRequestWithContent(OpenAiRequestDto requestContent)
	{
		try
		{
			var client = new HttpClient();
			client.Timeout = TimeSpan.FromSeconds(60);

			var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_API_KEY");
			var baseUrl = "https://api.openai.com";
			var requestUri = new Uri($"{baseUrl}/v1/chat/completions");

			var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri);
			httpRequest.Headers.Add("Authorization", $"Bearer {apiKey}");
			httpRequest.Content = new StringContent(JsonConvert.SerializeObject(requestContent), System.Text.Encoding.UTF8, "application/json");

			Stopwatch stopWatch = new Stopwatch();
			stopWatch.Start();
		
			_logger.LogInformation($"Sending request to OpenAI with content: {JsonConvert.SerializeObject(requestContent)}");
		
			var response = await client.SendAsync(httpRequest);
			var responseContent = await response.Content.ReadAsStringAsync();

			var result = JsonConvert.DeserializeObject<OpenAiChatResponse>(responseContent);

			stopWatch.Stop();
		
			_logger.LogInformation($"Received response from OpenAI in {stopWatch.ElapsedMilliseconds}ms with content: {responseContent}");
		
			var aiResponse = result.choices[0].message.content;

			return aiResponse;
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			_logger.LogError(e, e.Message);
			throw;
		}
	}
}