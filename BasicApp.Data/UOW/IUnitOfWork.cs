using BasicApp.Data.Domain;
using BasicApp.Data.Repository;

namespace BasicApp.Data.UOW;

public interface IUnitOfWork
{
    Task CompleteAsync(CancellationToken cancellationToken);
    void CompleteTransactionAsync(CancellationToken cancellationToken);
    IGenericRepository<User> UserRepository { get; }
}
