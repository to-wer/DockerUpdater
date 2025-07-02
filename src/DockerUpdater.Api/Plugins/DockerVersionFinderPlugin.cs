using System.ComponentModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.SemanticKernel;

namespace DockerUpdater.Api.Plugins;

public class DockerVersionFinderPlugin
{
    private readonly HttpClient _httpClient = new();
    
    [KernelFunction("get_latest_tags")]
    [Description("Get the latest version for a docker image from a docker compose file.")]
    public async Task<string?> GetLatestTagsAsync([Description("The name of the docker image")]string imageName,
        [Description("The current version of the image")]string? currentVersion = null)
    {
        // TODO: Add support for ghcr.io and other registries

        var tags = new List<string>();

        var split = imageName.Split('/');
        var owner = "library";
        var repository = split.Last();
        if (split.Length == 2)
        {
            owner = split[0];
            repository = split[1];
        }

        var url =
            $"https://hub.docker.com/v2/repositories/{owner}/{repository}/tags?page_size=1000";

        while (url != null)
        {
            var response = await _httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(response);

            var root = doc.RootElement;
            foreach (var result in root.GetProperty("results").EnumerateArray())
            {
                if (result.TryGetProperty("name", out var nameProp))
                    tags.Add(nameProp.GetString() ?? "");
            }

            url = root.TryGetProperty("next", out var nextProp) && nextProp.ValueKind == JsonValueKind.String
                ? nextProp.GetString()
                : null;
        }

        // Filter SemVer-compatible tags (e.g. 1.0.0, 2.1.3 etc.)
        var semverTags = tags
            .Select(tag => new { Tag = tag, SemVer = ParseSemVer(tag) })
            .Where(x => x.SemVer != null)
            .OrderByDescending(x => x.SemVer)
            .Select(x => x.Tag)
            .ToList();

        var json = JsonSerializer.Serialize(semverTags.Take(5).ToList());

        return json;
    }

    private Version? ParseSemVer(string tag)
    {
        tag = tag.StartsWith("v", StringComparison.OrdinalIgnoreCase) ? tag.Substring(1) : tag;

        if (Regex.IsMatch(tag, @"^\d+(\.\d+){1,3}$"))
        {
            try
            {
                return Version.Parse(tag);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }
}