#region Using declarations
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
#endregion

//This namespace holds Chart styles in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.ChartStyles
{
	public class Equivolume : ChartStyle
	{
		private object icon;

		public override object Icon { get { return icon ?? (icon = Gui.Tools.Icons.ChartEquivolume); } }

		public override int GetBarPaintWidth(int barWidth) { return 1 + 2 * (barWidth - 1) + 2 * (int) Math.Round(Stroke.Width); }

		public override void OnRender(ChartControl chartControl, ChartScale chartScale, ChartBars chartBars)
		{
			Bars			bars			= chartBars.Bars;
			Vector2			point0			= new Vector2();
			Vector2			point1			= new Vector2();
			RectangleF		rect			= new RectangleF();
			float			maxHalfWidth	= (GetBarPaintWidth(BarWidthUI) - 1) / 2;
			float			maxMeanAvgVol	= 0;

			for (int idx = chartBars.FromIndex; idx < chartBars.ToIndex; idx++)
			{
				float meanAvg = (bars.GetVolume(idx) + bars.GetVolume(idx + 1)) / 2;
				if (meanAvg > maxMeanAvgVol)
					maxMeanAvgVol = meanAvg;
			}

			for (int idx = chartBars.FromIndex; idx <= chartBars.ToIndex; idx++)
			{
				Brush		overriddenBarBrush		= chartControl.GetBarOverrideBrush(chartBars, idx);
				Brush		overriddenOutlineBrush	= chartControl.GetCandleOutlineOverrideBrush(chartBars, idx);
				double		closeValue				= bars.GetClose(idx);
				int			close					= chartScale.GetYByValue(closeValue);
				int			high					= chartScale.GetYByValue(bars.GetHigh(idx));
				int			low						= chartScale.GetYByValue(bars.GetLow(idx));
				float		barWidth				= 1 + (2 * (float) Math.Round(bars.GetVolume(idx) / maxMeanAvgVol * maxHalfWidth));
				double		openValue				= bars.GetOpen(idx);
				int			open					= chartScale.GetYByValue(openValue);
				int			x						= chartControl.GetXByBarIndex(chartBars, idx);

				if (Math.Abs(open - close) < 0.0000001)
				{
					// Line 
					point0.X	= x - barWidth * 0.5f;
					point0.Y	= close;
					point1.X	= x + barWidth * 0.5f;
					point1.Y	= close;
					Brush b		= overriddenOutlineBrush ?? Stroke.BrushDX;
					if (!(b is SolidColorBrush))
						TransformBrush(overriddenOutlineBrush ?? Stroke.BrushDX, new RectangleF(point0.X, point0.Y - Stroke.Width, barWidth, Stroke.Width));
					RenderTarget.DrawLine(point0, point1, b, Stroke.Width, Stroke.StrokeStyle);
				}
				else
				{
					// Candle
					rect.X		= x - barWidth * 0.5f + 0.5f;
					rect.Y		= Math.Min(close, open);
					rect.Width	= barWidth - 1;
					rect.Height	= Math.Max(open, close) - Math.Min(close, open);
					Brush brush	= overriddenBarBrush ?? (closeValue >= openValue ? UpBrushDX : DownBrushDX);
					if (!(brush is SolidColorBrush))
						TransformBrush(brush, rect);
					RenderTarget.FillRectangle(rect, brush);
					brush = overriddenOutlineBrush ?? Stroke.BrushDX;
					if (!(brush is SolidColorBrush))
						TransformBrush(brush, rect);
					RenderTarget.DrawRectangle(rect, overriddenOutlineBrush ?? Stroke.BrushDX, Stroke.Width, Stroke.StrokeStyle);
				}

				Brush br = overriddenOutlineBrush ?? Stroke2.BrushDX;

				// High wick
				if (high < Math.Min(open, close))
				{
					point0.X	= x;
					point0.Y	= high;
					point1.X	= x;
					point1.Y	= Math.Min(open, close);
					if (!(br is SolidColorBrush))
						TransformBrush(br, new RectangleF(point0.X - Stroke2.Width, point0.Y, Stroke2.Width, point1.Y - point0.Y));
					RenderTarget.DrawLine(point0, point1, br, Stroke2.Width, Stroke2.StrokeStyle);
				}

				// Low wick
				if (low > Math.Max(open, close))
				{
					point0.X = x;
					point0.Y = low;
					point1.X = x;
					point1.Y = Math.Max(open, close);
					if (!(br is SolidColorBrush))
						TransformBrush(br, new RectangleF(point1.X - Stroke2.Width, point1.Y, Stroke2.Width, point0.Y - point1.Y));
					RenderTarget.DrawLine(point0, point1, br, Stroke2.Width, Stroke2.StrokeStyle);
				}
			}
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name			= Custom.Resource.NinjaScriptChartStyleEquivolume;
				ChartStyleType	= ChartStyleType.Equivolume;
				BarWidth		= 5;
			}
			else if (State == State.Configure)
			{
				SetPropertyName("BarWidth",		Custom.Resource.NinjaScriptChartStyleBarWidth);
				SetPropertyName("DownBrush",	Custom.Resource.NinjaScriptChartStyleCandleDownBarsColor);
				SetPropertyName("UpBrush",		Custom.Resource.NinjaScriptChartStyleCandleUpBarsColor);
				SetPropertyName("Stroke",		Custom.Resource.NinjaScriptChartStyleCandleOutline);
				SetPropertyName("Stroke2",		Custom.Resource.NinjaScriptChartStyleCandleWick);
			}
		}
	}
}
