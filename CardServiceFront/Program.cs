using System.Net;
using System.Net.Security;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);
var backend = new Uri(builder.Configuration["Backend:BaseUrl"] ?? "http://localhost:5143");
if (backend.Scheme is not ("http" or "https") || !string.IsNullOrEmpty(backend.UserInfo))
    throw new InvalidOperationException("Backend:BaseUrl debe ser una URL HTTP(S) sin credenciales.");
builder.Services.AddHttpClient("backend", client =>
{
    client.BaseAddress = backend;
    client.Timeout = TimeSpan.FromSeconds(30);
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new SocketsHttpHandler { UseCookies = false, AllowAutoRedirect = false };
    if (builder.Environment.IsDevelopment() && backend.IsLoopback)
        handler.SslOptions.RemoteCertificateValidationCallback = (_, _, _, errors) =>
            errors is SslPolicyErrors.None or SslPolicyErrors.RemoteCertificateChainErrors;
    return handler;
});
var app = builder.Build();
app.UseExceptionHandler(handler => handler.Run(async context =>
{
    context.Response.StatusCode = 502;
    await context.Response.WriteAsJsonAsync(new
    {
        status = 502, title = "No pudimos conectar con el backend",
        detail = "Inicia CardService.Api y comprueba la conexión a PostgreSQL."
    });
}));
app.Map("/api/{**path}", async (HttpContext context, IHttpClientFactory factory) =>
{
    var destination = new Uri(backend, context.Request.Path + context.Request.QueryString);
    using var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), destination);
    if (context.Request.ContentLength > 0 || context.Request.Headers.ContainsKey("Transfer-Encoding"))
        request.Content = new StreamContent(context.Request.Body);
    var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Host", "Connection", "Keep-Alive", "Transfer-Encoding", "TE", "Trailer", "Upgrade",
        "Proxy-Authorization", "Proxy-Authenticate", "X-Forwarded-For", "X-Forwarded-Proto", "X-Forwarded-Host"
    };
    foreach (var item in context.Request.Headers)
    {
        if (excluded.Contains(item.Key)) continue;
        if (!request.Headers.TryAddWithoutValidation(item.Key, item.Value.ToArray()))
            request.Content?.Headers.TryAddWithoutValidation(item.Key, item.Value.ToArray());
    }
    request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", backend.Scheme);
    request.Headers.TryAddWithoutValidation("X-Forwarded-For", context.Connection.RemoteIpAddress?.ToString());
    using var response = await factory.CreateClient("backend").SendAsync(
        request, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
    context.Response.StatusCode = (int)response.StatusCode;
    foreach (var item in response.Headers.Concat(response.Content.Headers))
        if (!excluded.Contains(item.Key)) context.Response.Headers[item.Key] = item.Value.ToArray();
    if (response.Headers.Location is { IsAbsoluteUri: true } location && location.Authority == backend.Authority)
        context.Response.Headers.Location = $"{context.Request.Scheme}://{context.Request.Host}{location.PathAndQuery}";
    context.Response.Headers.Remove("transfer-encoding");
    await response.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
});
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");
app.Run();
