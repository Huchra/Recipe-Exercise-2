namespace Console
{
    public class Recipe
    {
        public Guid Guid { get; }
        public string Title { get; set; }
        public List<string> Ingredients { get; set; }
        public List<string> Instructions { get; set; }
        public List<string> Categories { get; set; }

        public Recipe(string title, List<string> ingredients, List<string> instructions, List<string> categories)
        {
            Guid = Guid.NewGuid();
            Title = title;
            Ingredients = ingredients;
            Instructions = instructions;
            Categories = categories;
        }
        // Editing functionalities for recipes.
        public void EditRecipeCategory(List<string> editedCategories)
        {
            ArgumentNullException.ThrowIfNull(editedCategories);

            Categories = editedCategories;
        }

        public void EditRecipeTitle(string editedTitle)
        {
            ArgumentNullException.ThrowIfNull(editedTitle);

            Title = editedTitle;
        }

        public void EditRecipeInstructions(List<string> editedInstructions)
        {
            ArgumentNullException.ThrowIfNull(editedInstructions);

            Instructions = editedInstructions;
        }

        public void EditRecipeIngredients(List<string> editedIngredients)
        {
            ArgumentNullException.ThrowIfNull(editedIngredients);

            Ingredients = editedIngredients;
        }

        // Adding functionalities for existing recipes.
        public void AddRecipeInstructions(List<string> addedInstructions)
        {
            ArgumentNullException.ThrowIfNull(addedInstructions);

            foreach (string instruction in addedInstructions)
                Instructions.Add(instruction);
        }

        public void AddIngredients(List<string> addedIngredients)
        {
            ArgumentNullException.ThrowIfNull(addedIngredients);

            foreach (string ingredient in addedIngredients)
                Ingredients.Add(ingredient);
        }

        public void AddRecipeCategory(List<string> categories)
        {
            ArgumentNullException.ThrowIfNull(categories);

            foreach (string category in categories)
                Categories.Add(category);
        }
    }
}
