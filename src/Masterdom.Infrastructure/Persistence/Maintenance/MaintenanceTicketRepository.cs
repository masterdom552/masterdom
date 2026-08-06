using Masterdom.Modules.Maintenance.Domain.Entities.Maintenance;
using Masterdom.Modules.Maintenance.Domain.Repositories;

namespace Masterdom.Infrastructure.Persistence.Maintenance;

public sealed class MaintenanceTicketRepository : IMaintenanceTicketRepository
{
    private readonly MasterdomDbContext _dbContext;

    public MaintenanceTicketRepository(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Add(MaintenanceTicket maintenanceTicket)
    {
        ArgumentNullException.ThrowIfNull(maintenanceTicket);
        _dbContext.MaintenanceTickets.Add(maintenanceTicket);
    }

    public void Update(MaintenanceTicket maintenanceTicket)
    {
        ArgumentNullException.ThrowIfNull(maintenanceTicket);
        _dbContext.MaintenanceTickets.Update(maintenanceTicket);
    }

    public MaintenanceTicket? GetById(MaintenanceTicketId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _dbContext.MaintenanceTickets
            .FirstOrDefault(x => x.Id == id);
    }
}
