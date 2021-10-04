using System;
using System.Collections.Generic;
using System.Text;

namespace ImooseApiClient.Models
{
    public class OrderQueryResult
    {

        public string from { get; set; }
        public Order[] orders { get; set;}

    }
}
