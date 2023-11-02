using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Book.Models
{
    [NotMapped]

    public class Frequency
    {
        public int FrequencyID { get; set; }

        public string FrequencyName { get; set;}   

    }
}
