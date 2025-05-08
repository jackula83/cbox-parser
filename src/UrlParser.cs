using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CboxParser
{
    public class UrlParser
    {
        public const int PAGE_SIZE = 40;

        private readonly HttpClient _httpClient;

        private readonly string _basePath;

        public UrlParser(HttpClient httpClient, int boxId, string boxTag)
        {
            var baseAddress = new Uri($"https://www4.cbox.ws");

            _httpClient = httpClient;
            _httpClient.BaseAddress = baseAddress;
            _basePath = $"/box/?boxid={boxId}&boxtag={boxTag}&sec=archive";
        }

        public async Task<List<string>> Start(ParserConfig config)
        {
            var urls = new List<string>();
            for (int i = 0; i < config.Pages; ++i)
            {
                var response = await _httpClient.GetAsync($"{_basePath}&i={config.StartIndex}&pi={config.StartIndex + PAGE_SIZE + config.PageIncrement * i}&p={i + 1}");
                var html = await response.Content.ReadAsStringAsync();
                urls.AddRange(this.findUrlsWithPattern(html, config.Pattern));
            }
            return urls;
        }

        private IEnumerable<string> findUrlsWithPattern(string html, string pattern)
        {
            var regex = new Regex($@"\bhttps?://[^\s""'<>\[\]()]*{pattern}[^\s""'<>\[\]()]*");
            var matches = regex.Matches(html);
            return matches.Cast<Match>().Select(m => m.Value);
        }
    }
}
