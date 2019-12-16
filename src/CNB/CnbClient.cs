using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace CNB
{
	public class CnbClient
	{
		/// <summary>
		/// CNB URL for Exchange rates
		/// </summary>
		public const string URL_EXCHANGE = "https://www.cnb.cz/cs/financni_trhy/devizovy_trh/kurzy_devizoveho_trhu/denni_kurz.txt";

		/// <summary>
		/// CNB URL for Bank codes
		/// </summary>
		public const string URL_BANK_CODES = "https://www.cnb.cz/cs/platebni-styk/.galleries/ucty_kody_bank/download/kody_bank_CR.csv";

		/// <summary>
		/// CNB URL for Exchange rates other countries
		/// </summary>
		public const string URL_EXCHANGE_OTHER = "https://www.cnb.cz/cs/financni-trhy/devizovy-trh/kurzy-ostatnich-men/kurzy-ostatnich-men/kurzy.txt";

		/// <summary>
		/// CNB URL for Exchange rates english
		/// </summary>
		public const string URL_EXCHANGE_EN = "https://www.cnb.cz/en/financial-markets/foreign-exchange-market/central-bank-exchange-rate-fixing/central-bank-exchange-rate-fixing/daily.txt";

		/// <summary>
		/// CNB URL for Exchange rates other english
		/// </summary>
		public const string URL_EXCHANGE_OTHER_EN = "https://www.cnb.cz/en/financial-markets/foreign-exchange-market/fx-rates-of-other-currencies/fx-rates-of-other-currencies/fx_rates.txt";

		#region DI

		private readonly IHttpClientFactory _clientFactory;

		public CnbClient(IHttpClientFactory clientFactory)
		{
			_clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
		}

		#endregion

		/// <summary>
		/// get exchange rate by currency code
		/// </summary>
		public async Task<decimal> ExchangeRateCode(string code, DateTime? date = null,bool searchOther = false)
		{
			var all = await ExchangeRateAll(date);

			var item = all.FirstOrDefault(x => x.Code == code);
			if (item == null)
			{
				if (!searchOther)
				throw new ArgumentException($"Cannot find: '{code}'.");
				else
				{
					var other = await ExchangeRateOther(date);
					item = other.FirstOrDefault(x => x.Code == code);
					if (item == null) throw new ArgumentException($"Cannot find: '{code}'.");
				}
			}
			return item.Rate;
		}

		/// <summary>
		/// get all exchange rates for date
		/// </summary>
		public async Task<IReadOnlyCollection<ExchangeRate>> ExchangeRateAll(DateTime? date = null, string lang = null)
		{
			var client = _clientFactory.CreateClient("cnb");

			var urlConst = URL_EXCHANGE;
			if (lang == "CS")
				urlConst = URL_EXCHANGE;
			if (lang == "EN")
				urlConst = URL_EXCHANGE_EN;

			var url = $"{urlConst}?date={(date ?? DateTime.Today).ToString("dd.MM.yyyy")}";
			var content = await client.GetStringAsync(url);

			// remove first 1 line ('28.12.2018 #249')
			var lines = content.Split(Environment.NewLine.ToCharArray()).Skip(1).ToArray();
			var cleaned = string.Join(Environment.NewLine, lines);

			ExchangeRate[] result = null;
			using (var reader = new StringReader(cleaned))
			using (var csv = new CsvReader(reader))
			{
				csv.Configuration.Delimiter = "|";
				if (lang == "EN")
					csv.Configuration.RegisterClassMap<ExchangeRateMapperEN>();
				else
				csv.Configuration.RegisterClassMap<ExchangeRateMapper>();

				result = csv.GetRecords<ExchangeRate>().ToArray();

			}
			return result;
		}

		/// <summary>
		/// Get all exchange rates for date for other countries
		/// </summary>
		public async Task<IReadOnlyCollection<ExchangeRate>> ExchangeRateOther(DateTime? date = null, string lang = null)
		{
			var client = _clientFactory.CreateClient("cnb");

			var urlConst = URL_EXCHANGE_OTHER;
			if (lang == "CS")
				urlConst = URL_EXCHANGE_OTHER;
			if (lang == "EN")
				urlConst = URL_EXCHANGE_OTHER_EN;

			var url = $"{urlConst}?year={(date ?? DateTime.Today).Year.ToString()}&month={(date ?? DateTime.Today).Month.ToString()}";
			var content = await client.GetStringAsync(url);

			// remove first 1 line ('29 Nov 2019 #11')
			var lines = content.Split(Environment.NewLine.ToCharArray()).Skip(1).ToArray();
			var cleaned = string.Join(Environment.NewLine, lines);

			ExchangeRate[] result = null;
			using (var reader = new StringReader(cleaned))
			using (var csv = new CsvReader(reader))
			{
				csv.Configuration.Delimiter = "|";
				if (lang == "EN")
					csv.Configuration.RegisterClassMap<ExchangeRateMapperEN>();
				else
					csv.Configuration.RegisterClassMap<ExchangeRateMapper>();

				result = csv.GetRecords<ExchangeRate>().ToArray();
								
			}
			return result;
		}

		/// <summary>
		/// get all bank codes
		/// </summary>
		public async Task<IReadOnlyCollection<BankCode>> BankCodeAll()
		{
			var client = _clientFactory.CreateClient("cnb");

			using (var stream = await client.GetStreamAsync($"{URL_BANK_CODES}"))
			using (var reader = new StreamReader(stream))
			using (var csv = new CsvReader(reader, new Configuration() { HasHeaderRecord = true }))
			{
				csv.Configuration.Delimiter = ";";
				csv.Configuration.RegisterClassMap<BankCodeMapper>();

				return csv.GetRecords<BankCode>().ToArray();
			}
		}
	}
}
