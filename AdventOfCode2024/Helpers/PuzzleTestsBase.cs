using AdventOfCode;

using Microsoft.Extensions.Configuration;

namespace AdventOfCode2024;

public abstract class PuzzleTestsBase
{
	public SiteRunner Runner { get; }

	protected PuzzleTestsBase()
	{
		const string sessionTokenPath = "AdventOfCode:sessionToken";

		var config = new ConfigurationBuilder().AddUserSecrets(typeof(PuzzleTestsBase).Assembly).Build();
		string? sessionToken = config[sessionTokenPath];

		if (sessionToken is null)
		{
			throw new InvalidOperationException(
				$"No Advent Of Code site session token found. Set value in the user secret under '{sessionTokenPath}'"
				);
		}

		Runner = new SiteRunner(
			new HttpClient(),
			new Uri("https://adventofcode.com"),
			sessionToken
			);
	}
}
