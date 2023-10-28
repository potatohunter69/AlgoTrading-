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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Linear Regression Slope
	/// </summary>
	public class LinRegSlope : Indicator
	{
		private double	avg;
		private double	divisor;
		private double	myPeriod;
		private double	priorSumXY;
		private	double	priorSumY;
		private double	sumX2;
		private double	sumXY;
		private double	sumY;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionLinRegSlope;
				Name						= NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameLinRegSlope;
				IsSuspendedWhileInactive	= true;
				Period						= 14;

				AddPlot(Brushes.Goldenrod, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameLinRegSlope);
			}
			else if (State == State.Configure)
				avg	= divisor = myPeriod = priorSumXY = priorSumY = sumX2 = sumY = sumXY = 0;
		}

		protected override void OnBarUpdate()
		{
			if (BarsArray[0].BarsType.IsRemoveLastBarSupported)
			{
				double sumX = (double)Period * (Period - 1) * 0.5;
				double divisor = sumX * sumX - (double)Period * Period * (Period - 1) * (2 * Period - 1) / 6;
				double sumXY = 0;

				for (int count = 0; count < Period && CurrentBar - count >= 0; count++)
					sumXY += count * Input[count];

				Value[0] = ((double)Period * sumXY - sumX * SUM(Inputs[0], Period)[0]) / divisor;
			}
			else
			{
				if (IsFirstTickOfBar)
				{
					priorSumY = sumY;
					priorSumXY = sumXY;
					myPeriod = Math.Min(CurrentBar + 1, Period);
					sumX2 = myPeriod * (myPeriod + 1) * 0.5;
					divisor = myPeriod * (myPeriod + 1) * (2 * myPeriod + 1) / 6 - sumX2 * sumX2 / myPeriod;
				}

				double input0 = Input[0];
				sumXY = priorSumXY - (CurrentBar >= Period ? priorSumY : 0) + myPeriod * input0;
				sumY = priorSumY + input0 - (CurrentBar >= Period ? Input[Period] : 0);
				avg = sumY / myPeriod;
				Value[0] = CurrentBar <= Period ? 0 : (sumXY - sumX2 * avg) / divisor;
			}
		}

		#region Properties
		[Range(2, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LinRegSlope[] cacheLinRegSlope;
		public LinRegSlope LinRegSlope(int period)
		{
			return LinRegSlope(Input, period);
		}

		public LinRegSlope LinRegSlope(ISeries<double> input, int period)
		{
			if (cacheLinRegSlope != null)
				for (int idx = 0; idx < cacheLinRegSlope.Length; idx++)
					if (cacheLinRegSlope[idx] != null && cacheLinRegSlope[idx].Period == period && cacheLinRegSlope[idx].EqualsInput(input))
						return cacheLinRegSlope[idx];
			return CacheIndicator<LinRegSlope>(new LinRegSlope(){ Period = period }, input, ref cacheLinRegSlope);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LinRegSlope LinRegSlope(int period)
		{
			return indicator.LinRegSlope(Input, period);
		}

		public Indicators.LinRegSlope LinRegSlope(ISeries<double> input , int period)
		{
			return indicator.LinRegSlope(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LinRegSlope LinRegSlope(int period)
		{
			return indicator.LinRegSlope(Input, period);
		}

		public Indicators.LinRegSlope LinRegSlope(ISeries<double> input , int period)
		{
			return indicator.LinRegSlope(input, period);
		}
	}
}

#endregion
