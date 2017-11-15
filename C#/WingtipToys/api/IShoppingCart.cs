using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Refit;
using WingtipToys.Models;

namespace WingtipToys.api
{
    
    interface IShoppingCart
    {
        [Get("/api/cart/{username}")]
        Task<List<Item>> GetCart(string username, string PartitionKey, string PartitionKind = "Int64Range");

        [Post("/api/cart/{username}/{productid}")]
        Task AddItem(string username, int productid, string PartitionKey, string PartitionKind = "Int64Range");

        [Post("/api/cart/{username}")]
        Task Replace(string username, [Body] List<Item> items, string PartitionKey, string PartitionKind = "Int64Range");

        [Put("/api/cart/{username}/{productid}/{quantity}")]
        Task Put(string username, int productid, int quantity, string PartitionKey, string PartitionKind = "Int64Range");

        [Delete("/api/cart/{username}")]
        Task EmptyCart(string username, string PartitionKey, string PartitionKind = "Int64Range");

        [Delete("/api/cart/{username}/{productid}")]
        Task Delete(string username, int productid, string PartitionKey, string PartitionKind = "Int64Range");
    }


    public class Item
    {
        public Item(int productId)
        {
            ProductId = productId;
        }

        public string ItemId { get; private set; }

        public int Quantity { get; set; }

        public System.DateTime DateCreated { get; private set; }

        public int ProductId { get; private set; }
    }
}
        
