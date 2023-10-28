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
	public class MaxPercentProfitableShort : OptimizationFitness
	{
		protected override void OnCalculatePerformanceValue(StrategyBase strategy)
		{
			if (strategy.SystemPerformance.ShortTrades.TradesCount == 0)
				Value = 0;
			else
				Value = (double) strategy.SystemPerformance.ShortTrades.WinningTrades.TradesCount / strategy.SystemPerformance.ShortTrades.TradesCount;
		}

		protected override void OnStateChange()
		{               
			if (State == State.SetDefaults)
				Name = NinjaTrader.Custom.Resource.NinjaScriptOptimizationFitnessNameMaxPercentProfitableShort;
		}
	}
}
