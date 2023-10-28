// 
// Copyright (C) 2022, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
	public class DaysUntilRollover : MarketAnalyzerColumn
	{
		Data.SessionIterator						sessionIterator;
		System.Windows.Threading.DispatcherTimer	timer;

		private void CalculateDays()
		{
			DateTime now = Cbi.Connection.PlaybackConnection != null ? Cbi.Connection.PlaybackConnection.Now : Core.Globals.Now;
			if (sessionIterator == null)
				sessionIterator = new SessionIterator(Instrument.MasterInstrument.TradingHours);
			if (now > sessionIterator.ActualTradingDayEndLocal)
			{
				sessionIterator.GetNextSession(now, false);

				lock (Instrument.MasterInstrument.RolloverCollection)
				{
					foreach (Rollover rollover in Instrument.MasterInstrument.RolloverCollection)
					{
						if (rollover.ContractMonth == Instrument.Expiry)
						{
							CurrentValue = Instrument.MasterInstrument.GetNextRolloverDate(rollover.Date).Subtract(sessionIterator.ActualTradingDayExchange).TotalDays;
							return;
						}
					}
				}
			}
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description				= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnDescriptionDaysUntilRollover;
				Name					= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnNameDaysUntilRollover;
				IsDataSeriesRequired	= false;
			}
			else if (State == State.Realtime)
			{
				Dispatcher.InvokeAsync(() =>
				{
					timer = new System.Windows.Threading.DispatcherTimer { Interval = new TimeSpan(0, 0, 1), IsEnabled = true };
					timer.Tick += (s, e) => { CalculateDays(); };
				});
			}
			else if (State == State.Terminated)
			{
				if (timer == null)
					return;

				timer.IsEnabled = false;
				timer = null;
			}
		}

		protected override void OnMarketData(Data.MarketDataEventArgs marketDataUpdate)
		{
			if (marketDataUpdate.IsReset)
			{
				CurrentValue	= double.MinValue;
				sessionIterator	= null;
				return;
			}
		}
	}
}
