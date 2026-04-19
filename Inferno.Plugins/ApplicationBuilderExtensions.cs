using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;

namespace Inferno.Plugins;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Serves static files (images, css, js, ...) that ship with plugins out
    /// of <c>~/Plugins/{plugin}/...</c> under the <c>/Plugins</c> URL prefix,
    /// with a one-week public cache header.
    /// </summary>
    public static IApplicationBuilder UseInfernoPlugins(this IApplicationBuilder app) =>
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Plugins")),
            RequestPath = new PathString("/Plugins"),
            OnPrepareResponse = ctx => ctx.Context.Response.Headers
                .Append(HeaderNames.CacheControl, "public,max-age=604800")
        });
}
