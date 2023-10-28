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
	public class PositionAvgPrice : MarketAnalyzerColumn
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				AccountName				= Cbi.Account.SimulationAccountName;
				Description				= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnDescriptionPositionAvgPrice;
				FormatDecimals			= 5;
				Name					= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnNamePositionAvgPrice;
				IsDataSeriesRequired	= false;				
			}
		}

		protected override void OnConnectionStatusUpdate(Cbi.ConnectionStatusEventArgs connectionStatusUpdate)
		{
			BackColor					= null;		// reset color
			CurrentValue				= 0;

			Cbi.Account		account		= null;
			Cbi.Position	position	= null;
			lock (Cbi.Account.All)
				account = Cbi.Account.All.FirstOrDefault(o => o.Name == AccountName);

			if (account != null)
				lock (account.Positions)
					position = account.Positions.FirstOrDefault(o => o.Instrument.FullName == Instrument.FullName);

			if (position != null)
				CurrentValue = position.AveragePrice;
		}

		protected override void OnPositionUpdate(Cbi.PositionEventArgs positionUpdate)
		{
			if (positionUpdate.Position.Instrument == Instrument && positionUpdate.Position.Account.Name == AccountName)
				CurrentValue = (positionUpdate.Operation == Cbi.Operation.Remove ? 0 : positionUpdate.AveragePrice);
		}

		#region Properties
		[NinjaScriptProperty]
		[TypeConverter(typeof(AccountNameConverter))]
		[Display(ResourceType = typeof(NinjaTrader.Resource), Name = "NinjaScriptColumnBaseAccount", GroupName = "NinjaScriptSetup", Order = 0)]
		public string AccountName
		{ get; set; }
		#endregion
	}
}
