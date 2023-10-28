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
	public class PositionSize : MarketAnalyzerColumn
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				AccountName				= Cbi.Account.SimulationAccountName;
				Description				= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnDescriptionPositionSize;
				Name					= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnNamePositionSize;
				IsDataSeriesRequired	= false;
				FormatDecimals			= 0;
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
				CurrentValue = (position.MarketPosition == Cbi.MarketPosition.Long ? 1 : -1) * position.Quantity;
		}

		protected override void OnPositionUpdate(Cbi.PositionEventArgs positionUpdate)
		{
			if (positionUpdate.Position.Instrument == Instrument && positionUpdate.Position.Account.Name == AccountName)
				CurrentValue = (positionUpdate.Operation == Cbi.Operation.Remove ? 0 : (positionUpdate.Position.MarketPosition == Cbi.MarketPosition.Long ? 1 : -1) * positionUpdate.Position.Quantity);
		}

		#region Properties
		[NinjaScriptProperty]
		[TypeConverter(typeof(AccountNameConverter))]
		[Display(ResourceType = typeof(NinjaTrader.Resource), Name = "NinjaScriptColumnBaseAccount", GroupName = "NinjaScriptSetup", Order = 0)]
		public string AccountName
		{ get; set; }
		#endregion

		#region Miscellaneous
		public override string Format(double value)
		{
			if (CellConditions.Count == 0)
				BackColor = value == 0 ? null : value > 0 ? Application.Current.FindResource("LongBackground") as Brush : Application.Current.FindResource("ShortBackground") as Brush;

			return (value == 0 ? string.Empty : Core.Globals.FormatQuantity((long) Math.Abs(value), false));
		}
		#endregion
	}
}
