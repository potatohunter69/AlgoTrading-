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

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// The TSF (Time Series Forecast) calculates probable future values for the price
	/// by fitting a linear regression line over a given number of price bars and following
	///  that line forward into the future. A linear regression line is a straight line which
	///  is as close to all of the given price points as possible. Also see the Linear Regression indicator.
	/// </summary>
	public class TSF : Indicator
	{
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
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionTSF;
				Name						= NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameTSF;
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				Period						= 14;
				Forecast					= 3;

				AddPlot(Brushes.Goldenrod, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameTSF);
			}
			else if (State == State.Configure)
			{
				avg	= divisor = intercept = myPeriod = priorSumXY
					= priorSumY = slope = sumX = sumX2 = sumY = sumXY = 0;
			}
			else if (State == State.DataLoaded)
			{
				sum = SUM(Inputs[0], Period);
				y	= new Series<double>(this);
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

				y[0] = Input[0];

				double slope = ((double)Period * sumXY - sumX * SUM(y, Period)[0]) / divisor;
				double intercept = (SUM(y, Period)[0] - slope * sumX) / Period;

				Value[0] = intercept + slope * ((Period - 1) + Forecast);
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
				intercept = (sum[0] - slope * sumX) / myPeriod;
				Value[0] = CurrentBar == 0 ? input0 : intercept + slope * ((myPeriod - 1) + Forecast);
			}
		}


		#region Properties
		[Range(-10, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Forecast", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Forecast
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 1)]
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
		private TSF[] cacheTSF;
		public TSF TSF(int forecast, int period)
		{
			return TSF(Input, forecast, period);
		}

		public TSF TSF(ISeries<double> input, int forecast, int period)
		{
			if (cacheTSF != null)
				for (int idx = 0; idx < cacheTSF.Length; idx++)
					if (cacheTSF[idx] != null && cacheTSF[idx].Forecast == forecast && cacheTSF[idx].Period == period && cacheTSF[idx].EqualsInput(input))
						return cacheTSF[idx];
			return CacheIndicator<TSF>(new TSF(){ Forecast = forecast, Period = period }, input, ref cacheTSF);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TSF TSF(int forecast, int period)
		{
			return indicator.TSF(Input, forecast, period);
		}

		public Indicators.TSF TSF(ISeries<double> input , int forecast, int period)
		{
			return indicator.TSF(input, forecast, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TSF TSF(int forecast, int period)
		{
			return indicator.TSF(Input, forecast, period);
		}

		public Indicators.TSF TSF(ISeries<double> input , int forecast, int period)
		{
			return indicator.TSF(input, forecast, period);
		}
	}
}

#endregion
