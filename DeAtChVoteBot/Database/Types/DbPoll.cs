namespace DeAtChVoteBot.Database.Types
{
    public class DbPoll
    {
        public int Id { get; set; }
        public required int MessageId { get; set; }
        public required virtual Category Category { get; set; }
    }
}
