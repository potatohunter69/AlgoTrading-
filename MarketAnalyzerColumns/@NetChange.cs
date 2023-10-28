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
	public class NetChange : MarketAnalyzerColumn  	
	{
		private Account account;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description				= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnDescriptionNetChange;
				Name					= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnNameNetChange;
				IsDataSeriesRequired	= false;	
				Unit					= Cbi.PerformanceUnit.Percent;
			}
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

			if (marketDataUpdate.MarketDataType != Data.MarketDataType.Last || marketDataUpdate.Instrument.MarketData.LastClose == null)
				return;

			bool	tryAgainLater;
			double	rate = 0;
			if (account != null)
				rate = marketDataUpdate.Instrument.GetConversionRate(Data.MarketDataType.Bid, account.Denomination, out tryAgainLater);

			switch (Unit)
			{
				case Cbi.PerformanceUnit.Percent:	CurrentValue = (marketDataUpdate.Price - marketDataUpdate.Instrument.MarketData.LastClose.Price) / marketDataUpdate.Instrument.MarketData.LastClose.Price; break;
				case Cbi.PerformanceUnit.Pips:		CurrentValue = ((marketDataUpdate.Price - marketDataUpdate.Instrument.MarketData.LastClose.Price) / Instrument.MasterInstrument.TickSize) * (Instrument.MasterInstrument.InstrumentType == Cbi.InstrumentType.Forex ? 0.1 : 1); break; ;
				case Cbi.PerformanceUnit.Ticks:		CurrentValue = (marketDataUpdate.Price - marketDataUpdate.Instrument.MarketData.LastClose.Price) / Instrument.MasterInstrument.TickSize; break;
				case Cbi.PerformanceUnit.Currency:	CurrentValue = (marketDataUpdate.Price - marketDataUpdate.Instrument.MarketData.LastClose.Price) * Instrument.MasterInstrument.PointValue * rate * (Instrument.MasterInstrument.InstrumentType == Cbi.InstrumentType.Forex ? (account != null ? account.ForexLotSize : Cbi.Account.DefaultLotSize) : 1); break;
				case Cbi.PerformanceUnit.Points:	CurrentValue = (marketDataUpdate.Price - marketDataUpdate.Instrument.MarketData.LastClose.Price); break;
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
				case PerformanceUnit.Currency:
				{
					Currency formatCurrency;
					if (account != null)
						formatCurrency = account.Denomination;
					else
						formatCurrency = Instrument.MasterInstrument.Currency;
					return Core.Globals.FormatCurrency(value, formatCurrency);
				}
				case PerformanceUnit.Points: return value == double.MinValue ? string.Empty : Instrument.MasterInstrument.FormatPrice(value);
				case PerformanceUnit.Percent: return (value).ToString("P", Core.Globals.GeneralOptions.CurrentCulture);
				case PerformanceUnit.Pips:
					{
						CultureInfo forexCulture = Core.Globals.GeneralOptions.CurrentCulture.Clone() as CultureInfo;
						if (forexCulture != null)
							forexCulture.NumberFormat.NumberDecimalSeparator = "'";
						return (Math.Round(value * 10) / 10.0).ToString("0.0", forexCulture);
					}
				case PerformanceUnit.Ticks: return Math.Round(value).ToString(Core.Globals.GeneralOptions.CurrentCulture);
				default: return "0";
			}		
		}
		#endregion
	}
}