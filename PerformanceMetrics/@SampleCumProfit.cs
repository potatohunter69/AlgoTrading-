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

namespace NinjaTrader.NinjaScript.PerformanceMetrics
{
	public class SampleCumProfit : PerformanceMetric
	{
		private Cbi.Currency denomination = (Cbi.Currency) (-1);

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
				Name	= Custom.Resource.PerformanceMetricSampleCumProfit;
			else if (State == State.Configure)
				Values	= new double[5];					// There needed to be one value per Cbi.PerformanceUnit, which is why the values are managed in an array of length "ValueArrayLength"
			else if (State == State.Active)	
				Array.Clear(Values, 0, Values.Length);		// Needed to be reset before every backtest iteration. 
		}

		// This is called as each trade is added
		protected override void OnAddTrade(Cbi.Trade trade)
		{
			if (denomination == (Cbi.Currency) (-1))
				denomination = trade.Exit.Account.Denomination;

			Values[(int)Cbi.PerformanceUnit.Currency]	+= trade.ProfitCurrency;
			Values[(int)Cbi.PerformanceUnit.Percent]	= (1.0 + Values[(int)Cbi.PerformanceUnit.Percent]) * (1.0 + trade.ProfitPercent) - 1;
			Values[(int)Cbi.PerformanceUnit.Pips]		+= trade.ProfitPips;
			Values[(int)Cbi.PerformanceUnit.Points]		+= trade.ProfitPoints;
			Values[(int)Cbi.PerformanceUnit.Ticks]		+= trade.ProfitTicks;
		}

		// This is called as the values of a trade metric are saved, which occurs e.g. in the strategy analyzer on optimizer runs
		protected override void OnCopyTo(PerformanceMetricBase target)
		{
			// You need to cast, in order to access the right type
			SampleCumProfit targetMetrics = (target as SampleCumProfit);

			if (targetMetrics != null)
				Array.Copy(Values, targetMetrics.Values, Values.Length);
		}

		// This is called as the trade metric is merged, which occurs e.g. in the strategy analyzer for the total row and for aggregate 
		protected override void OnMergePerformanceMetric(PerformanceMetricBase target)
		{
			// You need to cast, in order to access the right type
			SampleCumProfit targetMetrics = (target as SampleCumProfit);

			// This is just a simple weighted average sample
			if (targetMetrics != null && TradesPerformance.TradesCount + targetMetrics.TradesPerformance.TradesCount > 0)
				for (int i = 0; i < Values.Length; i++)
					targetMetrics.Values[i] = (targetMetrics.Values[i] * targetMetrics.TradesPerformance.TradesCount + Values[i] * TradesPerformance.TradesCount) / (TradesPerformance.TradesCount + targetMetrics.TradesPerformance.TradesCount);
		}

		// The attribute determines the name of the performance value on the grid (the actual property name below is irrelevant for that matter)
		[Display(ResourceType = typeof(Custom.Resource), Description = "SampleCumProfitDescription", Name = "SampleCumProfit", Order = 0)]
		public double[] Values
		{ get; private set; }

		#region Miscellaneous
		// The format method allows you to customize the rendering of the performance value on the summary grid.
		public override string Format(object value, Cbi.PerformanceUnit unit, string propertyName)
		{
			double[] tmp = value as double[];
			if (tmp != null && tmp.Length == 5)
				switch (unit)
				{
					case Cbi.PerformanceUnit.Currency	: return Core.Globals.FormatCurrency(tmp[0], denomination);
					case Cbi.PerformanceUnit.Percent	: return tmp[1].ToString("P");
					case Cbi.PerformanceUnit.Pips		: return Math.Round(tmp[2]).ToString(Core.Globals.GeneralOptions.CurrentCulture);
					case Cbi.PerformanceUnit.Points		: return Math.Round(tmp[3]).ToString(Core.Globals.GeneralOptions.CurrentCulture);
					case Cbi.PerformanceUnit.Ticks		: return Math.Round(tmp[4]).ToString(Core.Globals.GeneralOptions.CurrentCulture);
				}
			return value.ToString();			// should not happen
		}
		#endregion
	}
}
