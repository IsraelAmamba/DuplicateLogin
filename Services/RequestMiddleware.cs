using DuplicateLogin.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuplicateLogin.Services
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestHeaderMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestHeaderMiddleware>();
        }
    }

    public class RequestHeaderMiddleware
    {
        private readonly RequestDelegate _next;
        LoginHelper _loginHelper;
        public RequestHeaderMiddleware(RequestDelegate next, LoginHelper loginHelper)
        {
            _next = next;
            _loginHelper = loginHelper;
        }

        /*
         * A middleware is used because each http request that passes through the Session Middleware resets the session time
         * https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-3.1
         */
        public async Task Invoke(HttpContext context)
        {
            //The CheckSingleLogin called in here would refresh the user time in the memory cache provided the user is logged in
            _loginHelper.CheckSingleLogin(context);

            await _next.Invoke(context);
        }
    }
}
