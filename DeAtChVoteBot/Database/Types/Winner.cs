namespace DeAtChVoteBot.Database.Types
{
    public class Winner
    {
        public int Id { get; set; }
        public required virtual Option Option { get; set; }
    }
}
