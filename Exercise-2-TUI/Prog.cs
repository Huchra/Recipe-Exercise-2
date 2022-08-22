using Spectre.Console;
using System.Text;

namespace Console
{
    class Program
    {
        public static void Main()
        {
            var manipulator = new Manipulator();
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
                        RecipeTable(manipulator);

                        break;
                    case "Categories":
                        CategoryTable(manipulator);
                        break;
                }
            }
        }

        public static void RecipeTable(Manipulator manipulator)
        {
            AnsiConsole.Clear();
            List<Recipe> recipes = manipulator.Recipes;

            var errorEditorListTable = new Table()
                    .AddColumn("[underline][red]There are currently no recipes, please add some and try again![/][/]!")
                    .Centered().Expand();
            var errorAddTable = new Table()
                .AddColumn("[underline][red]There are currently no categories, please add categories and try again![/][/]")
                .Centered().Expand();

            string recipeChoice;
            do
            {
                recipeChoice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                    .Title("[yellow]What would you like to do?[/]")
                    .PageSize(10)
                    .AddChoices(new[] { "Add a Recipe", "List Recipes", "Edit a Recipe", "Return" }
                    ));

                if ((recipeChoice == "List Recipes") && (recipes.Count == 0))
                {
                    AnsiConsole.Clear();
                    AnsiConsole.Write(errorEditorListTable);
                }
                else if ((recipeChoice == "Add a Recipe") && (manipulator.Categories.Categories.Count == 0))
                {
                    AnsiConsole.Clear();
                    AnsiConsole.Write(errorAddTable);

                }
                else if ((recipeChoice == "Edit a Recipe") && (recipes.Count == 0))
                {
                    AnsiConsole.Clear();
                    AnsiConsole.Write(errorEditorListTable);
                }
                else break;

            } while (true);


            switch (recipeChoice)
            {
                case "List Recipes":
                    var recipeTable = new Table()
                        .AddColumn("[red]Please select a recipe to display[/]");
                    var titles = new List<string>();
                    var titleBuilder = new StringBuilder();
                    var recipeMap = new Dictionary<string, Guid>();

                    AnsiConsole.Clear();
                    AnsiConsole.Write(recipeTable);

                    foreach (var recipe in recipes)
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
                        .PageSize(10)
                        .AddChoices(titles));

                    var recipeToList = manipulator.Recipes.Find(x => x.Guid == recipeMap[recipeTitle]);

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
                    break;

                case "Add a Recipe":
                    var categories = manipulator.Categories.Categories.Select(x => x.Key).ToList();

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
                        .PageSize(10)
                        .InstructionsText("[yellow]Pick the relevant categories[/]")
                        .AddChoices(categories));

                    if (manipulator.Recipes.Find(x => x.Title == recipeAddTitle && x.Categories.All(recipeAddCategories.Contains)) == null)
                    {
                        var recipeToAdd = new Recipe(recipeAddTitle, recipeInstructions, recipeIngredients, recipeAddCategories);
                        manipulator.AddRecipe(recipeToAdd);
                        manipulator.RecipeSeralize();
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
                    }
                    break;
                case "Edit a Recipe":
                    AnsiConsole.Clear();

                    var toEditTitles = new List<string>();


                    foreach (Recipe recipe in recipes)
                        toEditTitles.Add(recipe.Title);

                    // Query the recipe's title.

                    var recipeToEditTitle = AnsiConsole.Ask<string>("[yellow]Enter name of recipe to edit[/]");
                    var recipeCategories = manipulator.Categories.Categories.Select(x => x.Key).ToList();
                    // Find all recipes with the relevant title.
                    var matchingRecipes = manipulator.Recipes.FindAll(x => x.Title == recipeToEditTitle);
                    // Filter out their categories to use for querying the exact recipe.
                    var matchingCategories = new Dictionary<List<string>, Guid>();
                    var categoryList = new List<string>();
                    var categBuilder = new StringBuilder();

                    matchingCategories = matchingRecipes.ToDictionary(x => x.Categories, x => x.Guid);

                    foreach (var categList in matchingCategories.Keys.ToList())
                    {
                        foreach (var categ in categList)
                            categBuilder.Append(categ)
                                .Append(", ");

                        categoryList.Add(categBuilder.ToString().Remove(categBuilder.ToString().LastIndexOf(", ")));
                        categBuilder.Clear();
                    }

                    var recipeEditCategories = AnsiConsole.Prompt(
                        new MultiSelectionPrompt<string>()
                        .Title("Please pick categories the recipe belongs to")
                        .Required()
                        .PageSize(10)
                        .InstructionsText("[yellow]Pick the relevant categories[/]")
                        .AddChoices(categoryList)
                        );

                    // This query should use the Guid, but I'm not sure where to "acquire" it.
                    var result = recipes.Find(x => x.Title == recipeToEditTitle && x.Categories.All(recipeEditCategories.Contains));
                    ArgumentNullException.ThrowIfNull(result);

                    var toEdit = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                        .Title("[yellow]What part of this recipe would you like to edit?[/]")
                        .PageSize(10)
                        .AddChoices(new[] { "Instructions", "Ingredients", "Categories" }
                       ));

                    switch (toEdit)
                    {
                        case "Instructions":
                            AnsiConsole.Clear();
                            string recipeEditInstructions = AnsiConsole.Ask<string>("[yellow]Please enter recipe ingredients, seperated by comma: [/]");
                            List<string> rInstructions = recipeEditInstructions.Split(", ").ToList();
                            result.EditRecipeInstructions(rInstructions);
                            manipulator.RecipeSeralize();
                            break;
                        case "Ingredients":
                            AnsiConsole.Clear();
                            string recipeEditIngredients = AnsiConsole.Ask<string>("[yellow]Please enter recipe instructions, seperated by comma: [/]");
                            List<string> rIngredients = recipeEditIngredients.Split(", ").ToList();
                            result.EditRecipeIngredients(rIngredients);
                            manipulator.RecipeSeralize();
                            break;
                        case "Categories":
                            var rCategoryPrompt = AnsiConsole.Prompt(
                                new SelectionPrompt<string>()
                                .Title("[yellow]Would you like to remove or add categories?[/]")
                                .PageSize(10)
                                .AddChoices(new[] { "Remove", "Add" }
                                ));

                            switch (rCategoryPrompt)
                            {
                                case "Remove":
                                    if (result.Categories.Count == 1)
                                    {
                                        AnsiConsole.Prompt(
                                            new TextPrompt<string>("[red]Cannot remove categories from a recipe that contains only one. Press Enter to return[/]")
                                            .AllowEmpty());
                                        AnsiConsole.Clear();

                                    }
                                    else
                                    {
                                        var categoriesToRemove = AnsiConsole.Prompt(
                                            new MultiSelectionPrompt<string>()
                                            .Title("Please pick categories the recipe belongs to")
                                            .Required()
                                            .PageSize(10)
                                            .InstructionsText("[yellow]Pick the categories you want to remove from the recipe[/]")
                                            .AddChoices(result.Categories));

                                        result.EditRecipeCategory(categoriesToRemove);
                                        manipulator.RecipeSeralize();

                                    }

                                    break;

                                case "Add":
                                    var categoriesToAdd = AnsiConsole.Prompt(
                                        new MultiSelectionPrompt<string>()
                                        .Title("Please pick categories the recipe belongs to")
                                        .Required()
                                        .PageSize(10)
                                        .InstructionsText("[yellow]Pick the relevant categories[/]")
                                        .AddChoices(recipeCategories));

                                    result.AddRecipeCategory(categoriesToAdd);
                                    break;
                            }
                            break;
                    }
                    break;
                case "Return":
                    return;

            }
        }
        public static void CategoryTable(Manipulator manipulator)
        {
            AnsiConsole.Clear();
            var categoryPrompt = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("[yellow]Would you like to remove or add categories?[/]")
                .PageSize(10)
                .AddChoices(new[] { "Edit", "Add", "Return" }
                ));

            switch (categoryPrompt)
            {
                case "Edit":
                    AnsiConsole.Clear();
                    if (manipulator.Categories.Categories.Count == 0)
                    {
                        AnsiConsole.Clear();

                        var editCategoryError = new Table()
                            .AddColumn("[red]Please enter a category first[/]").Centered();
                        AnsiConsole.Write(editCategoryError);

                        return;
                    }

                    var categoryToEdit = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                        .Title("Please pick category to edit")
                        .PageSize(10)
                        .AddChoices(manipulator.Categories.Categories.Keys.ToList()));

                    string editedCategory = AnsiConsole.Ask<string>("[yellow]Enter the edited name: [/]");

                    ArgumentNullException.ThrowIfNull(categoryToEdit);
                    manipulator.Categories.EditCategory(categoryToEdit, editedCategory);
                    var affectedRecipes = manipulator.Recipes.FindAll(x => x.Categories.Contains(categoryToEdit));
                    foreach (var affectedRecipe in affectedRecipes)
                    {
                        if (affectedRecipe.Categories.Contains(categoryToEdit))
                        {
                            affectedRecipe.Categories.Remove(categoryToEdit);
                            affectedRecipe.Categories.Add(editedCategory);
                        }
                    }
                    manipulator.RecipeSeralize();
                    manipulator.CategorySerialize();

                    AnsiConsole.Prompt(
                        new TextPrompt<string>("[green]Edited! Press Enter to go back...[/]")
                        .AllowEmpty());

                    return;

                case "Add":
                    AnsiConsole.Clear();

                    string categoryToAdd = AnsiConsole.Ask<string>("[yellow]Enter category name to add: [/]");
                    ArgumentNullException.ThrowIfNull(categoryToAdd);

                    if (manipulator.Categories.Categories.ContainsKey(categoryToAdd))
                    {
                        var errAddTable = new Table()
                            .AddColumn("[underline][red]Category already exists.[/][/]")
                            .Centered().Expand();
                        AnsiConsole.Write(errAddTable);
                        AnsiConsole.Prompt(
                            new TextPrompt<string>("[red]Press Enter to go back...[/]")
                            .AllowEmpty());

                        CategoryTable(manipulator);
                    }
                    else
                    {
                        manipulator.Categories.AddCategory(categoryToAdd);
                        manipulator.CategorySerialize();
                        AnsiConsole.Prompt(
                            new TextPrompt<string>("[yellow]Added succesfully. Press Enter to go back...[/]")
                            .AllowEmpty());

                        CategoryTable(manipulator);
                    }
                    break;
                case "Return":
                    return;
            }
        }
    }
}