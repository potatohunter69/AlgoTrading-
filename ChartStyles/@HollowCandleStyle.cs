#region Using declarations
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using NinjaTrader.Gui;

#endregion

namespace NinjaTrader.NinjaScript.ChartStyles
{
	public class HollowCandleStyle : ChartStyle
	{
		private				object									icon;
		private				System.Windows.Media.Brush				dojiBrush;
		private				Brush									dojiBrushDX;

		public override int GetBarPaintWidth(int barWidth) { return 1 + 2 * (barWidth - 1) + 2 * (int) Math.Round(Stroke.Width); }

		public override object Icon { get { return icon ?? (icon = Gui.Tools.Icons.ChartChartStyleHollow); } }

		public override void OnRender(ChartControl chartControl, ChartScale chartScale, ChartBars chartBars)
		{
			Bars			bars			= chartBars.Bars;
			float			barWidth		= GetBarPaintWidth(BarWidthUI);
			Vector2			point0			= new Vector2();
			Vector2			point1			= new Vector2();
			RectangleF		rect			= new RectangleF();

			for (int idx = chartBars.FromIndex; idx <= chartBars.ToIndex; idx++)
			{
				//Brush		overriddenBarBrush		= chartControl.GetBarOverrideBrush(chartBars, idx);
				Brush		overriddenOutlineBrush	= chartControl.GetCandleOutlineOverrideBrush(chartBars, idx);
				double		closeValue				= bars.GetClose(idx);
				int			close					= chartScale.GetYByValue(closeValue);
				int			high					= chartScale.GetYByValue(bars.GetHigh(idx));
				int			low						= chartScale.GetYByValue(bars.GetLow(idx));
				double		openValue				= bars.GetOpen(idx);
				int			open					= chartScale.GetYByValue(openValue);
				int			x						= chartControl.GetXByBarIndex(chartBars, idx);
				Brush		brush					= overriddenOutlineBrush ?? (closeValue > openValue ? UpBrushDX : closeValue < openValue ? DownBrushDX : DojiBrushDX);
				//Brush br = overriddenOutlineBrush ?? Stroke2.BrushDX;

				if (Math.Abs(open - close) < 0.0000001)
				{
					// Line 
					point0.X	= x - barWidth * 0.5f;
					point0.Y	= close;
					point1.X	= x + barWidth * 0.5f;
					point1.Y	= close;
					if (!(brush is SolidColorBrush))
						TransformBrush(overriddenOutlineBrush ?? DojiBrushDX, new RectangleF(point0.X, point0.Y - LineWidth, barWidth, LineWidth));
					RenderTarget.DrawLine(point0, point1, brush, LineWidth);
				}
				else
				{
					// Candle
					rect.X		= x - barWidth * 0.5f + 0.5f;
					rect.Y		= Math.Min(close, open);
					rect.Width	= barWidth - 1;
					rect.Height	= Math.Max(open, close) - Math.Min(close, open);
					if (!(brush is SolidColorBrush))
						TransformBrush(brush, rect);
					RenderTarget.DrawRectangle(rect, brush, LineWidth);
					if (chartBars.IsInHitTest)
						RenderTarget.FillRectangle(rect, chartControl.SelectionBrush);
				}

				// High wick
				if (high < Math.Min(open, close))
				{
					point0.X	= x;
					point0.Y	= high;
					point1.X	= x;
					point1.Y	= Math.Min(open, close);
					if (!(brush is SolidColorBrush))
						TransformBrush(brush, new RectangleF(point0.X - Stroke2.Width, point0.Y, LineWidth, point1.Y - point0.Y));
					RenderTarget.DrawLine(point0, point1, brush, LineWidth);
				}

				// Low wick
				if (low > Math.Max(open, close))
				{
					point0.X = x;
					point0.Y = low;
					point1.X = x;
					point1.Y = Math.Max(open, close);
					if (!(brush is SolidColorBrush))
						TransformBrush(brush, new RectangleF(point1.X - Stroke2.Width, point1.Y, LineWidth, point0.Y - point1.Y));
					RenderTarget.DrawLine(point0, point1, brush, LineWidth);
				}
			}
		}

		public override void OnRenderTargetChanged()
		{
			if (dojiBrushDX != null)
				dojiBrushDX.Dispose();
			dojiBrushDX = null;
			base.OnRenderTargetChanged();
		}

		[Range(1, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptChartStyleLineWidth", GroupName = "NinjaScriptGeneral")]
		public int LineWidth { get; set; }


		[Display (ResourceType = typeof(Custom.Resource), Name = "GuiChartStyleDojiBrush", GroupName = "NinjaScriptGeneral")]
		[XmlIgnore]
		public System.Windows.Media.Brush DojiBrush
		{
			get { return dojiBrush ?? (dojiBrush = System.Windows.Media.Brushes.DimGray); }
			set
			{
				dojiBrush = value;
				if (dojiBrush != null && dojiBrush.CanFreeze) 
					dojiBrush.Freeze();
				dojiBrushDX = null;
			}
		}

		[Browsable(false)]
		[XmlIgnore]
		[CLSCompliant(false)]
		public Brush DojiBrushDX
		{
			get
			{
				if (dojiBrushDX == null || dojiBrushDX.IsDisposed)
					dojiBrushDX = DojiBrush.ToDxBrush(RenderTarget);
				return dojiBrushDX;
			}
		}

		[Browsable(false)]
		public string DojiBrushSerialize
		{
			get { return Serialize.BrushToString(DojiBrush); }
			set { DojiBrush = Serialize.StringToBrush(value); }
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name			= Custom.Resource.NinjaScriptChartStyleCandlestickHollow;
				ChartStyleType	= ChartStyleType.HollowCandleStick;
				LineWidth		= 1;
			}
			else if (State == State.Configure)
			{
				SetPropertyName("BarWidth",		Custom.Resource.NinjaScriptChartStyleBarWidth);
				SetPropertyName("DownBrush",	Custom.Resource.NinjaScriptChartStyleCandleDownBarsColor);
				SetPropertyName("UpBrush",		Custom.Resource.NinjaScriptChartStyleCandleUpBarsColor);

				Properties.Remove(Properties.Find("Stroke", true));
				Properties.Remove(Properties.Find("Stroke2", true));
			}
		}
	}
}
