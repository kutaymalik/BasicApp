using BasicApp.Data.Context;
using BasicApp.Data.Domain;
using BasicApp.Data.Repository;

namespace BasicApp.Data.UOW;

public class UnitOfWork : IUnitOfWork
{
    private readonly BasicAppDbContext dbContext;

    public UnitOfWork(BasicAppDbContext dbContext)
    {
        this.dbContext = dbContext;
        UserRepository = new GenericRepository<User>(dbContext);
    }

    public IGenericRepository<User> UserRepository { get; private set; }

    public async Task CompleteAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async void CompleteTransactionAsync(CancellationToken cancellationToken)
    {
        using (var transaction = dbContext.Database.BeginTransaction())
        {
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine("CompleteTransaction", ex);
            }
        }
    }
}
