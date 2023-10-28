#region Using declarations
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
#endregion

namespace NinjaTrader.NinjaScript.ChartStyles
{
	public class KagiStyle : ChartStyle
	{
		private object	icon;
		private bool	thickLine;

		public override object Icon { get { return icon ?? (icon = Gui.Tools.Icons.ChartKagiLine); } }

		public override int GetBarPaintWidth(int barWidth) { return 1 + 2 * barWidth + 2 * (int) Math.Round(Stroke2.Width); }

		public override void OnRender(ChartControl chartControl, ChartScale chartScale, ChartBars chartBars)
		{
			if (chartBars.FromIndex > chartBars.ToIndex)
				return;

			if (chartBars.FromIndex > 0)
				chartBars.FromIndex--;

			Bars		bars				= chartBars.Bars;
			float		barWidth			= GetBarPaintWidth(BarWidthUI);
			int			sessionStartIndex	= chartBars.FromIndex;
			int			thickOffsetTop		= (int)	(Stroke.Width * 0.5);
			int			thinOffsetTop		= (int) (Stroke2.Width * 0.5);
			Vector2		point0				= new Vector2();
			Vector2		point1				= new Vector2();
			
			while (sessionStartIndex > 0 && bars.BarsType.IsIntraday)
			{
				if (bars.BarsSeries.GetIsFirstBarOfSession(sessionStartIndex))
					break;
				sessionStartIndex--;
			}

			if (sessionStartIndex < 0) // Occurs when all bars are off screen
				return;

			thickLine = bars.GetClose(sessionStartIndex) > bars.GetOpen(sessionStartIndex);

			//Determine the next bar coming up is thick or thin
			for (int k = sessionStartIndex + 1; k < chartBars.FromIndex; k++)
			{
				double closeTest = bars.GetClose(k);
				if (closeTest > bars.GetOpen(k))
				{
					if (Math.Max(bars.GetOpen(k - 1), bars.GetClose(k - 1)) < closeTest)
						thickLine = true;
				}
				else if (closeTest < Math.Min(bars.GetOpen(k - 1), bars.GetClose(k - 1)))
					thickLine = false;
			}

			for (int idx = chartBars.FromIndex; idx <= chartBars.ToIndex; idx++)
			{
				Brush		overriddenBrush	= chartControl.GetBarOverrideBrush(chartBars, idx);
				double		openValue		= bars.GetOpen(idx);
				float		open			= chartScale.GetYByValue(openValue);
				double		closeValue		= bars.GetClose(idx);
				float		close			= chartScale.GetYByValue(closeValue);
				float		x				= chartControl.GetXByBarIndex(chartBars, idx);
				double		prevOpenValue	= idx == 0 ? openValue : bars.GetOpen(idx - 1);
				double		prevCloseValue	= idx == 0 ? closeValue : bars.GetClose(idx - 1);

				float	startPosition;
				if (idx == 0 && chartBars.ToIndex >= 1)
				{
					float x0		= chartControl.GetXByBarIndex(chartBars, 0);
					float x1		= chartControl.GetXByBarIndex(chartBars, 1);
					float diffX		= Math.Max(1, x1 - x0);
					startPosition	= x0 - diffX;
				}
				else
					startPosition = idx == chartBars.FromIndex ? chartControl.GetXByBarIndex(chartBars, idx) : chartControl.GetXByBarIndex(chartBars, idx - 1);

				startPosition = startPosition < 0 ? 0 : startPosition;

				if (bars.BarsType.IsIntraday && bars.IsResetOnNewTradingDay && bars.BarsSeries.GetIsFirstBarOfSession(idx))
				{
					// First bar
					if (closeValue > openValue)
					{
						point0.X = x;
						point0.Y = open;
						point1.X = x;
						point1.Y = close;
						TransformBrush(overriddenBrush ?? Stroke.BrushDX, new RectangleF(point0.X, point0.Y - Stroke.Width, barWidth, Stroke.Width));
						RenderTarget.DrawLine(point0, point1, overriddenBrush ?? Stroke.BrushDX, Stroke.Width, Stroke.StrokeStyle);
						thickLine = true;
					}
					else
					{
						point0.X = x;
						point0.Y = open;
						point1.X = x;
						point1.Y = close;
						TransformBrush(overriddenBrush ?? Stroke2.BrushDX, new RectangleF(point0.X, point0.Y - Stroke2.Width, barWidth, Stroke2.Width));
						RenderTarget.DrawLine(point0, point1, overriddenBrush ?? Stroke2.BrushDX, Stroke2.Width, Stroke2.StrokeStyle);
						thickLine = false;
					}
				}
				else
				{
					if (closeValue > openValue)
					{
						if (closeValue <= Math.Max(prevCloseValue, prevOpenValue))
						{
							// Maintain previous thickness
							if (thickLine)
							{																			
								point0.X = x;															
								point0.Y = open + thickOffsetTop;										
								point1.X = x;															
								point1.Y = close - thickOffsetTop;										
								TransformBrush(overriddenBrush ?? Stroke.BrushDX, new RectangleF(point0.X, point0.Y - Stroke.Width, barWidth, Stroke.Width));
								RenderTarget.DrawLine(point0, point1, overriddenBrush ?? Stroke.BrushDX, Stroke.Width, Stroke.StrokeStyle);
								point0.X = startPosition;												
								point0.Y = open;														
								point1.X = x;															
								point1.Y = open;														
								TransformBrush(overriddenBrush ?? Stroke.BrushDX, new RectangleF(point0.X, point0.Y - Stroke.Width, barWidth, Stroke.Width));
								RenderTarget.DrawLine(point0, point1, overriddenBrush ?? Stroke.BrushDX, Stroke.Width, Stroke.StrokeStyle);
							}
							else
							{
								point0.X = x;																			
								point0.Y = open + thinOffsetTop;														
								point1.X = x;																			
								point1.Y = close - thinOffsetTop;														
								TransformBrush(overriddenBrush ?? Stroke2.BrushDX, new RectangleF(point0.X, point0.Y - Stroke2.Width, barWidth, Stroke2.Width));
								RenderTarget.DrawLine(point0, point1, overriddenBrush ?? Stroke2.BrushDX, Stroke2.Width, Stroke2.StrokeStyle);	
								point0.X = startPosition;																
								point0.Y = open;																		
								point1.X = x;																			
								point1.Y = open;																		
								TransformBrush(overriddenBrush ?? Stroke2.BrushDX, new RectangleF(point0.X, point0.Y - Stroke2.Width, barWidth, Stroke2.Width));
								RenderTarget.DrawLine(point0, point1, overriddenBrush ?? Stroke2.BrushDX, Stroke2.Width, Stroke2.StrokeStyle);	
							}
						}
						else if (closeValue > Math.Max(prevCloseValue, prevOpenValue))
						{
							double transitionPoint = Math.Max(prevCloseValue, prevOpenValue);

							point0.X = x;																
							point0.Y = close - thickOffsetTop;											
							point1.X = x;																
							point1.Y = chartScale.GetYByValue(transitionPoint);							
							TransformBrush(overriddenBrush ?? Stroke.BrushDX, new RectangleF(point0.X, point0.Y - Stroke.Width, barWidth, Stroke.Width));
							RenderTarget.DrawLine(point0, point1, overriddenBrush ?? Stroke.BrushDX, Stroke.Width, Stroke.StrokeStyle);			
							point0.X = x;																
							point0.Y = chartScale.GetYByValue(transitionPoint);							
							point1.X = x;																
							point1.Y = open + (thickLine ? thickOffsetTop : thinOffsetTop);				
							TransformBrush(overriddenBrush ?? (thickLine ? Stroke.BrushDX : Stroke2.BrushDX), new RectangleF(point0.X, point0.Y - (thickLine ? Stroke.Width : Stroke2.Width), barWidth, thickLine ? Stroke.Width : Stroke2.Width));
							RenderTarget.DrawLine(point0, point1, overriddenBrush ?? (thickLine ? Stroke.BrushDX : Stroke2.BrushDX), thickLine ? Stroke.Width : Stroke2.Width, thickLine ? Stroke.StrokeStyle : Stroke2.StrokeStyle);
							point0.X = startPosition; 
							point0.Y = open;
							point1.X = x;
							point1.Y = open;
							TransformBrush(overriddenBrush ?? (thickLine ? Stroke.BrushDX : Stroke2.BrushDX), new RectangleF(point0.X, point0.Y - (thickLine ? Stroke.Width : Stroke2.Width), barWidth, thickLine ? Stroke.Width : Stroke2.Width));
							RenderTarget.DrawLine(point0, point1, overriddenBrush ?? (thickLine ? Stroke.BrushDX : Stroke2.BrushDX), thickLine ? Stroke.Width : Stroke2.Width, thickLine ? Stroke.StrokeStyle : Stroke2.StrokeStyle);

							thickLine = true;
						}
					}
					else
					{
						if (Math.Min(prevCloseValue, prevOpenValue) <= closeValue)
						{
							// Maintain previous thickness
							if (thickLine)
							{
								point0.X = x;														
								point0.Y = open - thickOffsetTop;									
								point1.X = x;														
								point1.Y = close + thickOffsetTop;									
								TransformBrush(overriddenBrush ?? Stroke.BrushDX, new RectangleF(point0.X, point0.Y - Stroke.Width, barWidth, Stroke.Width));
								RenderTarget.DrawLine(point0, point1, overriddenBrush ?? Stroke.BrushDX, Stroke.Width, Stroke.StrokeStyle);		
								point0.X = startPosition;											
								point0.Y = open;													
								point1.X = x;														
								point1.Y = open;													
								TransformBrush(overriddenBrush ?? Stroke.BrushDX, new RectangleF(point0.X, point0.Y - Stroke.Width, barWidth, Stroke.Width));
								RenderTarget.DrawLine(point0, point1, overriddenBrush ?? Stroke.BrushDX, Stroke.Width, Stroke.StrokeStyle);		
							}
							else
							{
								point0.X = x;														 
								point0.Y = open - thinOffsetTop;									 
								point1.X = x;														
								point1.Y = close + thinOffsetTop;									 
								TransformBrush(overriddenBrush ?? Stroke2.BrushDX, new RectangleF(point0.X, point0.Y - Stroke2.Width, barWidth, Stroke2.Width));
								RenderTarget.DrawLine(point0, point1, overriddenBrush ?? Stroke2.BrushDX, Stroke2.Width, Stroke2.StrokeStyle);	
								point0.X = startPosition;											 
								point0.Y = open;													
								point1.X = x;														 
								point1.Y = open;													
								TransformBrush(overriddenBrush ?? Stroke2.BrushDX, new RectangleF(point0.X, point0.Y - Stroke2.Width, barWidth, Stroke2.Width));
								RenderTarget.DrawLine(point0, point1, overriddenBrush ?? Stroke2.BrushDX, Stroke2.Width, Stroke2.StrokeStyle);	 
							}
						}
						else if (closeValue < Math.Min(prevCloseValue, prevOpenValue))
						{
							double transitionPoint = Math.Min(prevCloseValue, prevOpenValue);

							point0.X = startPosition;																													
							point0.Y = open;																															
							point1.X = x;																																
							point1.Y = open;																															
							TransformBrush(overriddenBrush ?? (thickLine ? Stroke.BrushDX : Stroke2.BrushDX), new RectangleF(point0.X, point0.Y - (thickLine ? Stroke.Width : Stroke2.Width), barWidth, thickLine ? Stroke.Width : Stroke2.Width));
							RenderTarget.DrawLine(point0, point1, overriddenBrush ?? (thickLine ? Stroke.BrushDX : Stroke2.BrushDX), thickLine ? Stroke.Width : Stroke2.Width, thickLine ? Stroke.StrokeStyle : Stroke2.StrokeStyle);	
							point0.X = x;																																
							point0.Y = open - (thickLine ? thickOffsetTop : thinOffsetTop);																				
							point1.X = x;																																
							point1.Y = chartScale.GetYByValue(transitionPoint);																							
							TransformBrush(overriddenBrush ?? (thickLine ? Stroke.BrushDX : Stroke2.BrushDX), new RectangleF(point0.X, point0.Y - (thickLine ? Stroke.Width : Stroke2.Width), barWidth, thickLine ? Stroke.Width : Stroke2.Width));
							RenderTarget.DrawLine(point0, point1, overriddenBrush ?? (thickLine ? Stroke.BrushDX : Stroke2.BrushDX), thickLine ? Stroke.Width : Stroke2.Width, thickLine ? Stroke.StrokeStyle : Stroke2.StrokeStyle);	
							point0.X = x;
							point0.Y = chartScale.GetYByValue(transitionPoint);
							point1.X = x;
							point1.Y = close + thinOffsetTop;
							TransformBrush(overriddenBrush ?? Stroke2.BrushDX, new RectangleF(point0.X, point0.Y - Stroke2.Width, barWidth, Stroke2.Width));
							RenderTarget.DrawLine(point0, point1, overriddenBrush ?? Stroke2.BrushDX, Stroke2.Width, Stroke2.StrokeStyle);

							thickLine = false;
						}
					}
				}
			}
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name			= Custom.Resource.NinjaScriptChartStyleKagi;
				ChartStyleType	= ChartStyleType.KagiLine;

				Stroke = new Gui.Stroke(System.Windows.Media.Brushes.LimeGreen, 3) { IsOpacityVisible = false };
				Stroke2 = new Gui.Stroke(System.Windows.Media.Brushes.Red, 1) { IsOpacityVisible = false };
			}
			else if (State == State.Configure)
			{
				Properties.Remove(Properties.Find("BarWidthUI", true));
				Properties.Remove(Properties.Find("DownBrush", true));
				Properties.Remove(Properties.Find("UpBrush", true));

				SetPropertyName("Stroke", Custom.Resource.NinjaScriptChartStyleKagiThickLine);
				SetPropertyName("Stroke2", Custom.Resource.NinjaScriptChartStyleKagiThinLine);
			}
		}
	}
}
