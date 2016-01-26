using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureCoreService.Entity;

namespace AzureCoreService.Repository
{
    public class NewsRepo: GenericRepository<News>
    {
        public NewsRepo(AzureDbContext context)
            : base(context)
        { }
    }
}
