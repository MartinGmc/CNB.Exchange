## CNB.Exchange

CNB.cz .NET Standard 2.0 Library (netstandard2.0). By [ČNB](https://www.cnb.cz) 
- Daily Exchange Rates Uses public [plain-text API - CSV](https://www.cnb.cz/cs/financni_trhy/devizovy_trh/kurzy_devizoveho_trhu/denni_kurz.txt)]
- Bank codes [[plain-text API - CSV](https://www.cnb.cz/cs/platebni-styk/.galleries/ucty_kody_bank/download/kody_bank_CR.csv)]

Supports
- Dependency injection by **IHttpClientFactory**
- direct query for exchange-rate by **ExchangeRateCode** 

**Dependency injection**

More in [CNB.Tests](/src/CNB.Tests) project, file: [TestFixture.cs](/src/CNB.Tests/TestFixture.cs) and [BasicTest.cs](/src/CNB.Tests/BasicTest.cs).
```c#
// DI configuration
services.AddHttpClient();
services.AddScoped<CnbClient>();

// in constructor
_cnb = test.Services.GetRequiredService<CnbClient>();
```

**Using**
```c#
// return decimal value of Exchange rate for EUR
await _cnb.ExchangeRateCode("EUR");

// return decimal value of Exchange rate for EUR for given date
await _cnb.ExchangeRateCode("EUR", date);

// return decimal value of Exchange rate for EUR for given date, include other countries in search
await _cnb.ExchangeRateCode("EUR", date, true);

// return all Exchange rates for actual date
await _cnb.ExchangeRateAll();

// return all Exchange rates on given date
await _cnb.ExchangeRateAll(date);

// return all Exchange rates on given date in English
await _cnb.ExchangeRateAll(date, "EN");

oter = more than 120 countries from other country list of CNB
// return other Exchange rates for acual date
await _cnb.ExchangeRateOther();

// return other Exchange rates on given date
await _cnb.ExchangeRateOther(date);

// return other Exchange rates on given date in English
await _cnb.ExchangeRateOther(date, "EN");

// return all Bank codes
await _cnb.BankCodeAll();
```

[ExchangeRate.cs](/src/CNB/DO/ExchangeRate.cs) contains:
- **Country** - country name (in Czech, for english use lang = "EN")
- **CurrencyName** - currency name (in Czech, for english use lang = "EN")
- **Amount** - currency amount (example: 1, 100)
- **Code** - currency code (as string; example: EUR, USD, ...)
- **Rate** - exchange rate (as decimal; example: 25.877)


[BankCode.cs](/src/CNB/DO/BankCode.cs) contains:
- **Code** - bank code
- **Name** - bank name (in Czech)
- **BIC** - BIC (SWIFT)
- **CERTIS** - CERTIS
