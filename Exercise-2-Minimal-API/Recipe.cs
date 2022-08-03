public class Recipe
{
    public Guid Guid { get; set; }
    public string Title { get; set; }
    public List<string> Ingredients { get; set; }
    public List<string> Instructions { get; set; }
    public List<string> Categories { get; set; }
    public Recipe(Guid guid, string title, List<string> ingredients, List<string> instructions, List<string> categories)
    {
        Guid = guid;
        Title = title;
        Ingredients = ingredients;
        Instructions = instructions;
        Categories = categories;
    }
}
