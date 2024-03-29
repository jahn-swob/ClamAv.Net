﻿using System;
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
        public async Task<PingResult> GetAsync(CancellationToken cancellation)
            => new(await TryPingAsync(cancellation));

        [HttpPost]
        public async Task<IActionResult> CheckFileAsync(CancellationToken cancellation)
        {
            // Get file from body
            var file = await GetFileBytesAsync(Request.Body, cancellation);

            // Scanning for viruses...
            var scan   = await CreateClamClient.SendAndScanFileAsync(file, cancellation);
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

        private async Task<bool> TryPingAsync(CancellationToken cancellation)
        {
            try
            {
                return await CreateClamClient.PingAsync(cancellation);
            }
            catch
            {
                 // Ignoring the exception so that false is returned on the pong
                 // instead of an exception bubbling up
                 return false;
            }
        }

        private async Task<byte[]?> GetFileBytesAsync(Stream stream, CancellationToken cancellation)
        {
            // TODO: Do not buffer entire file in memory; use Stream instead.

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
                await ms.WriteAsync(buffer, 0, read, cancellation);
            }

            return ms.ToArray();
        }

        // TODO: When it's time for tests, inject an IClamClient factory.
        // TODO: Is ClamClient thread-safe?  If so, could just inject a singleton IClamClient.
        private IClamClient CreateClamClient
            => new ClamClient(ClamAvServer, ClamAvServerPort);

        // TODO: Use ASP.NET Core's configuration system
        private string? ClamAvServer
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
