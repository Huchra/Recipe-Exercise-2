using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
       
    });
});

var app = builder.Build();
app.UseCors();
var port = Environment.GetEnvironmentVariable("PORT") ?? "3000";


var recipePath = "Recipe.json";
var categoryPath = "Category.json";
var recipeList = new List<Recipe>();
var categories = new Category(new Dictionary<string, bool>());

app.UseSwagger();
app.UseSwaggerUI();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

ReadOnAwake();

app.MapGet("/recipes", () =>
{
    return Results.Ok(recipeList);
});

app.MapGet("/recipes/{id}", (Guid id) =>
{
    if (recipeList.Find(recipe => recipe.Guid == id) is Recipe recipe)
        return Results.Ok(recipe);

    return Results.NotFound();
});

app.MapPost("/recipes", async (Recipe recipeToPost) =>
{
    recipeList.Add(recipeToPost);
    await SaveRecipes();
    return Results.Ok();
});

app.MapDelete("/recipes/{id}", async (Guid id) =>
{
    if (recipeList.Find(recipe => recipe.Guid == id) is Recipe recipe)
    {
        recipeList.Remove(recipe);
        await SaveRecipes();
        return Results.Created("/recipes/{id}", recipe);
    }
    return Results.NotFound();
});

app.MapPut("/recipes/{id}", async (Recipe recipeUpdated) =>
{
    if (recipeList.Find(recipe => recipe.Guid.Equals(recipeUpdated.Guid) ) is Recipe recipe)
    {
        recipeList.Remove(recipe);
        recipeList.Add(recipeUpdated);
        await SaveRecipes();
        return Results.Ok(recipeList);
    }
    return Results.NotFound();
});

app.MapGet("/categories", () =>
{
    return Results.Ok(categories);
});

app.MapPost("/categories", async ([FromBody] string categoryToPost) =>
{
    if (string.IsNullOrEmpty(categoryToPost))
        return Results.BadRequest();

    if (categories.Categories.ContainsKey(categoryToPost))
        return Results.StatusCode(303);

    categories.Categories.Add(categoryToPost, true);
    await SaveCategories();
    return Results.Ok();
});

app.MapPut("/categories", async (string categoryToUpdate, string categoryUpdated) =>
{
    if (string.IsNullOrEmpty(categoryUpdated) || string.IsNullOrEmpty(categoryToUpdate))
        return Results.BadRequest();

    if (!categories.Categories.ContainsKey(categoryToUpdate))
        return Results.BadRequest();

    var affectedRecipes = recipeList.FindAll(recipe => recipe.Categories.Contains(categoryToUpdate));

    foreach (var affectedRecipe in affectedRecipes)
    {
        affectedRecipe.Categories.Remove(categoryToUpdate);
        affectedRecipe.Categories.Add(categoryUpdated);
    }
    await SaveRecipes();

    categories.Categories.Remove(categoryToUpdate);
    categories.Categories.Add(categoryUpdated, true);
    await SaveCategories();
    return Results.Ok();
});

app.MapDelete("/categories", async (string CategoryToDelete) =>
{
    if (string.IsNullOrEmpty(CategoryToDelete))
        return Results.BadRequest();


    var affectedRecipes = recipeList.FindAll(recipe => recipe.Categories.Contains(CategoryToDelete));

    foreach (var affectedRecipe in affectedRecipes)
    {
        affectedRecipe.Categories.Remove(CategoryToDelete);

        if (affectedRecipe.Categories.Count == 0)
            recipeList.Remove(affectedRecipe);
    }

    await SaveRecipes();

    categories.Categories.Remove(CategoryToDelete);
    await SaveCategories();
    return Results.Ok();

});


async Task SaveRecipes()
{

    string jsonRecipeString = JsonSerializer.Serialize(recipeList);
    await File.WriteAllTextAsync(recipePath, jsonRecipeString);
}

async Task SaveCategories()
{
    string jsonCategoryString = JsonSerializer.Serialize(categories);
    await File.WriteAllTextAsync(categoryPath, jsonCategoryString);
}

async void ReadOnAwake()
{
    // Read categories if previously exists.
    if (File.Exists(categoryPath))
    {
        if (new FileInfo(recipePath).Length > 0)
        {
            using var read = new StreamReader(categoryPath);
            string file = await File.ReadAllTextAsync(categoryPath);
            var jsonCategoryFile = JsonSerializer.Deserialize<Category>(file);

            categories = jsonCategoryFile;
        }
    }
    
    // Read recipes if previously exists.
    if (File.Exists(recipePath))
    {
        if (new FileInfo(recipePath).Length > 0)
        {
            using var read = new StreamReader(recipePath);
            string file = await File.ReadAllTextAsync(recipePath);
            var jsonRecipeFile = JsonSerializer.Deserialize<List<Recipe>>(file);

            recipeList = jsonRecipeFile;
        }
    }
}
app.Run();