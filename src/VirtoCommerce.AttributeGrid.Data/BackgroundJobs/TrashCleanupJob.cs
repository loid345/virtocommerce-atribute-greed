using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.AttributeGrid.Core.Services;

namespace VirtoCommerce.AttributeGrid.Data.BackgroundJobs;

public class TrashCleanupJob
{
    private readonly IPropertyTrashService _trashService;

    public TrashCleanupJob(IPropertyTrashService trashService)
    {
        _trashService = trashService;
    }

    [DisableConcurrentExecution(60 * 60)]
    public async Task Process()
    {
        await _trashService.CleanupExpiredAsync();
    }
}
