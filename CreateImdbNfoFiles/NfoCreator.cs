using HtmlAgilityPack;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace CreateImdbNfoFiles
{
	internal class NfoCreator
	{
		private static readonly Regex MOVIE_INFO_REGEX = new Regex(@"^(?<Name>.+) \((?<Year>[0-9]{4})\)$");
		private const string BASE_IMDB_URL = @"https://www.imdb.com";

		public NfoCreator()
		{
		}

		internal void ProcessBaseDirectory(string baseDirectory)
		{
			foreach (string movieDirectory in Directory.EnumerateDirectories(baseDirectory))
			{
				ProcessMovieDirectory(movieDirectory);
			}
		}

		private void ProcessMovieDirectory(string movieDirectory)
		{
			string movieDirectoryNodeName = Path.GetFileName(movieDirectory);
			var matches = MOVIE_INFO_REGEX.Match(movieDirectoryNodeName);
			if (!matches.Success)
			{
				Console.WriteLine($"Does not look like a movie directory: '{movieDirectory}' - must match regex: {MOVIE_INFO_REGEX.ToString()}");
				return;
			}

			string movieFilePath = GetMovieFilePath(movieDirectory);
			if (movieFilePath == null)
			{
				// Console.WriteLine($"Could not identify a single video file in movie directory: '{movieDirectory}'");
				return;
			}

			string nfoFilePath = GetNfoFilePath(movieFilePath);

			if (File.Exists(nfoFilePath))
			{
				string firstLine = File.ReadLines(nfoFilePath).First();
				if (firstLine?.Contains(BASE_IMDB_URL) ?? false)
				{
					return; // NFO file already exists with IMDB URL
				}
				Console.WriteLine($"NFO file exists but does not contain IMDB URL: '{nfoFilePath}' - attempting to replace...");
			}

			string movieName = matches.Groups["Name"].Value;
			string movieYear = matches.Groups["Year"].Value;

			string imdbUrl = GetImdbUrl(movieName, movieYear);

			if (imdbUrl == null)
			{
				Console.WriteLine($"Could not find a matching movie on IMDB.com for movie directory: '{movieDirectory}'");
				return;
			}

			Console.WriteLine($"Writing NFO file: '{nfoFilePath}'");
			File.WriteAllText(nfoFilePath, imdbUrl + Environment.NewLine);
		}

		private string GetNfoFilePath(string movieFilePath)
		{
			int lastDotIndex = movieFilePath.LastIndexOf('.');
			if (lastDotIndex < 0)
				return $"{movieFilePath}.nfo";

			return movieFilePath.Substring(0, lastDotIndex) + ".nfo";
		}

		private string GetMovieFilePath(string movieDirectory)
		{
			var videoFiles = Directory.GetFiles(movieDirectory, "*.*", SearchOption.TopDirectoryOnly).Where(d =>
					   d.EndsWith(".avi", StringComparison.InvariantCultureIgnoreCase)
					|| d.EndsWith(".divx", StringComparison.InvariantCultureIgnoreCase)
					|| d.EndsWith(".img", StringComparison.InvariantCultureIgnoreCase)
					|| d.EndsWith(".iso", StringComparison.InvariantCultureIgnoreCase)
					|| d.EndsWith(".m2ts", StringComparison.InvariantCultureIgnoreCase)
					|| d.EndsWith(".m4v", StringComparison.InvariantCultureIgnoreCase)
					|| d.EndsWith(".mkv", StringComparison.InvariantCultureIgnoreCase)
					|| d.EndsWith(".mp4", StringComparison.InvariantCultureIgnoreCase)
					|| d.EndsWith(".mpg", StringComparison.InvariantCultureIgnoreCase)
					|| d.EndsWith(".ts", StringComparison.InvariantCultureIgnoreCase)
				);

			if (videoFiles.Count() < 1)
			{
				Console.WriteLine($"Could not find a video file in movie directory: '{movieDirectory}'");
				return null;
			}

			if (videoFiles.Count() > 1)
			{
				Console.WriteLine($"Found multiple video files in movie directory: '{movieDirectory}'");
				return null;
			}

			return videoFiles.First();
		}

		private string GetImdbUrl(string movieName, string movieYear)
		{
			movieName = movieName.Replace('_', ' ');
			string query = $"{movieName} ({movieYear})";
			Console.WriteLine($"Searching IMDB.com for movie: {query}");
			var request = (HttpWebRequest)WebRequest.Create($"{BASE_IMDB_URL}/find?q={HttpUtility.UrlEncode(query)}");
			string htmlResponse = GetWebResponseString(request);

			HtmlDocument document = new HtmlDocument();
			document.LoadHtml(htmlResponse);

			var resultListTable = document.DocumentNode.SelectSingleNode(@"//table[@class='findList']");

			if ((resultListTable?.ChildNodes?.Count() ?? 0) == 0)
			{
				Console.WriteLine($"Could not find any matching movies on IMDB.com for movie '{movieName}' ({movieYear})");
				return null;
			}

			var firstResultRow = resultListTable?.SelectSingleNode(@"tr");
			var resultTextCell = firstResultRow?.SelectSingleNode(@"td[@class='result_text']");
			var aTag = resultTextCell?.SelectSingleNode(@"a");
			string movieRelativeUrl = aTag?.Attributes["href"].Value;
			int startJunkIndex = movieRelativeUrl?.IndexOf("?") ?? -1;
			if (startJunkIndex >= 0)
				movieRelativeUrl = movieRelativeUrl.Substring(0, startJunkIndex);

			if (movieRelativeUrl == null)
				return null;

			return $"{BASE_IMDB_URL}{movieRelativeUrl}";
		}

		private string GetWebResponseString(HttpWebRequest request)
		{
			using (WebResponse response = request.GetResponse())
			{
				using (Stream webResponseStream = response.GetResponseStream())
				{
					using (StreamReader webResponseReader = new StreamReader(webResponseStream))
					{
						return webResponseReader.ReadToEnd();
					}
				}
			}
		}
	}
}