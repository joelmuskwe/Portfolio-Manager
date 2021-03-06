﻿using Dapper;
using DavidBerry.Framework.Functional;
using Securities.Core.DataAccess;
using Securities.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Securities.Data.Dapper.Repositories
{
    public class SecurityPriceRepository : ISecurityPriceRepository
    {

        public SecurityPriceRepository(String connectionString)
        {
            _connectionString = connectionString;
        }

        private readonly String _connectionString;

        public static readonly String BASE_SQL =
            @"SELECT
                  s.Ticker,
                  s.SecurityName AS Name,
		          s.SecurityTypeCode AS SecurityType,
		          p.TradeDate,
		          p.OpenPrice AS OpeningPrice,
		          p.ClosePrice AS ClosingPrice,
		          p.DailyHigh,
		          p.DailyLow,
		          p.Volume,
		          p.Change,
		          p.ChangePercent
              FROM SecurityPrices p
	          INNER JOIN Securities s
	              ON s.Ticker = p.Ticker";


        public Maybe<SecurityPrice> GetSecurityPrice(string ticker, TradeDate tradeDate)
        {
            string sql = $@"{BASE_SQL}
               WHERE p.TradeDate = @TradeDate
                   AND s.Ticker = @Ticker";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var price = connection.QueryFirstOrDefault<SecurityPrice>(sql,
                    new { TradeDate = tradeDate.Date, Ticker = ticker });
                return Maybe.Create<SecurityPrice>(price);
            }
        }

        public List<SecurityPrice> GetSecurityPrices(TradeDate tradeDate)
        {
            string sql = $@"{BASE_SQL}
               WHERE p.TradeDate = @TradeDate";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Query<SecurityPrice>(sql,
                    new { TradeDate = tradeDate.Date })
                    .ToList();
            }
        }

        public List<SecurityPrice> GetSecurityPrices(TradeDate tradeDate, IEnumerable<string> tickers)
        {
            string sql = $@"{BASE_SQL}
               WHERE p.TradeDate = @TradeDate
                   AND s.Ticker IN @Tickers";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Query<SecurityPrice>(sql,
                    new { TradeDate = tradeDate.Date, Tickers = tickers })
                    .ToList();
            }
        }

        public List<SecurityPrice> GetSecurityPrices(string ticker, DateTime startDate, DateTime endDate)
        {
            string sql = $@"{BASE_SQL}
               WHERE s.Ticker = @Ticker
                   AND p.TradeDate >= @StartDate
                   AND p.TradeDate <= @EndDate";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Query<SecurityPrice>(sql,
                    new { Ticker = ticker, StartDate = startDate.Date, EndDate = endDate.Date })
                    .ToList();
            }
        }
    }
}
