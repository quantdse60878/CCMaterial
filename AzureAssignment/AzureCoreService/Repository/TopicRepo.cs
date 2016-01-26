using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureCoreService.Entity;

namespace AzureCoreService.Repository
{
    public class TopicRepo : GenericRepository<Topic>
    {
        public TopicRepo(AzureDbContext context) : base(context)
        {
            
        }
    }
}
