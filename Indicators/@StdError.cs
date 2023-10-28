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
	/// Standard Error shows how near prices go around a linear regression line.
	/// </summary>
	public class StdError : Indicator
	{
		// Documentation of Linear Regression: http://en.wikipedia.org/wiki/Linear_regression
		// Documentation of Standard Error: http://tadoc.org/indicator/STDERR.htm
		private double			avg;
		private double			divisor;
		private	double			intercept;
		private double			myPeriod;
		private double			priorSumXY;
		private	double			priorSumY;
		private double			slope;
		private double			sumX2;
		private	double			sumX;
		private double			sumXY;
		private double			sumY;
		private SUM				sum;
		private Series<double>	y;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionStdError;
				Name						= NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameStdError;
				IsSuspendedWhileInactive	= true;
				Period						= 14;
				IsOverlay					= true;

				AddPlot(Brushes.Goldenrod,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameLinReg);
				AddPlot(Brushes.DarkCyan,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorUpper);
				AddPlot(Brushes.DarkCyan,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorLower);
			}
			else if (State == State.Configure)
			{
				avg	= divisor = intercept = myPeriod = priorSumXY
					= priorSumY = slope = sumX = sumX2 = sumY = sumXY = 0;
			}
			else if (State == State.DataLoaded)
			{
				y	= new Series<double>(this);
				sum = SUM(Inputs[0], Period);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsArray[0].BarsType.IsRemoveLastBarSupported)
			{
				// calculate Linear Regression
				double sumX = (double)Period * (Period - 1) * 0.5;
				double divisor = sumX * sumX - (double)Period * Period * (Period - 1) * (2 * Period - 1) / 6;
				double sumXY = 0;

				for (int count = 0; count < Period && CurrentBar - count >= 0; count++)
					sumXY += count * Input[count];

				y[0] = Input[0];
				double slope = ((double)Period * sumXY - sumX * SUM(y, Period)[0]) / divisor;
				double intercept = (SUM(y, Period)[0] - slope * sumX) / Period;
				double linReg = intercept + slope * (Period - 1);

				// Calculate Standard Error
				double sumSquares = 0;
				for (int count = 0; count < Period && CurrentBar - count >= 0; count++)
				{
					double linRegX = intercept + slope * (Period - 1 - count);
					double valueX = Input[count];
					double diff = Math.Abs(valueX - linRegX);

					sumSquares += diff * diff;
				}
				double stdErr = Math.Sqrt(sumSquares / Period);

				Middle[0]	= linReg;
				Upper[0]	= linReg + stdErr;
				Lower[0]	= linReg - stdErr;
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
					divisor = myPeriod != 1 ? myPeriod * (myPeriod + 1) * (2 * myPeriod + 1) / 6 - sumX2 * sumX2 / myPeriod : 1.0;
				}

				double input0 = Input[0];
				sumXY = priorSumXY - (CurrentBar >= Period ? priorSumY : 0) + myPeriod * input0;
				sumY = priorSumY + input0 - (CurrentBar >= Period ? Input[Period] : 0);
				avg = myPeriod != 0 ? sumY / myPeriod : 0.0;
				slope = (sumXY - sumX2 * avg) / divisor;
				intercept = myPeriod != 0 ? (sum[0] - slope * sumX) / myPeriod : 0.0;
				double linReg = (intercept + slope * (myPeriod - 1));

				// Calculate Standard Error
				double sumSquares = 0;
				for (int count = 0; count < Period && CurrentBar - count >= 0; count++)
				{
					double linRegX = intercept + slope * (Period - 1 - count);
					double valueX = Input[count];
					double diff = Math.Abs(valueX - linRegX);
					sumSquares += diff * diff;
				}

				double stdErr = Math.Sqrt(sumSquares / Period);
				Middle[0] = CurrentBar == 0 ? input0 : linReg;
				Upper[0] = CurrentBar == 0 ? input0 : linReg + stdErr;
				Lower[0] = CurrentBar == 0 ? input0 : linReg - stdErr;
			}
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Lower
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Middle
		{
			get { return Values[0]; }
		}

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Upper
		{
			get { return Values[1]; }
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private StdError[] cacheStdError;
		public StdError StdError(int period)
		{
			return StdError(Input, period);
		}

		public StdError StdError(ISeries<double> input, int period)
		{
			if (cacheStdError != null)
				for (int idx = 0; idx < cacheStdError.Length; idx++)
					if (cacheStdError[idx] != null && cacheStdError[idx].Period == period && cacheStdError[idx].EqualsInput(input))
						return cacheStdError[idx];
			return CacheIndicator<StdError>(new StdError(){ Period = period }, input, ref cacheStdError);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.StdError StdError(int period)
		{
			return indicator.StdError(Input, period);
		}

		public Indicators.StdError StdError(ISeries<double> input , int period)
		{
			return indicator.StdError(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.StdError StdError(int period)
		{
			return indicator.StdError(Input, period);
		}

		public Indicators.StdError StdError(ISeries<double> input , int period)
		{
			return indicator.StdError(input, period);
		}
	}
}

#endregion
