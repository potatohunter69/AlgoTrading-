#region Using declarations
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Windows.Media;
#endregion

namespace NinjaTrader.NinjaScript.ChartStyles
{
	public class PointAndFigureStyle : ChartStyle
	{
		private object icon;

		private bool isUp;
		private bool trendDetermined;

		public override int GetBarPaintWidth(int barWidth) { return 1 + 2 * (barWidth - 1) + 2 * (int) Math.Round(Stroke.Width); }

		public override object Icon { get { return icon ?? (icon = Gui.Tools.Icons.ChartPnF); } }

		public override void OnRender(ChartControl chartControl, ChartScale chartScale, ChartBars chartBars)
		{
			double		boxHeightActual = Math.Floor(10000000.0 * chartBars.Bars.BarsPeriod.Value * chartBars.Bars.Instrument.MasterInstrument.TickSize) / 10000000.0;
			int			boxSize			= (int) Math.Round(chartScale.Height.ConvertToVerticalPixels(chartControl.PresentationSource) / Math.Round(chartScale.MaxMinusMin / boxHeightActual, 0));

			AntialiasMode oldAliasMode = RenderTarget.AntialiasMode;
			RenderTarget.AntialiasMode = AntialiasMode.PerPrimitive;
			trendDetermined = false;

			for (int idx = chartBars.FromIndex; idx <= chartBars.ToIndex; idx++)
			{
				int			barWidth = GetBarPaintWidth(BarWidthUI);
				double		closeVal = chartBars.Bars.GetClose(idx);
				double		openVal = chartBars.Bars.GetOpen(idx);

				int			boxDrawCount = openVal == closeVal ? 1 : (int) Math.Round(Math.Abs(openVal - closeVal) / boxHeightActual, 0) + 1;
				float		close = chartScale.GetYByValue(closeVal);
				float		open = chartScale.GetYByValue(openVal);

				float		nextBox = Math.Min(open, close);
				float		x = chartControl.GetXByBarIndex(chartBars, idx);

				float		diff = Math.Abs(open - close) + boxSize - (int) Math.Round((double)(boxSize * boxDrawCount));

				if (closeVal == openVal)
				{
					if (idx == 0)
					{
						RenderTarget.DrawRectangle(new RectangleF(x - barWidth / 2.0f + 1, nextBox - boxSize / 2.0f + 2, barWidth - 1, boxSize - 2), DownBrushDX, Stroke.Width );
						Vector2 point0;
						Vector2 point1;

						point0.X = x - barWidth / 2.0f;
						point0.Y = nextBox - boxSize / 2.0f;
						point1.X = x + barWidth / 2.0f;
						point1.Y = nextBox + boxSize - boxSize / 2.0f;
						if (!(UpBrushDX is SharpDX.Direct2D1.SolidColorBrush))
							TransformBrush(UpBrushDX, new RectangleF(point0.X, point0.Y - Stroke.Width, barWidth, Stroke.Width));
						RenderTarget.DrawLine(point0, point1, UpBrushDX, Stroke.Width, Stroke.StrokeStyle);

						point0.X = x - barWidth / 2.0f;
						point0.Y = nextBox + boxSize - boxSize / 2.0f;
						point1.X = x + barWidth / 2.0f;
						point1.Y = nextBox - boxSize / 2.0f;
						if (!(UpBrushDX is SharpDX.Direct2D1.SolidColorBrush))
							TransformBrush(UpBrushDX, new RectangleF(point0.X, point0.Y - Stroke.Width, barWidth, Stroke.Width));
						RenderTarget.DrawLine(point0, point1, UpBrushDX, Stroke.Width, Stroke.StrokeStyle);
						
						continue;
					}
					
					if (!trendDetermined)
					{
						if (chartBars.Bars.GetOpen(idx - 1) == chartBars.Bars.GetClose(idx - 1))
						{
							if (chartBars.Bars.GetHigh(idx) < chartBars.Bars.GetHigh(idx - 1))
								isUp = false;
						}
						else
							isUp = !(chartBars.Bars.GetOpen(idx - 1) < chartBars.Bars.GetClose(idx - 1));

						trendDetermined = true;
					}
					else
						isUp = !isUp;
				}
				else
					isUp = closeVal > openVal;
				
				for (int k = 0; k < boxDrawCount; k++)
				{
					if (diff != 0)
					{
						nextBox += diff > 0 ? 1 : -1;
						diff	+= diff > 0 ? -1 : 1;
					}

					if (!isUp)
					{
						Ellipse ellipse;
						ellipse.Point = new Vector2(x, nextBox);
						ellipse.RadiusX = barWidth / 2.0f;
						ellipse.RadiusY = boxSize / 2.0f - 1;
						if (!(DownBrushDX is SharpDX.Direct2D1.SolidColorBrush))
							TransformBrush(DownBrushDX, new RectangleF(ellipse.Point.X - ellipse.RadiusX, ellipse.Point.Y - ellipse.RadiusY - Stroke.Width, barWidth, Stroke.Width));
						RenderTarget.DrawEllipse(ellipse, DownBrushDX, Stroke.Width);
					}
					else
					{
						Vector2 point0;
						Vector2 point1;

						point0.X = x - barWidth / 2.0f;
						point0.Y = nextBox - boxSize / 2.0f;
						point1.X = x + barWidth / 2.0f;
						point1.Y = nextBox + boxSize - boxSize / 2.0f;
						if (!(UpBrushDX is SharpDX.Direct2D1.SolidColorBrush))
							TransformBrush(UpBrushDX, new RectangleF(point0.X, point0.Y - Stroke.Width, barWidth, Stroke.Width));
						RenderTarget.DrawLine(point0, point1, UpBrushDX, Stroke.Width, Stroke.StrokeStyle);

						point0.X = x - barWidth / 2.0f;
						point0.Y = nextBox + boxSize - boxSize / 2.0f;
						point1.X = x + barWidth / 2.0f;
						point1.Y = nextBox - boxSize / 2.0f;
						if (!(UpBrushDX is SharpDX.Direct2D1.SolidColorBrush))
							TransformBrush(UpBrushDX, new RectangleF(point0.X, point0.Y - Stroke.Width, barWidth, Stroke.Width));
						RenderTarget.DrawLine(point0, point1, UpBrushDX, Stroke.Width, Stroke.StrokeStyle);
					}

					nextBox += boxSize;
				}
			}
			RenderTarget.AntialiasMode = oldAliasMode;
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name			= Custom.Resource.NinjaScriptChartStylePointAndFigure;
				ChartStyleType	= ChartStyleType.PointAndFigure;
			}
			else if (State == State.Configure)
			{
				Properties.Remove(Properties.Find("Stroke", true));
				Properties.Remove(Properties.Find("Stroke2", true));

				SetPropertyName("BarWidth",		Custom.Resource.NinjaScriptChartStyleBarWidth);
				SetPropertyName("DownBrush",	Custom.Resource.NinjaScriptChartStylePointAndFigureDownColor);
				SetPropertyName("UpBrush",		Custom.Resource.NinjaScriptChartStylePointAndFigureUpColor);
			}
		}
	}
}
