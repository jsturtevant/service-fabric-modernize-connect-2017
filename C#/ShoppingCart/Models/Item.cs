using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ShoppingCart.Models
{
    [DataContract]
    public class Item
    {
        public Item(int productId)
        {
            ProductId = productId;
            Quantity = 1;
            ItemId = Guid.NewGuid().ToString();
            DateCreated = DateTime.Now;
        }

        private Item(Item fromItem, int quantity) 
        {
            ProductId = fromItem.ProductId;
            Quantity = quantity;
            ItemId = fromItem.ItemId;
            DateCreated = fromItem.DateCreated;
        }

        public static Item IncrementQuantity(Item fromItem)
        {
            return new Item(fromItem, fromItem.Quantity + 1);
        }

        public static Item UpdateQuantity(Item fromItem, int quantity)
        {
            return new Item(fromItem, quantity);
        }

        [DataMember]
        public string ItemId { get; private set; }

        [DataMember]
        public int Quantity { get; private set; }

        [DataMember]
        public System.DateTime DateCreated { get; private set; }

        [DataMember]
        public int ProductId { get; private set; }
    }
}
