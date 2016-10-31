namespace Cake.Pact.AcceptanceTests
{
    using Cake.Core;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Shouldly;
    using TestStack.BDDfy;
    using Xunit;

    public class AcceptanceSpecs
    {
        private readonly string _server;

        private ICakeContext _context;

        private string _pact;

        private JObject _jobjectPact;

        private string _version;

        private bool _publishResult;

        public AcceptanceSpecs()
        {
            _server = "http://192.168.99.100:8070";
        }

        [Fact]
        public void CreateingPactFromStringWhenItDoesntExist()
        {
            this.Given(_ => _.GivenACakeContext())
                .And(_ => _.GivenWeHaveAValidStringPact())
                .And(_ => _.GivenWeHaveAValidVersion())
                .When(_ => _.WhenWePublishThePact())
                .Then(_ => _.ThenTheResultSuccessful())
                .BDDfy();
        }

        [Fact]
        public void CreatingPactFromJObjectWhenItDoesntExist()
        {
            this.Given(_ => _.GivenACakeContext())
                .And(_ => _.GivenWeHaveAValidJObjectPact())
                .And(_ => _.GivenWeHaveAValidVersion())
                .When(_ => _.WhenWePublishTheJObjectPact())
                .Then(_ => _.ThenTheResultSuccessful())
                .BDDfy();
        }

        private void GivenACakeContext()
        {
            _context = null;
        }

        private void GivenWeHaveAValidStringPact()
        {
            _pact = TestData.ResourceLoader.Load("MyPact.json");
        }

        private void GivenWeHaveAValidJObjectPact()
        {
            var pact = TestData.ResourceLoader.Load("MyPact.json");
            _jobjectPact = JsonConvert.DeserializeObject<JObject>(pact);
        }

        private void GivenWeHaveAValidVersion()
        {
            _version = "1.2.3.5";
        }

        private void WhenWePublishThePact()
        {
            _publishResult = PactAliases.PactPublishToBroker(_context, _server, _pact, _version);
        }

        private void WhenWePublishTheJObjectPact()
        {
            _publishResult = PactAliases.PactPublishToBroker(_context, _server, _jobjectPact, _version);
        }

        private void ThenTheResultSuccessful()
        {
            _publishResult.ShouldBe(true);
        }
    }
}
