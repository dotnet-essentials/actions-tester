// =====================================================================================================================
// = LICENSE:       Copyright (c) 2023 Kevin De Coninck
// =
// =                Permission is hereby granted, free of charge, to any person
// =                obtaining a copy of this software and associated documentation
// =                files (the "Software"), to deal in the Software without
// =                restriction, including without limitation the rights to use,
// =                copy, modify, merge, publish, distribute, sublicense, and/or sell
// =                copies of the Software, and to permit persons to whom the
// =                Software is furnished to do so, subject to the following
// =                conditions:
// =
// =                The above copyright notice and this permission notice shall be
// =                included in all copies or substantial portions of the Software.
// =
// =                THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// =                EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// =                OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// =                NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// =                HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// =                WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// =                FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// =                OTHER DEALINGS IN THE SOFTWARE.
// =====================================================================================================================
namespace Kwality.UVault.Users.Managers;

using JetBrains.Annotations;

using Kwality.UVault.Users.Models;
using Kwality.UVault.Users.Operations.Mappers.Abstractions;
using Kwality.UVault.Users.Stores.Abstractions;

[PublicAPI]
public class UserManager<TModel, TKey, TData>(IUserStore<TModel, TKey> store, IUserDataStore<TData, TKey> dataStore)
    where TModel : UserModel<TKey>
    where TKey : IEquatable<TKey>
    where TData : class
{
    public Task<TModel> GetByKeyAsync(TKey key)
    {
        return store.GetByKeyAsync(key);
    }

    public Task<IEnumerable<TModel>> GetByEmailAsync(string email)
    {
        return store.GetByEmailAsync(email);
    }

    public async Task<TKey> CreateAsync(
        TModel model, TData data, IUserOperationMapper mapper, IUserDataOperationMapper dataMapper)
    {
        TKey key = await store.CreateAsync(model, mapper)
                              .ConfigureAwait(false);

        await dataStore.CreateAsync(key, data, dataMapper)
                       .ConfigureAwait(false);

        return key;
    }

    public Task UpdateAsync(TKey key, TModel model, IUserOperationMapper mapper)
    {
        return store.UpdateAsync(key, model, mapper);
    }

    public Task DeleteByKeyAsync(TKey key)
    {
        return store.DeleteByKeyAsync(key);
    }
}
