using System.Text.RegularExpressions;

var client = new HttpClient();
var response = await client.GetAsync($"https://passist.org/siteswap/{args[0]}?jugglers={args[1]}");
var html = await response.Content.ReadAsStringAsync();
var pattern = @"<th>\s*([A-Z])\s*<sub>(\d+\|\d+)</sub>\s*</th>";
var matches = Regex.Matches(html, pattern);
foreach (Match match in matches)
{
    var letter = match.Groups[1].Value;
    var numbers = match.Groups[2].Value;
    Console.WriteLine($"{letter}: {numbers}");
}
