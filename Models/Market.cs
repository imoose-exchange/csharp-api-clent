using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImooseApiClient.Models
{
    public class Market
    {




		public String id {get;set;}

		public String name {get;set;}
		[JsonProperty("base_id")]
		public String baseId {get;set;}
		[JsonProperty("quote_id")]
		public String quoteId {get;set;}
		[JsonProperty("base_name")]
		public String baseName {get;set;}
		[JsonProperty("quote_name")]
		public String quoteName {get;set;}
		private String type {get;set;}
		[JsonProperty("base_precision")]
		public int basePrecision {get;set;}
		[JsonProperty("quote_precision")]
		public int quotePrecision {get;set;}

		[JsonProperty("buy_min_volume")]
		public Decimal buyMinVolume {get;set;}
		[JsonProperty("buy_max_volume")]
		public Decimal buyMaxVolume {get;set;}
		[JsonProperty("sell_min_volume")]
		public Decimal sellMinVolume {get;set;}
		[JsonProperty("sell_max_volume")]
		public Decimal sellMaxVolume {get;set;}





	}
}
