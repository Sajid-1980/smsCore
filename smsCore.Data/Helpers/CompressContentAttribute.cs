﻿using Microsoft.AspNetCore.Mvc.Filters;
/// <summary>
///     Attribute that can be added to controller methods to force content
///     to be GZip encoded if the client supports it
/// </summary>
public class CompressContentAttribute : IAsyncActionFilter //ActionFilterAttribute
{
 

    /// <summary>
    ///     Determines if GZip is supported
    /// </summary>
    /// <returns></returns>
    public bool IsGZipSupported()
    {
        //var AcceptEncoding = HttpContext.Current.Request.Headers["Accept-Encoding"];
        //if (!string.IsNullOrEmpty(AcceptEncoding) &&
        //    (AcceptEncoding.Contains("gzip") || AcceptEncoding.Contains("deflate")))
        //    return true;
        return false;
    }

    /// <summary>
    ///     Sets up the current page or handler to use GZip through a Response.Filter
    ///     IMPORTANT:
    ///     You have to call this method before any output is generated!
    /// </summary>
    public static void GZipEncodePage()
    {
#if !DEBUG
        //var Response = HttpContext.Current.Response;

        //if (IsGZipSupported())
        //{
        //    var AcceptEncoding = HttpContext.Current.Request.Headers["Accept-Encoding"];

        //    if (AcceptEncoding.Contains("gzip"))
        //    {
        //        Response.Filter = new GZipStream(Response.Filter,
        //            CompressionMode.Compress);
        //        Response.Headers.Remove("Content-Encoding");
        //        Response.AppendHeader("Content-Encoding", "gzip");
        //    }
        //    else
        //    {
        //        Response.Filter = new DeflateStream(Response.Filter,
        //            CompressionMode.Compress);
        //        Response.Headers.Remove("Content-Encoding");
        //        Response.AppendHeader("Content-Encoding", "deflate");
        //    }
        //}

        //// Allow proxy servers to cache encoded and unencoded versions separately
        //Response.AppendHeader("Vary", "Content-Encoding");
#endif
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        GZipEncodePage();
    }
}