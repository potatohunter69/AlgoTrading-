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
	/// The Commodity Channel Index (CCI) measures the variation of a security's price
	/// from its statistical mean. High values show that prices are unusually high
	/// compared to average prices whereas low values indicate that prices are unusually low.
	/// </summary>
	public class CCI : Indicator
	{
		private SMA sma;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionCCI;
				Name						= NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameCCI;
				IsSuspendedWhileInactive	= true;
				Period						= 14;

				AddPlot(Brushes.Goldenrod,			NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameCCI);
				AddLine(Brushes.DarkGray,	200,	NinjaTrader.Custom.Resource.CCILevel2);
				AddLine(Brushes.DarkGray,	100,	NinjaTrader.Custom.Resource.CCILevel1);
				AddLine(Brushes.DarkGray,	0,		NinjaTrader.Custom.Resource.NinjaScriptIndicatorZeroLine);
				AddLine(Brushes.DarkGray,	-100,	NinjaTrader.Custom.Resource.CCILevelMinus1);
				AddLine(Brushes.DarkGray,	-200,	NinjaTrader.Custom.Resource.CCILevelMinus2);
			}
			else if (State == State.DataLoaded)
				sma  = SMA(Typical, Period);
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar == 0)
				Value[0] = 0;
			else
			{
				double mean = 0;
				double sma0 = sma[0];

				for (int idx = Math.Min(CurrentBar, Period - 1); idx >= 0; idx--)
					mean += Math.Abs(Typical[idx] - sma0);

				Value[0] = (Typical[0] - sma0) / (mean.ApproxCompare(0) == 0 ? 1 : (0.015 * (mean / Math.Min(Period, CurrentBar + 1))));
			}
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
		private CCI[] cacheCCI;
		public CCI CCI(int period)
		{
			return CCI(Input, period);
		}

		public CCI CCI(ISeries<double> input, int period)
		{
			if (cacheCCI != null)
				for (int idx = 0; idx < cacheCCI.Length; idx++)
					if (cacheCCI[idx] != null && cacheCCI[idx].Period == period && cacheCCI[idx].EqualsInput(input))
						return cacheCCI[idx];
			return CacheIndicator<CCI>(new CCI(){ Period = period }, input, ref cacheCCI);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CCI CCI(int period)
		{
			return indicator.CCI(Input, period);
		}

		public Indicators.CCI CCI(ISeries<double> input , int period)
		{
			return indicator.CCI(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CCI CCI(int period)
		{
			return indicator.CCI(Input, period);
		}

		public Indicators.CCI CCI(ISeries<double> input , int period)
		{
			return indicator.CCI(input, period);
		}
	}
}

#endregion
