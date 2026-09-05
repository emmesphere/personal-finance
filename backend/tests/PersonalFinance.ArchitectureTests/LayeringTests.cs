using NetArchTest.Rules;

using Shouldly;

namespace PersonalFinance.ArchitectureTests;
public class LayeringTests
{
    [Fact]
    public void Domain_ShouldNotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(typeof(Domain.PersonalFinanceDomainMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("PersonalFinance.Infrastructure")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(result.ToString());
    }

    [Fact]
    public void Application_ShouldNotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(typeof(Application.Abstractions.PersonalFinanceApplicationMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("PersonalFinance.Infrastructure")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(result.ToString());
    }

    [Fact]
    public void Domain_ShouldNotDependOn_WebApi()
    {
        var result = Types.InAssembly(typeof(Domain.PersonalFinanceDomainMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("PersonalFinance.WebApi")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(result.ToString());
    }

    [Fact]
    public void Application_ShouldNotDependOn_WebApi()
    {
        var result = Types.InAssembly(typeof(Application.Abstractions.PersonalFinanceApplicationMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("PersonalFinance.WebApi")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(result.ToString());
    }
}
