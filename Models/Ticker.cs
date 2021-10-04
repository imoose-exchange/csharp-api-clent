using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImooseApiClient.Models
{
    public class Ticker
    {

		[JsonProperty("market_id")]
	public String marketId { get; set; }
		public Decimal open { get; set; }
		public Decimal volume { get; set; }
		public Decimal low { get; set; }
		public Decimal high { get; set; }
		public Decimal last { get; set; }
		public Decimal sell { get; set; }
		public Decimal buy { get; set; }

	}
}
