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
	public class RealizedProfitLoss : MarketAnalyzerColumn
	{
		private	readonly	List<Cbi.Execution>		executions				= new List<Cbi.Execution>();
		private				Currency				accountDenomination		= Currency.UsDollar;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				AccountName				= Cbi.Account.SimulationAccountName;
				Description				= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnDescriptionRealizedProfitLoss;
				Name					= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnNameRealizedProfitLoss;
				IsDataSeriesRequired	= false;
				ShowInTotalRow			= true;
			}
		}

		protected override void OnAccountItemUpdate(Cbi.AccountItemEventArgs accountItemUpdate)
		{
			if (AccountName != Account.SimulationAccountName || accountItemUpdate.Account.Name != AccountName || accountItemUpdate.AccountItem != AccountItem.RealizedProfitLoss)
				return;

			accountDenomination	= accountItemUpdate.Account.Denomination;
			executions.Clear();
			foreach (Cbi.Execution execution in accountItemUpdate.Account.Executions)
				if (execution.Instrument == Instrument
						|| (execution.Instrument.MasterInstrument.InstrumentType == InstrumentType.Stock && execution.Instrument.FullName == Instrument.FullName))
					executions.Add(execution);
			CurrentValue = Cbi.SystemPerformance.Calculate(executions).AllTrades.TradesPerformance.Currency.CumProfit;
		}

		protected override void OnConnectionStatusUpdate(Cbi.ConnectionStatusEventArgs connectionStatusUpdate)
		{
			if (connectionStatusUpdate.Status == Cbi.ConnectionStatus.Connected || connectionStatusUpdate.PreviousStatus == Cbi.ConnectionStatus.ConnectionLost)
			{
				Cbi.Account account;
				lock (connectionStatusUpdate.Connection.Accounts)
					account = connectionStatusUpdate.Connection.Accounts.FirstOrDefault(o => o.Name == AccountName);

				if (account != null)
				{
					accountDenomination	= account.Denomination;
					executions.Clear();
					foreach (Cbi.Execution execution in account.Executions)
						if (execution.Instrument == Instrument
								|| (execution.Instrument.MasterInstrument.InstrumentType == InstrumentType.Stock && execution.Instrument.FullName == Instrument.FullName))
							executions.Add(execution);
					CurrentValue = Cbi.SystemPerformance.Calculate(executions).AllTrades.TradesPerformance.Currency.CumProfit;
				}
			}
		}

		protected override void OnExecutionUpdate(Cbi.ExecutionEventArgs executionUpdate)
		{
			if (executionUpdate.Execution.Account.Name != AccountName 
					|| (executionUpdate.Execution.Instrument.MasterInstrument.InstrumentType != InstrumentType.Stock && executionUpdate.Execution.Instrument != Instrument)
					|| (executionUpdate.Execution.Instrument.MasterInstrument.InstrumentType == InstrumentType.Stock && executionUpdate.Execution.Instrument.FullName != Instrument.FullName))
				return;

			executions.Add(executionUpdate.Execution);
			CurrentValue = Cbi.SystemPerformance.Calculate(executions).AllTrades.TradesPerformance.Currency.CumProfit;
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
