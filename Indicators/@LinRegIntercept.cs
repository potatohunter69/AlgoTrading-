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
	/// Linnear Regression Intercept
	/// </summary>
	public class LinRegIntercept : Indicator
	{
		private double	avg;
		private double	divisor;
		private double	myPeriod;
		private double	priorSumXY;
		private	double	priorSumY;
		private double	slope;
		private double	sumX2;
		private	double	sumX;
		private double	sumXY;
		private double	sumY;
		private SUM		sum;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionLinRegIntercept;
				Name						= NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameLinRegIntercept;
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				Period						= 14;

				AddPlot(Brushes.Goldenrod, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameLinRegIntercept);
			}
			else if (State == State.Configure)
			{
				avg	= divisor = myPeriod = priorSumXY = priorSumY = slope = sumX = sumX2 = sumY = sumXY = 0;
			}
			else if (State == State.DataLoaded)
			{
				sum = SUM(Inputs[0], Period);
			}
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

				double slope = ((double)Period * sumXY - sumX * SUM(Inputs[0], Period)[0]) / divisor;
				Value[0] = (SUM(Inputs[0], Period)[0] - slope * sumX) / Period;
			}
			else
			{
				if (IsFirstTickOfBar)
				{
					priorSumY = sumY;
					priorSumXY = sumXY;
					myPeriod = Math.Min(CurrentBar + 1, Period);
					sumX = myPeriod * (myPeriod - 1) * 0.5;
					sumX2 = myPeriod * (myPeriod + 1) * 0.5;
					divisor = myPeriod * (myPeriod + 1) * (2 * myPeriod + 1) / 6 - sumX2 * sumX2 / myPeriod;
				}

				double input0 = Input[0];
				sumXY = priorSumXY - (CurrentBar >= Period ? priorSumY : 0) + myPeriod * input0;
				sumY = priorSumY + input0 - (CurrentBar >= Period ? Input[Period] : 0);
				avg = sumY / myPeriod;
				slope = (sumXY - sumX2 * avg) / divisor;
				Value[0] = CurrentBar == 0 ? input0 : (sum[0] - slope * sumX) / myPeriod;
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
		private LinRegIntercept[] cacheLinRegIntercept;
		public LinRegIntercept LinRegIntercept(int period)
		{
			return LinRegIntercept(Input, period);
		}

		public LinRegIntercept LinRegIntercept(ISeries<double> input, int period)
		{
			if (cacheLinRegIntercept != null)
				for (int idx = 0; idx < cacheLinRegIntercept.Length; idx++)
					if (cacheLinRegIntercept[idx] != null && cacheLinRegIntercept[idx].Period == period && cacheLinRegIntercept[idx].EqualsInput(input))
						return cacheLinRegIntercept[idx];
			return CacheIndicator<LinRegIntercept>(new LinRegIntercept(){ Period = period }, input, ref cacheLinRegIntercept);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LinRegIntercept LinRegIntercept(int period)
		{
			return indicator.LinRegIntercept(Input, period);
		}

		public Indicators.LinRegIntercept LinRegIntercept(ISeries<double> input , int period)
		{
			return indicator.LinRegIntercept(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LinRegIntercept LinRegIntercept(int period)
		{
			return indicator.LinRegIntercept(Input, period);
		}

		public Indicators.LinRegIntercept LinRegIntercept(ISeries<double> input , int period)
		{
			return indicator.LinRegIntercept(input, period);
		}
	}
}

#endregion
