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
	public class TradedContracts : MarketAnalyzerColumn
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				AccountName				= Cbi.Account.SimulationAccountName;
				Description				= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnDescriptionTradedContracts;
				Name					= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnNameTradedContracts;
				IsDataSeriesRequired	= false;
				FormatDecimals			= 0;
				ShowInTotalRow			= true;
			}
		}

		protected override void OnConnectionStatusUpdate(Cbi.ConnectionStatusEventArgs connectionStatusUpdate)
		{
			if (connectionStatusUpdate.Status == Cbi.ConnectionStatus.Connected && connectionStatusUpdate.PreviousStatus == Cbi.ConnectionStatus.Connecting)
			{
				lock (connectionStatusUpdate.Connection.Accounts)
				{
					Cbi.Account account = connectionStatusUpdate.Connection.Accounts.FirstOrDefault(o => o.Name == AccountName);
					if (account != null)
					{
						CurrentValue = 0;
						foreach (Cbi.Execution execution in account.Executions)
							if (execution.Instrument == Instrument)
								CurrentValue += execution.Quantity;
					}
				}
			}
			else if (connectionStatusUpdate.Status == Cbi.ConnectionStatus.Disconnected && connectionStatusUpdate.PreviousStatus == Cbi.ConnectionStatus.Disconnecting)
			{
				lock (connectionStatusUpdate.Connection.Accounts)
					if (connectionStatusUpdate.Connection.Accounts.FirstOrDefault(o => o.Name == AccountName) != null)
						CurrentValue = 0;
			}
		}

		protected override void OnExecutionUpdate(Cbi.ExecutionEventArgs executionUpdate)
		{
			if (executionUpdate.Operation == Cbi.Operation.Add && executionUpdate.Execution.Instrument == Instrument && executionUpdate.Execution.Account.Name == AccountName)
				CurrentValue += executionUpdate.Execution.Quantity;
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
