using Microsoft.EntityFrameworkCore;
using StandingOrderCase.Api.Enums;
using StandingOrderCase.Api.Models;
using StandingOrderCase.Api.Records;

namespace StandingOrderCase.Api.Services;

public class StandingOrderService
{
    private readonly StandingOrderCaseContext _context;
    private readonly NotificationService _notificationService;

    public StandingOrderService(StandingOrderCaseContext context, NotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public Task<GetStandingOrder?> Get(Guid id)
    {
        return _context
            .StandingOrders
            .Where(x => x.Id == id && x.OrderStatusEnum == OrderStatusEnum.Pending)
            .Select(x => new GetStandingOrder
                (
                    x.Id,
                    x.Amount,
                    x.OrderStatusEnum,
                    x.ExecutionDate,
                    x.CreatedAt
                )
            )
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    public Task<GetStandingOrder?> GetByUserId(Guid userId)
    {
        return _context
            .StandingOrders
            .Where(x => x.UserId == userId && x.OrderStatusEnum == OrderStatusEnum.Pending)
            .Select(x => new GetStandingOrder
                (
                    x.Id,
                    x.Amount,
                    x.OrderStatusEnum,
                    x.ExecutionDate,
                    x.CreatedAt
                )
            )
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    public Task<bool> Exists(Guid userId)
    {
        return _context.StandingOrders.AnyAsync(x =>
            x.UserId == userId && x.OrderStatusEnum == OrderStatusEnum.Pending);
    }

    public async Task<Guid> Create(CreateStandingOrder model)
    {
        var orderId = Guid.NewGuid();
        var order = new StandingOrder
        {
            Id = orderId,
            UserId = model.UserId,
            ExecutionDate = model.ExecutionDate,
            Amount = model.Amount
        };

        _context.StandingOrders.Add(order);

        var notifications = model.Notifications.Select(n =>
            new CreateNotification(
                model.UserId,
                order.Id,
                n,
                "Your standing order has been created")
        ).ToArray();

        await _notificationService.Create(notifications);
        
        await _context.SaveChangesAsync();

        return order.Id;
    }

    public async Task<bool> Cancel(Guid id)
    {
        var order = await _context
            .StandingOrders
            .SingleOrDefaultAsync(x => x.Id == id && x.OrderStatusEnum == OrderStatusEnum.Pending);

        if (order == null)
        {
            return false;
        }

        order.OrderStatusEnum = OrderStatusEnum.Cancelled;
        await _context.SaveChangesAsync();
        return true;
    }
}