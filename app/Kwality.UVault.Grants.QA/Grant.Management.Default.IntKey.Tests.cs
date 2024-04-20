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
namespace Kwality.UVault.Grants.QA;

using System.Diagnostics.CodeAnalysis;

using AutoFixture.Xunit2;

using FluentAssertions;

using JetBrains.Annotations;

using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Core.Helpers;
using Kwality.UVault.Core.Keys;
using Kwality.UVault.Core.Models;
using Kwality.UVault.Grants.Managers;
using Kwality.UVault.Grants.Models;
using Kwality.UVault.Grants.Operations.Filters.Abstractions;
using Kwality.UVault.Grants.Operations.Mappers;
using Kwality.UVault.Grants.QA.Internal.Factories;
using Kwality.UVault.QA.Common.Xunit.Traits;

using Xunit;

[SuppressMessage("ReSharper", "MemberCanBeFileLocal")]
public sealed class GrantManagementDefaultIntKeyTests
{
    private const int pageSize = 100;
    private readonly GrantManager<Model, IntKey> manager = new GrantManagerFactory().Create<Model, IntKey>();

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Get all (pageIndex: 0, all data showed) succeeds.")]
    internal async Task GetAll_FirstPageWhenAllDataShowed_Succeeds(Model model)
    {
        // ARRANGE.
        await this.manager.CreateAsync(model, new GrantCreateOperationMapper())
                  .ConfigureAwait(true);

        // ACT.
        PagedResultSet<Model> result = await this.manager.GetAllAsync(0, 10)
                                                 .ConfigureAwait(true);

        // ASSERT.
        result.HasNextPage.Should()
              .BeFalse();

        result.ResultSet.Count()
              .Should()
              .Be(1);

        result.ResultSet.Take(1)
              .First()
              .Should()
              .BeEquivalentTo(model);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Get all (pageIndex: 1, all data showed) succeeds.")]
    internal async Task GetAll_SecondPageWhenAllDataShowed_Succeeds(Model model)
    {
        // ARRANGE.
        await this.manager.CreateAsync(model, new GrantCreateOperationMapper())
                  .ConfigureAwait(true);

        // ACT.
        PagedResultSet<Model> result = await this.manager.GetAllAsync(1, 10)
                                                 .ConfigureAwait(true);

        // ASSERT.
        result.HasNextPage.Should()
              .BeFalse();

        result.ResultSet.Count()
              .Should()
              .Be(0);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Get all (pageIndex: 0, all data NOT showed) succeeds.")]
    internal async Task GetAll_FirstPageWhenNotAllDataShowed_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        await this.manager.CreateAsync(modelOne, new GrantCreateOperationMapper())
                  .ConfigureAwait(true);

        await this.manager.CreateAsync(modelTwo, new GrantCreateOperationMapper())
                  .ConfigureAwait(true);

        // ACT.
        PagedResultSet<Model> result = await this.manager.GetAllAsync(0, 1)
                                                 .ConfigureAwait(true);

        // ASSERT.
        result.HasNextPage.Should()
              .BeTrue();

        result.ResultSet.Count()
              .Should()
              .Be(1);

        result.ResultSet.Take(1)
              .First()
              .Should()
              .BeEquivalentTo(modelOne);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Get all (pageIndex: 1, all data NOT showed) succeeds.")]
    internal async Task GetAll_SecondPageWhenNotAllDataShowed_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        await this.manager.CreateAsync(modelOne, new GrantCreateOperationMapper())
                  .ConfigureAwait(true);

        await this.manager.CreateAsync(modelTwo, new GrantCreateOperationMapper())
                  .ConfigureAwait(true);

        // ACT.
        PagedResultSet<Model> result = await this.manager.GetAllAsync(1, 1)
                                                 .ConfigureAwait(true);

        // ASSERT.
        result.HasNextPage.Should()
              .BeFalse();

        result.ResultSet.Count()
              .Should()
              .Be(1);

        result.ResultSet.Take(1)
              .First()
              .Should()
              .BeEquivalentTo(modelTwo);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Get all with filter succeeds.")]
    internal async Task GetAll_WithFilter_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        await this.manager.CreateAsync(modelOne, new GrantCreateOperationMapper())
                  .ConfigureAwait(true);

        await this.manager.CreateAsync(modelTwo, new GrantCreateOperationMapper())
                  .ConfigureAwait(true);

        PagedResultSet<Model> result = await this.manager.GetAllAsync(0, 10, new OperationFilter(modelTwo.Scopes))
                                                 .ConfigureAwait(true);

        // ASSERT.
        result.ResultSet.Count()
              .Should()
              .Be(1);

        result.ResultSet.First()
              .Should()
              .BeEquivalentTo(modelTwo);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Create succeeds.")]
    internal async Task Create_Succeeds(Model model)
    {
        // ACT.
        await this.manager.CreateAsync(model, new GrantCreateOperationMapper())
                  .ConfigureAwait(true);

        // ASSERT.
        (await this.manager.GetAllAsync(0, pageSize)
                   .ConfigureAwait(true)).ResultSet.Should()
                                         .ContainEquivalentOf(model);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Update succeeds.")]
    internal async Task Update_Succeeds(Model model)
    {
        // ARRANGE.
        IntKey key = await this.manager.CreateAsync(model, new GrantCreateOperationMapper())
                               .ConfigureAwait(true);

        // ACT.
        model.Scopes = new[] { "newScope", "newScope2" };

        await this.manager.UpdateAsync(key, model, new GrantUpdateOperationMapper())
                  .ConfigureAwait(true);

        // ASSERT.
        (await this.manager.GetAllAsync(0, pageSize)
                   .ConfigureAwait(true)).ResultSet.Count()
                                         .Should()
                                         .Be(1);

        (await this.manager.GetAllAsync(0, pageSize)
                   .ConfigureAwait(true)).ResultSet.Should()
                                         .ContainEquivalentOf(model);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Update raises an exception when the key is not found.")]
    internal async Task Update_UnknownKey_RaisesException(IntKey key, Model model)
    {
        // ACT.
        Func<Task> act = () => this.manager.UpdateAsync(key, model, new GrantUpdateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UpdateException>()
                 .WithMessage($"Failed to update client grant: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // ARRANGE.
        IntKey key = await this.manager.CreateAsync(model, new GrantCreateOperationMapper())
                               .ConfigureAwait(true);

        // ACT.
        await this.manager.DeleteByKeyAsync(key)
                  .ConfigureAwait(true);

        // ASSERT.
        (await this.manager.GetAllAsync(0, pageSize)
                   .ConfigureAwait(true)).ResultSet.Should()
                                         .BeEmpty();
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Delete succeeds when the key is not found.")]
    internal async Task Delete_UnknownKey_Succeeds(IntKey key)
    {
        // ACT.
        Func<Task> act = () => this.manager.DeleteByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .NotThrowAsync()
                 .ConfigureAwait(true);
    }

    private sealed class OperationFilter(IEnumerable<string> scopes) : IGrantFilter
    {
        public TDestination Create<TDestination>()
            where TDestination : class
        {
            if (typeof(TDestination) != typeof(Func<Model, bool>))
            {
                throw new ReadException(
                    $"Invalid {nameof(IGrantFilter)}: Destination is NOT `{typeof(Func<Model, bool>).Name}`.");
            }

            return ((Func<Model, bool>)Filter).UnsafeAs<Func<Model, bool>, TDestination>();

            // The filter which is filters out data in the store.
            bool Filter(Model model)
            {
                return Equals(model.Scopes, scopes);
            }
        }
    }

    
#pragma warning disable CA1812
    internal sealed class Model(IntKey key) : GrantModel<IntKey>(key)
#pragma warning restore CA1812
    {
        public IEnumerable<string> Scopes { get; set; } = Array.Empty<string>();
    }
}
