﻿using FiledRecipes.Domain;
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
        public virtual void Show(IRecipe recipe)
        {

        }
        public virtual void Show(IEnumerable<IRecipe> recipes)
        {

        }
    }
}
