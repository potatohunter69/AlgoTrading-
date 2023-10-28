/* Trend Following Automated Trading System
Author Jacob Amaral Youtube Channel : Jacob Amaral
Note : This code works with the NinjaTrader platform, if your using something else you will have to convert the code if you want it to work with TradeStation,MetaTrader,Multicharts,Python etc....
ZN/10YR Futures Daily Bars
Trading Hours Use Instrument Settings
Optimization Instructions :
dollarStop : ZN 2000,5000 increment 1000 10YR 200,500 increment 100
nBarsHighestHigh : 15,45 increment 15
nBarsLowest : 45,90 increment 15
Run optimization at the last trading day of the year December 27th-28th 
Test period 365 days, opt period 730 days
For 2022 current parameters for stopLoss = 3000, highestHigh 30, lowest low = 60
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
	public class TrendFollower : Strategy
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "TrendFollower";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				nBarsLowestLow = 10;
				nBarsHighestHigh = 10;
				dollarStop = 3000;
			}
			else if (State == State.Configure)
			{
				SetStopLoss(CalculationMode.Currency,dollarStop);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 20) return;
			
			if (High[0] > MAX(High, nBarsHighestHigh)[1]) EnterLong();
			if (Low[0] < MIN(Low, nBarsLowestLow)[1]) EnterShort();
		}
		
		[NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Lowest Low Look Back")]
        public int nBarsLowestLow
        {
            get;set;
        }
		[NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Highest High Look Back")]
        public int nBarsHighestHigh
        {
            get;set;
        }
		[NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Stop Loss $")]
        public double dollarStop
        {
            get;set;
        }
	}
}
