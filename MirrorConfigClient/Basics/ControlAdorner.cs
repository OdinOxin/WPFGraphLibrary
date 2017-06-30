using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MirrorConfigClient.Basics
{
    #region [eAdornerPosition]
    public enum eAdornerPosition
    {
        /*
         *             TOP_LEFT      TOP_CENTER     TOP_RIGHT
         * LEFT_TOP    ###################################### RIGHT_TOP
         * LEFT_CENTER #               CENTER               # RIGHT_CENTER
         * LEFT_BOTTOM ###################################### RIGHT_BOTTOM
         *             BOTTOM_LEFT BOTTOM_CENTER BOTTOM_RIGHT
         */
        TOP_LEFT,
        TOP_CENTER,
        TOP_RIGHT,
        LEFT_TOP,
        LEFT_CENTER,
        LEFT_BOTTOM,
        RIGHT_TOP,
        RIGHT_CENTER,
        RIGHT_BOTTOM,
        BOTTOM_LEFT,
        BOTTOM_CENTER,
        BOTTOM_RIGHT,
        CENTER,
    }
    #endregion

    public class ControlAdorner : Adorner
    {
        #region [Needs]
        private VisualCollection _Visuals;
        private ContentPresenter _ContentPresenter;
        private Button btnClose;
        private Border Border;
        private Polygon Polygon;
        private bool Arrageable = true;
        private bool IsAdorned = false;
        private static Dictionary<DependencyObject, ControlAdorner> Mapping = new Dictionary<DependencyObject, ControlAdorner>();
        #endregion

        #region ctor
        public ControlAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            if (!Mapping.ContainsKey(adornedElement))
                Mapping.Add(adornedElement, this);
            _Visuals = new VisualCollection(this);
            _ContentPresenter = new ContentPresenter();
            _ContentPresenter.Content = (AdornerDecorator)new ResourceDictionary() { Source = new Uri("/Basics/Styles.xaml", UriKind.RelativeOrAbsolute) }["popUpTemplate"];
            _Visuals.Add(_ContentPresenter);
            Loaded += delegate (object sender, RoutedEventArgs e)
            {
                Update();
            };
        }
        #endregion

        #region [Events]

        #region BtnClose_Click
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            SetIsOpen(AdornedElement, false);
        }
        #endregion

        #endregion

        #region [Dependency Properties]

        #region ShowCloseButton
        /// <summary>
        /// The backing field for the ShowCloseButton dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowCloseButtonProperty = DependencyProperty.RegisterAttached("ShowCloseButton", typeof(bool), typeof(ControlAdorner), new PropertyMetadata(true, OnShowCloseButtonPropertyChanged));

        /// <summary>
        /// Sets the content.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="value">The value.</param>
        public static void SetShowCloseButton(UIElement adornedElement, bool value)
        {
            adornedElement.SetValue(ShowCloseButtonProperty, value);
        }

        /// <summary>
        /// Gets the is open.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public static bool GetShowCloseButton(UIElement adornedElement)
        {
            return (bool)adornedElement.GetValue(ShowCloseButtonProperty);
        }

        /// <summary>
        /// Called when the ShowCloseButton depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnShowCloseButtonPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Update(o);
        }
        #endregion

        #region ShowDecoration
        /// <summary>
        /// The backing field for the ShowDecoration dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowDecorationProperty = DependencyProperty.RegisterAttached("ShowDecoration", typeof(bool), typeof(ControlAdorner), new PropertyMetadata(true, OnShowDecorationPropertyChanged));

        /// <summary>
        /// Sets the content.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="value">The value.</param>
        public static void SetShowDecoration(UIElement adornedElement, bool value)
        {
            adornedElement.SetValue(ShowDecorationProperty, value);
        }

        /// <summary>
        /// Gets the is open.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public static bool GetShowDecoration(UIElement adornedElement)
        {
            return (bool)adornedElement.GetValue(ShowDecorationProperty);
        }

        /// <summary>
        /// Called when the ShowDecoration depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnShowDecorationPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Update(o);
        }
        #endregion

        #region Content
        /// <summary>
        /// The backing field for the Content dependency property.
        /// </summary>
        public static readonly DependencyProperty ContentProperty = DependencyProperty.RegisterAttached("Content", typeof(UIElement), typeof(ControlAdorner), new PropertyMetadata(null, OnContentPropertyChanged));

        /// <summary>
        /// Sets the content.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="value">The value.</param>
        public static void SetContent(UIElement adornedElement, UIElement value)
        {
            adornedElement.SetValue(ContentProperty, value);
        }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public static UIElement GetContent(UIElement adornedElement)
        {
            return (UIElement)adornedElement.GetValue(ContentProperty);
        }

        /// <summary>
        /// Called when the Content depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnContentPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Update(o);
        }
        #endregion

        #region BackgroundColor
        /// <summary>
        /// The backing field for the BackgroundColor dependency property.
        /// </summary>
        public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.RegisterAttached("BackgroundColor", typeof(Color), typeof(ControlAdorner), new PropertyMetadata(default(Color), OnBackgroundColorPropertyChanged));

        /// <summary>
        /// Sets the background color.
        /// </summary>
        /// <param name="adornedElement"></param>
        /// <param name="value"></param>
        public static void SetBackgroundColor(UIElement adornedElement, Color value)
        {
            adornedElement.SetValue(BackgroundColorProperty, value);
        }

        /// <summary>
        /// Gets the background color.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public static Color GetBackgroundColor(UIElement adornedElement)
        {
            return (Color)adornedElement.GetValue(BackgroundColorProperty);
        }

        /// <summary>
        /// Called when the BackgroundColor depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnBackgroundColorPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Update(o);
        }
        #endregion

        #region IsOpen
        /// <summary>
        /// The backing field for the IsOpen dependency property.
        /// </summary>
        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.RegisterAttached("IsOpen", typeof(bool), typeof(ControlAdorner), new PropertyMetadata(false, OnIsOpenPropertyChanged));

        /// <summary>
        /// Sets the is open.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void SetIsOpen(UIElement adornedElement, bool value)
        {
            adornedElement.SetValue(IsOpenProperty, value);
        }

        /// <summary>
        /// Gets the is open.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public static bool GetIsOpen(UIElement adornedElement)
        {
            return (bool)adornedElement.GetValue(IsOpenProperty);
        }

        /// <summary>
        /// Called when the IsOpen depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnIsOpenPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Update(o);
        }
        #endregion

        #region Position
        /// <summary>
        /// The backing field for the Position dependency property.
        /// </summary>
        public static readonly DependencyProperty PositionProperty = DependencyProperty.RegisterAttached("Position", typeof(eAdornerPosition), typeof(ControlAdorner), new PropertyMetadata(eAdornerPosition.CENTER, OnPositionPropertyChanged));

        /// <summary>
        /// Sets the position.
        /// </summary>
        /// <param name="adornedElement"></param>
        /// <param name="value"></param>
        public static void SetPosition(UIElement adornedElement, eAdornerPosition value)
        {
            adornedElement.SetValue(PositionProperty, value);
        }

        /// <summary>
        /// Gets the position.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public static eAdornerPosition GetPosition(UIElement adornedElement)
        {
            return (eAdornerPosition)adornedElement.GetValue(PositionProperty);
        }

        /// <summary>
        /// Called when the Position depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnPositionPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Update(o);
        }
        #endregion

        #endregion

        #region [Methods]
        protected override Size MeasureOverride(Size constraint)
        {
            _ContentPresenter.Measure(constraint);
            return _ContentPresenter.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_ContentPresenter != null && Arrageable)
            {
                _ContentPresenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                if (GetShowDecoration(AdornedElement))
                {
                    _ContentPresenter.Arrange(new Rect(-_ContentPresenter.DesiredSize.Width, -8, _ContentPresenter.DesiredSize.Width, _ContentPresenter.DesiredSize.Height));
                    Arrageable = _ContentPresenter.DesiredSize.Width == 44 && _ContentPresenter.DesiredSize.Height == 30;
                }
                else
                {
                    AdornedElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    switch (GetPosition(AdornedElement))
                    {
                        case eAdornerPosition.TOP_LEFT:
                            _ContentPresenter.Arrange(new Rect(0, -_ContentPresenter.DesiredSize.Height, _ContentPresenter.DesiredSize.Width, _ContentPresenter.DesiredSize.Height));
                            break;
                        case eAdornerPosition.TOP_CENTER:
                            _ContentPresenter.Arrange(new Rect(AdornedElement.DesiredSize.Width / 2 - _ContentPresenter.DesiredSize.Width / 2, -_ContentPresenter.DesiredSize.Height, _ContentPresenter.DesiredSize.Width, _ContentPresenter.DesiredSize.Height));
                            break;
                        case eAdornerPosition.TOP_RIGHT:
                            _ContentPresenter.Arrange(new Rect(AdornedElement.DesiredSize.Width - _ContentPresenter.DesiredSize.Width, -_ContentPresenter.DesiredSize.Height, _ContentPresenter.DesiredSize.Width, _ContentPresenter.DesiredSize.Height));
                            break;
                        case eAdornerPosition.LEFT_TOP:
                            _ContentPresenter.Arrange(new Rect(-_ContentPresenter.DesiredSize.Width, 0, _ContentPresenter.DesiredSize.Width, _ContentPresenter.DesiredSize.Height));
                            break;
                        case eAdornerPosition.LEFT_CENTER:
                            _ContentPresenter.Arrange(new Rect(-_ContentPresenter.DesiredSize.Width, AdornedElement.DesiredSize.Height / 2 - _ContentPresenter.DesiredSize.Height / 2, _ContentPresenter.DesiredSize.Width, _ContentPresenter.DesiredSize.Height));
                            break;
                        case eAdornerPosition.LEFT_BOTTOM:
                            _ContentPresenter.Arrange(new Rect(-_ContentPresenter.DesiredSize.Width, AdornedElement.DesiredSize.Height - _ContentPresenter.DesiredSize.Height, _ContentPresenter.DesiredSize.Width, _ContentPresenter.DesiredSize.Height));
                            break;
                        case eAdornerPosition.RIGHT_TOP:
                            _ContentPresenter.Arrange(new Rect(AdornedElement.DesiredSize.Width, 0, _ContentPresenter.DesiredSize.Width, _ContentPresenter.DesiredSize.Height));
                            break;
                        case eAdornerPosition.RIGHT_CENTER:
                            _ContentPresenter.Arrange(new Rect(AdornedElement.DesiredSize.Width, AdornedElement.DesiredSize.Height / 2 - _ContentPresenter.DesiredSize.Height / 2, _ContentPresenter.DesiredSize.Width, _ContentPresenter.DesiredSize.Height));
                            break;
                        case eAdornerPosition.RIGHT_BOTTOM:
                            _ContentPresenter.Arrange(new Rect(AdornedElement.DesiredSize.Width, AdornedElement.DesiredSize.Height - _ContentPresenter.DesiredSize.Height, _ContentPresenter.DesiredSize.Width, _ContentPresenter.DesiredSize.Height));
                            break;
                        case eAdornerPosition.BOTTOM_LEFT:
                            _ContentPresenter.Arrange(new Rect(0, AdornedElement.DesiredSize.Height, _ContentPresenter.DesiredSize.Width, _ContentPresenter.DesiredSize.Height));
                            break;
                        case eAdornerPosition.BOTTOM_CENTER:
                            _ContentPresenter.Arrange(new Rect(AdornedElement.DesiredSize.Width / 2 - _ContentPresenter.DesiredSize.Width / 2, AdornedElement.DesiredSize.Height, _ContentPresenter.DesiredSize.Width, _ContentPresenter.DesiredSize.Height));
                            break;
                        case eAdornerPosition.BOTTOM_RIGHT:
                            _ContentPresenter.Arrange(new Rect(AdornedElement.DesiredSize.Width - _ContentPresenter.DesiredSize.Width, AdornedElement.DesiredSize.Height, _ContentPresenter.DesiredSize.Width, _ContentPresenter.DesiredSize.Height));
                            break;
                        case eAdornerPosition.CENTER:
                            _ContentPresenter.Arrange(new Rect(AdornedElement.DesiredSize.Width / 2 - _ContentPresenter.DesiredSize.Width / 2, AdornedElement.DesiredSize.Height / 2 - _ContentPresenter.DesiredSize.Height / 2, _ContentPresenter.DesiredSize.Width, _ContentPresenter.DesiredSize.Height));
                            break;
                    }
                }
            }
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        { return _Visuals[index]; }

        protected override int VisualChildrenCount
        { get { return _Visuals.Count; } }

        private void Update()
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(AdornedElement);
            if (layer == null)
            {
                // if we don't have an adorner layer it's probably
                // because it's too early in the window's construction
                // Let's re-run at a slightly later time
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => Update()));
                return;
            }
            if (GetIsOpen(AdornedElement) && !IsAdorned)
            {
                layer.Add(this);
                IsAdorned = true;
            }
            else if (!GetIsOpen(AdornedElement) && IsAdorned)
            {
                layer.Remove(this);
                IsAdorned = false;
                Mapping.Remove(AdornedElement);
                return;
            }

            if (GetShowDecoration(AdornedElement))
            {
                ContentPresenter ContentPresenter = UIHelper.FindChild<ContentPresenter>(_ContentPresenter, "ContentPresenter");
                if (ContentPresenter != null)
                    ContentPresenter.Content = GetContent(AdornedElement);
                if (btnClose == null)
                    btnClose = UIHelper.FindChild<Button>(_ContentPresenter, "btnClose");
                if (btnClose != null)
                {
                    btnClose.Click += BtnClose_Click;
                    btnClose.Visibility = GetShowCloseButton(AdornedElement) ? Visibility.Visible : Visibility.Collapsed;
                }
                if (Border == null)
                    Border = UIHelper.FindChild<Border>(_ContentPresenter, "Border");
                if (Border != null)
                    Border.Background = new SolidColorBrush(GetBackgroundColor(AdornedElement));
                if (Polygon == null)
                    Polygon = UIHelper.FindChild<Polygon>(_ContentPresenter, "Polygon");
                if (Polygon != null)
                    Polygon.Fill = new SolidColorBrush(GetBackgroundColor(AdornedElement));
            }
            else
                _ContentPresenter.Content = GetContent(AdornedElement);
        }

        private static void Update(DependencyObject o)
        {
            if (!Mapping.ContainsKey(o))
                new ControlAdorner(o as UIElement);
            Mapping[o]?.Update();
        }
        #endregion
    }
}
