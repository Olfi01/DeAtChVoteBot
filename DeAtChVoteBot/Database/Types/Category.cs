namespace DeAtChVoteBot.Database.Types;

public class Category
{
    public int CategoryId { get; set; }
    public required string Name { get; set; }
    public required virtual List<Option> Options { get; set; }
}