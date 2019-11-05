using System;

namespace ClamAv.Net.Models
{
    public class ScanResult
    {
        public DateTime Date           { get; } = DateTime.UtcNow;
        public bool     IsInfected     { get; set; }
        public string   VirusName      { get; set; }
        public bool?    ScanFailed     { get; set; }
        public string   FailureMessage { get; set; }
    }
}
