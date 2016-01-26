using AzureCoreService.Entity;

namespace AzureCoreService.Repository
{
    public class AdRepo : GenericRepository<Ad>
    {
        public AdRepo(AzureDbContext context) : base(context)
        {
        }
    }
}