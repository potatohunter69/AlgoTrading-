#region Using declarations
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using SharpDX;
using SharpDX.Direct2D1;
using System;
#endregion

namespace NinjaTrader.NinjaScript.ChartStyles
{
	public class BoxStyle : ChartStyle
	{
		private object icon;

		public override int GetBarPaintWidth(int barWidth)
		{
			return 10 - 2 * (int) Math.Round(Stroke.Width);
		}

		public override object Icon
		{
			get { return icon ?? (icon = NinjaTrader.Gui.Tools.Icons.ChartBox2); }
		}

		public override void OnRender(ChartControl chartControl, ChartScale chartScale, ChartBars chartBars)
		{
			Bars			bars				= chartBars.Bars;
			float			chartMinX			= ConvertToHorizontalPixels(chartControl, chartControl.CanvasLeft + chartControl.Properties.BarMarginRight);
			RectangleF		rect				= new RectangleF();
			int				toIndex				= chartBars.ToIndex;

			if (toIndex >= 0 && toIndex < bars.Count - 1)
				toIndex++;

			for (int idx = chartBars.FromIndex; idx <= toIndex; idx++)
			{
				double	closeValue				= bars.GetClose(idx);
				float	high					= chartScale.GetYByValue(bars.GetHigh(idx));
				float	low						= chartScale.GetYByValue(bars.GetLow(idx));
				double	openValue				= bars.GetOpen(idx);
				Brush	overriddenBarBrush		= chartControl.GetBarOverrideBrush(chartBars, idx);
				Brush	overriddenOutlineBrush	= chartControl.GetCandleOutlineOverrideBrush(chartBars, idx);
				float	x						= chartControl.GetXByBarIndex(chartBars, idx);
				float	boxStartPosition;
				
				if (idx == chartBars.FromIndex && (toIndex == 0 || idx == 0))
				{
					if (toIndex == 0)
						boxStartPosition = chartMinX;
					else
						boxStartPosition = 2 * x - chartControl.GetXByBarIndex(chartBars, idx + 1);
				}
				else
					boxStartPosition = chartControl.GetXByBarIndex(chartBars, idx - 1);

				if (Math.Abs(x - boxStartPosition) < 0.2)
					continue;

				float width = Math.Max(2f, Math.Abs(x - boxStartPosition));

				if (closeValue > openValue)
				{
					width		-= Stroke.Width;
					rect.X		= boxStartPosition;
					rect.Y		= high;
					rect.Width	= width;
					rect.Height	= low - high;
					TransformBrush(overriddenBarBrush ?? UpBrushDX, rect);
					TransformBrush(overriddenOutlineBrush ?? Stroke.BrushDX, rect);
					RenderTarget.FillRectangle(rect, overriddenBarBrush ?? UpBrushDX);
					RenderTarget.DrawRectangle(rect, overriddenOutlineBrush ?? Stroke.BrushDX, Stroke.Width, Stroke.StrokeStyle);
				}
				else
				{
					width		-= Stroke2.Width;
					rect.X		= boxStartPosition;
					rect.Y		= high;
					rect.Width	= width;
					rect.Height	= low - high;
					TransformBrush(overriddenBarBrush ?? DownBrushDX, rect);
					TransformBrush(overriddenOutlineBrush ?? Stroke2.BrushDX, rect);
					RenderTarget.FillRectangle(rect, overriddenBarBrush ?? DownBrushDX);
					RenderTarget.DrawRectangle(rect, overriddenOutlineBrush ?? Stroke2.BrushDX, Stroke2.Width, Stroke2.StrokeStyle);
				}
			}
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name			= Custom.Resource.NinjaScriptChartStyleBox;
				ChartStyleType	= ChartStyleType.Box;
				BarWidth		= 1;
			}
			else if (State == State.Configure)
			{
				Properties.Remove(Properties.Find("BarWidthUI", true));

				SetPropertyName("DownBrush",	Custom.Resource.NinjaScriptChartStyleBoxDownBarsColor);
				SetPropertyName("UpBrush",		Custom.Resource.NinjaScriptChartStyleBoxUpBarsColor);
				SetPropertyName("Stroke",		Custom.Resource.NinjaScriptChartStyleBoxUpBarsOutline);
				SetPropertyName("Stroke2",		Custom.Resource.NinjaScriptChartStyleBoxDownBarsOutline);
			}
		}
	}
}
