using System.Diagnostics;
using System.Globalization;
using Simplifier.Core.Cache;
using Simplifier.Core.Clients;
using Simplifier.Core.Enums;
using Simplifier.Core.Extensions;
using Simplifier.Core.Models;
using Microsoft.Extensions.Logging;

namespace Simplifier.Core.Services;

public class SimplifierService
{
	#region Variables

	private readonly ILogger<SimplifierService> _logger;
	private readonly OpenAiClient _openAiClient;
	private readonly ArticleService _articleService;
	private readonly RedisHelper _redisHelper;
	
	private SemaphoreSlim _simplifierSemaphore = new SemaphoreSlim(1, 5);

	#endregion
	#region Initialization

	public SimplifierService(
		ILogger<SimplifierService> logger,
		OpenAiClient openAiClient,
		ArticleService articleService,
		RedisHelper redisHelper
	)
	{
		_logger = logger;
		_openAiClient = openAiClient;
		_articleService = articleService;
		_redisHelper = redisHelper;
	}
	
	#endregion
	#region Requests
	
    public async Task<SimplifiedArticleResponse> SimplifyArticleByUrl(
	    string articleUrl,
	    LanguageSpeakerLevel targetSpeakerLevel = LanguageSpeakerLevel.Beginner,
		string targetLanguageCode = null)
    {
	    var requestHash = HashHelper.GenerateHash(new
	    {
		    ArticleUrl = articleUrl
	    });		
	    
	    string languageCacheSnippet = !string.IsNullOrWhiteSpace(targetLanguageCode) ? $"Language-{targetLanguageCode}-" : string.Empty;
	    string targetSpeakerLevelString = EnumExtensions.GetDescription(targetSpeakerLevel);

	    var inProgressRedisKey = $"Simplifier:Article:{requestHash}:SimplificationInProgress";
	    var responseCacheRedisKey = $"Simplifier:Article:{requestHash}:SimplifiedContent:{languageCacheSnippet}Level-{targetSpeakerLevelString}";
	    var cachedSimplifiedArticle = _redisHelper.GetObject<SimplifiedArticleResponse>(responseCacheRedisKey);
	    var isSimplificationInProgress = _redisHelper.GetObject<bool>(inProgressRedisKey);

	    if (!isSimplificationInProgress && (cachedSimplifiedArticle == null || string.IsNullOrWhiteSpace(cachedSimplifiedArticle.Content)))
	    {
		    try
		    {
			    _redisHelper.SetObject(inProgressRedisKey, true, TimeSpan.FromMinutes(1));
			    
			    await _simplifierSemaphore.WaitAsync();

			    var systemPrompt = "You are an expert simplifier of news articles. Your job is to simplify the following news article.";
			    var articleContent = await _articleService.GetArticleContentFromUrl(articleUrl);
			    var articleTitle = await _articleService.GetArticleTitleFromUrl(articleUrl);

			    // MAKE DYNAMIC
			    
			    var determineArticleLanguagePrompt =
				    "You are an expert in determining the language of news articles. Your job is to determine the language of the following news article. Return the language via language code, for example 'en' or 'nl' or any other language. Please do not return any other information.";
			    
			    var determineArticleLanguageRequest =
				    $"Please determine the language of the following article, with the title of '{articleTitle}' and content of '{articleContent}'.";

			    if (string.IsNullOrWhiteSpace(targetLanguageCode)) 
				    targetLanguageCode = await _openAiClient.GenerateSingleAssistantResponse(determineArticleLanguagePrompt, determineArticleLanguageRequest);
			    
			    var requestString =
				    $"Please re-write the following article in simplified language. The language of the resulting simplified article should be in this language code: '{targetLanguageCode}'. If necessary, translate the article to the previously determined language. Do not shorten the article and do not remove details if possible. Remove unneeded content like menu information, image captions, content that is obviously from footers and website headers. You are great at simplifying the articles but preserving the details. You use simple language, so the simplified article should be understandable to a {targetSpeakerLevelString} speaker. Here is the article content: '{articleContent}'";

			    Stopwatch stopWatch = new Stopwatch();
			    stopWatch.Start();
			    
			    _logger.LogInformation($"Starting simplification for url: {articleUrl} and language: {targetLanguageCode} and speaker level: {targetSpeakerLevelString}");

			    var simplifiedContent = await _openAiClient.GenerateSingleAssistantResponse(systemPrompt, requestString);

			    var simplifiedArticle = new SimplifiedArticleResponse
			    {
				    Title = articleTitle,
				    Content = simplifiedContent
			    };
			    
			    _redisHelper.SetObject(responseCacheRedisKey, simplifiedArticle, TimeSpan.FromDays(1));
			    _redisHelper.DeleteKey(inProgressRedisKey);

			    stopWatch.Stop();
			    _logger.LogInformation($"Article simplified in {stopWatch.ElapsedMilliseconds}ms for url: {articleUrl} and language: {targetLanguageCode} and speaker level: {targetSpeakerLevelString}");

			    return simplifiedArticle;
		    }
		    catch (Exception ex)
		    {
			    _logger.LogError(ex, ex.Message);
		    }
		    finally
		    {
			    _simplifierSemaphore.Release();
		    }
	    }

	    return cachedSimplifiedArticle;
    }

	#endregion
}