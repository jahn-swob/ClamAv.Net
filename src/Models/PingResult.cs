namespace ClamAv.Net.Models
{
    public class PingResult : Result
    {
        public PingResult(bool pong)
        {
            Pong = pong;
        }

        public bool Pong { get; }
    }
}
