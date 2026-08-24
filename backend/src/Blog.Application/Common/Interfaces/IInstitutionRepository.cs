using Blog.Domain.Entities;

namespace Blog.Application.Common.Interfaces;

public interface IInstitutionRepository : IRepository<Institution>
{
    Task<IReadOnlyList<Institution>> ListOrderedByNameAsync(
        CancellationToken cancellationToken = default);
}
