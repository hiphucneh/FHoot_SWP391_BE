﻿using System.Net;

namespace Kahoot.API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
                _logger.LogError(ex, "An unhandled exception occurred");

                if (!context.Response.HasStarted)
                {
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = 500;

                    var response = new
                    {
                        Code = 500,
                        Message = ex.Message,
                        Detailed = ex.StackTrace
                    };

                    var responseText = System.Text.Json.JsonSerializer.Serialize(response);

                    await context.Response.WriteAsync(responseText);
                }
                else
                {
                    _logger.LogWarning("Response has already started, unable to write error response.");
                }
            }

            if (!context.Response.HasStarted && (context.Response.StatusCode == 401 || context.Response.StatusCode == 403))
            {
                var statusCode = context.Response.StatusCode;
                var message = statusCode == 401 ? "Unauthorized access" : "Forbidden access";

                _logger.LogError($"Status code: {statusCode}");

                context.Response.ContentType = "application/json";

                var response = new
                {
                    Code = statusCode,
                    Message = message,
                };

                var responseText = System.Text.Json.JsonSerializer.Serialize(response);

                await context.Response.WriteAsync(responseText);
            }
        }
    }
}