#region Assembly ATAS.Indicators.Technical, Version=6.0.2.311, Culture=neutral, PublicKeyToken=330427d8594115c7
// C:\Program Files (x86)\ATAS Platform\ATAS.Indicators.Technical.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Resources;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Media;
using ATAS.Indicators.Technical.Properties;
using OFT.Attributes;
using OFT.Rendering;
using OFT.Rendering.Tools;
using Utils.Common.Localization;
using ATAS.Indicators;
using ATAS.Indicators.Drawing;
using System.Drawing;
using Color = System.Windows.Media.Color;
using OFT.Rendering.Context;
using OFT.Rendering.Settings;
using System.Runtime.CompilerServices;

///////////////
///Ideas 
///Plot a bullish or bearish triangle on the chart once all selected conditions are met. 
///remove vertical and horizontal offset and just use Height and width to position the diplay
///////////////

///////////////
///Issues
///if all alert conditions are off the Counter only updates once when selecting a condition and will not update again until next bar 
///Confirmation counter does not update when conditions are selected/deselected
///////////////


///////////////
///Completed Updates
///Bullish conditions come before bearish conditions in alerts window
///Can select which signals to look for.
///Added ability to select between only Bullish or Bearish signals.
///Fixed order of Select Alerts to go from most commonly used to less commonly used
///Alerts only show bullish confirmations when bullish signals are selected n vice versa
///optimized alert conitions using UpdateCounters()
///Fix issue where "show active signals" activates both "show active signals" and "Show confirmation counter"
///fixed issue where Confirmation Counters displayed position changes upon changing between "Show Active Conditions" & "Show Confirmation Counter"
///Reordered settings menu
///"No Siganls Selected" shows if bullish & bearish signals are not selected in settings.
///Conditions are now hidden when deselected
///if total bullish or bearish conditions equals 0 then the respective counter displays "No (Bullish/Bearish) Signals".
//////////////



namespace ATAS.Indicators.Technical
{


    [DisplayName("Ichimoku w/ Alerts")]

    public class Ichimoku_Alerts : Indicator
    {
        #region Variables & Inputs

        // is conditions triggered?
        private bool tk_TriggeredBullish = false;
        private bool tk_TriggeredBearish = false;
        private bool lagging_Bullish_Triggered = false;
        private bool lagging_Bearish_Triggered = false;
        private bool bullishCloud_Triggered = false;
        private bool bearishCloud_Triggered = false;
        private bool above_Kumo_Triggered = false;
        private bool below_Kumo_Triggered = false;

        // select desired signals input
        private bool isTk_Cross = true;
        private bool isKumo_Flip = true;
        private bool isLagging_Span = true;
        private bool isKumoClose = true;
        private bool isBullishConf = true;
        private bool isBearishConf = true;


        [LocalizedCategory(typeof(Resources), "Select Alerts")]
        [DisplayName("Enable Alerts")]
        public bool enable_Alerts
        {
            get
            {
                return _enable_Alerts;
            }
            set
            {
                _enable_Alerts = value;
                UpdateCounters();
                RecalculateValues();

            }
        }

        [LocalizedCategory(typeof(Resources), "Select Alerts")]
        [DisplayName("Bullish Alerts")]
        public bool i_isBullishConf
        {
            get
            {
                return isBullishConf;
            }
            set
            {
                isBullishConf = value;
                UpdateCounters();
                RecalculateValues();

            }
        }

        [LocalizedCategory(typeof(Resources), "Select Alerts")]
        [DisplayName("Bearish Alerts")]

        public bool i_isBearishConf
        {
            get
            {
                return isBearishConf;
            }
            set
            {
                isBearishConf = value;
                UpdateCounters();
                RecalculateValues();

            }
        }



        [LocalizedCategory(typeof(Resources), "Select Alerts")]
        [DisplayName("Tenkan-sen Cross")]

        public bool i_Tk_Cross
        {
            get
            {
                return isTk_Cross;
            }
            set
            {
                isTk_Cross = value;
                UpdateCounters();
                RecalculateValues();

            }

        }

        [LocalizedCategory(typeof(Resources), "Select Alerts")]
        [DisplayName("Close Above/Below Kumo")]

        public bool i_Kumo_Close
        {
            get
            {
                return isKumoClose;
            }
            set
            {
                isKumoClose = value;
                UpdateCounters();
                RecalculateValues();
            }
        }

        [LocalizedCategory(typeof(Resources), "Select Alerts")]
        [DisplayName("Chikou Cross (Lagging)")]

        public bool i_Lagging_Span
        {
            get
            {
                return isLagging_Span;
            }
            set
            {
                isLagging_Span = value;
                UpdateCounters();
                RecalculateValues();

            }
        }
        [LocalizedCategory(typeof(Resources), "Select Alerts")]
        [DisplayName("Kumo Twist")]

        public bool i_Kumo_Flip
        {
            get
            {
                return isKumo_Flip;
            }
            set
            {
                isKumo_Flip = value;
                UpdateCounters();
                RecalculateValues();
            }
        }
        // ichimoku components

        private readonly Highest _baseHigh = new Highest
        {
            Period = 26
        };

        private readonly ValueDataSeries _baseLine = new ValueDataSeries("Base")
        {
            Color = Colors.Red
        };

        private readonly Lowest _baseLow = new Lowest
        {
            Period = 26
        };

        private readonly Highest _conversionHigh = new Highest
        {
            Period = 9
        };

        private readonly ValueDataSeries _conversionLine = new ValueDataSeries("Conversion")
        {
            Color = Colors.Blue
        };

        private readonly Lowest _conversionLow = new Lowest
        {
            Period = 9
        };

        private readonly RangeDataSeries _downSeries = new RangeDataSeries("Down")
        {
            RangeColor = Color.FromArgb(60, 255, 0, 0)
        };

        private readonly ValueDataSeries _laggingSpan = new ValueDataSeries("Lagging Span")
        {
            Color = Colors.Green
        };

        private readonly ValueDataSeries _leadLine1 = new ValueDataSeries("Lead1")
        {
            Color = Colors.Green
        };

        private readonly ValueDataSeries _leadLine2 = new ValueDataSeries("Lead2")
        {
            Color = Colors.Red
        };

        private readonly Highest _spanHigh = new Highest
        {
            Period = 52
        };

        private readonly Lowest _spanLow = new Lowest
        {
            Period = 52
        };

        private readonly RangeDataSeries _upSeries = new RangeDataSeries("Up")
        {
            RangeColor = Color.FromArgb(60, 0, 128, 0)
        };

        private int _days;
        private int _displacement = 26;
        private int _targetBar;
        private bool _enable_Alerts = true;

        [Display(ResourceType = typeof(Resources), GroupName = "Calculation", Name = "DaysLookBack", Order = int.MaxValue, Description = "DaysLookBackDescription")]
        [Range(0, 10000)]
        public int Days
        {
            get
            {
                return _days;
            }
            set
            {
                _days = value;
                RecalculateValues();
            }
        }

        [LocalizedCategory(typeof(Resources), "Settings")]
        [DisplayName("Tenkan-sen")]
        [Range(1, 10000)]
        public int Tenkan
        {
            get
            {
                return _conversionHigh.Period;
            }
            set
            {
                int num3 = (_conversionHigh.Period = (_conversionLow.Period = value));
                RecalculateValues();
            }
        }

        [LocalizedCategory(typeof(Resources), "Settings")]
        [DisplayName("Kijun-sen")]
        [Range(1, 10000)]
        public int Kijun
        {
            get
            {
                return _baseHigh.Period;
            }
            set
            {
                int num3 = (_baseHigh.Period = (_baseLow.Period = value));
                RecalculateValues();
            }
        }

        [LocalizedCategory(typeof(Resources), "Settings")]
        [DisplayName("Senkou Span B")]
        [Range(1, 10000)]
        public int Senkou
        {
            get
            {
                return _spanHigh.Period;
            }
            set
            {
                int num3 = (_spanHigh.Period = (_spanLow.Period = value));
                RecalculateValues();
            }
        }

        [LocalizedCategory(typeof(Resources), "Settings")]
        [DisplayName("Displacement")]
        [Range(1, 10000)]
        public int Displacement
        {
            get
            {
                return _displacement;
            }
            set
            {
                _displacement = value;
                RecalculateValues();
            }
        }

        #endregion

        #region Initialize Class
        public Ichimoku_Alerts()
            : base(useCandles: true)
        {
            //IL_0028: Unknown result type (might be due to invalid IL or missing references)
            //IL_0074: Unknown result type (might be due to invalid IL or missing references)
            //IL_00ab: Unknown result type (might be due to invalid IL or missing references)
            //IL_00cf: Unknown result type (might be due to invalid IL or missing references)
            //IL_00ea: Unknown result type (might be due to invalid IL or missing references)
            //IL_0105: Unknown result type (might be due to invalid IL or missing references)
            //IL_014f: Unknown result type (might be due to invalid IL or missing references)
            AdditionalFont.PropertyChanged += (a, b) => RedrawChart();


            DenyToChangePanel = true;
            EnableCustomDrawing = true;
            SubscribeToDrawingEvents(DrawingLayouts.Historical);
            DrawAbovePrice = false;

            base.DataSeries[0] = _conversionLine;
            base.DataSeries.Add(_laggingSpan);
            base.DataSeries.Add(_baseLine);
            base.DataSeries.Add(_leadLine1);
            base.DataSeries.Add(_leadLine2);
            base.DataSeries.Add(_upSeries);
            base.DataSeries.Add(_downSeries);
        }
        #endregion

        #region Confirmation Counter


        private int lastBarAlert = 0;
        private int closeAboveKumoCount = 0;
        private int tkBullishCount = 0;
        private int laggingBullishCount = 0;
        private int bullishCloudCount = 0;
        private int totalBullishConf = 0;
        private int closeBelowKumoCount = 0;
        private int tkBearishCount = 0;
        private int laggingBearishCount = 0;
        private int bearishCloudCount = 0;
        private int totalBearishConf = 0;

        //// bullish conf counter

        void TkBullishCount()
        {
            if (tk_TriggeredBullish)
            {
                tkBullishCount = 1;
            }
            else { tkBullishCount = 0; }

        }
        void LaggingBullishCount()
        {
            if (lagging_Bullish_Triggered)
            {
                laggingBullishCount = 1;
            }
            else { laggingBullishCount = 0; }
        }
        void BullishCloudCount()
        {
            if (bullishCloud_Triggered)
            {
                bullishCloudCount = 1;
            }
            else { bullishCloudCount = 0; }
        }
        void AboveCloudCount()
        {
            if (above_Kumo_Triggered)
            {
                closeAboveKumoCount = 1;
            }
            else
            {
                closeAboveKumoCount = 0;
            }
        }
        void CalcBullishConf()
        {
            TkBullishCount();
            LaggingBullishCount();
            BullishCloudCount();
            AboveCloudCount();
            totalBullishConf = bullishCloudCount + tkBullishCount + closeAboveKumoCount + laggingBullishCount;
        }

        ///bearish conf counter


        void TkBearishCount()
        {
            if (tk_TriggeredBearish)
                tkBearishCount = 1;
            else
                tkBearishCount = 0;
        }

        void LaggingBearishCount()
        {
            if (lagging_Bearish_Triggered)
                laggingBearishCount = 1;
            else
                laggingBearishCount = 0;
        }

        void BearishCloudCount()
        {
            if (bearishCloud_Triggered)
                bearishCloudCount = 1;
            else
                bearishCloudCount = 0;
        }

        void BelowCloudCount()
        {
            if (below_Kumo_Triggered)
                closeBelowKumoCount = 1;
            else
                closeBelowKumoCount = 0;
        }

        void CalcBearishConf()
        {
            TkBearishCount();
            LaggingBearishCount();
            BearishCloudCount();
            BelowCloudCount();
            totalBearishConf = closeBelowKumoCount + tkBearishCount + laggingBearishCount + bearishCloudCount;

        }
        void UpdateCounters()
        {
            CalcBullishConf();
            CalcBearishConf();
        }

        void ThrowConfirmations()
        {
            if (isBullishConf && isBearishConf)
            {
                AddAlert("Alert1", ChartInfo.TimeFrame + " " + InstrumentInfo.Instrument, totalBullishConf + " bullish condition(s) currently present\n" + totalBearishConf + " bearish condition(s) currently present", Colors.Black, Colors.Green);
            }
            else if (isBullishConf && !isBearishConf)
            {
                AddAlert("Alert1", ChartInfo.TimeFrame + " " + InstrumentInfo.Instrument, totalBullishConf + " bullish condition(s) currently present", Colors.Black, Colors.Green);
            }
            else if (!isBullishConf && isBearishConf)
            {
                AddAlert("Alert1", ChartInfo.TimeFrame + " " + InstrumentInfo.Instrument, totalBearishConf + " bearish condition(s) currently present", Colors.Black, Colors.Green);
            }

        }

        #endregion

        #region Ichimoku components

        // ATAS ichimoku Code
        protected override void OnCalculate(int bar, decimal value)
        {
            IndicatorCandle candle = GetCandle(bar);
            if (bar == 0)
            {
                base.DataSeries.ForEach(delegate (IDataSeries x)
                {
                    x.Clear();
                });
                _targetBar = 0;
                if (_days > 0)
                {
                    int num = 0;
                    for (int num2 = base.CurrentBar - 1; num2 >= 0; num2--)
                    {
                        _targetBar = num2;
                        if (IsNewSession(num2))
                        {
                            num++;
                            if (num == _days)
                            {
                                break;
                            }
                        }
                    }

                    if (_targetBar > 0)
                    {
                        _conversionLine.SetPointOfEndLine(_targetBar - 1);
                        _laggingSpan.SetPointOfEndLine(_targetBar - _displacement);
                        _baseLine.SetPointOfEndLine(_targetBar - 1);
                        _leadLine1.SetPointOfEndLine(_targetBar + _displacement - 2);
                        _leadLine2.SetPointOfEndLine(_targetBar + _displacement - 2);
                    }
                }
            }

            _conversionHigh.Calculate(bar, candle.High);
            _conversionLow.Calculate(bar, candle.Low);
            _baseHigh.Calculate(bar, candle.High);
            _baseLow.Calculate(bar, candle.Low);
            _spanHigh.Calculate(bar, candle.High);
            _spanLow.Calculate(bar, candle.Low);
            if (bar < _targetBar)
            {
                return;
            }

            _baseLine[bar] = (_baseHigh[bar] + _baseLow[bar]) / 2m;
            _conversionLine[bar] = (_conversionHigh[bar] + _conversionLow[bar]) / 2m;
            int index = bar + Displacement;
            _leadLine1[index] = (_conversionLine[bar] + _baseLine[bar]) / 2m;
            _leadLine2[index] = (_spanHigh[bar] + _spanLow[bar]) / 2m;
            if (bar - _displacement + 1 >= 0)
            {
                int num3 = bar - _displacement;
                _laggingSpan[num3] = candle.Close;
                if (bar == base.CurrentBar - 1)
                {
                    for (int i = num3 + 1; i < base.CurrentBar; i++)
                    {
                        _laggingSpan[i] = candle.Close;
                    }
                }
            }

            if (_leadLine1[bar] == 0m || _leadLine2[bar] == 0m)
            {
                return;
            }

            if (_leadLine1[bar] > _leadLine2[bar])
            {
                _upSeries[bar].Upper = _leadLine1[bar];
                _upSeries[bar].Lower = _leadLine2[bar];
                if (_leadLine1[bar - 1] < _leadLine2[bar - 1])
                {
                    _downSeries[bar] = _upSeries[bar];
                }
            }
            else
            {
                _downSeries[bar].Upper = _leadLine2[bar];
                _downSeries[bar].Lower = _leadLine1[bar];
                if (_leadLine1[bar - 1] > _leadLine2[bar - 1])
                {
                    _upSeries[bar] = _downSeries[bar];
                }
            }
            #endregion

            #region Alert Logic

            //Alert Conditions
            bool tkInvalid = _conversionLine[bar] == _baseLine[bar];
            bool tk_Bullish = _conversionLine[bar] > _baseLine[bar] && !tkInvalid && !tk_TriggeredBullish;
            bool tk_Bearish = _conversionLine[bar] < _baseLine[bar] && !tkInvalid && !tk_TriggeredBearish;

            bool laggingSpanInvalid = _laggingSpan[bar] > _leadLine1[bar - Displacement] && _laggingSpan[bar] < _leadLine2[bar - Displacement] || _laggingSpan[bar] < _leadLine1[bar - Displacement] && _laggingSpan[bar] > _leadLine2[bar - Displacement];
            bool lagging_Bullish = _laggingSpan[bar] > _leadLine1[bar - Displacement] && _laggingSpan[bar] > _leadLine2[bar - Displacement] && !laggingSpanInvalid && !lagging_Bullish_Triggered;
            bool lagging_Bearish = _laggingSpan[bar] < _leadLine1[bar - Displacement] && _laggingSpan[bar] < _leadLine2[bar - Displacement] && !laggingSpanInvalid && !lagging_Bearish_Triggered;

            bool cloudInvalid = _leadLine1[bar] == _leadLine2[bar];
            bool bullishCloud = _leadLine1[bar] > _leadLine2[bar] && !cloudInvalid && !bullishCloud_Triggered;
            bool bearishCloud = _leadLine1[bar] < _leadLine2[bar] && !cloudInvalid && !bearishCloud_Triggered;

            bool kumoCloseInvalid = _laggingSpan[bar] > _leadLine1[bar] && _laggingSpan[bar] < _leadLine2[bar] || _laggingSpan[bar] < _leadLine1[bar] && _laggingSpan[bar] > _leadLine2[bar];
            bool close_Above_Kumo = _laggingSpan[bar] > _leadLine1[bar] && _laggingSpan[bar] > _leadLine2[bar] && !kumoCloseInvalid && !above_Kumo_Triggered;
            bool close_Below_Kumo = _laggingSpan[bar] < _leadLine1[bar] && _laggingSpan[bar] < _leadLine2[bar] && !kumoCloseInvalid && !below_Kumo_Triggered;

            //// Alerts logic
            if (enable_Alerts && CurrentBar - 1 == bar && lastBarAlert != bar)
            {

                if (tkInvalid && isTk_Cross)
                {
                    if (tk_TriggeredBullish)
                    {
                        AddAlert("Alert1", ChartInfo.TimeFrame + " " + InstrumentInfo.Instrument, "Bullish Tenkan Invalidated", Colors.Black, Colors.Green);
                        ThrowConfirmations();
                    }
                    if (tk_TriggeredBearish)
                    {
                        AddAlert("Alert1", ChartInfo.TimeFrame + " " + InstrumentInfo.Instrument, "Bearish Tenkan Invalidated", Colors.Black, Colors.Green);
                        ThrowConfirmations();
                    }
                    lastBarAlert = bar;
                    tk_Bullish = false;
                    tk_Bearish = false;
                    tk_TriggeredBullish = false;
                    tk_TriggeredBearish = false;
                    UpdateCounters();


                }
                else if (tk_Bullish && isTk_Cross && isBullishConf)
                {
                    lastBarAlert = bar;
                    tk_TriggeredBullish = true;
                    tk_TriggeredBearish = false;
                    AddAlert("Alert1", ChartInfo.TimeFrame + " " + InstrumentInfo.Instrument, "Bullish Crossover", Colors.Black, Colors.Green);
                    UpdateCounters();
                    ThrowConfirmations();


                }
                else if (tk_Bearish && isTk_Cross && isBearishConf)
                {
                    lastBarAlert = bar;
                    tk_TriggeredBullish = false;
                    tk_TriggeredBearish = true;
                    AddAlert("Alert1", ChartInfo.TimeFrame + " " + InstrumentInfo.Instrument, "Bearish Crossover", Colors.Black, Colors.Green);
                    UpdateCounters();
                    ThrowConfirmations();
                }

                if (laggingSpanInvalid && isLagging_Span)
                {
                    if (lagging_Bullish_Triggered)
                    {
                        AddAlert("Alert1", ChartInfo.TimeFrame + " " + InstrumentInfo.Instrument, "Bullish Lagging Span Invalidated", Colors.Black, Colors.Green);
                        ThrowConfirmations();

                    }
                    if (lagging_Bearish_Triggered)
                    {
                        AddAlert("Alert1", ChartInfo.TimeFrame + " " + InstrumentInfo.Instrument, "Bearish Lagging Span Invalidated", Colors.Black, Colors.Green);
                        ThrowConfirmations();

                    }
                    lastBarAlert = bar;
                    lagging_Bullish = false;
                    lagging_Bearish = false;
                    lagging_Bullish_Triggered = false;
                    lagging_Bearish_Triggered = false;
                    UpdateCounters();
                }
                else if (lagging_Bullish && isLagging_Span && isBullishConf)
                {
                    lastBarAlert = bar;
                    lagging_Bullish_Triggered = true;
                    lagging_Bearish_Triggered = false;
                    AddAlert("Alert1", ChartInfo.TimeFrame + " " + InstrumentInfo.Instrument, "Lagging Span is Bullish", Colors.Black, Colors.Green);
                    UpdateCounters();
                    ThrowConfirmations();
                }
                else if (lagging_Bearish && isLagging_Span && isBearishConf)
                {
                    lastBarAlert = bar;
                    lagging_Bullish_Triggered = false;
                    lagging_Bearish_Triggered = true;
                    AddAlert("Alert1", ChartInfo.TimeFrame + " " + InstrumentInfo.Instrument, "Lagging Span is Bearish", Colors.Black, Colors.Green);
                    UpdateCounters();
                    ThrowConfirmations();
                }

                if (cloudInvalid && isKumo_Flip)
                {
                    if (bullishCloud_Triggered)
                    {
                        AddAlert("Alert1", ChartInfo.TimeFrame + " " + InstrumentInfo.Instrument, "Bullish Cloud Invalidated", Colors.Black, Colors.Green);
                        ThrowConfirmations();
                    }
                    if (bearishCloud_Triggered)
                    {
                        AddAlert("Alert1", ChartInfo.TimeFrame + " " + InstrumentInfo.Instrument, "Bearish Cloud Invalidated", Colors.Black, Colors.Green);
                        ThrowConfirmations();
                    }

                    lastBarAlert = bar;
                    bullishCloud = false;
                    bearishCloud = false;
                    bullishCloud_Triggered = false;
                    bearishCloud_Triggered = false;


                }
                else if (bullishCloud && isKumo_Flip && isBullishConf)
                {
                    lastBarAlert = bar;
                    bullishCloud_Triggered = true;
                    bearishCloud_Triggered = false;
                    AddAlert("Alert1", ChartInfo.TimeFrame + " " + InstrumentInfo.Instrument, "Bullish Cloud", Colors.Black, Colors.Green);
                    UpdateCounters();
                    ThrowConfirmations();
                }
                else if (bearishCloud && isKumo_Flip && isBearishConf)
                {
                    lastBarAlert = bar;
                    bullishCloud_Triggered = false;
                    bearishCloud_Triggered = true;
                    AddAlert("Alert1", ChartInfo.TimeFrame + " " + InstrumentInfo.Instrument, "Bearish Cloud", Colors.Black, Colors.Green);
                    UpdateCounters();
                    ThrowConfirmations();

                }

                if (kumoCloseInvalid && isKumoClose)
                {
                    if (above_Kumo_Triggered)
                    {
                        AddAlert("Alert1", ChartInfo.TimeFrame + " " + InstrumentInfo.Instrument, "Bullish Close Invalidated", Colors.Black, Colors.Green);
                        ThrowConfirmations();

                    }
                    if (below_Kumo_Triggered)
                    {
                        AddAlert("Alert1", ChartInfo.TimeFrame + " " + InstrumentInfo.Instrument, "Bearish Close Invalidated", Colors.Black, Colors.Green);
                        ThrowConfirmations();

                    }
                    lastBarAlert = bar;
                    close_Above_Kumo = false;
                    close_Below_Kumo = false;
                    above_Kumo_Triggered = false;
                    below_Kumo_Triggered = false;

                    UpdateCounters();


                }
                else if (close_Above_Kumo && isKumoClose && isBullishConf)
                {
                    lastBarAlert = bar;
                    above_Kumo_Triggered = true;
                    below_Kumo_Triggered = false;
                    AddAlert("Alert1", ChartInfo.TimeFrame + " " + InstrumentInfo.Instrument, "Close Above CLoud", Colors.Black, Colors.Green);
                    UpdateCounters();
                    ThrowConfirmations();
                }
                else if (close_Below_Kumo && isKumoClose && isBearishConf)
                {
                    lastBarAlert = bar;
                    above_Kumo_Triggered = false;
                    below_Kumo_Triggered = true;
                    AddAlert("Alert1", ChartInfo.TimeFrame + " " + InstrumentInfo.Instrument, "Close Below CLoud", Colors.Black, Colors.Green);
                    UpdateCounters();
                    ThrowConfirmations();

                }

            }
        }
        #endregion

        #region Display Logic
        public enum Location
        {
            [Display(Name = "Center")]
            Center,

            [Display(Name = "TopLeft")]
            TopLeft,

            [Display(Name = "TopRight")]
            TopRight,

            [Display(Name = "BottomLeft")]
            BottomLeft,

            [Display(Name = "BottomRight")]
            BottomRight
        }



        #region Display Properties

        [Display(Name = "Color", GroupName = "Display Settings", Order = 10)]
        public Color TextColor { get; set; } = Color.FromArgb(255, 225, 225, 225);

        [Display(Name = "TextLocation", GroupName = "Display Settings", Order = 20)]
        public Location TextLocation { get; set; } = Location.TopRight;

        [Display(Name = "HorizontalOffset", GroupName = "Display Settings", Order = 83)]
        public int HorizontalOffset { get; set; } = -302;

        public int VerticalOffset = 11;

        [Display(Name = "Font", GroupName = "Display Settings", Order = 11)]

        public FontSetting AdditionalFont { get; set; } = new FontSetting { Size = 8 };

        [Display(Name = "VerticalOffset", GroupName = "Display Settings", Order = 84)]
        public int confTextYOffset { get; set; } = -39;

        [Display(Name = "Show Signal Counter", GroupName = "Display Settings", Order = 1)]
        public bool ShowConfText { get; set; } = true;


        [Display(Name = "Show Active Signals", GroupName = "Display Settings", Order = 2)]
        public bool ShowCondText { get; set; } = true;

        [Display(Name = "Horizontal Position", GroupName = "Display Settings", Order = 81)]
        public int width { get; set; } = 372;

        [Display(Name = "Vertical Position", GroupName = "Display Settings", Order = 82)]
        public int height { get; set; } = 161;

        #endregion

        #region OnRenderLogic
        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            string confText = "";
            string condText = "";

            var textColor = TextColor.Convert();
            Size size = new Size(width, height);

            var confTextRectangle = new Rectangle(0, 0, (int)size.Width, (int)size.Height);
            var condTextRectangle = new Rectangle(0, 0, (int)size.Width, (int)size.Height);


            if (ShowConfText)
            {
                if (isBullishConf && isBearishConf)
                {
                    if (totalBullishConf == 0 && totalBearishConf != 0)
                        confText = "No Bullish Signals" + "\nBearish Signals: " + totalBearishConf + "\n";
                    else if (totalBullishConf != 0 && totalBearishConf == 0)
                        confText = "Bullish Signals: " + totalBullishConf + "\nNo Bearish Signals" + "\n";
                    else if (totalBullishConf == 0 && totalBearishConf == 0)
                        confText = "No Signals present\n";
                    else
                        confText = "Bullish Signals: " + totalBullishConf + "\nBearish Signals: " + totalBearishConf + "\n";

                }
                else if (isBullishConf && !isBearishConf)
                {
                    if (totalBullishConf == 0)
                        confText = "No Bullish Signals" + "\n";
                    else
                        confText = "Bullish Signals: " + totalBullishConf + "\n";
                }
                else if (!isBullishConf && isBearishConf)
                {
                    if (totalBearishConf == 0)
                        confText = "No Bearish Signals" + "\n";
                    else
                        confText = "Bearish Signals: " + totalBearishConf + "\n";
                }
                else
                {
                    confText = "No Signals Selected";
                }


            }

            if (ShowCondText)
            {

                if (above_Kumo_Triggered && isKumoClose && isBullishConf)
                    condText += "Bullish Close\n";

                if (below_Kumo_Triggered && isKumoClose && isBearishConf)
                    condText += "Bearish Close\n";

                if (tk_TriggeredBullish && isTk_Cross && isBullishConf)
                    condText += "Bullish Tenkan\n";

                if (tk_TriggeredBearish && isTk_Cross && isBearishConf)
                    condText += "Bearish Tenkan\n";

                if (lagging_Bullish_Triggered && isLagging_Span && isBullishConf)
                    condText += "Bullish Lagging Span\n";

                if (lagging_Bearish_Triggered && isLagging_Span && isBearishConf)
                    condText += "Bearish Lagging Span\n";

                if (bullishCloud_Triggered && isKumo_Flip && isBullishConf)
                    condText += "Bullish Cloud\n";

                if (bearishCloud_Triggered && isKumo_Flip && isBearishConf)
                    condText += "Bearish Cloud\n";

                if (condText == "")
                    condText = "No Active Signals";


            }


            if (ShowConfText && !ShowCondText)
            {
                int secondLineX;
                var y = 0;

                var totalHeight = confTextRectangle.Height + condTextRectangle.Height + confTextYOffset;
                switch (TextLocation)
                {
                    case Location.Center:
                        {
                            secondLineX = ChartInfo.PriceChartContainer.Region.Width / 2 - condTextRectangle.Width / 2 + HorizontalOffset;
                            y = ChartInfo.PriceChartContainer.Region.Height / 2 - totalHeight / 2 + VerticalOffset;

                            break;
                        }
                    case Location.TopLeft:
                        {
                            secondLineX = HorizontalOffset;
                            break;
                        }
                    case Location.TopRight:
                        {
                            secondLineX = ChartInfo.PriceChartContainer.Region.Width - condTextRectangle.Width + HorizontalOffset;

                            break;
                        }
                    case Location.BottomLeft:
                        {
                            secondLineX = HorizontalOffset;
                            y = ChartInfo.PriceChartContainer.Region.Height - totalHeight + VerticalOffset;

                            break;
                        }
                    case Location.BottomRight:
                        {
                            secondLineX = ChartInfo.PriceChartContainer.Region.Width - condTextRectangle.Width + HorizontalOffset;
                            y = ChartInfo.PriceChartContainer.Region.Height - totalHeight + VerticalOffset;

                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                context.DrawString(confText, AdditionalFont.RenderObject, textColor, secondLineX, y + confTextRectangle.Height + confTextYOffset);
            }
            else if (!ShowConfText && ShowCondText)
            {
                int secondLineX;
                var y = 0;

                var totalHeight = confTextRectangle.Height + condTextRectangle.Height + confTextYOffset;
                switch (TextLocation)
                {
                    case Location.Center:
                        {
                            secondLineX = ChartInfo.PriceChartContainer.Region.Width / 2 - condTextRectangle.Width / 2 + HorizontalOffset;
                            y = ChartInfo.PriceChartContainer.Region.Height / 2 - totalHeight / 2 + VerticalOffset;

                            break;
                        }
                    case Location.TopLeft:
                        {
                            secondLineX = HorizontalOffset;
                            break;
                        }
                    case Location.TopRight:
                        {
                            secondLineX = ChartInfo.PriceChartContainer.Region.Width - condTextRectangle.Width + HorizontalOffset;

                            break;
                        }
                    case Location.BottomLeft:
                        {
                            secondLineX = HorizontalOffset;
                            y = ChartInfo.PriceChartContainer.Region.Height - totalHeight + VerticalOffset;

                            break;
                        }
                    case Location.BottomRight:
                        {
                            secondLineX = ChartInfo.PriceChartContainer.Region.Width - condTextRectangle.Width + HorizontalOffset;
                            y = ChartInfo.PriceChartContainer.Region.Height - totalHeight + VerticalOffset;

                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                context.DrawString(condText, AdditionalFont.RenderObject, textColor, secondLineX, y + confTextRectangle.Height + confTextYOffset);
            }
            else if (ShowConfText && ShowCondText)
            {
                int secondLineX;
                var y = 0;

                var totalHeight = confTextRectangle.Height + condTextRectangle.Height + confTextYOffset;
                switch (TextLocation)
                {
                    case Location.Center:
                        {
                            secondLineX = ChartInfo.PriceChartContainer.Region.Width / 2 - condTextRectangle.Width / 2 + HorizontalOffset;
                            y = ChartInfo.PriceChartContainer.Region.Height / 2 - totalHeight / 2 + VerticalOffset;

                            break;
                        }
                    case Location.TopLeft:
                        {
                            secondLineX = HorizontalOffset;
                            break;
                        }
                    case Location.TopRight:
                        {
                            secondLineX = ChartInfo.PriceChartContainer.Region.Width - condTextRectangle.Width + HorizontalOffset;

                            break;
                        }
                    case Location.BottomLeft:
                        {
                            secondLineX = HorizontalOffset;
                            y = ChartInfo.PriceChartContainer.Region.Height - totalHeight + VerticalOffset;

                            break;
                        }
                    case Location.BottomRight:
                        {
                            secondLineX = ChartInfo.PriceChartContainer.Region.Width - condTextRectangle.Width + HorizontalOffset;
                            y = ChartInfo.PriceChartContainer.Region.Height - totalHeight + VerticalOffset;

                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                context.DrawString(confText + "\n" + condText, AdditionalFont.RenderObject, textColor, secondLineX, y + confTextRectangle.Height + confTextYOffset);
            }




        }


        private void DrawString(RenderContext context, string text, RenderFont AdditionalFont, System.Drawing.Color color, Rectangle rectangle)
        {
            switch (TextLocation)
            {
                case Location.Center:
                    {
                        context.DrawString(text, AdditionalFont, color, ChartInfo.PriceChartContainer.Region.Width / 2 - rectangle.Width / 2 + HorizontalOffset,
                                           ChartInfo.PriceChartContainer.Region.Height / 2 - rectangle.Height / 2 + VerticalOffset);
                        break;
                    }
                case Location.TopLeft:
                    {
                        context.DrawString(text, AdditionalFont, color, HorizontalOffset, VerticalOffset);
                        break;
                    }
                case Location.TopRight:
                    {
                        context.DrawString(text, AdditionalFont, color, ChartInfo.PriceChartContainer.Region.Width - rectangle.Width + HorizontalOffset, VerticalOffset);
                        break;
                    }
                case Location.BottomLeft:
                    {
                        context.DrawString(text, AdditionalFont, color, HorizontalOffset, ChartInfo.PriceChartContainer.Region.Height - rectangle.Height + VerticalOffset);
                        break;
                    }
                case Location.BottomRight:
                    {
                        context.DrawString(text, AdditionalFont, color, ChartInfo.PriceChartContainer.Region.Width - rectangle.Width + HorizontalOffset,
                                           ChartInfo.PriceChartContainer.Region.Height - rectangle.Height + VerticalOffset);
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        #endregion

        #endregion

    }
}
