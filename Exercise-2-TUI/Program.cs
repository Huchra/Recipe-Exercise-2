using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Console;

IConfigurationBuilder builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.Developlment.josn");

var config = builder.Build();

HttpClient client = new();
client.BaseAddress = new Uri(configuration["BaseUrl"]);
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
        .PageSize(10)
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
                    case "List recipes":
                        ListAndDisplayRecipes(await GetRecipesAsync());
                    case "Edit a recipe":
                        var recipeToEdit = EditRecipe(await GetRecipesAsync(), await GetCategoriesAsync());
                    case "Return":
                        return;

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
    recipeList = await client.GetFromJsonAsync<List<Recipe>>("recipes");
    if (recipeList != null)
        return recipeList;

    return new List<Recipe>();
}

async Task PostRecipeAsync(Recipe recipe)
{
    var response = await client.PostAsJsonAsync("recipes", Recipe);
    response.EnsureSuccessStatusCode();
}

async Task DeleteRecipeAsync(Guid id)
{
    var response = await client.DeleteFromJsonAsync("recipes/{id}", Guid);
    response.EnsureSuccessStatusCode();
}

async Task PutRecipeAsync(Recipe recipe)
{
    var response = await client.PutAsJsonAsync("recipes/{recipe.Guid}", Recipe);
    response.EnsureSuccessStatusCode();
}

async Task GetCategoriesAsync()
{
    var response = await client.GetFromJsonAsync("categories", Category);
    response.EnsureSuccessStatusCode();
}

async Task PostCategoriesAsync(string category)
{
    var response = await client.PostAsJsonAsync("categories?category={category}", category);
    response.EnsureSuccessStatusCode();
}

async Task PutCategoriesAsync(string categoryToUpdate, string categoryUpdated)
{
    var response = await client.PutAsJsonAsync("categories/{categoryToUpdate}?categoryUpdated={categoryUpdated}", null);
    response.EnsureSuccessStatusCode();
}

async Task DeleteCategoriesAsync(string category)
{
    var response = await client.DeleteFromJsonAsync("categories/{category}", category);
    response.EnsureSuccessStatusCode();
}