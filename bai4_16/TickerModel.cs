using System;

namespace bai416
{
    public class TickerModel
    {
        public string Symbol { get; set; } = "BTC/USD";
        public decimal Price { get; set; }
        public string Trend { get; set; } // "Tăng" hoặc "Giảm"
        public DateTime UpdateTime { get; set; }
    }
}