namespace Console
{
    public class Category
    {
        public Dictionary<string, bool> Categories { get; set; }

        public Category(Dictionary<string, bool> categories)
        {
            Categories = categories;
        }

        public bool AddCategory(string category)
        {
            ArgumentNullException.ThrowIfNull(category);

            if (!Categories.ContainsKey(category))
            {
                Categories.Add(category, true);
                return true;
            }
            else return false;
        }

        public bool EditCategory(string oldCategoryName, string newCategoryName)
        {
            ArgumentNullException.ThrowIfNull(oldCategoryName);
            ArgumentNullException.ThrowIfNull(newCategoryName);

            if (!Categories.ContainsKey(oldCategoryName))
                return false;

            Categories.Remove(oldCategoryName);
            Categories.Add(newCategoryName, true);

            return true;
        }
    }
}
