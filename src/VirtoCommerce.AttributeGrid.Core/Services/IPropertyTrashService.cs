using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.AttributeGrid.Core.Models;

namespace VirtoCommerce.AttributeGrid.Core.Services;

public interface IPropertyTrashService
{
    Task<IList<PropertyTrashEntry>> GetTrashEntriesAsync();
    Task MoveToTrashAsync(string propertyId, string deletedBy);
    Task RestoreFromTrashAsync(string trashEntryId);
    Task DeletePermanentlyAsync(string propertyId);
    Task CleanupExpiredAsync();
}
