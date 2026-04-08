using Microsoft.AspNetCore.Mvc;
using SIRU.Core.Domain.Common.Enums;
using SIRU.Core.Domain.Common.Results;

namespace SIRU.Presentation.Api.Handlers
{
    public static class ResultHandler
    {
        public static IActionResult Handle<T>(this Result<T> result, string path, Func<T, IActionResult> onSuccess)
        {
            if (!result.IsSuccess)
            {
                return result.ErrorTypeCode switch
                {
                    ErrorType.NotFound => new NotFoundObjectResult(new ProblemDetails
                    {
                        Title = "Not Found",
                        Detail = string.Join(", ", result.Errors),
                        Status = StatusCodes.Status404NotFound,
                        Instance = path
                    }),

                    ErrorType.BadRequest => new BadRequestObjectResult(new ProblemDetails
                    {
                        Title = "Bad Request",
                        Detail = string.Join(", ", result.Errors),
                        Status = StatusCodes.Status400BadRequest,
                        Instance = path
                    }),

                    ErrorType.Unauthorized => new BadRequestObjectResult(new ProblemDetails
                    {
                        Title = "Unauthorized",
                        Detail = string.Join(", ", result.Errors),
                        Status = StatusCodes.Status401Unauthorized,
                        Instance = path
                    }),

                    ErrorType.Forbidden => new ObjectResult(new ProblemDetails
                    {
                        Title = "Forbidden",
                        Detail = string.Join(", ", result.Errors),
                        Status = StatusCodes.Status403Forbidden,
                        Instance = path
                    })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    },

                    ErrorType.Locked => new ObjectResult(new ProblemDetails
                    {
                        Title = "Locked",
                        Detail = string.Join(", ", result.Errors),
                        Status = StatusCodes.Status423Locked,
                        Instance = path
                    }){
                        StatusCode = StatusCodes.Status423Locked
                    }
                };
            }

            return onSuccess(result.Value!);
        }

        public static IActionResult Handle(this Result result, string path, Func<IActionResult> onSuccess)
        {
            if (!result.IsSuccess)
            {
                return result.ErrorTypeCode switch
                {
                    ErrorType.NotFound => new NotFoundObjectResult(new ProblemDetails
                    {
                        Title = "Not Found",
                        Detail = string.Join(", ", result.Errors),
                        Status = StatusCodes.Status404NotFound,
                        Instance = path
                    }),

                    ErrorType.BadRequest => new BadRequestObjectResult(new ProblemDetails
                    {
                        Title = "Bad Request",
                        Detail = string.Join(", ", result.Errors),
                        Status = StatusCodes.Status400BadRequest,
                        Instance = path
                    }),

                    ErrorType.Unauthorized => new BadRequestObjectResult(new ProblemDetails
                    {
                        Title = "Unauthorized",
                        Detail = string.Join(", ", result.Errors),
                        Status = StatusCodes.Status401Unauthorized,
                        Instance = path
                    }),

                    ErrorType.Forbidden => new ObjectResult(new ProblemDetails
                    {
                        Title = "Forbidden",
                        Detail = string.Join(", ", result.Errors),
                        Status = StatusCodes.Status403Forbidden,
                        Instance = path
                    })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    },

                    ErrorType.Locked => new ObjectResult(new ProblemDetails
                    {
                        Title = "Locked",
                        Detail = string.Join(", ", result.Errors),
                        Status = StatusCodes.Status423Locked,
                        Instance = path
                    })
                    {
                        StatusCode = StatusCodes.Status423Locked
                    }
                };
            }

            return onSuccess();
        }
    }
}
