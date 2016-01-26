using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureCoreService.Entity;

namespace AzureCoreService
{
    public partial class AzureDbContext : DbContext
    {
        public AzureDbContext() : base("name=AzureDbContext")
        {
            
        }

        public AzureDbContext(string name) : base(name)
        {

        }

        public virtual DbSet<Ad> Ad { get; set; }

        public virtual DbSet<Category> Category { get; set; }

        public virtual DbSet<Keyword> Keyword { get; set; }

        public virtual DbSet<News> News { get; set; }

        public virtual DbSet<NewsKeyword> NewsKeyword { get; set; } 
    }
}
