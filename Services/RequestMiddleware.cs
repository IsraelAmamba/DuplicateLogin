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

        public async Task Invoke(HttpContext context)
        {
            _loginHelper.CheckSingleLogin(context);

            await _next.Invoke(context);
        }
    }
}
