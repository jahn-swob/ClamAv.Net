using System;

namespace ClamAv.Net.Models
{
    public class PingResult
    {
        public PingResult(bool pong)
        {
            Pong = pong;
        }

        public DateTime Date { get; } = DateTime.UtcNow;
        public bool     Pong { get; set; }
    }
}
