namespace DeAtChVoteBot.Database.Types;

public class Option
{
    public int OptionId { get; set; }
    public required string Name { get; set; }
    public required virtual Category Category { get; set; }
}