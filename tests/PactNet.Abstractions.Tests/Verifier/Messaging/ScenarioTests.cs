using System;
using FluentAssertions;
using PactNet.Verifier.Messaging;
using Xunit;

namespace PactNet.Abstractions.Tests.Verifier.Messaging
{
    public class ScenarioTests
    {
        [Fact]
        public void InvokeScenario_Should_Invoke_Scenario_And_Return_Object()
        {
            object expected = new { field = "value" };
            var scenario = new Scenario("a scenario", () => expected);

            object actual = scenario.Invoke();

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Should_Be_Able_To_Get_Description_And_Metadata()
        {
            object expectedMetadata = new { key = "vvv" };
            var expectedDescription = "a scenario";
            var scenario = new Scenario(expectedDescription, () => string.Empty, expectedMetadata, null);

            Assert.Equal(expectedMetadata, scenario.Metadata);
            Assert.Equal(expectedDescription, scenario.Description);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Ctor_Should_Fail_If_Invalid_Description(string description)
        {
            object expected = new { field = "value" };
            object expectedMetadata = new { key = "vvv" };

            Action actual = () => new Scenario(description, () => expected, expectedMetadata, null);

            actual.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Ctor_Should_Fail_If_Invalid_Invoker()
        {
            object expectedMetadata = new { key = "vvv" };

            Action actual = () => new Scenario("description", null, expectedMetadata, null);

            actual.Should().Throw<ArgumentException>();
        }
    }
}
