using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureCoreService.Entity;

namespace AzureCoreService.Repository
{
    class NewsKeywordRepo: GenericRepository<NewsKeyword>
    {
        public NewsKeywordRepo(AzureDbContext context) :
            base(context)
        {
            
        }
    }
}
