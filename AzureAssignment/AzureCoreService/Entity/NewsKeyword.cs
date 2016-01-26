using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace AzureCoreService.Entity
{
    [Table("News_Keyword")]
    public class NewsKeyword
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public News IdNews { get; set; }
        public Keyword IdKeyword { get; set; }

        [Column("TF")]
        public float Tf { get; set; }

        [Column("TFIDF")]
        public float TfIdf { get; set; }

    }
}
