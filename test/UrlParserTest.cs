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
    public class UrlParserTest
    {
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly UrlParser _sut;

        public UrlParserTest()
        {
            _mockHttp = new MockHttpMessageHandler();
            _sut = new UrlParser(_mockHttp.ToHttpClient(), 1, "a");
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

        [TestMethod]
        public async Task Start_GivenRegex_ReturnArrayWithMultipleFoundUrl()
        {
            var url1 = "http://abc.com/file1.zip";
            var url2 = "https://www.def.org/file2.zip";
            var parserConfig = new ParserConfig
            {
                StartIndex = 1,
                Pages = 1,
                PageIncrement = 0,
                // lang=regex
                Pattern = @".\.zip",
            };
            this.CreateExpect(parserConfig, $"<a href='{url1}'>my link 1</a><br /><a href='{url2}'>my link 2</a>");
            var urls = await _sut.Start(parserConfig);
            _mockHttp.VerifyNoOutstandingExpectation();
            Assert.AreEqual(url1, urls[0]);
            Assert.AreEqual(url2, urls[1]);
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
