using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using ShoppingCart.Helpers;
using ShoppingCart.Models;

namespace ShoppingCart.Controllers
{
    [Route("api/[controller]")]
    public class CartController : Controller
    {
        private readonly IReliableStateManager stateManager;

        public CartController(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> Get(string username)
        {
            IReliableDictionary<string, Cart> cartDictionary = await this.stateManager.GetCartDictionary();

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                var result = await cartDictionary.TryGetValueAsync(tx, username);
                if (result.HasValue)
                {
                    return Ok(result.Value.Items);
                }

                return Ok(new List<Item>());
            }
        }

        [HttpPost("{username}/{productid}")]
        public async Task<IActionResult> Post(string username, int productid)
        {
            var cartDictionary = await this.stateManager.GetCartDictionary();

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                var result = await cartDictionary.TryGetValueAsync(tx, username);
                if (!result.HasValue)
                {
                    //nothing exists.  add new list with item
                    var newCart = Cart.NewCart(productid);
                    await cartDictionary.SetAsync(tx, username, newCart);

                    await tx.CommitAsync();
                    return Ok();
                }

                var cart = result.Value;
                var updatedCart = cart.AddItem(productid);

                await cartDictionary.SetAsync(tx, username, updatedCart);
                await tx.CommitAsync();

                return Ok();
            }
        }

        [HttpPost("{username}")]
        public async Task<IActionResult> Replace(string username, [FromBody] List<Item> items)
        {
            var cartDictionary = await this.stateManager.GetCartDictionary();

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                var result = await cartDictionary.TryGetValueAsync(tx, username);
                if (!result.HasValue)
                {
                    //nothing exists.  add new list with item
                    return NotFound();
                }

                var replacedCart = new Cart(items);
                await cartDictionary.SetAsync(tx, username, replacedCart);
                await tx.CommitAsync();

                return Ok();
            }
        }

        [HttpPut("{username}/{productid}/{quantity}")]
        public async Task<IActionResult> Put(string username, int productid, int quantity)
        {
            var cartDictionary = await this.stateManager.GetCartDictionary();

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                var result = await cartDictionary.TryGetValueAsync(tx, username);
                if (!result.HasValue)
                {
                    return NotFound();
                }

                var cart = result.Value;
                var item = cart.Items.FirstOrDefault(x => x.ProductId == productid);
                if (item == null)
                {
                    return NotFound();
                }

                Cart updatedCart = cart.UpdateItem(item.ProductId, quantity);
                await cartDictionary.SetAsync(tx, username, updatedCart);
                await tx.CommitAsync();

                return Ok();
            }
        }

        [HttpDelete("{username}")]
        public async Task<IActionResult> EmptyCart(string username)
        {
            var cartDictionary = await this.stateManager.GetCartDictionary();

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                var result = await cartDictionary.TryGetValueAsync(tx, username);
                if (!result.HasValue)
                {
                    return Ok();
                }

                var emptyCart = new Cart();
                await cartDictionary.SetAsync(tx, username, emptyCart);
                await tx.CommitAsync();

                return Ok();
            }
        }

        [HttpDelete("{username}/{productid}")]
        public async Task<IActionResult> Delete(string username, int productid)
        {
            var cartDictionary = await this.stateManager.GetCartDictionary();

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                var result = await cartDictionary.TryGetValueAsync(tx, username);
                if (!result.HasValue)
                {
                    return Ok();
                }

                var cart = result.Value;
                var item = cart.Items.FirstOrDefault(x => x.ProductId == productid);
                if (item == null)
                {
                    return Ok();
                }

                var updatedCart = cart.RemoveItem(item.ProductId);
                await cartDictionary.SetAsync(tx, username, updatedCart);
                await tx.CommitAsync();

                return Ok();
            }
        }
    }
}
