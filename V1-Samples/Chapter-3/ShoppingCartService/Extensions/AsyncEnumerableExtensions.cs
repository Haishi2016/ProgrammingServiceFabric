using Microsoft.ServiceFabric.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShoppingCartService.Extensions
{
    public static class AsyncEnumerableExtensions
    {
        /// <summary>
        /// Converts the collection to a list
        /// </summary>
        /// <typeparam name="TValType">value type of the collection</typeparam>
        /// <param name="enumerator">enumerator to convert</param>
        /// <param name="ct">cancellation token for the async operations</param>
        /// <param name="tx">tx to enforce that this is called in a transactional context</param>
        /// <returns>a list containing all elements in the origin collection</returns>
        public static async Task<IList<TValType>> ToListAsync<TValType>(
            this IAsyncEnumerator<TValType> enumerator, CancellationToken ct, ITransaction tx)
        {
            IList<TValType> ret = new List<TValType>();
            while (await enumerator.MoveNextAsync(ct).ConfigureAwait(false))
            {
                ret.Add(enumerator.Current);
            }
            return ret;
        }

        /// <summary>
        /// Converts the collection to a list
        /// </summary>
        /// <typeparam name="TValType">value type of the collection</typeparam>
        /// <param name="enumerator">enumerator to convert</param>
        /// <param name="tx">tx to enforce that this is called in a transactional context</param>
        /// <returns>a list containing all elements in the origin collection</returns>
        public static Task<IList<TValType>> ToListAsync<TValType>(
            this IAsyncEnumerator<TValType> enumerator, ITransaction tx)
        {
            return enumerator.ToListAsync(CancellationToken.None, tx);
        }

        /// <summary>
        /// Converts the collection to a list
        /// </summary>
        /// <typeparam name="TValType">value type of the collection</typeparam>
        /// <param name="enumerable">enumerator to convert</param>
        /// <param name="ct">cancellation token for the async operations</param>
        /// <param name="tx">tx to enforce that this is called in a transactional context</param>
        /// <returns>a list containing all elements in the origin collection</returns>
        public static Task<IList<TValType>> ToListAsync<TValType>(this IAsyncEnumerable<TValType> enumerable,
            CancellationToken ct, ITransaction tx)
        {
            return enumerable.GetAsyncEnumerator().ToListAsync(ct, tx);
        }

        /// <summary>
        /// Converts the collection to a list
        /// </summary>
        /// <typeparam name="TValType">value type of the collection</typeparam>
        /// <param name="enumerable">enumerator to convert</param>
        /// <param name="tx">tx to enforce that this is called in a transactional context</param>
        /// <returns>a list containing all elements in the origin collection</returns>
        public static Task<IList<TValType>> ToListAsync<TValType>(this IAsyncEnumerable<TValType> enumerable, ITransaction tx)
        {
            return enumerable.GetAsyncEnumerator().ToListAsync(tx);
        }

    }
}
