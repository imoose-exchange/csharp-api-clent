using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ImooseApiClient.Models;
using ImooseApiClient.Models.Response;
using Newtonsoft.Json;

namespace ImooseApiClient
{
    public class ImooseClient
    {

        private readonly string _apiKey;
        private readonly string _secretKey;
        private readonly HttpClient HttpClient;
        private const string APIHeader = "Api-Key";
        private const string ContentTypeHeader = "Content-Type";
        private const string ContentType = "application/x-www-form-urlencodede";
        private const string BaseUri = "https://api.imoose.com";


        public ImooseClient(string apiKey, string apiSecret)
        {

            _apiKey = apiKey;
            _secretKey = apiSecret;

            var httpClientHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            };

            HttpClient = new HttpClient(httpClientHandler);
            HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(ContentTypeHeader, ContentType);
            HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(APIHeader, apiKey);
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private string getQueryString(Dictionary<string, string> parameters)
        {
            return "?" + string.Join("&",
                parameters.Select(kvp =>
                    string.Format("{0}={1}", kvp.Key, kvp.Value)));
        }

        private string CreateHMACSignature(string key, Dictionary<string, string> parameters)
        {
            string totalParams = string.Empty;
            string[] keysSorted = new string[parameters.Keys.Count];
            parameters.Keys.CopyTo(keysSorted, 0);
            Array.Sort(keysSorted);

            for (int i = 0; i < keysSorted.Length; i++)
            {
                if (i != keysSorted.Length - 1)
                {
                    totalParams += string.Format("{0}={1}&", keysSorted[i], parameters[keysSorted[i]]);
                }
                else
                {
                    totalParams += string.Format("{0}={1}", keysSorted[i], parameters[keysSorted[i]]);
                }
            }
            var messageBytes = Encoding.UTF8.GetBytes(totalParams);
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var hash = new HMACSHA256(keyBytes);
            var computedHash = hash.ComputeHash(messageBytes);
            return BitConverter.ToString(computedHash).Replace("-", "").ToLower();
        }

        private async Task<HttpResponseMessage> Request(Uri endpoint, HttpMethod verb, Dictionary<string, string> parameters)
        {
            Task<HttpResponseMessage> task = null;

            if (parameters == null)
            {
                parameters = new Dictionary<string, string>();
            }

            parameters.Add("timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());

            string signature = string.Empty;

            if (endpoint.AbsolutePath.Contains("/private"))
            {
                signature = CreateHMACSignature(_secretKey, parameters);
            }

            var taskFunction = new Func<Task<HttpResponseMessage>, Task<HttpResponseMessage>>(t =>
            {
                return t;
            });


            if (verb == HttpMethod.Get || verb == HttpMethod.Delete)
            {
                endpoint = new Uri(endpoint.ToString() + getQueryString(parameters));
            }

            using (var requestMessage =
             new HttpRequestMessage(verb, endpoint))
            {
                if (signature != string.Empty)
                {
                    requestMessage.Headers.Add("Api-Sign", signature);
                }
                if (verb == HttpMethod.Post || verb == HttpMethod.Put)
                {
                    requestMessage.Content = new FormUrlEncodedContent(parameters);
                }
                else
                {

                }

                task = await HttpClient.SendAsync(requestMessage).ContinueWith(taskFunction);
            }

            return await task;

        }

        private async Task<T> HandleResponse<T>(HttpResponseMessage message, string requestMessage) where T : class
        {
            if (message.IsSuccessStatusCode)
            {
                var messageJson = await message.Content.ReadAsStringAsync();
                ImooseResponse<T> messageObject = null;
                try
                {
                    messageObject = JsonConvert.DeserializeObject<ImooseResponse<T>>(messageJson);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                if (messageObject == null)
                {
                    throw new Exception("Unable to deserialize to provided type");
                }
                return messageObject.data;
            }

            var errorJson = await message.Content.ReadAsStringAsync();
            ImooseResponse<string> errorObject = JsonConvert.DeserializeObject<ImooseResponse<string>>(errorJson);
            if (errorObject == null) throw new Exception("Unexpected Error whilst handling the response", null);
            throw new Exception(errorObject.getErrorMessage(), null);
        }

        private async Task<T> ProcessRequest<T>(string endpoint, HttpMethod verb, Dictionary<string, string> parameters) where T : class
        {
            HttpResponseMessage message = await Request(new Uri(BaseUri + endpoint), verb, parameters);
            return await HandleResponse<T>(message, endpoint.ToString());
        }


        public async Task<ServerTime> GetServerTime()
        {
            return await ProcessRequest<ServerTime>("/v1/public/time", HttpMethod.Get, null);
        }

        public async Task<ServerStatus> GetServerStatus()
        {
            return await ProcessRequest<ServerStatus>("/v1/public/status", HttpMethod.Get, null);
        }


        public async Task<Asset> GetAsset(string id)
        {
            return await ProcessRequest<Asset>("/v1/public/asset", HttpMethod.Get, new Dictionary<string, string>() { { "id", id } });
        }

        public async Task<Asset[]> GetAssets()
        {
            return await ProcessRequest<Asset[]>("/v1/public/asset", HttpMethod.Get, null);
        }

        public async Task<Market> GetMarket(string id)
        {
            return await ProcessRequest<Market>("/v1/public/market", HttpMethod.Get, new Dictionary<string, string>() { { "id", id } });
        }

        public async Task<Market[]> GetMarkets()
        {
            return await ProcessRequest<Market[]>("/v1/public/market", HttpMethod.Get, null);
        }

        public async Task<Market[]> GetMarkets(string marketType)
        {
            return await ProcessRequest<Market[]>("/v1/public/market", HttpMethod.Get, new Dictionary<string, string>() { { "type", marketType } });
        }

        public async Task<Ticker> GetMarketTicker(string marketId)
        {
            return await ProcessRequest<Ticker>("/v1/public/ticker", HttpMethod.Get, new Dictionary<string, string>() { { "id", marketId } });
        }

        public async Task<Ticker[]> GetMarketTickers()
        {
            return await ProcessRequest<Ticker[]>("/v1/public/ticker", HttpMethod.Get, null);
        }

        public async Task<Ticker[]> GetMarketTickers(string marketType)
        {
            return await ProcessRequest<Ticker[]>("/v1/public/ticker", HttpMethod.Get, new Dictionary<string, string>() { { "type", marketType } });
        }

        public async Task<Trade[]> GetMarketTrades(string marketId)
        {
            return await getMarketTrades(marketId);
        }

        public async Task<Trade[]> GetMarketTrades(string marketId, int limit)
        {
            return await getMarketTrades(marketId, limit);
        }


        private async Task<Trade[]> getMarketTrades(string marketId, int limit = 0)
        {
            string[][] trades = await ProcessRequest<string[][]>("/v1/public/trade", HttpMethod.Get, new Dictionary<string, string>() { { "id", marketId }, { "limit", limit.ToString() } });

            Trade[] ProcessedTrade = new Trade[trades.GetUpperBound(0)];

            for (int i = 0; i < trades.GetUpperBound(0); i++)
            {

                Trade t = new Trade();

                Decimal price = 0;
                Decimal volume = 0;
                long time = 0;

                Decimal.TryParse(trades[i][0], out price);
                Decimal.TryParse(trades[i][1], out volume);
                long.TryParse(trades[i][2], out time);

                t.price = price;
                t.volume = volume;
                t.time = time;

                ProcessedTrade[i] = t;

            }

            return ProcessedTrade;
        }


        public async Task<OrderBook> GetMarketDepth(string marketId)
        {
            return await getMarketDepth(marketId);
        }

        public async Task<OrderBook> GetMarketDepth(string marketId, int limit)
        {
            return await getMarketDepth(marketId, limit);
        }

        private async Task<OrderBook> getMarketDepth(string marketId, int limit = 0)
        {
            RawOrderBook book = await ProcessRequest<RawOrderBook>("/v1/public/depth", HttpMethod.Get, new Dictionary<string, string>() { { "id", marketId }, { "limit", limit.ToString() } });


            OrderBookItem[] asks = new OrderBookItem[book.asks.GetUpperBound(0)];
            for (int i = 0; i < asks.GetUpperBound(0); i++)
            {
                OrderBookItem depthLevel = new OrderBookItem();
                Decimal price = 0;
                Decimal volume = 0;
                Decimal.TryParse(book.asks[i][0], out price);
                Decimal.TryParse(book.asks[i][1], out volume);
                depthLevel.price = price;
                depthLevel.volume = volume;
                asks[i] = depthLevel;

            }

            OrderBookItem[] bids = new OrderBookItem[book.bids.GetUpperBound(0)];
            for (int i = 0; i < bids.GetUpperBound(0); i++)
            {
                OrderBookItem depthLevel = new OrderBookItem();
                Decimal price = 0;
                Decimal volume = 0;
                Decimal.TryParse(book.bids[i][0], out price);
                Decimal.TryParse(book.bids[i][1], out volume);
                depthLevel.price = price;
                depthLevel.volume = volume;
                bids[i] = depthLevel;

            }

            return new OrderBook { asks = asks, bids = bids };

        }

        public async Task<Ohlc[]> GetOhlc(string marketId, int interval)
        {
            return await getOhlc(marketId, interval);
        }

        public async Task<Ohlc[]> GetOhlc(string marketId, int interval, long since)
        {
            return await getOhlc(marketId, interval, since);
        }


        private async Task<Ohlc[]> getOhlc(string marketId, int interval = 1, long since = 0)
        {
            String[][] rawData = await ProcessRequest<String[][]>("/v1/public/ohlc", HttpMethod.Get, new Dictionary<string, string>() { { "id", marketId }, { "interval", interval.ToString() }, { "since", since.ToString() } });


            Ohlc[] ohlcData = new Ohlc[rawData.GetUpperBound(0)];

            for (int i = 0; i < rawData.GetUpperBound(0); i++)
            {
                Ohlc ohlc = new Ohlc();
                Decimal open = 0;
                Decimal high = 0;
                Decimal low = 0;
                Decimal close = 0;
                Decimal volume = 0;
                long time = 0;

                long.TryParse(rawData[i][0], out time);
                Decimal.TryParse(rawData[i][1], out open);
                Decimal.TryParse(rawData[i][2], out high);
                Decimal.TryParse(rawData[i][3], out low);
                Decimal.TryParse(rawData[i][4], out close);
                Decimal.TryParse(rawData[i][5], out volume);


                ohlc.open = open;
                ohlc.high = high;
                ohlc.low = low;
                ohlc.close = close;
                ohlc.time = time;
                ohlc.volume = volume;


                ohlcData[i] = ohlc;

            }

            return ohlcData;

        }


     

        public async Task<OrderQueryResult> QueryOpenOrders(string portfolioId, int limit)
        {
            return await QueryOpenOrders(portfolioId,"", limit);
        }

        public async Task<OrderQueryResult> QueryOpenOrders(string portfolioId, string from)
        {
            return await QueryOpenOrders(portfolioId, from);
        }

        public async Task<OrderQueryResult> QueryClosedOrders(string portfolioId, int limit)
        {
            return await QueryClosedOrders(portfolioId, "", limit);
        }

        public async Task<OrderQueryResult> QueryClosedOrders(string portfolioId, string from)
        {
            return await QueryClosedOrders(portfolioId, from);
        }

        public async Task<OrderQueryResult> QueryOpenOrders(string portfolioId, string from = "", int limit = 0)
        {
            return await ProcessRequest<OrderQueryResult>("/v1/private/order/open", HttpMethod.Get, new Dictionary<string, string>() { { "id", portfolioId }, { "from", from } });
        }

        public async Task<OrderQueryResult> QueryClosedOrders(string portfolioId, string from = "", int limit = 0)
        {
            return await ProcessRequest<OrderQueryResult>("/v1/private/order/closed", HttpMethod.Get, new Dictionary<string, string>() { { "id", portfolioId }, { "from", from } });
        }



        public async Task<Order> GetOrder(string orderId)
        {
            return await ProcessRequest<Order>("/v1/private/order", HttpMethod.Get, new Dictionary<string, string>() { { "id", orderId } });
        }

        public async Task<int> CancelOrder(string orderId)
        {
            return await cancelOrder(orderId);
        }

        private async Task<int> cancelOrder(string orderId)
        {
            CancelResponse resp = await ProcessRequest<CancelResponse>("/v1/private/order", HttpMethod.Delete, new Dictionary<string, string>() { { "id", orderId } });
            return resp.canceled;
        }

        public async Task<Portfolio[]> GetPortfolios(string portfoloType = "")
        {
            return await ProcessRequest<Portfolio[]>("/v1/private/portfolio", HttpMethod.Get, new Dictionary<string, string>() { { "type", portfoloType } });
        }


        public async Task<Order> PlaceLimitOrder(string portfolioId, string marketId, string orderSide, decimal price, decimal volume)
        {
            return await PlaceOrder(portfolioId, marketId, orderSide, "limit", price, volume);
        }

        public async Task<Order> PlaceLimitSellOrder(string portfolioId, string marketId, decimal price, decimal volume)
        {
            return await PlaceOrder(portfolioId, marketId, "sell", "limit", price, volume);
        }

        public async Task<Order> PlaceLimitBuyOrder(string portfolioId, string marketId, decimal price, decimal volume)
        {
            return await PlaceOrder(portfolioId, marketId, "buy", "limit", price, volume);
        }

        public async Task<Order> PlaceMarketOrder(string portfolioId, string marketId, string orderSide, decimal volume)
        {
            return await PlaceOrder(portfolioId, marketId, orderSide, "market", 0, volume);
        }

        public async Task<Order> PlaceMarketBuyOrder(string portfolioId, string marketId, decimal volume)
        {
            return await PlaceOrder(portfolioId, marketId, "buy", "market", 0, volume);
        }

        public async Task<Order> PlaceMarketSellOrder(string portfolioId, string marketId, decimal volume)
        {
            return await PlaceOrder(portfolioId, marketId, "sell", "market", 0, volume);
        }

        public async Task<Order> PlaceOrder(string portfolioId, string marketId, string orderSide, string orderType, decimal price, decimal volume)
        {
            return await ProcessRequest<Order>("/v1/private/order", HttpMethod.Post, new Dictionary<string, string>() {
                { "portfolio_id", portfolioId },
                { "type", orderType } ,
                { "side", orderSide },
                { "market", marketId },
                { "price", price.ToString() },
                { "volume", volume.ToString() }});
        }


    

    }
}
