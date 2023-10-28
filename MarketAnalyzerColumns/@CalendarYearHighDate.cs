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
	public class CalendarYearHighDate : MarketAnalyzerColumn
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description				= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnDescriptionCalendarYearHighDate;
				Name					= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnNameCalendarYearHighDate;
				IsDataSeriesRequired	= false;
				DataType				= typeof(string);
			}
			else if (State == State.Configure)
				CurrentText	= string.Empty;
			else if (State == State.Realtime)
			{
				if (Instrument != null && Instrument.FundamentalData != null && Instrument.FundamentalData.CalendarYearHighDate != null)
				{
					CurrentValue	= Instrument.FundamentalData.CalendarYearHighDate.Value.Subtract(Core.Globals.MinDate).TotalDays;
					CurrentText		= Format(CurrentValue);
				}
			}
		}

		protected override void OnFundamentalData(Data.FundamentalDataEventArgs fundamentalDataUpdate)
		{
			if (fundamentalDataUpdate.IsReset)
				CurrentValue	= double.MinValue;
			else if (fundamentalDataUpdate.FundamentalDataType == Data.FundamentalDataType.CalendarYearHighDate)
			{
				CurrentValue	= fundamentalDataUpdate.DateTimeValue.Subtract(Core.Globals.MinDate).TotalDays;
				CurrentText		= Format(CurrentValue);
			}
		}

		#region Miscellaneous
		public new string Format(double value)
		{
			return Core.Globals.MinDate.AddDays(value).ToString(Core.Globals.GeneralOptions.CurrentCulture.DateTimeFormat.ShortDatePattern, Core.Globals.GeneralOptions.CurrentCulture);
		}
		#endregion
	}
}
