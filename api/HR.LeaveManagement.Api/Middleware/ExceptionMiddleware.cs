using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using HR.LeaveManagement.Application.Exceptions;
using System.Net;
using HR.LeaveManagement.Api.Models;


namespace HR.LeaveManagement.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            CustomValidationProblemDetails error = new();

            switch (ex)
            {
                case BadRequestException badRequestException:
                    statusCode = HttpStatusCode.BadRequest;
                    error = new CustomValidationProblemDetails
                    {
                        Title = badRequestException.Message,
                        Status = (int)statusCode,
                        Detail = badRequestException.InnerException?.Message,
                        Type = nameof(BadRequestException),
                        Errors = badRequestException.ValidationErrors
                    };
                    _logger.LogWarning(ex, "Bad Request Exception: {Message}", badRequestException.Message);
                    break;
                case NotFoundException notFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    error = new CustomValidationProblemDetails
                    {
                        Title = notFoundException.Message,
                        Status = (int)statusCode,
                        Detail = notFoundException.InnerException?.Message,
                        Type = nameof(NotFoundException), 
                    };
                    _logger.LogWarning(ex, "NotFoundException: {Message}", notFoundException.Message);
                    break;
                case UnauthorizedException unauthorizedException:
                    statusCode = HttpStatusCode.Unauthorized;  // 401
                    error = new CustomValidationProblemDetails
                    {
                        Title = unauthorizedException.Message,
                        Status = (int)statusCode,
                        Type = nameof(UnauthorizedException),
                    };
                    _logger.LogWarning(unauthorizedException, "Unauthorized: {Message}", unauthorizedException.Message);
                    break;
                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    error = new CustomValidationProblemDetails
                    {
                        Title = ex.Message,
                        Status = (int)statusCode,
                        Detail = ex.StackTrace,
                        Type = nameof(HttpStatusCode.InternalServerError),
                    };
                    _logger.LogWarning(ex, "Exception: {Message}", ex.Message);
                    break;
            }

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(error);
        }
    }
}