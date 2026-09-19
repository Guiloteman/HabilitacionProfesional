using Microsoft.AspNetCore.Http;

namespace Services.Client
{
    public class CookieHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookieHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var cookieName = ".AspNetCore.Identity.Application";
                if (httpContext.Request.Cookies.TryGetValue(cookieName, out var cookieValue))
                {
                    request.Headers.Add("Cookie", $"{cookieName}={cookieValue}");
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
