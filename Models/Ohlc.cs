using System;
using System.Collections.Generic;
using System.Text;

namespace ImooseApiClient.Models
{
    public class Ohlc
    {
		public long time { get; set; }
		public Decimal open { get; set; }
		public Decimal high { get; set; }
		public Decimal low { get; set; }
		public Decimal close { get; set; }
		public Decimal volume { get; set; }
	}
}
