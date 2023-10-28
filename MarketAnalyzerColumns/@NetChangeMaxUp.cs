// 
// Copyright (C) 2022, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
	public class NetChangeMaxUp : MarketAnalyzerColumn
	{
		private Cbi.Account		account;
		private Cbi.Instrument	instrument;
		private bool			isInitialCalculation = true;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description				= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnDescriptionNetChangeMaxUp;
				Name					= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnNameNetChangeMaxUp;
				IsDataSeriesRequired	= false;
				Unit					= Cbi.PerformanceUnit.Percent;
			}
			if (State == State.Configure)
				instrument = Instruments[0];
		}

		protected override void OnConnectionStatusUpdate(Cbi.ConnectionStatusEventArgs connectionStatusUpdate)
		{
			if (connectionStatusUpdate.PriceStatus == Cbi.ConnectionStatus.Connected && connectionStatusUpdate.PreviousStatus == Cbi.ConnectionStatus.Connecting
					&& connectionStatusUpdate.Connection.Accounts.Count > 0 && account == null)
				account = connectionStatusUpdate.Connection.Accounts[0];
			else if (connectionStatusUpdate.Status == Cbi.ConnectionStatus.Disconnected && connectionStatusUpdate.PreviousStatus == Cbi.ConnectionStatus.Disconnecting
					&& account != null && account.Connection == connectionStatusUpdate.Connection)
				account = null;
		}

		protected override void OnMarketData(Data.MarketDataEventArgs marketDataUpdate)
		{
			if (marketDataUpdate.IsReset)
			{
				CurrentValue = double.MinValue;
				return;
			}

			double dailyHigh = double.MinValue;
			double lastClose = double.MinValue;

			if (marketDataUpdate.MarketDataType == MarketDataType.DailyHigh && marketDataUpdate.Instrument.MarketData.LastClose != null)
			{
				dailyHigh = marketDataUpdate.Price;
				lastClose = marketDataUpdate.Instrument.MarketData.LastClose.Price;
			}
			else if (marketDataUpdate.MarketDataType == MarketDataType.LastClose && marketDataUpdate.Instrument.MarketData.DailyHigh != null)
			{
				dailyHigh = marketDataUpdate.Instrument.MarketData.DailyHigh.Price;
				lastClose = marketDataUpdate.Price;
			}
			else if (isInitialCalculation)
			{
				if (marketDataUpdate.Instrument.MarketData.DailyHigh != null && marketDataUpdate.Instrument.MarketData.LastClose != null)
				{
					dailyHigh = marketDataUpdate.Instrument.MarketData.DailyHigh.Price;
					lastClose = marketDataUpdate.Instrument.MarketData.LastClose.Price;
				}

				isInitialCalculation = false;
			}

			if (dailyHigh == double.MinValue || lastClose == double.MinValue)
				return;

			bool	tryAgainLater;
			double	rate = 0;
			if (account != null)
				rate = marketDataUpdate.Instrument.GetConversionRate(Data.MarketDataType.Bid, account.Denomination, out tryAgainLater);

			switch (Unit)
			{
				case Cbi.PerformanceUnit.Percent:	CurrentValue = (dailyHigh - lastClose) / lastClose; break;
				case Cbi.PerformanceUnit.Pips:		CurrentValue = ((dailyHigh - lastClose) / Instrument.MasterInstrument.TickSize) * (Instrument.MasterInstrument.InstrumentType == Cbi.InstrumentType.Forex ? 0.1 : 1); break; ;
				case Cbi.PerformanceUnit.Ticks:		CurrentValue = (dailyHigh - lastClose) / Instrument.MasterInstrument.TickSize; break;
				case Cbi.PerformanceUnit.Currency:	CurrentValue = (dailyHigh - lastClose) * Instrument.MasterInstrument.PointValue * rate * (Instrument.MasterInstrument.InstrumentType == Cbi.InstrumentType.Forex ? (account != null ? account.ForexLotSize : Cbi.Account.DefaultLotSize) : 1); break;
				case Cbi.PerformanceUnit.Points:	CurrentValue = (dailyHigh - lastClose); break;
			}
		}

		#region Properties
		public Cbi.PerformanceUnit Unit
		{ get; set; }
		#endregion

		#region Miscellaneous
		public override string Format(double value)
		{
			if (value == double.MinValue)
				return string.Empty;

			switch (Unit)
			{
				case Cbi.PerformanceUnit.Currency:
					{
                        Cbi.Currency formatCurrency;
                        if (account != null)
                            formatCurrency = account.Denomination;
                        else
                            formatCurrency = Instrument.MasterInstrument.Currency;
                        return Core.Globals.FormatCurrency(value, formatCurrency);
                    }
				case Cbi.PerformanceUnit.Points: return value.ToString(Core.Globals.GetTickFormatString(Instrument.MasterInstrument.TickSize), Core.Globals.GeneralOptions.CurrentCulture);
				case Cbi.PerformanceUnit.Percent: return (value).ToString("P", Core.Globals.GeneralOptions.CurrentCulture);
				case Cbi.PerformanceUnit.Pips:
					{
						CultureInfo forexCulture = Core.Globals.GeneralOptions.CurrentCulture.Clone() as CultureInfo;
						if (forexCulture != null)
							forexCulture.NumberFormat.NumberDecimalSeparator = "'";
						return (Math.Round(value * 10) / 10.0).ToString("0.0", forexCulture);
					}
				case Cbi.PerformanceUnit.Ticks: return Math.Round(value).ToString(Core.Globals.GeneralOptions.CurrentCulture);
				default: return "0";
			}
		}
		#endregion
	}
}
