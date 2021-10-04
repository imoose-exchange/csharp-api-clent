using System;
using System.Collections.Generic;
using System.Text;

namespace ImooseApiClient.Models
{
    public class OrderBook
    {

        public OrderBookItem[] bids { get; set; }
        public  OrderBookItem[] asks { get; set; }
    }
}
