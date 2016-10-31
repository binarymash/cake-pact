namespace Cake.Pact.Tests
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;

    using Cake.Core;
    using Cake.Core.Diagnostics;
    using Cake.Pact.TestData;
    using Cake.Testing;
    using Flurl;
    using Moq;
    using Flurl.Http.Testing;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Shouldly;
    using TestStack.BDDfy;
    using Xunit;

    public class PactAliasesSpecs : IDisposable
    {
        private readonly HttpTest _httpTest;

        private readonly string _server;

        private JObject _pactJObject;

        private string _version;

        private bool _result;

        private readonly Mock<ICakeContext> _mockContext;

        private readonly ICakeLog _log;

        public PactAliasesSpecs()
        {
            _httpTest = new HttpTest();
            _server = "http://192.168.99.100:8070";
            _mockContext = new Mock<ICakeContext>();

            _log = new FakeLog();
            _mockContext.Setup(c => c.Log).Returns(_log);
        }

        [Fact]
        public void PublishingPactWhenVersionDoesntAlreadyExist()
        {
            this.Given(_ => _.GivenValidPactJObject())
                .And(_ => _.GivenAValidVersion())
                .And(_ => _.GivenTheBrokerWillReturnACreatedStatusCode())
                .When(_ => _.WhenPublishingPactToBroker())
                .Then(_ => _.ThenThePactIsSentToTheBroker())
                .And(_ => _.ThenTheResultIsTrue())
                .BDDfy();
        }

        [Fact]
        public void PublishingPactWhenVersionAlreadyExists()
        {
            this.Given(_ => _.GivenValidPactJObject())
                .And(_ => _.GivenAValidVersion())
                .And(_ => _.GivenTheBrokerWillReturnAnOkStatusCode())
                .When(_ => _.WhenPublishingPactToBroker())
                .Then(_ => _.ThenThePactIsSentToTheBroker())
                .And(_ => _.ThenTheResultIsTrue())
                .BDDfy();
        }

        [Fact]
        public void PublishingPactWhenTimeoutFromBroker()
        {
            this.Given(_ => _.GivenValidPactJObject())
                .And(_ => _.GivenAValidVersion())
                .And(_ => _.GivenTheBrokerWillTimeOut())
                .When(_ => _.WhenPublishingPactToBroker())
                .Then(_ => _.ThenThePactIsSentToTheBroker())
                .And(_ => _.ThenTheResultIsFalse())
                .BDDfy();
        }

        ////[Fact]
        ////public void PublishingPactWhenContextIsNotSet()
        ////{
        ////    throw new NotImplementedException();
        ////}

        ////[Fact]
        ////public void PublishingPactWhenServerIsNotSet()
        ////{
        ////    throw new NotImplementedException();
        ////}

        public void Dispose()
        {
            _httpTest.Dispose();
        }

        private void GivenAValidVersion()
        {
            _version = "1.2.3.5";
        }

        private void GivenValidPactJObject()
        {
            var pactJson = ResourceLoader.Load("MyPact.json");
            _pactJObject = JsonConvert.DeserializeObject<JObject>(pactJson);
        }

        private void GivenTheBrokerWillReturnACreatedStatusCode()
        {
            // TODO: check response body
            _httpTest.RespondWith(string.Empty, (int)HttpStatusCode.Created);
        }

        private void GivenTheBrokerWillReturnAnOkStatusCode()
        {
            // TODO: check response body
            _httpTest.RespondWith(string.Empty, (int)HttpStatusCode.OK);
        }

        private void GivenTheBrokerWillTimeOut()
        {
            _httpTest.SimulateTimeout();
        }

        private void WhenPublishingPactToBroker()
        {
            _result = PactAliases.PactPublishToBroker(_mockContext.Object, _server, _pactJObject, _version);
        }

        private void ThenThePactIsSentToTheBroker()
        {
            var uri = _server.AppendPathSegments("pacts/provider/Animal%20Service/consumer/Zoo%20App/versions", _version);
            _httpTest
                .ShouldHaveCalled(uri)
                .WithVerb(HttpMethod.Put)
                .WithContentType("application/json")
                .With(c => Header(c.Request.Headers, "Accept", "application/json"))
                .Times(1);
        }

        private void ThenTheResultIsTrue()
        {
            _result.ShouldBeTrue();
        }

        private void ThenTheResultIsFalse()
        {
            _result.ShouldBeFalse();
        }

        private bool Header(HttpRequestHeaders headers, string name, string value)
        {
            return headers.Any(h => h.Key == name && h.Value.First() == value);
        }
    }
}
