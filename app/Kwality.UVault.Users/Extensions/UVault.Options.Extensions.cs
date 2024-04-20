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
namespace Kwality.UVault.Users.Extensions;

using JetBrains.Annotations;

using Kwality.UVault.Core.Options;
using Kwality.UVault.Users.Internal.Stores;
using Kwality.UVault.Users.Managers;
using Kwality.UVault.Users.Models;
using Kwality.UVault.Users.Options;
using Kwality.UVault.Users.Stores.Abstractions;

using Microsoft.Extensions.DependencyInjection;

[PublicAPI]
public static class UVaultOptionsExtensions
{
    public static void UseUserManagement<TModel, TKey>(this UVaultOptions options)
        where TModel : UserModel<TKey>
        where TKey : IEquatable<TKey>
    {
        options.UseUserManagement<TModel, TKey>(ServiceLifetime.Scoped);
    }

    public static void UseUserManagement<TModel, TKey>(this UVaultOptions options, ServiceLifetime storeLifetime)
        where TModel : UserModel<TKey>
        where TKey : IEquatable<TKey>
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Services.AddScoped<UserManager<TModel, TKey>>();

        options.Services.Add(new ServiceDescriptor(typeof(IUserStore<TModel, TKey>), typeof(StaticStore<TModel, TKey>),
            storeLifetime));
    }

    public static void UseUserManagement<TModel, TKey>(
        this UVaultOptions options, Action<UserManagementOptions<TModel, TKey>> action)
        where TModel : UserModel<TKey>
        where TKey : IEquatable<TKey>
    {
        options.UseUserManagement(action, ServiceLifetime.Scoped);
    }

    public static void UseUserManagement<TModel, TKey>(
        this UVaultOptions options, Action<UserManagementOptions<TModel, TKey>> action, ServiceLifetime storeLifetime)
        where TModel : UserModel<TKey>
        where TKey : IEquatable<TKey>
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(action);
        options.Services.AddScoped<UserManager<TModel, TKey>>();

        options.Services.Add(new ServiceDescriptor(typeof(IUserStore<TModel, TKey>), typeof(StaticStore<TModel, TKey>),
            storeLifetime));

        action.Invoke(new UserManagementOptions<TModel, TKey>(options.Services));
    }

    public static void UseUserManagement<TModel, TKey, TData>(this UVaultOptions options)
        where TModel : UserModel<TKey>
        where TKey : IEquatable<TKey>
        where TData : class
    {
        options.UseUserManagement<TModel, TKey, TData>(ServiceLifetime.Scoped);
    }

    public static void UseUserManagement<TModel, TKey, TData>(this UVaultOptions options, ServiceLifetime storeLifetime)
        where TModel : UserModel<TKey>
        where TKey : IEquatable<TKey>
        where TData : class
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Services.AddScoped<UserManager<TModel, TKey, TData>>();

        options.Services.Add(new ServiceDescriptor(typeof(IUserStore<TModel, TKey>), typeof(StaticStore<TModel, TKey>),
            storeLifetime));
    }

    public static void UseUserManagement<TModel, TKey, TData>(
        this UVaultOptions options, Action<UserManagementOptions<TModel, TKey>> action)
        where TModel : UserModel<TKey>
        where TKey : IEquatable<TKey>
        where TData : class
    {
        options.UseUserManagement<TModel, TKey, TData>(action, ServiceLifetime.Scoped);
    }

    public static void UseUserManagement<TModel, TKey, TData>(
        this UVaultOptions options, Action<UserManagementOptions<TModel, TKey>> action, ServiceLifetime storeLifetime)
        where TModel : UserModel<TKey>
        where TKey : IEquatable<TKey>
        where TData : class
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(action);
        options.Services.AddScoped<UserManager<TModel, TKey, TData>>();

        options.Services.Add(new ServiceDescriptor(typeof(IUserStore<TModel, TKey>), typeof(StaticStore<TModel, TKey>),
            storeLifetime));

        action.Invoke(new UserManagementOptions<TModel, TKey>(options.Services));
    }
}
