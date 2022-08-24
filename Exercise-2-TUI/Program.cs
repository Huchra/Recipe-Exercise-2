using Spectre.Console;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Console;
using static Console.UI_Helper;

IConfigurationBuilder builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.Development.json");

var config = builder.Build();

HttpClient client = new();
client.BaseAddress = new Uri(config["BaseUrl"]);
client.DefaultRequestHeaders.Accept.Clear();
client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

while (true)
{
    AnsiConsole.Clear();

    var menuTable = new Table();

    menuTable.AddColumn("[underline][red]Welcome to The Recipe Console[/][/]!")
        .Centered();

    AnsiConsole.Write(menuTable);

    var recCatChoice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
        .Title("[yellow]How can we serve you today?[/]")
        .PageSize(3)
        .AddChoices(new[] { "Categories", "Recipes", "Exit" }
        ));

    switch (recCatChoice)
    {
        case "Exit":
            Environment.Exit(0);
            break;
        case "Recipes":
            try
            {
                var recipeChoice = RecipeChoices(await GetRecipesAsync(), await GetCategoriesAsync());

                switch (recipeChoice)
                {
                    case "Add a recipe":
                        var recipeToAdd = AddRecipe(await GetRecipesAsync(), await GetCategoriesAsync());

                        if (recipeToAdd is not null)
                            await PostRecipeAsync(recipeToAdd);

                        break;
                    case "List recipes":
                        ListAndDisplayRecipes(await GetRecipesAsync());
                        break;
                    case "Edit a recipe":
                        var recipeToEdit = EditRecipe(await GetRecipesAsync(), await GetCategoriesAsync());
                        if (recipeToEdit is not null)
                            await PutRecipeAsync(recipeToEdit);
                        break;
                    case "Return":
                        break;

                }
            }
            catch
            {
                AnsiConsole.MarkupLine("error occured");
            }

            break;
        case "Categories":
            try
            {
                var categoryChoice = CategoryChoices();
                switch (categoryChoice)
                {
                    case "Edit":
                        var editedCategory = EditCategory(await GetCategoriesAsync());

                        if (editedCategory is not null)
                            await PutCategoriesAsync(editedCategory[0], editedCategory[1]);

                        break;
                    case "Add":
                        var addedCategory = AddCategory(await GetCategoriesAsync());

                        if (addedCategory is not null)
                            await PostCategoriesAsync(addedCategory);

                        break;
                    case "Return":
                        break;
                }
            }
            catch
            {
                AnsiConsole.MarkupLine("error occured");
            }
            break;
    }
}



async Task<List<Recipe>> GetRecipesAsync()
{
    var recipeList = await client.GetFromJsonAsync<List<Recipe>>("recipes");
    if (recipeList != null)
        return recipeList;

    return new List<Recipe>();
}

async Task PostRecipeAsync(Recipe recipe)
{
    var response = await client.PostAsJsonAsync("recipes", recipe);
    response.EnsureSuccessStatusCode();
}

async Task DeleteRecipeAsync(Guid id)
{
    var response = await client.DeleteAsync($"recipes/{id}");
    response.EnsureSuccessStatusCode();
}

async Task PutRecipeAsync(Recipe recipe)
{
    var response = await client.PutAsJsonAsync("recipes/{recipe.Guid}", recipe);
    response.EnsureSuccessStatusCode();
}

async Task<Category> GetCategoriesAsync()
{
    var response = await client.GetFromJsonAsync<Category>("categories");

    if (response == null)
        return new Category(new Dictionary<string, bool>());

    return response;
}

async Task PostCategoriesAsync(string category)
{
    var response = await client.PostAsJsonAsync("categories?category={category}", category);
    response.EnsureSuccessStatusCode();
}

async Task PutCategoriesAsync(string categoryToUpdate, string categoryUpdated)
{
    var response = await client.PutAsync($"categories/{categoryToUpdate}?categoryUpdated={categoryUpdated}", null);
    response.EnsureSuccessStatusCode();
}

async Task DeleteCategoriesAsync(string category)
{
    var response = await client.DeleteAsync($"categories/{category}");
    response.EnsureSuccessStatusCode();
}