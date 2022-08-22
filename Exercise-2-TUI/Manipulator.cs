using System.Text.Json;
using System.Text.Json.Serialization;

namespace Console
{
    // This class is for containg recipes and cateogories as well serializing and deserializing them.
    public class Manipulator
    {
        public List<Recipe> Recipes { get; set; } = new List<Recipe>();
        public Category Categories { get; set; } = new Category(new Dictionary<string, bool>());
        [JsonIgnore]
        private readonly string _recipePath;
        private readonly string _categoryPath;

        public Manipulator()
        {
            _recipePath = "Recipe.json";
            _categoryPath = "Category.json";

            if (File.Exists(_categoryPath))
            {
                using (var read = new StreamReader(_categoryPath))
                {
                    string file = read.ReadToEnd();
                    var jsonFile = JsonSerializer.Deserialize<Category>(file);

                    ArgumentNullException.ThrowIfNull(jsonFile);

                    Categories = jsonFile;
                }
            }

            if (File.Exists(_recipePath))
            {
                using (var read = new StreamReader(_recipePath))
                {
                    string file = read.ReadToEnd();
                    var jsonFile = JsonSerializer.Deserialize<List<Recipe>>(file);

                    ArgumentNullException.ThrowIfNull(jsonFile);

                    Recipes = jsonFile;
                }
            }
        }

        public void AddRecipe(Recipe recipe)
        {
            ArgumentNullException.ThrowIfNull(recipe);
            Recipes.Add(recipe);
        }

        public void RecipeSeralize()
        {
            string jsonString = JsonSerializer.Serialize(Recipes);
            File.WriteAllText(_recipePath, jsonString);
        }

        public void CategorySerialize()
        {
            string jsonString = JsonSerializer.Serialize(Categories);
            File.WriteAllText(_categoryPath, jsonString);
        }
    }
}
