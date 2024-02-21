using System.Text;
using Simplifier.Core.Cache;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace Simplifier.Core.Services;

public class ArticleService
{
	private readonly ILogger<ArticleService> _logger;
	private readonly RedisHelper _redisHelper;
	
	public ArticleService(
		ILogger<ArticleService> logger,
		RedisHelper redisHelper)
	{
		_logger = logger;
		_redisHelper = redisHelper;
	}
	
	public async Task<string> GetArticleContentFromUrl(string articleUrl)
	{
		var requestHash = HashHelper.GenerateHash(new
		{
			ArticleUrl = articleUrl
		});		
			
		var redisKey = $"Simplifier:Article:{requestHash}:Content";
		var cachedArticleContent = _redisHelper.GetObject<string>(redisKey);

		if (string.IsNullOrWhiteSpace(cachedArticleContent))
		{
			var web = new HtmlWeb();
			var doc = await web.LoadFromWebAsync(articleUrl);

			var bodyNode = doc.DocumentNode.SelectSingleNode("//body");

			if (bodyNode != null)
			{
				StringBuilder articleContent = new StringBuilder();
				// Iterating through nodes, including images but excluding scripts and styles
				foreach (var node in bodyNode.Descendants())
				{
					if (node.NodeType == HtmlNodeType.Text &&
					    node.ParentNode.Name != "script" &&
					    node.ParentNode.Name != "style")
					{
						string text = node.InnerText.Trim();
						if (!string.IsNullOrEmpty(text))
						{
							articleContent.AppendLine(text);
						}
					}
					else if (node.Name == "img")
					{
						// Extracting image src (and alt) attribute
						var src = node.GetAttributeValue("src", string.Empty);
						var alt = node.GetAttributeValue("alt", string.Empty);
						if (!string.IsNullOrEmpty(src))
						{
							// Including the image in the output in a way that suits your application
							// This is a simple HTML img tag, but you might adjust based on your needs
							articleContent.AppendLine($"<img src=\"{src}\" alt=\"{alt}\">");
						}
					}
				}

				var articleContentString = articleContent.ToString();
				_redisHelper.SetObject(redisKey, articleContentString, TimeSpan.FromDays(1));
				
				_logger.LogInformation($"Retrieved article content from URL: {articleUrl}");

				return articleContentString;
			}

			_logger.LogInformation($"No article content found for URL: {articleUrl}");
			return null;
		}

		_logger.LogInformation($"Returned cached article content for URL: {articleUrl}");

		return cachedArticleContent;
	}
	
	public async Task<string> GetArticleTitleFromUrl(string articleUrl)
	{
		var requestHash = HashHelper.GenerateHash(new
		{
			ArticleUrl = articleUrl
		});		
			
		var redisKey = $"Simplifier:Article:{requestHash}:Title";
		var cachedArticleTitle = _redisHelper.GetObject<string>(redisKey);

		if (string.IsNullOrWhiteSpace(cachedArticleTitle))
		{
			var web = new HtmlWeb();
			var doc = await web.LoadFromWebAsync(articleUrl);
			var titleNode = doc.DocumentNode.SelectSingleNode("//h1");
			var titleString = titleNode?.InnerText;
			_redisHelper.SetObject(redisKey, titleString, TimeSpan.FromDays(1));
			_logger.LogInformation($"Retrieved article title from URL: {articleUrl}");

			return titleString;
		}

		_logger.LogInformation($"Returned cached article title for URL: {articleUrl}");
		return cachedArticleTitle;
	}
}