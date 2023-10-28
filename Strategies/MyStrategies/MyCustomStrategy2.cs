/* GapReversal Automated Trading System
Author Jacob Amaral Youtube Channel : Jacob Amaral
Note : This code works with the NinjaTrader platform, if your using something else you will have to convert the code if you want it to work with TradeStation,MetaTrader,Multicharts,Python etc....
ES Futures 10 Minute Bars, works with NQ too.
Trading Hours 9:30am-4pm EST / US Equities RTH (Real-time hours)
Optimization Instructions :
profitTarget : 1000,3000 increment 500
gapDownAmount : 1,2 increment 1
gapUpAmount : 0.5,1 increment 0.5
Run Walk-forward Optimization at the last trading day of the year December 27th-28th
765 optimization period (days), 365 test period (days)
For 2022 current parameters for profitTarget = 2000, gapDownAmount = 1 and gapUpAmount = 0.5 which are already the defaults to save you time.
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
	public class GapReversal : Strategy
	{
		protected override void OnStateChange()
		{
			//Set default vars
			if (State == State.SetDefaults)
			{
				Description									= @"Gap Reversal strategy";
				Name										= "GapReversal";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 1;
				IncludeCommission = true;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				OptimizationPeriod = 730;
				TestPeriod = 365;
				gapDownAmount = 0.05;
				gapUpAmount = 0.01;
				profitTarget = 1000;
				stopLoss = 1000;
			}
			else if (State == State.Configure)
			{
				//Profit Target $
				SetProfitTarget(CalculationMode.Currency,profitTarget);
				SetStopLoss(CalculationMode.Currency, stopLoss);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar <= BarsRequiredToTrade) return; //Wait until enough bars have loaded
			
			//Gap Down % Enter Short
			if (((Close[1] - Open[0]) / Close[1]) * 100 >= gapDownAmount) EnterShort();
			//Gap Up % Enter Long
			if (((Open[0] - Close[1]) / Open[0]) * 100 >= gapUpAmount) EnterLong();
		}
		
		
		[NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Gap up %")]
        public double gapUpAmount
        {
            get; set;
        }
        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Gap down %")]
        public double gapDownAmount
        { get; set; }
		
		[NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "$ Profit Target")]
        public int profitTarget
        {
            get; set;
        }
		

		[NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "$ StopLoss")]
        public int stopLoss
        {
            get; set;
        }
	}
}

