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
	/// The APZ (Adaptive Prize Zone) forms a steady channel based on double smoothed
	/// exponential moving averages around the average price. See S/C, September 2006, p.28.
	/// </summary>
	public class APZ : Indicator
	{
		private EMA		emaEMA;
		private EMA		emaRange;
		private int		newPeriod;
		private int		period;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionAPZ;
				Name						= NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameAPZ;
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				BandPct						= 2;
				Period						= 20;

				AddPlot(Brushes.Crimson, NinjaTrader.Custom.Resource.NinjaScriptIndicatorLower);
				AddPlot(Brushes.Crimson, NinjaTrader.Custom.Resource.NinjaScriptIndicatorUpper);
			}
			else if (State == State.DataLoaded)
			{
				emaEMA		= EMA(EMA(newPeriod), newPeriod);
				emaRange	= EMA(Range(), Period);
				newPeriod	= 0;
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < Period)
			{
				Lower[0] = Input[0];
				Upper[0] = Input[0];
				return;
			}

			double rangeOffset	= BandPct * emaRange[0];
			double emaEMA0		= emaEMA[0];

			Lower[0] = emaEMA0 - rangeOffset;
			Upper[0] = emaEMA0 + rangeOffset;
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BandPct", GroupName = "NinjaScriptParameters", Order = 0)]
		public double BandPct
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Lower
		{
			get { return Values[0]; }
		}

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 1)]
		public int Period
		{
			get { return period; }
			set
			{
				period = value;
				newPeriod = Convert.ToInt32(Math.Sqrt(Convert.ToDouble(value)));
			}
		}

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
		private APZ[] cacheAPZ;
		public APZ APZ(double bandPct, int period)
		{
			return APZ(Input, bandPct, period);
		}

		public APZ APZ(ISeries<double> input, double bandPct, int period)
		{
			if (cacheAPZ != null)
				for (int idx = 0; idx < cacheAPZ.Length; idx++)
					if (cacheAPZ[idx] != null && cacheAPZ[idx].BandPct == bandPct && cacheAPZ[idx].Period == period && cacheAPZ[idx].EqualsInput(input))
						return cacheAPZ[idx];
			return CacheIndicator<APZ>(new APZ(){ BandPct = bandPct, Period = period }, input, ref cacheAPZ);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.APZ APZ(double bandPct, int period)
		{
			return indicator.APZ(Input, bandPct, period);
		}

		public Indicators.APZ APZ(ISeries<double> input , double bandPct, int period)
		{
			return indicator.APZ(input, bandPct, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.APZ APZ(double bandPct, int period)
		{
			return indicator.APZ(Input, bandPct, period);
		}

		public Indicators.APZ APZ(ISeries<double> input , double bandPct, int period)
		{
			return indicator.APZ(input, bandPct, period);
		}
	}
}

#endregion
