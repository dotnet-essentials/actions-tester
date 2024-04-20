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
namespace Kwality.UVault.APIs.QA;

using System.Diagnostics.CodeAnalysis;

using AutoFixture.Xunit2;

using FluentAssertions;

using JetBrains.Annotations;

using Kwality.UVault.APIs.Managers;
using Kwality.UVault.APIs.Models;
using Kwality.UVault.APIs.Operations.Mappers;
using Kwality.UVault.APIs.QA.Internal.Factories;
using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Core.Keys;
using Kwality.UVault.QA.Common.Xunit.Traits;

using Xunit;

[SuppressMessage("ReSharper", "MemberCanBeFileLocal")]
public sealed class ApiManagementDefaultIntKeyTests
{
    private readonly ApiManager<Model, IntKey> manager = new ApiManagerFactory().Create<Model, IntKey>();

    [AutoData]
    [ApiManagement]
    [Theory(DisplayName = "Get by key raises an exception when the key is NOT found.")]
    internal async Task GetByKey_UnknownKey_RaisesException(IntKey key)
    {
        // ACT.
        Func<Task<Model>> act = () => this.manager.GetByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Failed to read API: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [ApiManagement]
    [Theory(DisplayName = "Create succeeds.")]
    internal async Task Create_Succeeds(Model model)
    {
        // ARRANGE.
        IntKey key = await this.manager.CreateAsync(model, new CreateOperationMapper())
                               .ConfigureAwait(true);

        // ASSERT.
        (await this.manager.GetByKeyAsync(key)
                   .ConfigureAwait(true)).Should()
                                         .BeEquivalentTo(model);
    }

    [AutoData]
    [ApiManagement]
    [Theory(DisplayName = "Delete succeeds when the key is not found.")]
    internal async Task Delete_UnknownKey_Succeeds(IntKey key)
    {
        // ACT.
        await this.manager.DeleteByKeyAsync(key)
                  .ConfigureAwait(true);

        // ASSERT.
        Func<Task<Model>> act = () => this.manager.GetByKeyAsync(key);

        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Failed to read API: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [ApiManagement]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // ARRANGE.
        IntKey key = await this.manager.CreateAsync(model, new CreateOperationMapper())
                               .ConfigureAwait(true);

        // ACT.
        await this.manager.DeleteByKeyAsync(key)
                  .ConfigureAwait(true);

        // ASSERT.
        Func<Task<Model>> act = () => this.manager.GetByKeyAsync(key);

        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Failed to read API: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

#pragma warning disable CA1812
    internal sealed class Model(IntKey name) : ApiModel<IntKey>(name);
#pragma warning restore CA1812
}
