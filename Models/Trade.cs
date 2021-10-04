using System;
using System.Collections.Generic;
using System.Text;

namespace ImooseApiClient.Models
{
    public class Trade
    {

        public Decimal price { get; set; }
        public Decimal volume { get; set; }
        public long time { get; set; }
    }
}
