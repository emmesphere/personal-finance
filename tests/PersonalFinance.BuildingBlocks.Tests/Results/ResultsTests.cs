using PersonalFinance.BuildingBlocks.Results;

using Shouldly;

namespace PersonalFinance.BuildingBlocks.Tests.Results
{
    public sealed class ResultsTests
    {
        [Fact]
        public void Success_ShouldHaveNoError()
        {
            var result = Result.Success();

            result.IsSuccess.ShouldBeTrue();
            result.IsFailure.ShouldBeFalse();
            result.Error.ShouldBe(ResultError.None);
        }

        [Fact]
        public void Failure_ShouldHaveError()
        {
            var error = ResultError.Failure("X", "Boom");
            var result = Result.Failure(error);

            result.IsSuccess.ShouldBeFalse();
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe(error);
        }

        [Fact]
        public void GenericSuccess_ShouldExposeValue()
        {
            var result = Result.Success(123);

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldBe(123);
        }

        [Fact]
        public void GenericFailure_ShouldThrow_WhenAccessingValue()
        {
            var result = Result.Failure<int>(ResultError.Failure("X", "Boom"));

            result.IsFailure.ShouldBeTrue();
            Should.Throw<InvalidOperationException>(() => _ = result.Value);
        }

        [Fact]
        public void ImplicitOperator_ShouldCreateFailure_WhenValueIsNull()
        {
            string? value = null;

            Result<string> result = value;

            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe(ResultError.NullValue);
        }

    }

}
