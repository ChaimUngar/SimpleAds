using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAdsAuth.Data
{
    public class Ad
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public string Details { get; set; }
        public DateTime DateCreated { get; set; }
        public int ListerId { get; set; }
        public string ListerName { get; set; }
        public bool CanDelete { get; set; }
    }
}
