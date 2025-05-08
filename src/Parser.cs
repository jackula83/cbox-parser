using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CboxParser
{
    public class Parser
    {
        public const int PAGE_SIZE = 40;

        private readonly HttpClient _httpClient;

        private readonly string _basePath;

        public Parser(HttpClient httpClient, int boxId, string boxTag)
        {
            var baseAddress = new Uri($"https://www4.cbox.ws");

            _httpClient = httpClient;
            _httpClient.BaseAddress = baseAddress;
            _basePath = $"/box/?boxid={boxId}&boxtag={boxTag}&sec=archive";
        }

        public async Task Start(ParserConfig config)
        {
            for (int i = 0; i < config.Pages; ++i)
            {
                await _httpClient.GetAsync($"{_basePath}&i={config.StartIndex}&pi={config.StartIndex + PAGE_SIZE + config.PageIncrement * i}&p={i + 1}");
            }
        }
    }
}
