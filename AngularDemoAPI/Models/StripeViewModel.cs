using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class createcardrequest
    {
        public createcardrequest()
        {
            address = new cardaddress();
        }
        public string fullname { get; set; }
        public DateTime birthdate { get; set; }
        public cardaddress address { get; set; }
        public long amount { get; set; }
    }

    public class virtualcardresponse
    {
        public bool success { get; set; } = false;
        public string message { get; set; }
    }

    public class cardaddress
    {
        public string line1 { get; set; }
        public string line2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postalcode { get; set; }
        public string country { get; set; }
    }
}
