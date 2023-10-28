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
	/// Chaikin Money Flow.
	/// </summary>
	public class ChaikinMoneyFlow : Indicator
	{
		private	Series<double>		moneyFlow;
		private SUM					sumMoneyFlow;
		private SUM					sumVolume;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionChaikinMoneyFlow;
				Name						= NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameChaikinMoneyFlow;
				IsSuspendedWhileInactive	= true;
				DrawOnPricePanel			= false;
				Period						= 21;

				AddPlot(Brushes.Goldenrod, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameChaikinMoneyFlow);
			}
			else if (State == State.DataLoaded)
			{
				moneyFlow		= new Series<double>(this);
				sumMoneyFlow	= SUM(moneyFlow, Period);
				sumVolume		= SUM(Volume, Period);
			}
			else if (State == State.Historical)
			{
				if (Calculate == Calculate.OnPriceChange)
				{
					Draw.TextFixed(this, "NinjaScriptInfo", string.Format(NinjaTrader.Custom.Resource.NinjaScriptOnPriceChangeError, Name), TextPosition.BottomRight);
					Log(string.Format(NinjaTrader.Custom.Resource.NinjaScriptOnPriceChangeError, Name), LogLevel.Error);
				}
			}
		}

		protected override void OnBarUpdate()
		{
			double close0		= Close[0];
			double low0			= Low[0];
			double high0		= High[0];
			double volume0		= Instrument.MasterInstrument.InstrumentType == InstrumentType.CryptoCurrency ? Core.Globals.ToCryptocurrencyVolume((long)Volume[0]) : Volume[0];
			double sumVolume0	= Instrument.MasterInstrument.InstrumentType == InstrumentType.CryptoCurrency ? Core.Globals.ToCryptocurrencyVolume((long)sumVolume[0]) : sumVolume[0];

			moneyFlow[0]		= volume0 * ((close0 - low0) - (high0 - close0)) / ((high0 - low0).ApproxCompare(0) == 0 ? 1 : (high0 - low0));
			
			double val 			= 100 * sumMoneyFlow[0] / sumVolume0;
			Value[0]			= double.IsNaN(val) ? 0 : val;
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
		private ChaikinMoneyFlow[] cacheChaikinMoneyFlow;
		public ChaikinMoneyFlow ChaikinMoneyFlow(int period)
		{
			return ChaikinMoneyFlow(Input, period);
		}

		public ChaikinMoneyFlow ChaikinMoneyFlow(ISeries<double> input, int period)
		{
			if (cacheChaikinMoneyFlow != null)
				for (int idx = 0; idx < cacheChaikinMoneyFlow.Length; idx++)
					if (cacheChaikinMoneyFlow[idx] != null && cacheChaikinMoneyFlow[idx].Period == period && cacheChaikinMoneyFlow[idx].EqualsInput(input))
						return cacheChaikinMoneyFlow[idx];
			return CacheIndicator<ChaikinMoneyFlow>(new ChaikinMoneyFlow(){ Period = period }, input, ref cacheChaikinMoneyFlow);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ChaikinMoneyFlow ChaikinMoneyFlow(int period)
		{
			return indicator.ChaikinMoneyFlow(Input, period);
		}

		public Indicators.ChaikinMoneyFlow ChaikinMoneyFlow(ISeries<double> input , int period)
		{
			return indicator.ChaikinMoneyFlow(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ChaikinMoneyFlow ChaikinMoneyFlow(int period)
		{
			return indicator.ChaikinMoneyFlow(Input, period);
		}

		public Indicators.ChaikinMoneyFlow ChaikinMoneyFlow(ISeries<double> input , int period)
		{
			return indicator.ChaikinMoneyFlow(input, period);
		}
	}
}

#endregion
