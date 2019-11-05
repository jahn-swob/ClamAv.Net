using Microsoft.AspNetCore.Mvc;

using static Microsoft.AspNetCore.Http.StatusCodes;

namespace ClamAv.Net.Controllers.Infrastructure
{
    internal static class ActionResultHelpers
    {
        /// <summary>
        ///   Creates an <see cref="ObjectResult"/> that produces a
        ///   <see cref="Status503ServiceUnavailable"/> response.
        /// </summary>
        public static ObjectResult ServiceUnavailable(string details = null)
        {
            return new ObjectResult(new ProblemDetails
            {
                Type   = "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/503",
                Title  = "Service Unavailable",
                Detail = details ??
                    "The server is not ready to handle the request. "                   +
                    "Common causes are when server is waiting for third party "         +
                    "dependencies to start up, the server is down for maintenance, "    +
                    "or that the server is overloaded. Attempting the operation again " +
                    "after waiting momentarily may result in a successful request."
            })
            { StatusCode = Status503ServiceUnavailable };
        }
    }
}
