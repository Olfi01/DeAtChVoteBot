
namespace DeAtChVoteBot.Database.Types;

public class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required bool ExcludeLastWinner { get; set; }
    public virtual List<Option> Options { get; set; }

    public Category()
    {
        Options = [];
    }

    public override bool Equals(object? obj)
    {
        return obj is Category category &&
               Id == category.Id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }
}