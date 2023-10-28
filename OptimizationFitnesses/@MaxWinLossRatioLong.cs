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
using System.Windows.Media;
using System.Xml.Serialization;
#endregion

namespace NinjaTrader.NinjaScript.OptimizationFitnesses
{
	public class MaxWinLossRatioLong : OptimizationFitness
	{
		protected override void OnCalculatePerformanceValue(StrategyBase strategy)
		{
			if (strategy.SystemPerformance.LongTrades.LosingTrades.TradesPerformance.Percent.AverageProfit == 0)
				Value = 1;
			else
				Value = strategy.SystemPerformance.LongTrades.WinningTrades.TradesPerformance.Percent.AverageProfit / Math.Abs(strategy.SystemPerformance.LongTrades.LosingTrades.TradesPerformance.Percent.AverageProfit);
		}

		protected override void OnStateChange()
		{               
			if (State == State.SetDefaults)
				Name = NinjaTrader.Custom.Resource.NinjaScriptOptimizationFitnessNameMaxWinLossRatioLong;
		}
	}
}
