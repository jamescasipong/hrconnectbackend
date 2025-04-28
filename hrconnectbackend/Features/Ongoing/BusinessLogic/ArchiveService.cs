using hrconnectbackend.Data;

namespace hrconnectbackend.Features.Ongoing.BusinessLogic;

public class ArchiveService(DataContext context)
{
    // public async Task ArchiveOldOrders(DateTime cutoffDate)
    // {
    //     var ordersToArchive = await context.Orders
    //         .Where(o => o.OrderDate < cutoffDate)
    //         .ToListAsync();
    //                                         
    //     foreach (var order in ordersToArchive)
    //     {
    //         // Add to Archive
    //         var archivedOrder = new ArchivedOrder
    //         {
    //             Id = order.Id,
    //             CustomerId = order.CustomerId,
    //             OrderDate = order.OrderDate,
    //             Total = order.Total,
    //             ArchivedDate = DateTime.UtcNow
    //         };
    //         context.ArchivedOrders.Add(archivedOrder);
    //
    //         // Remove from Active Orders
    //         context.Orders.Remove(order);
    //     }
    //
    //     // Save changes to both tables
    //     await context.SaveChangesAsync();
    // }
}
