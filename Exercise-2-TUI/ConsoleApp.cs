using Spectre.Console;
using System.Text;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Console;
public class UI_Helper
{
    public static string RecipeChoices(List<Recipe> recipeList, Category categories)
    {
        AnsiConsole.Clear();

        string recipePrompt;

        var errorEditorListTable = new Table()
            .AddColumn("[underline][red]There are currently no recipes, please add a recipe and try again![/][/]")
            .Centered()
            .Expand();

        var errorAddTable = new Table()
            .AddColumn("[underline][red]There are currently no categories, please add categories and try again![/][/]")
            .Centered()
            .Expand();

        do
        {
            recipePrompt = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("[yellow]What would you like to do?[/]")
                .PageSize(4)
                .AddChoices(new[] { "Add a recipe", "List recipes", "Edit a recipe", "Return" }
                ));
            if ((recipePrompt == "List recipes") && (recipeList.Count == 0))
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(errorAddTable);

                AnsiConsole.Prompt(
                    new TextPrompt<string>("[red]Press enter to go back[/]")
                        .AllowEmpty());
            }
            else if ((recipePrompt == "Edit a recipe") && (recipeList.Count == 0))
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(errorEditorListTable);

                AnsiConsole.Prompt(
                    new TextPrompt<string>("[red]Press enter to go back[/]")
                        .AllowEmpty());
            }
            else if ((recipePrompt == "Add a recipe") && (categories.Categories.Count == 0))
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(errorEditorListTable);

                AnsiConsole.Prompt(
                    new TextPrompt<string>("[red]Press enter to go back[/]")
                        .AllowEmpty());
            }
            else break;
        }
        while (true);

        return recipePrompt;
    }

    public static string CategoryChoices(List<Recipe> recipeList, Category categories)
    {
        AnsiConsole.Clear();
        var categoryPrompt = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title("[yellow]Would you like to edit or add categories?[/")
            .PageSize(3)
            .AddChoices(new[] { "Edit", "Add", "Return" }
            ));

        return categoryPrompt;
    }

    public static void ListAndDisplayRecipes(List<Recipe> recipeList)
    {
        var recipeTable = new Table()
                            .AddColumn("[red]Please select a recipe to display[/]");
        var titles = new List<string>();
        var titleBuilder = new StringBuilder();
        var recipeMap = new Dictionary<string, Guid>();

        AnsiConsole.Clear();
        AnsiConsole.Write(recipeTable);

        foreach (var recipe in recipeList)
        {
            titleBuilder.Append(recipe.Title)
                .Append(". Categories: ");

            foreach (var category in recipe.Categories)
                titleBuilder.Append(category)
                    .Append(", ");

            string appendString = titleBuilder.ToString().Remove(titleBuilder.ToString().LastIndexOf(", "));

            titles.Add(appendString);
            recipeMap.Add(appendString, recipe.Guid);

            titleBuilder.Clear();
        }

        string recipeTitle = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title("[yellow]Recipes available: [/]")
            .PageSize(recipeList.Count)
            .AddChoices(titles)
        );

        var recipeToList = recipeList.Find(recipe => recipe.Guid == recipeMap[recipeTitle]);

        AnsiConsole.Clear();

        var listingTable = new Table();
        listingTable.AddColumn("[green]Ingredients[/]")
            .AddColumn("[green]Instructions[/]");
        var index = recipeToList.Ingredients.Count < recipeToList.Instructions.Count ? recipeToList.Instructions.Count : recipeToList.Ingredients.Count;

        for (int i = 0; i < index; ++i)
        {
            var instructionToAdd = i >= recipeToList.Instructions.Count ? "" : recipeToList.Instructions[i];
            var ingredientToAdd = i >= recipeToList.Ingredients.Count ? "" : recipeToList.Ingredients[i];
            listingTable.AddRow($"- {ingredientToAdd}", $"- {instructionToAdd}");

        }

        AnsiConsole.Write(listingTable.Centered().Expand());
        AnsiConsole.Prompt(
            new TextPrompt<string>($"[blue]Recipe: {recipeTitle}.\n[/] [yellow]Press Enter when you're done viewing to go back[/]")
            .AllowEmpty());
    }

    public static Recipe? AddRecipe(List<Recipe> recipeList, Category categories)
    {
        var categoryList = categories.Categories.Select(category => category.Key).ToList();

        AnsiConsole.Clear();

        string recipeAddTitle = AnsiConsole.Ask<string>("[yellow]What would you like to name the recipe?[/]");
        ArgumentNullException.ThrowIfNull(recipeAddTitle);

        string recipeAddIngredients = AnsiConsole.Ask<string>("[yellow]Please enter recipe instructions, seperated by comma (, ): [/]");
        ArgumentNullException.ThrowIfNull(recipeAddIngredients);

        string recipeAddInstructions = AnsiConsole.Ask<string>("[yellow]Please enter recipe ingredients, seperated by comma (, ): [/]");
        ArgumentNullException.ThrowIfNull(recipeAddInstructions);


        List<string> recipeInstructions = recipeAddInstructions.Split(", ").ToList();
        List<string> recipeIngredients = recipeAddIngredients.Split(", ").ToList();

        var recipeAddCategories = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
            .Title("Please pick categories")
            .Required()
            .PageSize(categoryList.Count)
            .InstructionsText("[yellow]Pick the relevant categories[/]")
            .AddChoices(categoryList)
            );

        if (recipeList.Find(recipe => recipe.Title == recipeAddTitle && recipe.Categories.All(recipeAddCategories.Contains)) == null)
        {
            var recipeToAdd = new Recipe(recipeAddTitle, recipeInstructions, recipeIngredients, recipeAddCategories);

            return recipeToAdd;
        }
        else
        {
            AnsiConsole.Clear();
            var recipeAddError = new Table()
                .AddColumn("[red]The recipe already exists[/]")
                .Centered().Expand();

            AnsiConsole.Write(recipeAddError);
            AnsiConsole.Prompt(
                new TextPrompt<string>("[red]Press Enter to go back...[/]")
                .AllowEmpty());

            return null;
        }
    }

    public static Recipe? EditRecipe(List<Recipe> recipeList, Category categories)
    {
        AnsiConsole.Clear();

        var toEditTitles = new List<string>();

        foreach (Recipe recipe in recipeList)
            toEditTitles.Add(recipe.Title);

        var recipeToEditTitle = AnsiConsole.Ask<string>("[yellow]Enter name of recipe to edit[/]");
        var recipeCategories = categories.Categories.Select(categ => categ.Key).ToList();
        var matchingRecipes = recipeList;
        var matchingCategories = new Dictionary<List<string>, Guid>();
        var categoryList = new List<string>();
        var categBuilder = new StringBuilder();

        matchingCategories = matchingRecipes.ToDictionary(recipe => recipe.Categories, recipe => recipe.Guid);

        foreach (var categList in matchingCategories.Keys.ToList())
        {
            foreach (var categ in categList)
                categBuilder.Append(categ)
                    .Append(", ");

            categoryList.Add(categBuilder.ToString()
                .Remove(categBuilder.ToString()
                    .LastIndexOf(", ")));
            categBuilder.Clear();
        }

        var recipeEditCategories = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
            .Title("Please pick categories the recipe belongs to")
            .Required()
            .PageSize(categoryList.Count)
            .InstructionsText("[yellow]Pick the relevant categories[/]")
            .AddChoices(categoryList)
        );

        var result = recipeList.Find(recipe => recipe.Title == recipeToEditTitle && recipe.Categories.All(recipeEditCategories.Contains));
        if (result == null)
        {
            AnsiConsole.Clear();

            var recipeListError = new Table()
                .AddColumn("[red]No recipe exists with these search conditions[/]")
                .Centered()
                .Expand();

            AnsiConsole.Write(recipeListError);
            AnsiConsole.Prompt(
                           new TextPrompt<string>("[red]Press Enter to go back...[/]")
                           .AllowEmpty());

            return null;
        }

        var toEdit = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title("[yellow]What part of this recipe would you like to edit?[/]")
            .PageSize(3)
            .AddChoices(new[] { "Instructions", "Ingredients", "Categories" }
        ));

        switch (toEdit)
        {
            case "Instructions":
                AnsiConsole.Clear();
                string recipeEditInstructions = AnsiConsole.Ask<string>("[yellow]Please enter recipe ingredients, seperated by comma (, ): [/]");
                List<string> rInstructions = recipeEditInstructions.Split(", ").ToList();
                result.EditRecipeIngredients(rInstructions);

                return result;
            case "Categories":
                var rCategoryPrompt = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                    .Title("[yellow]Would you like to remove or add categories?[/]")
                    .PageSize(2)
                    .AddChoices(new[] { "Remove", "Add" }
                ));

                switch (rCategoryPrompt)
                {
                    case "Remove":
                        if (result.Categories.Count == 1)
                        {
                            AnsiConsole.Prompt(
                                new TextPrompt<string>("[red]Cannot remove cateogires from a recipe that only contains one category. Press Enter to return [/]")
                                .AllowEmpty());

                            AnsiConsole.Clear();
                            return null;
                        }
                        else
                        {
                            var categoriesToRemove = AnsiConsole.Prompt(
                                new MultiSelectionPrompt<string>()
                                .Title("Please pick categories the recipe belongs to")
                                .Required()
                                .PageSize(categoryList.Count)
                                .InstructionsText("[yellow]Pick the categories you want to remove from the recipe[/]")
                                .AddChoices(result.Categories)
                            );

                            result.EditRecipeCategory(categoriesToRemove);
                            return result;
                        }
                    case "Add":
                        var categoriesToAdd = AnsiConsole.Prompt(
                            new MultiSelectionPrompt<string>()
                            .Title("Please pick categories the recipe belongs to")
                            .Required()
                            .PageSize(categoryList.Count)
                            .InstructionsText("[yellow]Pick relevant categories[/]")
                            .AddChoices(recipeCategories)
                        );

                        result.AddRecipeCategory(categoriesToAdd);
                        return result;
                }
                break;

            case "Return":
                return null;
        }
        return null;
    }

    public static string? EditCategory(Category categories)
    {

        var categoryList = categories.Categories.Select(categ => categ.Key).ToList();
        AnsiConsole.Clear();
        if (categoryList.Count == 0)
        {
            AnsiConsole.Clear();

            var editCategoryError = new Table()
                .AddColumn("[red]Please enter a category first[/]")
                .Centered();

            return null;
        }

        var categoryToEdit = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title("Please pick category to edit")
            .PageSize(categoryList.Count)
            .AddChoices(categoryList)
        );

        string editedCategory = AnsiConsole.Ask<string>("[yellow]Enter the edited name: [/]");

        ArgumentNullException.ThrowIfNull(categoryToEdit);
        if (!categories.Categories.ContainsKey(categoryToEdit))
        {
            AnsiConsole.Clear();
            var recipeAddError = new Table()
                .AddColumn("[red]The recipe to edit does not exist[/]")
                .Centered()
                .Expand();

            return null;
        }
        return editedCategory;
    }

    public static string? AddCategory(Category categories)
    {
        AnsiConsole.Clear();

        string categoryToAdd = AnsiConsole.Ask<string>("[yellow]Enter category name to add: [/]");
        ArgumentNullException.ThrowIfNull(categoryToAdd);

        if (categories.Categories.ContainsKey(categoryToAdd))
        {
            var errAddTable = new Table()
                 .AddColumn("[underline][red]Category already exists.[/][/]")
                 .Centered()
                 .Expand();
            AnsiConsole.Write(errAddTable);
            AnsiConsole.Prompt(
                new TextPrompt<string>("[red]Press Enter to go back ... [/]")
                .AllowEmpty());

            return null;
        }
        else
        {
            AnsiConsole.Prompt(
                new TextPrompt<string>("[yellow]Added successfully. Press Enter to go back...[/]")
                .AllowEmpty());

            return categoryToAdd;
        }
    }

}