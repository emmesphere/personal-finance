using PersonalFinance.BuildingBlocks.Results;

using Shouldly;

namespace PersonalFinance.BuildingBlocks.Tests.Results
{
    public sealed class ValidationErrorTests
    {
        [Fact]
        public void FromResults_ShouldCollectOnlyFailures()
        {
            var ok = Result.Success();
            var fail1 = Result.Failure(ResultError.Failure("A", "a"));
            var fail2 = Result.Failure(ResultError.Failure("B", "b"));

            var validation = ValidationError.FromResults(new[] { ok, fail1, fail2 });

            validation.Type.ShouldBe(ErrorType.Validation);
            validation.Errors.Count.ShouldBe(2);
            validation.Errors[0].Code.ShouldBe("A");
            validation.Errors[1].Code.ShouldBe("B");
        }
    }

}
