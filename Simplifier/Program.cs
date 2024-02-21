using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Simplifier.Components;
using Simplifier.Core.Cache;
using Simplifier.Core.Clients;
using Simplifier.Core.Services;
using StackExchange.Redis;
using Tailwind;

namespace Simplifier;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.
		builder.Services.AddRazorComponents()
			.AddInteractiveServerComponents();
		
		builder.Services.AddControllers();
		builder.Services.AddHttpClient();
		builder.Services.AddHttpContextAccessor();
		
		// REDIS SETTINGS
		var openAiApiKey = Environment.GetEnvironmentVariable("OPEN_AI_API_KEY");
		var openAiModel = Environment.GetEnvironmentVariable("OPEN_AI_MODEL");
		var hostUrl = Environment.GetEnvironmentVariable("HOST_URL");
		var redisUrl = Environment.GetEnvironmentVariable("REDIS_URL");

		// REQUIRED ENV VARIABLE CHECK
		
		if (string.IsNullOrWhiteSpace(openAiApiKey))
			throw new Exception("OPEN_AI_API_KEY environment variable is not set. Please get yours via https://platform.openai.com/api-keys. The key can be set in launchSettings.json or as an environment variable. If you're running the project via docker, you can set it in the .env file.");
		
		if (string.IsNullOrWhiteSpace(openAiModel))
			throw new Exception("OPEN_AI_MODEL environment variable is not set. Please set your preferred model, e.g. gpt-3.5-turbo-0125. See https://platform.openai.com/docs/models/overview for a full list of current models.");
		
		if (string.IsNullOrWhiteSpace(hostUrl))
			throw new Exception("HOST_URL environment variable is not set. This should be the URL of your deployed app, e.g. https://simplifier.herokuapp.com/ or http://localhost:5214/ for local development.");

		if (string.IsNullOrWhiteSpace(redisUrl))
			throw new Exception("REDIS_URL environment variable is not set");
		
		if (!string.IsNullOrEmpty(redisUrl))
		{
			var password = string.Empty;
			var hostAndPort = string.Empty;
			var useSsl = false;

			if (redisUrl.StartsWith("rediss://") || redisUrl.StartsWith("redis://"))
			{
				var uri = new Uri(redisUrl);
				password = uri.UserInfo.Split(':')[1];
				hostAndPort = $"{uri.Host}:{uri.Port}";
				useSsl = uri.Scheme == "rediss";
			}
			else
			{
				var parts = redisUrl.Split('@');
				password = parts.Length > 1 ? parts[0] : null;
				hostAndPort = parts.Length > 1 ? parts[1] : parts[0];
			}

			var redisTimeoutSec = Environment.GetEnvironmentVariable("REDIS_TIMEOUT_MS") is not null ? Convert.ToInt32(Environment.GetEnvironmentVariable("REDIS_TIMEOUT_MS")) : 3000;

			var config = new ConfigurationOptions
			{
				EndPoints = { hostAndPort },
				Password = password,
				Ssl = useSsl || false,
				AbortOnConnectFail = false,
				ConnectTimeout = redisTimeoutSec, // This is for initial connection
				SyncTimeout = redisTimeoutSec, // This is for each sync operation
				AsyncTimeout = redisTimeoutSec // This is for each async operation
			};

			config.CertificateValidation += ValidateServerCertificate;

			bool ValidateServerCertificate(
				object sender,
				X509Certificate? certificate,
				X509Chain? chain,
				SslPolicyErrors sslPolicyErrors)
			{
				//BEN: ADDED TO MAKE HEROKU REDIS WORK
				return true;
			}

			builder.Services.AddSingleton(ConnectionMultiplexer.Connect(config));
			builder.Services.AddSingleton<RedisHelper>();
		}
				
		builder.Services.AddTransient<OpenAiClient>();
		builder.Services.AddTransient<URLService>();
		builder.Services.AddTransient<ArticleService>();
		builder.Services.AddTransient<SimplifierService>();
		
		// LOGGING

		var logLevelFromEnv = Environment.GetEnvironmentVariable("LOGLEVEL");

		if (!Enum.TryParse(logLevelFromEnv, true, out LogLevel logLevel))
		{
			logLevel = LogLevel.Error; // default to Information if parsing fails
			if (!string.IsNullOrWhiteSpace(logLevelFromEnv))
				Console.WriteLine("Invalid 'LOGLEVEL' var passed. Valid Log Levels are: Trace, Debug, Information, Warning, Error, Critical, None");
		}
		
		if (logLevel == null || logLevel == LogLevel.None)
			logLevel = LogLevel.Warning;

		// Configure logging for the app, based on the "LOGLEVEL" env var.
		builder.Logging
			.ClearProviders()
			.AddConsole()
			.AddFilter("Microsoft.Hosting", LogLevel.Information)
			.SetMinimumLevel(logLevel);

		var app = builder.Build();

		if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler("/Error");
			app.UseHsts();
		}

		app.UseHttpsRedirection();
		app.UseStaticFiles();
		app.UseAntiforgery();

		app.MapRazorComponents<App>()
			.AddInteractiveServerRenderMode();
		
		app.MapControllers();
		
		// Convert tailwind classes to output.css file
		if (app.Environment.IsDevelopment())
		{
			_ = app.RunTailwind("tailwind", "./");
		}

		app.Run();
	}
}