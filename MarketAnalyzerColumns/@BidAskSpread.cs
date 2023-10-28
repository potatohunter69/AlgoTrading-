#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
#endregion

//This namespace holds Market Analyzer columns in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public class BidAskSpread : MarketAnalyzerColumn
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description				= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnDescriptionBidAskSpread;
				Name					= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnNameBidAskSpread;
				IsDataSeriesRequired	= false;
			}
		}

		protected override void OnMarketData(Data.MarketDataEventArgs marketDataUpdate)
		{
			if (marketDataUpdate.IsReset)
				CurrentValue = double.MinValue;
			else if ((marketDataUpdate.MarketDataType == Data.MarketDataType.Ask || marketDataUpdate.MarketDataType == Data.MarketDataType.Bid)
					&& Instrument.MarketData.Ask != null && Instrument.MarketData.Bid != null)
				CurrentValue = Instrument.MarketData.Ask.Price - Instrument.MarketData.Bid.Price;
		}

		#region Miscellaneous
		public override string Format(double value)
		{
			return (value == double.MinValue ? string.Empty : Instrument.MasterInstrument.FormatPrice(value));
		}
		#endregion
	}
}
