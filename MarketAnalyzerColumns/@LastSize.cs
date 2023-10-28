// 
// Copyright (C) 2012, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.NinjaScript.Indicators;
#endregion

//This namespace holds Market Analyzer columns in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public class LastSize : MarketAnalyzerColumn
	{
		Cbi.InstrumentType instrumentType = Cbi.InstrumentType.Unknown;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description				= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnDescriptionLastSize;
				Name					= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnNameLastSize;
				IsDataSeriesRequired	= false;
			}
			else if (State == State.Realtime)
			{
				if (Instrument != null && Instrument.MarketData != null && Instrument.MarketData.Last != null)
					CurrentValue = Instrument.MarketData.Last.Volume;
			}
		}

		protected override void OnMarketData(Data.MarketDataEventArgs marketDataUpdate)
		{
			if (marketDataUpdate.IsReset)
				CurrentValue = double.MinValue;
			else if (marketDataUpdate.MarketDataType == Data.MarketDataType.Last)
			{
				instrumentType	= marketDataUpdate.Instrument.MasterInstrument.InstrumentType;
				CurrentValue	= instrumentType == Cbi.InstrumentType.CryptoCurrency ? Core.Globals.ToCryptocurrencyVolume(marketDataUpdate.Volume) : marketDataUpdate.Volume;
			}
		}

		#region Miscellaneous
		public override string Format(double value)
		{
			return value == double.MinValue
				? string.Empty
				: instrumentType == Cbi.InstrumentType.CryptoCurrency
					? Core.Globals.FormatCryptocurrencyQuantity(value, false)
					: Core.Globals.FormatQuantity((long)value, false);
		}
		#endregion
	}
}
