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
namespace Kwality.UVault.M2M.QA.Internal.Factories;

using Kwality.UVault.Core.Extensions;
using Kwality.UVault.M2M.Extensions;
using Kwality.UVault.M2M.Managers;
using Kwality.UVault.M2M.Models;
using Kwality.UVault.M2M.Options;

using Microsoft.Extensions.DependencyInjection;

internal sealed class ApplicationManagerFactory
{
    private readonly IServiceCollection serviceCollection = new ServiceCollection();

    public ApplicationManager<TModel, TKey> Create<TModel, TKey>()
        where TModel : ApplicationModel<TKey>
        where TKey : IEquatable<TKey>
    {
        this.serviceCollection.AddUVault(static options => options.UseApplicationManagement<TModel, TKey>());

        return this.serviceCollection.BuildServiceProvider()
                   .GetRequiredService<ApplicationManager<TModel, TKey>>();
    }

    public ApplicationManager<TModel, TKey> Create<TModel, TKey>(
        Action<ApplicationManagementOptions<TModel, TKey>> action)
        where TModel : ApplicationModel<TKey>
        where TKey : IEquatable<TKey>
    {
        ArgumentNullException.ThrowIfNull(action);
        this.serviceCollection.AddUVault(options => options.UseApplicationManagement(action));

        return this.serviceCollection.BuildServiceProvider()
                   .GetRequiredService<ApplicationManager<TModel, TKey>>();
    }
}
