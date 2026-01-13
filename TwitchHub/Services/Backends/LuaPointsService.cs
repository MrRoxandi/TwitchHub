using Microsoft.EntityFrameworkCore;
using TwitchHub.Services.Backends.Data;
using TwitchHub.Services.Backends.Entities;

namespace TwitchHub.Services.Backends;

public sealed class LuaPointsService(IDbContextFactory<PointsDbContext> contextFactory)
{
    private readonly IDbContextFactory<PointsDbContext> _contextFactory = contextFactory;

    public async Task<long> GetBalanceAsync(string userId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var user = await context.UserPoints.FindAsync(userId);
        return user?.Balance ?? 0;
    }

    public async Task SetBalanceAsync(string userId, long amount)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var user = await context.UserPoints.FindAsync(userId);

        if (user is null)
        {
            user = new UserPoints { UserId = userId, Balance = amount };
            _ = context.UserPoints.Add(user);
        }
        else
        {
            user.Balance = amount;
        }

        _ = await context.SaveChangesAsync();
    }

    public async Task AddBalanceAsync(string userId, long amount)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var user = await context.UserPoints.FindAsync(userId);

        if (user is null)
        {
            user = new UserPoints { UserId = userId, Balance = amount };
            _ = context.UserPoints.Add(user);
        }
        else
        {
            user.Balance += amount;
        }

        _ = await context.SaveChangesAsync();
    }

    public async Task<bool> TakeBalanceAsync(string userId, long amount)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var user = await context.UserPoints.FindAsync(userId);

        if (user is null || user.Balance < amount)
        {
            return false;
        }

        user.Balance -= amount;
        _ = await context.SaveChangesAsync();
        return true;
    }
}
