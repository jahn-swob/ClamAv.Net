using System;

namespace ClamAv.Net.Models
{
    public abstract class Result
    {
        public DateTime Date { get; } = DateTime.UtcNow;
    }
}
