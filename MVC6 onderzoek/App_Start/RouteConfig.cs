using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MVC6_onderzoek
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            var c = new Models.ApplicationDbContext();
            if (c.Database!=null)
            {
                if (!c.Movies.Any())
                {
                    c.Movies.Add(new Models.Movie { Title = "sssss", Genre = "blabla", Price = 123, ReleaseDate = DateTime.Now });
                    c.SaveChanges();
                }
            }
        }
    }
}
