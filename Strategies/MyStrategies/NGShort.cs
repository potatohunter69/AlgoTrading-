/* NG Short Automated Trading System
Author Jacob Amaral Youtube Channel : Jacob Amaral
Note : This code works with the NinjaTrader platform, if your using something else you will have to convert the code if you want it to work with TradeStation,MetaTrader,Multicharts,Python etc....
NG Futures Daily Bars
Trading Hours 6pm-5pm EST / Use Instrument Settings
Optimization Instructions :
dollarProfit : 1000,3000 increment 500
openGap : 1.5,2.5 increment 0.5
Run optimization at the last trading day of the year December 27th-28th
Optimization period 730 days
Test Period 365 days
Optimize On Max net profit
For 2022 current parameters for dollarProfit = 2500, openGap 2.5 which are already the defaults to save you time.
*/
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
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class NGShort : Strategy
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "NGShort";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 2;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				IncludeCommission = true;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				openGap = 2.5d;
				dollarProfit = 2500;
			}
			else if (State == State.Configure)
			{
				AddDataSeries(BarsPeriodType.Minute, 1);
			}
		}

		protected override void OnBarUpdate()
		{
			//Wait for some bars to load first
			if (CurrentBars[0] < 20) return;
			
			//Check 1 minute bars
			if (BarsInProgress == 1) {

				//If the previous day candle was green and the difference between the close and open price is greater than our openGap in % Enter Short
				if (1.00d+(((Closes[0][0] - Opens[0][0])/Opens[0][0]))>=(1.00d+(openGap/100.00d)) && Closes[0][0] > Opens[0][0] && Times[1][0].Hour < 16 && Position.MarketPosition == MarketPosition.Flat) {
					EnterShort(1,DefaultQuantity,"Enter Short");
					//Set stop loss at a price based on the most current SMA with a period of 14
					SetStopLoss(CalculationMode.Price,SMA(14)[0]);
				}
				//Make sure we exit at session close
				if (Position.MarketPosition == MarketPosition.Short && Times[1][0].Hour == 16) ExitShort(1,Position.Quantity,"Buy to cover","Enter Short");
				//Profit Target intraday
				if (Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency,Close[0]) >= dollarProfit*Position.Quantity) ExitShort(1,Position.Quantity,"Profit Target","Enter Short");
			}
			
		}
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Close Gap", GroupName = "Settings", Order = 1)]
		public double openGap
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Profit Target in $", GroupName = "Settings", Order = 1)]
		public double dollarProfit
		{ get; set; }
	}
}
	