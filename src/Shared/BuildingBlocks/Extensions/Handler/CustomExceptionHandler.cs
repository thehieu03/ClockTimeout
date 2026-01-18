using Common.Configurations;
using Common.Constants;
using Common.Models;
using Common.Models.Reponses;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Extensions.Handler;

public sealed class CustomExceptionHandler(ILogger<CustomExceptionHandler> logger, IConfiguration cfg) : IExceptionHandler
{

    #region Implementations

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var includeInnerEx = cfg.GetValue<bool>($"{AppConfigCfg.Section}:{AppConfigCfg.IncludeInnerException}");
        var includeStackTrace = cfg.GetValue<bool>($"{AppConfigCfg.Section}:{AppConfigCfg.IncludeExceptionStackTrace}");

        (string ErrorMessage, int StatusCode, string? Details, string InnerException) details = exception switch
        {
            ValidationException => (
                exception.Message,
                StatusCodes.Status400BadRequest,
                MessageCode.BadRequest,
                includeInnerEx ? exception.GetType().Name : string.Empty
            ),
            ClientValidationException => (
                exception.Message,
                StatusCodes.Status400BadRequest,
                MessageCode.BadRequest,
                includeInnerEx ? exception.GetType().Name : string.Empty
            ),
            NotFoundException => (
                exception.Message,
                StatusCodes.Status404NotFound,
                MessageCode.NotFound,
                includeInnerEx ? exception.GetType().Name : string.Empty
            ),
            UnauthorizedException => (
                exception.Message,
                StatusCodes.Status401Unauthorized,
                MessageCode.Unauthorized,
                includeInnerEx ? exception.GetType().Name : string.Empty
            ),
            NoPermissionException => (
                exception.Message,
                StatusCodes.Status401Unauthorized,
                MessageCode.AccessDenied,
                includeInnerEx ? exception.GetType().Name : string.Empty
            ),
            _ => (
                includeInnerEx ? exception.Message : MessageCode.UnknownError,
                StatusCodes.Status500InternalServerError,
                includeStackTrace ? exception.StackTrace : null,
                includeInnerEx ? exception.InnerException?.Message ?? string.Empty : string.Empty
            )
        };

        context.Response.StatusCode = details.StatusCode;

        var errors = new List<ErrorResult>();

        if (exception is FluentValidation.ValidationException validationException)
        {
            foreach (var error in validationException.Errors)
            {
                errors.Add(new ErrorResult(error.ErrorMessage, error.PropertyName));
            }
        }
        else if (exception is ClientValidationException badRequestException)
        {
            errors.Add(new ErrorResult(badRequestException.Message, badRequestException.Details!));
        }
        else if (exception is NotFoundException notFoundException)
        {
            errors.Add(new ErrorResult(notFoundException.Message, notFoundException.Details!));
        }
        else
        {
            errors.Add(new ErrorResult(details.ErrorMessage, details.InnerException));
        }

        var response = ResultShareResponse<object>.Failure(
            statusCode: details.StatusCode,
            instance: context.Request.Path,
            errors: errors,
            message: details.Details);

        if (details.StatusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError("Error Message: {exceptionMessage}, Time of occurrence {time}", exception.Message, DateTime.UtcNow);
        }
        else
        {
            logger.LogWarning("Message: {exceptionMessage}, Time of occurrence {time}", exception.Message, DateTime.UtcNow);
        }

        await context.Response.WriteAsJsonAsync(response, cancellationToken: cancellationToken);

        return true;
    }

    #endregion

}