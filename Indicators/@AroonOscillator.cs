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
	/// The Aroon Oscillator is based upon his Aroon Indicator. Much like the Aroon Indicator,
	///  the Aroon Oscillator measures the strength of a trend.
	/// </summary>
	public class AroonOscillator : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionAroonOscillator;
				Name						= NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameAroonOscillator;
				IsSuspendedWhileInactive	= true;
				Period						= 14;

				AddLine(Brushes.DarkGray,	0,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorZeroLine);
				AddPlot(Brushes.Goldenrod,		NinjaTrader.Custom.Resource.NinjaScriptIndicatorUp);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar == 0)
				Value[0] = 0;
			else
			{
				int back = Math.Min(Period, CurrentBar);
				int idxMax = -1;
				int idxMin = -1;
				double max = double.MinValue;
				double min = double.MaxValue;

				for (int idx = back; idx >= 0; idx--)
				{
					if (High[back - idx].ApproxCompare(max) >= 0)
					{
						max = High[back - idx];
						idxMax = CurrentBar - back + idx;
					}

					if (Low[back - idx].ApproxCompare(min) <= 0)
					{
						min = Low[back - idx];
						idxMin = CurrentBar - back + idx;
					}
				}

				Value[0] = 100 * ((double)(back - (CurrentBar - idxMax)) / back) - 100 * ((double)(back - (CurrentBar - idxMin)) / back);
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
		private AroonOscillator[] cacheAroonOscillator;
		public AroonOscillator AroonOscillator(int period)
		{
			return AroonOscillator(Input, period);
		}

		public AroonOscillator AroonOscillator(ISeries<double> input, int period)
		{
			if (cacheAroonOscillator != null)
				for (int idx = 0; idx < cacheAroonOscillator.Length; idx++)
					if (cacheAroonOscillator[idx] != null && cacheAroonOscillator[idx].Period == period && cacheAroonOscillator[idx].EqualsInput(input))
						return cacheAroonOscillator[idx];
			return CacheIndicator<AroonOscillator>(new AroonOscillator(){ Period = period }, input, ref cacheAroonOscillator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AroonOscillator AroonOscillator(int period)
		{
			return indicator.AroonOscillator(Input, period);
		}

		public Indicators.AroonOscillator AroonOscillator(ISeries<double> input , int period)
		{
			return indicator.AroonOscillator(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AroonOscillator AroonOscillator(int period)
		{
			return indicator.AroonOscillator(Input, period);
		}

		public Indicators.AroonOscillator AroonOscillator(ISeries<double> input , int period)
		{
			return indicator.AroonOscillator(input, period);
		}
	}
}

#endregion
