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
	/// The Williams %R is a momentum indicator that is designed to identify overbought and oversold areas in a nontrending market.
	/// </summary>
	public class WilliamsR : Indicator
	{
		private MAX max;
		private MIN min;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionWilliamsR;
				Name						= NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameWilliamsR;
				IsSuspendedWhileInactive	= true;
				Period						= 14;

				AddLine(Brushes.DarkGray,	-25,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorUpper);
				AddLine(Brushes.DarkGray,	-75,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorLower);
				AddPlot(Brushes.Goldenrod,			NinjaTrader.Custom.Resource.WilliamsPercentR);
			}
			else if (State == State.DataLoaded)
			{
				max = MAX(High, Period);
				min	= MIN(Low, Period);
			}
		}

		protected override void OnBarUpdate()
		{
			double max0	= max[0];
			double min0	= min[0];
			Value[0]	= -100 * (max0 - Close[0]) / (max0 - min0 == 0 ? 1 : max0 - min0);
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
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
		private WilliamsR[] cacheWilliamsR;
		public WilliamsR WilliamsR(int period)
		{
			return WilliamsR(Input, period);
		}

		public WilliamsR WilliamsR(ISeries<double> input, int period)
		{
			if (cacheWilliamsR != null)
				for (int idx = 0; idx < cacheWilliamsR.Length; idx++)
					if (cacheWilliamsR[idx] != null && cacheWilliamsR[idx].Period == period && cacheWilliamsR[idx].EqualsInput(input))
						return cacheWilliamsR[idx];
			return CacheIndicator<WilliamsR>(new WilliamsR(){ Period = period }, input, ref cacheWilliamsR);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.WilliamsR WilliamsR(int period)
		{
			return indicator.WilliamsR(Input, period);
		}

		public Indicators.WilliamsR WilliamsR(ISeries<double> input , int period)
		{
			return indicator.WilliamsR(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.WilliamsR WilliamsR(int period)
		{
			return indicator.WilliamsR(Input, period);
		}

		public Indicators.WilliamsR WilliamsR(ISeries<double> input , int period)
		{
			return indicator.WilliamsR(input, period);
		}
	}
}

#endregion
