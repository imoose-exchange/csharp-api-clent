using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImooseApiClient.Models
{
    public class Order
    {


		public String id { get; set; }
		public String market { get; set; }
		public String side { get; set; }
		public String state { get; set; }
		public String type { get; set; }
		[JsonProperty("portfolio_id")]
	public String portfolioId { get; set; }

		public Decimal price { get; set; }
		public Decimal volume { get; set; }

		public Decimal balance { get; set; }
		[JsonProperty("balance_origional")]

	public Decimal balanceOrigional;
		public Decimal recivied { get; set; }
		[JsonProperty("trade_count")]

	public int tradeCount { get; set; } 
		public Decimal fee { get; set; }


		public String[] trades { get; set; }

		[JsonProperty("created_at")]

		public long createdAt { get; set; }
		[JsonProperty("updated_at")]

		public long updatedAt { get; set; }
	}
}
