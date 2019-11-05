using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClamAv.Net.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using nClam;

namespace ClamAv.Net.Controllers
{
    [ApiController]
    [Route("")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<PingResult> Get(CancellationToken cancellation)
            => new PingResult(await TryPing(cancellation));

        [HttpPost]
        public async Task<ScanResult> CheckFile(CancellationToken cancellation)
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
                    result.ScanFailed     = true;
                    result.FailureMessage = scan.RawResult;
                    break;
            }

            return result;
        }

        private async Task<bool> TryPing(CancellationToken cancellation)
        {
            try
            {
                return await ClamAvClient.PingAsync(cancellation);
            }
            catch { /* ignored */ }

            return false;
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
            => Environment.GetEnvironmentVariable("CLAMD_SERVER");

        private int ClamAvServerPort
            => int.TryParse(Environment.GetEnvironmentVariable("CLAMD_SERVER_PORT"), out var port)
                ? port
                : 0;

        private int ClamAvServerMaxFileSizeMegaBytes
            => int.TryParse(Environment.GetEnvironmentVariable("CLAMD_SERVER_MAX_FILESIZE_MB"), out var mbs)
                ? mbs
                : 0;
    }
}
