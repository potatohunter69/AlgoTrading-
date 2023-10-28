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
	public class UnrealizedProfitLoss : MarketAnalyzerColumn
	{
		private	Currency		accountDenomination		= Currency.UsDollar;
		private	Cbi.Position	position;				// holds the position for the actual instrument

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				AccountName				= Cbi.Account.SimulationAccountName;
				Description				= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnDescriptionUnrealizedProfitLoss;
				Name					= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnNameUnrealizedProfitLoss;
				IsDataSeriesRequired	= false;
				ShowInTotalRow			= true;
			}
		}

		protected override void OnConnectionStatusUpdate(Cbi.ConnectionStatusEventArgs connectionStatusUpdate)
		{
			if (connectionStatusUpdate.Status == Cbi.ConnectionStatus.Connected && connectionStatusUpdate.PreviousStatus == Cbi.ConnectionStatus.Connecting)
			{
				Cbi.Account account = null;
				lock (connectionStatusUpdate.Connection.Accounts)
					account = connectionStatusUpdate.Connection.Accounts.FirstOrDefault(o => o.Name == AccountName);

				if (account != null)
				{
					accountDenomination	= account.Denomination;
					lock (account.Positions)
						position = account.Positions.FirstOrDefault(o => o.Instrument.FullName == Instrument.FullName);
				}
			}
			else if (connectionStatusUpdate.Status == Cbi.ConnectionStatus.Disconnected && connectionStatusUpdate.PreviousStatus == Cbi.ConnectionStatus.Disconnecting)
			{
				if (position != null && position.Account.Connection == connectionStatusUpdate.Connection)
				{
					CurrentValue	= 0;
					position		= null;
				}
			}
		}

		protected override void OnMarketData(Data.MarketDataEventArgs marketDataUpdate)
		{
			CurrentValue = (position == null ? 0 : position.GetUnrealizedProfitLoss(Cbi.PerformanceUnit.Currency));
		}

		protected override void OnPositionUpdate(Cbi.PositionEventArgs positionUpdate)
		{
			if (positionUpdate.Position.Account.Name == AccountName && positionUpdate.Position.Instrument == Instrument)
			{
				position 			= (positionUpdate.Operation == Cbi.Operation.Remove ? null : positionUpdate.Position);
				CurrentValue 		= (position == null ? 0 : position.GetUnrealizedProfitLoss(Cbi.PerformanceUnit.Currency));
				accountDenomination = positionUpdate.Position.Account.Denomination;
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[TypeConverter(typeof(AccountNameConverter))]
		[Display(ResourceType = typeof(Resource), Name = "NinjaScriptColumnBaseAccount", GroupName = "NinjaScriptSetup", Order = 0)]
		public string AccountName
		{ get; set; }
		#endregion

		#region Miscellaneous
		public override string Format(double value)
		{
			if (CellConditions.Count == 0)
				ForeColor = (value >= 0 ? Application.Current.TryFindResource("MAGridForeground") : 
										Application.Current.TryFindResource("StrategyAnalyzerNegativeValueBrush")) as Brush;

			Cbi.Currency formatCurrency = accountDenomination;

			return Core.Globals.FormatCurrency(value, formatCurrency);
		}
		#endregion
	}
}
