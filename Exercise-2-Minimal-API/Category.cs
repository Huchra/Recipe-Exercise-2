public class Category
{
    public Dictionary<string, bool> Categories { get; set; }

    public Category(Dictionary<string, bool> categories)
    {
        Categories = categories;
    }
}
