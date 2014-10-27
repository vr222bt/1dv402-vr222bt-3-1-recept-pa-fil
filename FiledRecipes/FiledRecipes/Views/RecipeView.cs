using FiledRecipes.Domain;
using FiledRecipes.App.Mvp;
using FiledRecipes.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiledRecipes.Views
{
    /// <summary>
    /// 
    /// </summary>
    public class RecipeView : ViewBase, IRecipeView
    {
        //Visar recept
        public virtual void Show(IRecipe recipe)
        {
            //En header med receptets namn
            Header = recipe.Name;
            ShowHeaderPanel();

            //Skriver ut alla ingredienser  
            Console.WriteLine("\nIngredienser");
            Console.WriteLine("-------------");
            foreach (var ingredient in recipe.Ingredients)
            {
                Console.WriteLine(ingredient);
            }

            //Skriver ut alla instruktioner  
            Console.WriteLine("\nInstruktioner");
            Console.WriteLine("-------------");
            foreach (var instruction in recipe.Instructions)
            {
                Console.WriteLine(instruction);
            }
        }

        //Visar alla recept, bläddrar när man trycker ner en tangent
        public virtual void Show(IEnumerable<IRecipe> recipes)
        {
            foreach (var recipe in recipes)
            {
                Console.Clear();
                Show(recipe);
                ContinueOnKeyPressed();
                
            }
        }
    }
}
