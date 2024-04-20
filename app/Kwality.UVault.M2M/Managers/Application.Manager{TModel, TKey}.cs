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
namespace Kwality.UVault.M2M.Managers;

using Kwality.UVault.Core.Models;
using Kwality.UVault.M2M.Models;
using Kwality.UVault.M2M.Operations.Filters.Abstractions;
using Kwality.UVault.M2M.Operations.Mappers.Abstractions;
using Kwality.UVault.M2M.Stores.Abstractions;

public class ApplicationManager<TModel, TKey>(IApplicationStore<TModel, TKey> store)
    where TModel : ApplicationModel<TKey>
    where TKey : IEquatable<TKey>
{
    public Task<PagedResultSet<TModel>> GetAllAsync(int pageIndex, int pageSize)
    {
        return this.GetAllAsync(pageIndex, pageSize, null);
    }

    public Task<PagedResultSet<TModel>> GetAllAsync(int pageIndex, int pageSize, IApplicationFilter? filter)
    {
        return store.GetAllAsync(pageIndex, pageSize, filter);
    }

    public Task<TModel> GetByKeyAsync(TKey key)
    {
        return store.GetByKeyAsync(key);
    }

    public Task<TKey> CreateAsync(TModel model, IApplicationOperationMapper mapper)
    {
        return store.CreateAsync(model, mapper);
    }

    public Task UpdateAsync(TKey key, TModel model, IApplicationOperationMapper mapper)
    {
        return store.UpdateAsync(key, model, mapper);
    }

    public Task DeleteByKeyAsync(TKey key)
    {
        return store.DeleteByKeyAsync(key);
    }

    public Task<TModel> RotateClientSecretAsync(TKey key)
    {
        return store.RotateClientSecretAsync(key);
    }
}
