using PersonalFinance.BuildingBlocks.Results;

namespace PersonalFinance.WebApi.Endpoints;

internal static class Common
{
    public static IResult ToHttp(this Result result)
    {
        if (result.IsSuccess) return Results.Ok();

        var status = result.Error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Problem => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Problem(
            title: result.Error.Code,
            detail: result.Error.Description,
            statusCode: status);
    }
}
