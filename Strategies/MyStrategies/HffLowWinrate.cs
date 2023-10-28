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

namespace NinjaTrader.NinjaScript.Strategies.mystrategies
{
    public class smastrategy : Strategy
    {
   
		int size =5; 
		double MaxVol; 
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Strategy here.";
                Name = "TwoSMALowProfit";
                Calculate = Calculate.OnBarClose;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = false;
                ExitOnSessionCloseSeconds = 30;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                Slippage = 0;
                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = TimeInForce.Gtc;
                TraceOrders = false;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 20;
                IsInstantiatedOnEachOptimizationIteration = true;

                stopLoss =2000;
                profitTarget = 500;
                sma = 30; 
				sma2 = 7;  
				MaxVol = 500; 
            }
            else if (	State == State.Configure)
            {
                AddChartIndicator(SMA(Close, sma2));
				AddChartIndicator(SMA(Close, sma));  
				AddChartIndicator(VOL());
            }
        }

        protected override void OnBarUpdate()
        {
		
            	if (IsShortEntry())
            	{
                	EnterShort(size,"Enter Short");			
            	}
            	else if (IsLongEntry() )
            	{
                	EnterLong(size,"Enter Long");					
            	}

            	SetStopLoss(CalculationMode.Currency, stopLoss);
            	SetProfitTarget(CalculationMode.Currency, profitTarget);
				
			
		}		
        

        private bool IsShortEntry()	
        {
			
			double vol = VOL()[0]; 
            return Close[0] < SMA(Close, sma)[0] && Close[0] < SMA(Close, sma2)[0] && vol < MaxVol && IsTradeTime() &&
				Close[0] < Close[1]; 
        }

        private bool IsLongEntry()
        {
			
			double vol = VOL()[0]; 
            return Close[0] > SMA(Close, sma)[0] && Close[0] > SMA(Close, sma2)[0] && vol < MaxVol && IsTradeTime() &&
				Close[0] > Close[1]; 
        }

        private bool IsTradeTime()
        {
            int hour = Time[0].Hour;
            if (hour > 1 && hour <24 )
            {
                return true;
            }
            return false;
        }
		

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Stop Loss", Description = "Stop Loss")]
        public double stopLoss
        { get; set; }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Take Profit", Description = "Take Profit")]
        public double profitTarget
        { get; set; }
		
		
		[NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "High SMA", Description = "High SMA")]
        public int sma
        { get; set; }
		
		[NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Low SMA", Description = "Low SMA")]
        public int sma2
        { get; set; }
		

    }
}

