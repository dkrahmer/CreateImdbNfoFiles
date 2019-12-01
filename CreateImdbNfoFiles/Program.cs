using System;
using System.IO;

namespace CreateImdbNfoFiles
{
	internal class Program
	{
		private static int Main(string[] args)
		{
			if (args == null || args.Length < 1)
			{
				Console.WriteLine();
				Console.WriteLine();
				Console.WriteLine("Error, a base directory must bespecified. Usage:");
				Console.WriteLine("\tdotenet CreateImdbNfoFiles.dll <BaseDiectory>");
				Console.WriteLine();
				return 1;
			}

			string baseDirectory = args[0];
			if (!Directory.Exists(baseDirectory))
			{
				Console.WriteLine($"The specified directory does not exist: '{baseDirectory}'");
				return 1;
			}

			var nfoCreator = new NfoCreator();
			nfoCreator.ProcessBaseDirectory(baseDirectory);

			return 0;
		}
	}
}
