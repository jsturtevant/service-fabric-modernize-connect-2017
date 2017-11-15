using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;

namespace ShoppingCart.Models
{
    /// <summary>
    /// This uses immutable objects as recommended in Service Fabric Documentation
    /// https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-work-with-reliable-collections#define-immutable-data-types-to-prevent-programmer-error
    /// </summary>
    [DataContract]
    public class Cart
    {

        public Cart()
        {
            this.Items = new List<Item>().ToImmutableList();
        }

        private Cart(Item item)
        {
            var list =  new List<Item>()
            {
                item
            };

            this.Items = list.ToImmutableList();
        }

        public Cart(IEnumerable<Item> items)
        {
            this.Items = items.ToImmutableList();
        }

        public static Cart NewCart(int productid)
        {
            return new Cart(new Item(productid));
        }

        public Cart AddItem(int productid)
        {
            var cartItem = this.Items.FirstOrDefault(x => x.ProductId == productid);
            if (cartItem == null)
            {
                //add new item
                cartItem = new Item(productid);
                return new Cart(this.ImmutableItems.Add(cartItem));
            }
            else
            {
                var updatedItem = Item.IncrementQuantity(cartItem);
                return new Cart(this.ImmutableItems.Replace(cartItem, updatedItem));
            }
        }

        [DataMember]
        public IEnumerable<Item> Items { get; private set; }

        private ImmutableList<Item> ImmutableItems => this.Items as ImmutableList<Item>;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            // Convert the deserialized collection to an immutable collection
            Items = Items.ToImmutableList();
        }

        public Cart RemoveItem(int productid)
        {
            var cartItem = this.Items.FirstOrDefault(x => x.ProductId == productid);
            if (cartItem == null)
            {
                // no change
                return new Cart(this.ImmutableItems);
            }
            else
            {
                return new Cart(this.ImmutableItems.Remove(cartItem));
            }
        }

        public Cart UpdateItem(int productid, int qauntity)
        {
            var cartItem = this.Items.FirstOrDefault(x => x.ProductId == productid);
            if (cartItem == null)
            {
                return new Cart(this.ImmutableItems);
            }
            else
            {
                var updatedItem = Item.UpdateQuantity(cartItem, qauntity);
                return new Cart(this.ImmutableItems.Replace(cartItem, updatedItem));
            }
        }
    }
}