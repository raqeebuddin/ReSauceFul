using Newtonsoft.Json;
using reSauceFul.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace reSauceFul.Controllers
{
    public class HomeController : Controller
    {
        private ReSauceFulEntities db = new ReSauceFulEntities();
        public ActionResult Index()
        {
            ViewBag.cookingTime = new SelectList(new List<string> { "15", "30", "45"});
            ViewBag.preference = new SelectList(new List<string> {"vegan ", "vegetarian ", "gluten free " });
            return View();
        }

        [HttpPost]
        public ActionResult Index(UserSearch userSearch, string preference, string cookingTime)
        {
            if (ModelState.IsValid)
            {
                if (cookingTime == "")
                {
                    cookingTime = "30";
                }
                string userInput = userSearch.Ingredients;
                int cook = (Convert.ToInt32(cookingTime) * 60);
                string preferences = preference += " ";

                var apiCall = String.Format(
              $"http://api.yummly.com/v1/api/recipes?_app_id=a2c31330&_app_key=6deb7cc0912afcaec0cb23cb00c4ef07&q={preferences}{userInput} &maxTotalTimeInSeconds={cook}&maxResult=4&start=0&requirePictures=true");

                using (WebClient client = new WebClient())
                {
                    var json = client.DownloadString(apiCall);
                    var data = JsonConvert.DeserializeObject<reSauceFul.Models.ApiResponse>(json);
                    List<Recipes> recipes = new List<Recipes>();

                    if (data.Matches.Count() == 0)
                    {
                        ViewBag.noResult = "No Recipes Found please try again";
                        return View("Index");
                    }
                    else
                    {
                        foreach (Recipes recipe in data.Matches)
                        {
                            recipes.Add(recipe);
                        }
                        return View("RecipeResult", recipes);
                    }
                }
            }

            ViewBag.cookingTime = new SelectList(new List<string> { "15", "30", "45" });
            ViewBag.preference = new SelectList(new List<string> { "vegan ", "vegetarian ", "gluten free " });
            ModelState.AddModelError("", "Please enter a Ingredients (numbers and symbols are NOT permitted)");
            return View(userSearch);
        }
  
        public ActionResult IndivRecipe(string recipe)
        {
            using (WebClient client = new WebClient())
            {
                var singleRecipe = String.Format(
                    $" http://api.yummly.com/v1/api/recipe/{recipe}?_app_id=a2c31330&_app_key=6deb7cc0912afcaec0cb23cb00c4ef07");
                var json = client.DownloadString(singleRecipe);
                var data = JsonConvert.DeserializeObject<reSauceFul.Models.SingleRecipe>(json);

                List<object> myModel = new List<object>();
                var reviewMatches = from review in db.Reviews
                              where review.Recipe.Name.Contains(data.Name)
                              select review;
                myModel.Add(reviewMatches.ToList());
                myModel.Add(data);
                return View(myModel);
            }
        }

        public ActionResult LoginPage(string recipeName, string apiId)
        { 
            var dbRecipeQuery = from d in db.Recipes
                             orderby d.Name
                             select d.Name;

            if (dbRecipeQuery.Contains(recipeName))
            {
                var matches = from recipe in db.Recipes
                              where recipe.Name.Contains(recipeName)
                              select recipe.Id;
                ViewBag.recipeId = matches.ToList();
                return View();
            }
            else
            {
                Recipe recipe = new Recipe();
                recipe.Name = recipeName;
                recipe.ApiId = apiId;
                db.Recipes.Add(recipe);
                db.SaveChanges();

                var matches = from recipes in db.Recipes
                              where recipes.Name.Contains(recipeName)
                              select recipes.Id;
                ViewBag.recipeId = matches.ToList();
                return View();
            }  
        }

        [HttpPost]
        public ActionResult LoginPage(User userDetails, string recipeID)
        {
            if (ModelState.IsValid)
            {
                var dbUserQuery = from d in db.Users
                                  orderby d.Name
                                  select d.Name;
                //If user exists
                if (dbUserQuery.Contains(userDetails.Name))
                {
                    //get user id
                    var matches = from user in db.Users
                                  where user.Name.Equals(userDetails.Name)
                                  select user.Id;
                    var test = matches.ToList();
                    int userId = test[0];
                    var reviewSearch = db.Reviews.Where(b => b.UserId.Equals(userId)).FirstOrDefault();

                    if (reviewSearch.UserId == test[0])
                    {
                        return View("CreateReview", reviewSearch);
                    }
                    else
                    { 
                        //create new Review obj and add user id
                        Review review = new Review();
                        review.RecipeId = Convert.ToInt32(recipeID);
                        review.UserId = test[0];
                        //pass review object to have additional information added in from View
                        return View("CreateReview", review);
                    }
                }
                else
                {
                    //create new user obj and add the username
                    db.Users.Add(userDetails);
                    db.SaveChanges();

                    //get user id
                    var matches = from user in db.Users
                                  where user.Name.Equals(userDetails.Name)
                                  select user.Id;
                    var test = matches.ToList();
                    //create new review object and add user id
                    Review review = new Review();
                    review.RecipeId = Convert.ToInt32(recipeID);
                    review.UserId = test[0];
                    //pass reviw object to view to be ammended with additional information
                    return View("CreateReview", review);

                }

            }
            int id = Convert.ToInt32(recipeID);
            var recipeIdMatch = from recipes in db.Recipes
                          where recipes.Id.Equals(id)
                          select recipes.Id;

            ViewBag.recipeId = recipeIdMatch.ToList();
            ModelState.AddModelError("", "Please enter a Username (numbers and symbols are NOT permitted)");
            return View(userDetails);
        }

        [HttpPost]
        public ActionResult CreateReview(Review CompletedReview)
        {
            if (ModelState.IsValid)
            {
                var dbUserQuery = from d in db.Reviews
                                  orderby d.UserId
                                  select d.UserId;
                if (dbUserQuery.Contains(CompletedReview.UserId))
                {
                    db.Entry(CompletedReview).State = EntityState.Modified;
                    db.SaveChanges();
                    var search = db.Recipes.Include("Reviews").Where(b => b.Id.Equals(CompletedReview.RecipeId)).FirstOrDefault();
                    return RedirectToAction("IndivRecipe", new { recipe = search.ApiId });
                }
                else
                {
                    //receive Review data  and save
                    db.Reviews.Add(CompletedReview);
                    db.SaveChanges();
                    var search = db.Recipes.Include("Reviews").Where(b => b.Id.Equals(CompletedReview.RecipeId)).FirstOrDefault();
                    return RedirectToAction("IndivRecipe", new { recipe = search.ApiId });
                }
            }

            ModelState.AddModelError("", "Please enter a Comment");
            return View(CompletedReview);
        }


        public ActionResult ReviewRecipe(string recipeName = "fish")
        {
            Recipe recipe = new Recipe();
            recipe.Name = recipeName;
            db.Recipes.Add(recipe);
            db.SaveChanges();
            var search = db.Recipes.Include("Reviews").Where(b => b.Id.Equals(3)).FirstOrDefault();
            return View(search);
        }
    }
}