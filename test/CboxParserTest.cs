using CboxParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichardSzalay.MockHttp;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

namespace CBoxParser.Test
{
    [TestClass]
    public class ParserTest
    {
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly Parser _sut;

        public ParserTest()
        {
            _mockHttp = new MockHttpMessageHandler();
            _sut = new Parser(_mockHttp.ToHttpClient(), 1, "a");
        }

        [TestMethod]
        public async Task Start_ForOnePage_IndexAndPi40Apart()
        {
            var parserConfig = new ParserConfig
            {
                StartIndex = 1,
                Pages = 1,
                PageIncrement = 0,
                Pattern = string.Empty,
            };
            this.CreateExpect(parserConfig);

            await _sut.Start(parserConfig);
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task Start_ForMultiplePages_PiAndPageIncrements()
        {
            var parserConfig = new ParserConfig
            {
                StartIndex = 1,
                Pages = 3,
                PageIncrement = 33,
                Pattern = string.Empty,
            };
            this.CreateExpect(parserConfig);

            await _sut.Start(parserConfig);
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [TestMethod]
        public async Task Start_GivenRegex_ReturnArrayWithFoundUrl()
        {
            var url = "http://abc.com/myFile.zip";
            var parserConfig = new ParserConfig
            {
                StartIndex = 1,
                Pages = 1,
                PageIncrement = 0,
                // lang=regex
                Pattern = @".*\.zip",
            };
            this.CreateExpect(parserConfig, $"<a href='{url}'>my link</a>");
            var urls = await _sut.Start(parserConfig);
            _mockHttp.VerifyNoOutstandingExpectation();
            Assert.AreEqual(url, urls.FirstOrDefault());
        }

        private void CreateExpect(ParserConfig config, string html = "")
        {
            for (int i = 0; i < config.Pages; ++i)
            {
                _mockHttp.Expect($"/box/?boxid=1&boxtag=a&sec=archive&i={config.StartIndex}&pi={config.StartIndex + 40 + config.PageIncrement * i}&p={i + 1}").Respond(HttpStatusCode.OK, MediaTypeNames.Text.Html, html);
            }
        }
    }
}
