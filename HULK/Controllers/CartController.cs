﻿using HULK.Data;
using HULK.Models;
using HULK.Models.ViewModels;
using HULK.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HULK.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDBContext dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; }
        public CartController(ApplicationDBContext context, IWebHostEnvironment webHostEnvironment)
        {
            dbContext = context;
            _webHostEnvironment = webHostEnvironment;
        }
            [Authorize]
        public IActionResult Index()
        {
            List<ShoppingCartcs> shoppingCartList = new List<ShoppingCartcs>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCartcs>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCartcs>>(WC.SessionCart).Count() > 0)
            {
                //session exsits
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCartcs>>(WC.SessionCart);
            }
            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> prodList = dbContext.Product.Where(u => prodInCart.Contains(u.Id));

            return View(prodList);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost()
        {
            return RedirectToAction(nameof(Summary));
        }
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            //var userId = User.FindFirstValue(ClaimTypes.Name);

            List<ShoppingCartcs> shoppingCartList = new List<ShoppingCartcs>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCartcs>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCartcs>>(WC.SessionCart).Count() > 0)
            {
                //session exsits
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCartcs>>(WC.SessionCart);
            }
            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> prodList = dbContext.Product.Where(u => prodInCart.Contains(u.Id));
            ProductUserVM = new ProductUserVM()
            {
                ApplicationUser = dbContext.ApplicationUser.FirstOrDefault(u => u.Id == claim.Value),
                ProductList = prodList.ToList()
            };
            return View(ProductUserVM);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(ProductUserVM ProductUserVM)
        {
            var PathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                + "templates" + Path.DirectorySeparatorChar.ToString() +
                "Inquiry.html";
            var subject = "New Inquiry";
            string HtmlBody = "";
            using (StreamReader sr = System.IO.File.OpenText(PathToTemplate))
            {
                HtmlBody = sr.ReadToEnd();
            }
            //Name: { 0}
            //Email: { 1}
            //Phone: { 2}
            //Products: {3}
            StringBuilder productListSB = new StringBuilder();
            foreach (var prod in ProductUserVM.ProductList)
            {
                productListSB.Append($" - Name: { prod.Name} <span style='font-size:14px;'> (ID: {prod.Id})</span><br />");
            }
            string messageBody = string.Format(HtmlBody,
                ProductUserVM.ApplicationUser.FullName,
                ProductUserVM.ApplicationUser.Email,
                ProductUserVM.ApplicationUser.PhoneNumber,
                productListSB.ToString());
           // await _emailSender.SendEmailAsync(WC.EmailAdmin, subject, messageBody);
            return RedirectToAction(nameof(InquiryConfirmation));
        }
        public IActionResult InquiryConfirmation()
        {
            HttpContext.Session.Clear();
            return View();
        }




        public IActionResult Remove(int id)
        {
            List<ShoppingCartcs> shoppingCartList = new List<ShoppingCartcs>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCartcs>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCartcs>>(WC.SessionCart).Count() > 0)
            {
                //session exsits
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCartcs>>(WC.SessionCart);
            }
            shoppingCartList.Remove(shoppingCartList.FirstOrDefault(u => u.ProductId == id));
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }
    }
}
