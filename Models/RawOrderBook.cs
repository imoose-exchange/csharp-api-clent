using System;
using System.Collections.Generic;
using System.Text;

namespace ImooseApiClient.Models
{
    class RawOrderBook
    {

        public string[][] asks { get; set; }
        public string[][] bids { get; set; }

    }
}
