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

using NinjaTrader.Core.FloatingPoint;
#endregion

namespace NinjaTrader.NinjaScript.OptimizationFitnesses
{
	public class MaxStrengthShort : OptimizationFitness
	{
		protected override void OnCalculatePerformanceValue(StrategyBase strategy)
		{
			// Weighing in any <value> between 0 .. 1 by the formula:		x		= 1 - weight / (weight + <value>)
			// ... where 'weight' could be calculated by:					weight	= <value> * (1 - x) / x
			// Set x = 0.8 as a good pivot point

			Cbi.TradeCollection trades = strategy.SystemPerformance.ShortTrades;
			Value = 100
						* (trades.TradesPerformance.ProfitFactor < 1 ? 0 : 1)						// filter non profitable strategies
						* (trades.TradesCount == 0 ? 0
							: ((double) trades.WinningTrades.TradesCount / trades.TradesCount))		// as many trades profitable as possible
						* trades.TradesPerformance.RSquared											// we're looking for steady profits
						* (1.0 - 0.25 / (0.25 + (trades.TradesPerformance.ProfitFactor - 1)))		// <value> = 2 -> x = 0.8; <value> = 1 -> x = 0
						* (1.0 - 25.0 / (25.0 + trades.TradesCount));								// <value> = 100 -> x = 0.8
		}

		protected override void OnStateChange()
		{ 
			if (State == State.SetDefaults)
				Name = NinjaTrader.Custom.Resource.NinjaScriptOptimizationFitnessNameMaxStrengthShort;
		}
	}
}
