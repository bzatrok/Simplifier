using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Simplifier.Core.Cache;

public class HashHelper
{
	public static string GenerateHash<T>(T inputObject)
	{
		var inputJson = JsonConvert.SerializeObject(inputObject);
		var inputBytes = Encoding.UTF8.GetBytes(inputJson);

		string hash = null;
		using (MD5 md5 = MD5.Create())
		{
			var hashByteArray = md5.ComputeHash(inputBytes);

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < hashByteArray.Length; i++)
			{
				sb.Append(hashByteArray[i].ToString("X2"));
			}

			hash = sb.ToString();
		}

		return hash;
	}
}