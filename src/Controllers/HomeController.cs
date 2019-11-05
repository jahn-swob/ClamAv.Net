using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClamAv.Net.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using nClam;

using static System.Environment;
using static ClamAv.Net.Controllers.Infrastructure.ActionResultHelpers;

namespace ClamAv.Net.Controllers
{
    [ApiController]
    [Route("")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<PingResult> Get(CancellationToken cancellation)
            => new PingResult(await TryPing(cancellation));

        [HttpPost]
        public async Task<IActionResult> CheckFile(CancellationToken cancellation)
        {
            // Get file from body
            var file = await GetFileBytes(Request.Body, cancellation);

            // Scanning for viruses...
            var scan   = await ClamAvClient.SendAndScanFileAsync(file, cancellation);
            var result = new ScanResult();
            switch (scan.Result)
            {
                case ClamScanResults.VirusDetected:
                    result.IsInfected = true;
                    result.VirusName  = scan.InfectedFiles.First().VirusName;
                    break;
                case ClamScanResults.Error:
                    return ServiceUnavailable(scan.RawResult);
            }

            return Ok(result);
        }

        private async Task<bool> TryPing(CancellationToken cancellation)
        {
            try
            {
                return await ClamAvClient.PingAsync(cancellation);
            }
            catch
            {
                 // Ignoring the exception so that false is returned on the pong
                 // instead of an exception bubbling up
                 return false;
            }
        }

        private async Task<byte[]> GetFileBytes(Stream stream, CancellationToken cancellation)
        {
            // Get the max file size content length
            var maxStreamLength = ClamAvServerMaxFileSizeMegaBytes * 1024;

            // If you can seek, do it the easy way
            if (stream.CanSeek)
            {
                // Check if the length is greater than the maximum
                if (stream.Length > maxStreamLength)
                    return null;

                var result = new byte[stream.Length];
                await stream.ReadAsync(result, cancellation);
                return result;
            }

            // Else do it the long way
            await using var ms = new MemoryStream();
            byte[] buffer = new byte[maxStreamLength];
            int read;
            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, cancellation)) > 0)
            {
                ms.Write(buffer, 0, read);
            }

            return ms.ToArray();
        }

        private ClamClient ClamAvClient
            => new ClamClient(ClamAvServer, ClamAvServerPort);

        private string ClamAvServer
            => GetEnvironmentVariable("CLAMD_SERVER");

        private int ClamAvServerPort
            => int.TryParse(
                   GetEnvironmentVariable("CLAMD_SERVER_PORT"),
                   NumberStyles.Integer,
                   CultureInfo.InvariantCulture,
                   out var port)
                ? port
                : 0;

        private int ClamAvServerMaxFileSizeMegaBytes
            => int.TryParse(
                   GetEnvironmentVariable("CLAMD_SERVER_MAX_FILESIZE_MB"),
                   NumberStyles.Integer,
                   CultureInfo.InvariantCulture,
                   out var mbs)
                ? mbs
                : 16;
    }
}
