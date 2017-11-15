using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Refit;
using WingtipToys.api;
using WingtipToys.Models;
using System.Threading.Tasks;

namespace WingtipToys.Logic
{
    public class ShoppingCartActions : IDisposable
    {
        public string ShoppingCartId { get; set; }

        private ProductContext _db = new ProductContext();

        public const string CartSessionKey = "CartId";

        public async Task AddToCart(int id)
        {

            // Retrieve the product from the database.           
            ShoppingCartId = GetCartId();
            var client = RestService.For<IShoppingCart>(ShoppingCartHelpers.BaseUrl);

            await client.AddItem(ShoppingCartId, id, ShoppingCartId.GetPartionKey());
        }

        public void Dispose()
        {
            if (_db != null)
            {
                _db.Dispose();
                _db = null;
            }
        }

        public string GetCartId()
        {
            //switch from session to cookie so that it will work across load balanced apps.
            if (HttpContext.Current.Request.Cookies[CartSessionKey] == null)
            {
                if (!string.IsNullOrWhiteSpace(HttpContext.Current.User.Identity.Name))
                {
                    HttpContext.Current.Response.SetCookie(new HttpCookie(CartSessionKey, HttpContext.Current.User.Identity.Name));
                }
                else
                {
                    // Generate a new random GUID using System.Guid class.     
                    Guid tempCartId = Guid.NewGuid();
                    HttpContext.Current.Response.SetCookie(new HttpCookie(CartSessionKey, tempCartId.ToString()));
                }
            }
            return HttpContext.Current.Request.Cookies[CartSessionKey]?.ToString();
        }

        public async Task<List<CartItem>> GetCartItems()
        {
            ShoppingCartId = GetCartId();

            var client = RestService.For<IShoppingCart>(ShoppingCartHelpers.BaseUrl);
            var items = await client.GetCart(ShoppingCartId, ShoppingCartId.GetPartionKey());

            List<CartItem> cartItems = await MergeWithProducts(items);
            return cartItems;
        }

        public async Task<decimal> GetTotal()
        {
            ShoppingCartId = GetCartId();

            var client = RestService.For<IShoppingCart>(ShoppingCartHelpers.BaseUrl);
            var items = await client.GetCart(ShoppingCartId, ShoppingCartId.GetPartionKey());

            List<CartItem> cartItems = await MergeWithProducts(items);

            decimal? total = decimal.Zero;
            total = (decimal?)cartItems.Sum(x => x.Quantity * x.Product.UnitPrice);
            return total ?? decimal.Zero;
        }

        public ShoppingCartActions GetCart(HttpContext context)
        {
            using (var cart = new ShoppingCartActions())
            {
                cart.ShoppingCartId = cart.GetCartId();
                return cart;
            }
        }

        public async Task UpdateShoppingCartDatabase(ShoppingCartUpdates[] cartItemUpdates)
        {
            ShoppingCartId = GetCartId();
            var client = RestService.For<IShoppingCart>(ShoppingCartHelpers.BaseUrl);

            try
            {
                
                List<Item> items = await client.GetCart(ShoppingCartId, ShoppingCartId.GetPartionKey());
                int cartItemCount = cartItemUpdates.Count();
                foreach (var cartItem in items)
                {
                    // Iterate through all rows within shopping cart list
                    for (int i = 0; i < cartItemCount; i++)
                    {
                        if (cartItem.ProductId == cartItemUpdates[i].ProductId)
                        {
                            if (cartItemUpdates[i].PurchaseQuantity < 1 || cartItemUpdates[i].RemoveItem == true)
                            {
                                await client.Delete(ShoppingCartId, cartItem.ProductId, ShoppingCartId.GetPartionKey());
                            }
                            else
                            {
                                await client.Put(ShoppingCartId, cartItem.ProductId, cartItemUpdates[i].PurchaseQuantity, ShoppingCartId.GetPartionKey());
                            }
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                throw new Exception("ERROR: Unable to Update Cart Database - " + exp.Message.ToString(), exp);
            }

        }

        public void EmptyCart()
        {
            ShoppingCartId = GetCartId();
            var client = RestService.For<IShoppingCart>(ShoppingCartHelpers.BaseUrl);

            client.EmptyCart(ShoppingCartId, ShoppingCartId.GetPartionKey());
        }

        public async Task<int> GetCount()
        {
            ShoppingCartId = GetCartId();

            var client = RestService.For<IShoppingCart>(ShoppingCartHelpers.BaseUrl);
            var items = await client.GetCart(ShoppingCartId, ShoppingCartId.GetPartionKey());
            
            return items.Sum(x => x.Quantity);
        }

        public struct ShoppingCartUpdates
        {
            public int ProductId;
            public int PurchaseQuantity;
            public bool RemoveItem;
        }

        public async Task MigrateCart(string cartId, string userName)
        {
            var client = RestService.For<IShoppingCart>(ShoppingCartHelpers.BaseUrl);
            var items = await client.GetCart(ShoppingCartId, ShoppingCartId.GetPartionKey());

            await client.Replace(userName, items, userName.GetPartionKey());
        }

        private async Task<List<CartItem>> MergeWithProducts(List<Item> items)
        {
            var productIds = items.Select(x => x.ProductId);
            var products = await _db.Products.Where(x => productIds.Contains(x.ProductID)).ToListAsync();

            List<CartItem> cartItems = items.Join(products,
                item => item.ProductId,
                product => product.ProductID,
                (item, product) =>
                    new CartItem()
                    {
                        CartId = ShoppingCartId,
                        DateCreated = item.DateCreated,
                        ItemId = item.ItemId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Product = product
                    }).ToList();
            return cartItems;
        }
    }
}