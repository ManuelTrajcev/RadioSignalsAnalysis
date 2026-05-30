using System.Linq.Expressions;
using Domain.Domain_Models;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Repository.Interface;

namespace Services.Tests.TestHelpers;

/// <summary>
/// Helpers that collapse the verbose four-argument <see cref="IRepository{T}"/> query setups
/// (selector + optional predicate/orderBy/include) into one call. The selector/predicate
/// expressions are matched with <c>It.IsAny</c> because EF translates them — when the repo is
/// mocked the projection never executes, so these tests assert call-through + in-process logic,
/// not the EF projection itself.
/// </summary>
public static class RepositoryMockExtensions
{
    /// <summary>Set up <c>GetAsync</c> (single entity / projected value) to return <paramref name="result"/>.</summary>
    public static Mock<IRepository<T>> SetupGet<T, TResult>(this Mock<IRepository<T>> mock, TResult result)
        where T : BaseEntity
    {
        mock.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<T, TResult>>>(),
                It.IsAny<Expression<Func<T, bool>>>(),
                It.IsAny<Func<IQueryable<T>, IOrderedQueryable<T>>>(),
                It.IsAny<Func<IQueryable<T>, IIncludableQueryable<T, object>>>()))
            .ReturnsAsync(result);
        return mock;
    }

    /// <summary>Set up <c>GetAllAsync</c> to return <paramref name="result"/>.</summary>
    public static Mock<IRepository<T>> SetupGetAll<T, TResult>(this Mock<IRepository<T>> mock, IEnumerable<TResult> result)
        where T : BaseEntity
    {
        mock.Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<T, TResult>>>(),
                It.IsAny<Expression<Func<T, bool>>>(),
                It.IsAny<Func<IQueryable<T>, IOrderedQueryable<T>>>(),
                It.IsAny<Func<IQueryable<T>, IIncludableQueryable<T, object>>>()))
            .ReturnsAsync(result);
        return mock;
    }

    /// <summary>Set up <c>InsertAsync</c> to echo back the entity it was given.</summary>
    public static Mock<IRepository<T>> SetupInsertEcho<T>(this Mock<IRepository<T>> mock) where T : BaseEntity
    {
        mock.Setup(r => r.InsertAsync(It.IsAny<T>())).ReturnsAsync((T e) => e);
        return mock;
    }

    /// <summary>Set up <c>UpdateAsync</c> to echo back the entity it was given.</summary>
    public static Mock<IRepository<T>> SetupUpdateEcho<T>(this Mock<IRepository<T>> mock) where T : BaseEntity
    {
        mock.Setup(r => r.UpdateAsync(It.IsAny<T>())).ReturnsAsync((T e) => e);
        return mock;
    }

    /// <summary>Set up <c>DeleteAsync</c> to echo back the entity it was given.</summary>
    public static Mock<IRepository<T>> SetupDeleteEcho<T>(this Mock<IRepository<T>> mock) where T : BaseEntity
    {
        mock.Setup(r => r.DeleteAsync(It.IsAny<T>(), It.IsAny<Guid>())).ReturnsAsync((T e, Guid _) => e);
        return mock;
    }
}
