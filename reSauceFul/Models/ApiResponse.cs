using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace reSauceFul.Models
{
    public class ApiResponse
    {
        public List<Recipes> Matches { get; set; }

    }

    public class Recipes
    {
        public string SourceDisplayName { get; set; }
        public List<string> SmallImageUrls{ get; set; }
        public string Id { get; set; }
        public string RecipeName { get; set; }
        public int Rating { get; set; }
        public int TotalTimeInSeconds { get; set; }
        public List<string> Ingredients { get; set; }
    }

    public class SingleRecipe
    {
        public string Yield { get; set; }
        public string CookTime { get; set; }
        public string Id { get; set; }
        public List<Image> Images { get; set; }
        public string Name { get; set; }
        public Site Source { get; set; }
        public List<string> ingredientLines { get; set; }
        public int Rating { get; set; }

    }

    public class Image
    {
        public string hostedSmallUrl { get; set; }
        public string hostedMediumUrl { get; set; }
        public string hostedLargeUrl { get; set; }
    }

    public class Site
    {
        public string sourceRecipeUrl { get; set; }
    }
}