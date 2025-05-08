using CboxParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichardSzalay.MockHttp;
using System.Net;
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
            };
            _mockHttp.Expect($"/box/?boxid=1&boxtag=a&sec=archive&i={parserConfig.StartIndex}&pi={parserConfig.StartIndex + 40}&p=1").Respond(HttpStatusCode.OK);

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
            };
            _mockHttp.Expect($"/box/?boxid=1&boxtag=a&sec=archive&i={parserConfig.StartIndex}&pi={parserConfig.StartIndex + 40 + parserConfig.PageIncrement * 0}&p=1").Respond(HttpStatusCode.OK);
            _mockHttp.Expect($"/box/?boxid=1&boxtag=a&sec=archive&i={parserConfig.StartIndex}&pi={parserConfig.StartIndex + 40 + parserConfig.PageIncrement * 1}&p=2").Respond(HttpStatusCode.OK);
            _mockHttp.Expect($"/box/?boxid=1&boxtag=a&sec=archive&i={parserConfig.StartIndex}&pi={parserConfig.StartIndex + 40 + parserConfig.PageIncrement * 2}&p=3").Respond(HttpStatusCode.OK);

            await _sut.Start(parserConfig);
            _mockHttp.VerifyNoOutstandingExpectation();
        }
    }
}
