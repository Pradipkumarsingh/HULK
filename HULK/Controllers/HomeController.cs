using HULK.Data;
using HULK.Models;
using HULK.Models.ViewModels;
using HULK.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HULK.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDBContext dbcontext
            ;

        public HomeController(ILogger<HomeController> logger, ApplicationDBContext context)
        {
            _logger = logger;
            dbcontext = context;
        }

        public IActionResult Index()
        {
            HomeVM homeVM = new HomeVM()
            {
                Products = dbcontext.Product.Include(u => u.Category).Include(u => u.ApplicationType),
                Categories = dbcontext.Category
            };
            return View(homeVM);
        }
        public IActionResult Details(int id)
        {
            List<ShoppingCartcs> shoppingCartList = new List<ShoppingCartcs>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCartcs>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCartcs>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCartcs>>(WC.SessionCart);
            }

            DetailsVM DetailsVM = new DetailsVM()
            {
                Product = dbcontext.Product.Include(u => u.Category).Include(u => u.ApplicationType)
                .Where(u => u.Id == id).FirstOrDefault(),
                ExistsInCart = false
            };

            foreach(var item in shoppingCartList)
            {
                if(item.ProductId == id)
                {
                    DetailsVM.ExistsInCart = true;
                }
            }
            return View(DetailsVM);
        }


        [HttpPost, ActionName("Details")]
        public IActionResult DetailsPost(int id)
        {
            List<ShoppingCartcs> shoppingCartList = new List<ShoppingCartcs>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCartcs>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCartcs>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCartcs>>(WC.SessionCart);
            }
            shoppingCartList.Add(new ShoppingCartcs { ProductId = id });
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }
        public IActionResult RemoveFromCart(int id)
        {
            List<ShoppingCartcs> shoppingCartList = new List<ShoppingCartcs>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCartcs>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCartcs>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCartcs>>(WC.SessionCart);
            }

            var itemToRemove = shoppingCartList.SingleOrDefault(r => r.ProductId == id);
            if (itemToRemove != null)
            {
                shoppingCartList.Remove(itemToRemove);
            }

            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
