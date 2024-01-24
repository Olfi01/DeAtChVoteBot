namespace DeAtChVoteBot.Database.Types;

public class Option
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required virtual Category Category { get; set; }
}