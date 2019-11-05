using System;

namespace ClamAv.Net.Models
{
    public class ScanResult : Result
    {
        public bool   IsInfected { get; set; }
        public string VirusName  { get; set; }
    }
}
