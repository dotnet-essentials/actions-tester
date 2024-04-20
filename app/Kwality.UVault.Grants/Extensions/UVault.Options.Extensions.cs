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
namespace Kwality.UVault.Grants.Extensions;

using Kwality.UVault.Core.Options;
using Kwality.UVault.Grants.Internal.Stores;
using Kwality.UVault.Grants.Managers;
using Kwality.UVault.Grants.Models;
using Kwality.UVault.Grants.Options;
using Kwality.UVault.Grants.Stores.Abstractions;

using Microsoft.Extensions.DependencyInjection;

public static class UVaultOptionsExtensions
{
    public static void UseGrantManagement<TModel, TKey>(this UVaultOptions options)
        where TModel : GrantModel<TKey>
        where TKey : IEquatable<TKey>
    {
        options.UseGrantManagement<TModel, TKey>(ServiceLifetime.Scoped);
    }

    public static void UseGrantManagement<TModel, TKey>(this UVaultOptions options, ServiceLifetime storeLifetime)
        where TModel : GrantModel<TKey>
        where TKey : IEquatable<TKey>
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Services.AddScoped<GrantManager<TModel, TKey>>();

        options.Services.Add(new ServiceDescriptor(typeof(IGrantStore<TModel, TKey>), typeof(StaticStore<TModel, TKey>),
            storeLifetime));
    }

    public static void UseGrantManagement<TModel, TKey>(
        this UVaultOptions options, Action<GrantManagementOptions<TModel, TKey>> action)
        where TModel : GrantModel<TKey>
        where TKey : IEquatable<TKey>
    {
        options.UseGrantManagement(action, ServiceLifetime.Scoped);
    }

    public static void UseGrantManagement<TModel, TKey>(
        this UVaultOptions options, Action<GrantManagementOptions<TModel, TKey>> action, ServiceLifetime storeLifetime)
        where TModel : GrantModel<TKey>
        where TKey : IEquatable<TKey>
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(action);
        options.Services.AddScoped<GrantManager<TModel, TKey>>();

        options.Services.Add(new ServiceDescriptor(typeof(IGrantStore<TModel, TKey>), typeof(StaticStore<TModel, TKey>),
            storeLifetime));

        action.Invoke(new GrantManagementOptions<TModel, TKey>(options.Services));
    }
}
