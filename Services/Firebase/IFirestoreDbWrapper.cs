using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Services.Firestore
{
    public interface IFirestoreDbWrapper
    {
        Task<IEnumerable<object>> GetWarehousesAsync();
        Task<IEnumerable<object>> GetProductsByWarehouseIdAsync(string warehouseId);
    }
}