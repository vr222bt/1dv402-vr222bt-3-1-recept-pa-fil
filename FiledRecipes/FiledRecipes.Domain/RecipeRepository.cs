﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FiledRecipes.Domain
{
    /// <summary>
    /// Holder for recipes.
    /// </summary>
    public class RecipeRepository : IRecipeRepository
    {
        /// <summary>
        /// Represents the recipe section.
        /// </summary>
        private const string SectionRecipe = "[Recept]";

        /// <summary>
        /// Represents the ingredients section.
        /// </summary>
        private const string SectionIngredients = "[Ingredienser]";

        /// <summary>
        /// Represents the instructions section.
        /// </summary>
        private const string SectionInstructions = "[Instruktioner]";

        /// <summary>
        /// Occurs after changes to the underlying collection of recipes.
        /// </summary>
        public event EventHandler RecipesChangedEvent;

        /// <summary>
        /// Specifies how the next line read from the file will be interpreted.
        /// </summary>
        private enum RecipeReadStatus { Indefinite, New, Ingredient, Instruction };

        /// <summary>
        /// Collection of recipes.
        /// </summary>
        private List<IRecipe> _recipes;

        /// <summary>
        /// The fully qualified path and name of the file with recipes.
        /// </summary>
        private string _path;

        /// <summary>
        /// Indicates whether the collection of recipes has been modified since it was last saved.
        /// </summary>
        public bool IsModified { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the RecipeRepository class.
        /// </summary>
        /// <param name="path">The path and name of the file with recipes.</param>
        public RecipeRepository(string path)
        {
            // Throws an exception if the path is invalid.
            _path = Path.GetFullPath(path);

            _recipes = new List<IRecipe>();
        }

        /// <summary>
        /// Returns a collection of recipes.
        /// </summary>
        /// <returns>A IEnumerable&lt;Recipe&gt; containing all the recipes.</returns>
        public virtual IEnumerable<IRecipe> GetAll()
        {
            // Deep copy the objects to avoid privacy leaks.
            return _recipes.Select(r => (IRecipe)r.Clone());
        }

        /// <summary>
        /// Returns a recipe.
        /// </summary>
        /// <param name="index">The zero-based index of the recipe to get.</param>
        /// <returns>The recipe at the specified index.</returns>
        public virtual IRecipe GetAt(int index)
        {
            // Deep copy the object to avoid privacy leak.
            return (IRecipe)_recipes[index].Clone();
        }

        /// <summary>
        /// Deletes a recipe.
        /// </summary>
        /// <param name="recipe">The recipe to delete. The value can be null.</param>
        public virtual void Delete(IRecipe recipe)
        {
            // If it's a copy of a recipe...
            if (!_recipes.Contains(recipe))
            {
                // ...try to find the original!
                recipe = _recipes.Find(r => r.Equals(recipe));
            }
            _recipes.Remove(recipe);
            IsModified = true;
            OnRecipesChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Deletes a recipe.
        /// </summary>
        /// <param name="index">The zero-based index of the recipe to delete.</param>
        public virtual void Delete(int index)
        {
            Delete(_recipes[index]);
        }

        /// <summary>
        /// Raises the RecipesChanged event.
        /// </summary>
        /// <param name="e">The EventArgs that contains the event data.</param>
        protected virtual void OnRecipesChanged(EventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of 
            // a race condition if the last subscriber unsubscribes 
            // immediately after the null check and before the event is raised.
            EventHandler handler = RecipesChangedEvent;

            // Event will be null if there are no subscribers. 
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        //Läser in recept
        public virtual void Load()
        {
            //Skapar lista
            List<IRecipe> recipes = new List<IRecipe>();
            RecipeReadStatus status = RecipeReadStatus.Indefinite;
            Recipe totalRecipe = null;

            //Ser till så filen stängs när den inte används
            using (StreamReader reader = new StreamReader(_path))
            {
                
                String line;
                //Loop som läser in varje rad tills filen tar slut
                while ((line = reader.ReadLine()) != null)
                {


                    if (string.IsNullOrWhiteSpace(line) == false)
                    {
                        //Switch sats som håller ordning på om det är ingredienser etc..
                        switch (line)
                        {

                            case SectionRecipe:
                                status = RecipeReadStatus.New;
                                break;
                            case SectionIngredients:
                                status = RecipeReadStatus.Ingredient;
                                break;
                            case SectionInstructions:
                                status = RecipeReadStatus.Instruction;
                                break;
                            case "":
                                break;
                            default:
                                //Switch sats som gör olika saker med raden beroende på om den var instruktioner etc..
                                switch (status)
                                {
                                    case RecipeReadStatus.New:
                                        totalRecipe = new Recipe(line);
                                        recipes.Add(totalRecipe);
                                        break;
                                    case RecipeReadStatus.Ingredient:
                                        //Delar upp ingrediens raden i Amount, Measure och Name
                                        string[] ingredients = line.Split(new string[] { ";" }, StringSplitOptions.None);
                                        if (ingredients.Length != 3)
                                        {
                                            throw new FileFormatException();
                                        }
                                        Ingredient ingredient = new Ingredient();
                                        ingredient.Amount = ingredients[0];
                                        ingredient.Measure = ingredients[1];
                                        ingredient.Name = ingredients[2];
                                        totalRecipe.Add(ingredient);
                                        break;
                                    case RecipeReadStatus.Instruction:
                                        totalRecipe.Add(line);
                                        break;
                                    case RecipeReadStatus.Indefinite:
                                        throw new FileFormatException();
                                    default:
                                        break;
                                }
                                break;
                        } 
                    }
                }
            }
            //Sorterar recepten efter namnet
            _recipes = recipes.OrderBy(recipe => recipe.Name).ToList();
            IsModified = false;
            OnRecipesChanged(EventArgs.Empty);
        }

        
        public virtual void Save()
        {
            //Stänger filen när den inte används
            using (StreamWriter writer = new StreamWriter(_path))
            {
                //Loop som skriver receptet, körs så många gånger som det finns recept
                foreach (var recipe in _recipes)
                {
                    writer.WriteLine(SectionRecipe);
                    writer.WriteLine(recipe.Name);
                    writer.WriteLine(SectionIngredients);
                    foreach (var ingredient in recipe.Ingredients)
                    {
                        writer.WriteLine("{0};{1};{2}", ingredient.Amount, ingredient.Measure, ingredient.Name);
                    }
                    writer.WriteLine(SectionInstructions);
                    foreach (var instruction in recipe.Instructions)
                    {
                        writer.WriteLine(instruction);
                    }
                    writer.WriteLine(recipe.Instructions);

                }
            }
            IsModified = false;
            OnRecipesChanged(EventArgs.Empty);   
        }
    }
}
