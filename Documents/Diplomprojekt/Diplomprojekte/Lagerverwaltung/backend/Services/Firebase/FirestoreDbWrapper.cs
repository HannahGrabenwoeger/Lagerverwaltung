using Google.Cloud.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Services.Firestore
{
    public class FirestoreDbWrapper : IFirestoreDbWrapper
    {
        private readonly FirestoreDb _firestoreDb;

        public FirestoreDbWrapper(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<IEnumerable<object>> GetWarehousesAsync()
        {
            var snapshot = await _firestoreDb.Collection("warehouses").GetSnapshotAsync();
            var warehouses = snapshot.Documents.Select(doc => new
            {
                Id = doc.Id,
                Name = doc.GetValue<string>("Name"),
                Location = doc.GetValue<string>("Location"),
            }).ToList();

            return warehouses;
        }

        public async Task<IEnumerable<object>> GetProductsByWarehouseIdAsync(string warehouseId)
        {
            var docRef = _firestoreDb.Collection("warehouses").Document(warehouseId);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists || !snapshot.ContainsField("Products"))
                return new List<object>();

            var productsRaw = snapshot.GetValue<List<Dictionary<string, object>>>("Products");

            var products = productsRaw.Select(p => new
            {
                Id = p.ContainsKey("Id") ? p["Id"]?.ToString() : "",
                Name = p.ContainsKey("Name") ? p["Name"]?.ToString() : "",
                Quantity = p.ContainsKey("Quantity") ? Convert.ToInt32(p["Quantity"]) : 0,
                MinimumStock = p.ContainsKey("MinimumStock") ? Convert.ToInt32(p["MinimumStock"]) : 0
            }).ToList();

            return products;
        }
    }
}