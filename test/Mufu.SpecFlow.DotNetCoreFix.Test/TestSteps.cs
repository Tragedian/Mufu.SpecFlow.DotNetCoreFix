using TechTalk.SpecFlow;

namespace Mufu.SpecFlow.DotNetCoreFix.Test
{
    [Binding]
    public class TestSteps
    {
        [Given(@"I have entered (.*) into the calculator")]
        public void GivenIHaveEnteredIntoTheCalculator(int p0)
        {
            throw new PendingStepException();
        }

        [When(@"I press add")]
        public void WhenIPressAdd()
        {
            throw new PendingStepException();
        }

        [Then(@"the result should be (.*) on the screen")]
        public void ThenTheResultShouldBeOnTheScreen(int p0)
        {
            throw new PendingStepException();
        }
    }
}
