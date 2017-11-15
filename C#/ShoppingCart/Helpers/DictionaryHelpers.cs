using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using ShoppingCart.Models;

namespace ShoppingCart.Helpers
{
    public static class DictionaryHelpers
    {
        public class Constants
        {
            public const string CART_DICTIONARY = "cart-dictionary";
        }

        public static async Task<IReliableDictionary<string, Cart>> GetCartDictionary(this IReliableStateManager stateManager)
        {
            return await stateManager.GetOrAddAsync<IReliableDictionary<string, Cart>>(Constants.CART_DICTIONARY);
        }
    }
}
