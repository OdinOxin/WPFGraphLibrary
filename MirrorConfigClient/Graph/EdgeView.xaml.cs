using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using static MirrorConfigClient.Graph.GeometryUtils;

namespace MirrorConfigClient.Graph
{
    /// <summary>
    /// Interaction logic for EdgeView.xaml
    /// </summary>
    public partial class EdgeView : UserControl, Selectable, Hoverable
    {
        #region [Attributes]
        private const int ROUTING_OFFSET = 15;
        private const double DEG = 180 / Math.PI;
        private readonly DoubleCollection solid = new DoubleCollection(new double[] { 1, 0 }), dotted = new DoubleCollection(new double[] { 2, 1 });
        private bool IsInInitMode = true;
        private List<Line> path;
        private Polygon arrow;
        private int leftColumn;
        private int rightColumn;
        #endregion

        #region [ctor]
        public EdgeView(GraphView Graph, NodeView Start, NodeView End)
        {
            InitializeComponent();
            this.Graph = Graph;
            this.Start = Start;
            this.End = End;
            IsInInitMode = false;
            Draw();
        }
        #endregion

        #region [Dependency Properties]

        #region Graph
        /// <summary>
        /// The backing field for the Graph dependency property.
        /// </summary>
        public static readonly DependencyProperty GraphProperty = DependencyProperty.Register("Graph", typeof(GraphView), typeof(EdgeView), new PropertyMetadata(null, OnGraphPropertyChanged));

        /// <summary>
        /// Called when the Graph depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnGraphPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            EdgeView Sender = o as EdgeView;
            Sender.OnGraphChanged((GraphView)e.OldValue, (GraphView)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the Graph.
        /// </summary>
        /// <value>The layers.</value>
        public GraphView Graph
        {
            get
            {
                return (GraphView)GetValue(GraphProperty);
            }
            set
            {
                SetValue(GraphProperty, value);
            }
        }

        /// <summary>
        /// Called when the Graph dependency property changes.
        /// </summary>
        /// <param name="OldValue">The old value.</param>
        /// <param name="NewValue">The new value.</param>
        protected virtual void OnGraphChanged(GraphView OldValue, GraphView NewValue)
        {

        }
        #endregion

        #region Start
        /// <summary>
        /// The backing field for the Start dependency property.
        /// </summary>
        public static readonly DependencyProperty StartProperty = DependencyProperty.Register("Start", typeof(NodeView), typeof(EdgeView), new PropertyMetadata(null, OnStartPropertyChanged));

        /// <summary>
        /// Called when the Start depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnStartPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            EdgeView Sender = o as EdgeView;
            Sender.OnStartChanged((NodeView)e.OldValue, (NodeView)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the Start.
        /// </summary>
        /// <value>The layers.</value>
        public NodeView Start
        {
            get
            {
                return (NodeView)GetValue(StartProperty);
            }
            set
            {
                SetValue(StartProperty, value);
            }
        }

        /// <summary>
        /// Called when the Start dependency property changes.
        /// </summary>
        /// <param name="OldValue">The old value.</param>
        /// <param name="NewValue">The new value.</param>
        protected virtual void OnStartChanged(NodeView OldValue, NodeView NewValue)
        {
            Draw();
        }
        #endregion

        #region End
        /// <summary>
        /// The backing field for the End dependency property.
        /// </summary>
        public static readonly DependencyProperty EndProperty = DependencyProperty.Register("End", typeof(NodeView), typeof(EdgeView), new PropertyMetadata(null, OnEndPropertyChanged));

        /// <summary>
        /// Called when the End depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnEndPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            EdgeView Sender = o as EdgeView;
            Sender.OnEndChanged((NodeView)e.OldValue, (NodeView)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the End.
        /// </summary>
        /// <value>The layers.</value>
        public NodeView End
        {
            get
            {
                return (NodeView)GetValue(EndProperty);
            }
            set
            {
                SetValue(EndProperty, value);
            }
        }

        /// <summary>
        /// Called when the End dependency property changes.
        /// </summary>
        /// <param name="OldValue">The old value.</param>
        /// <param name="NewValue">The new value.</param>
        protected virtual void OnEndChanged(NodeView OldValue, NodeView NewValue)
        {
            Draw();
        }
        #endregion

        #region TmpEnd
        /// <summary>
        /// The backing field for the TmpEnd dependency property.
        /// </summary>
        public static readonly DependencyProperty TmpEndProperty = DependencyProperty.Register("TmpEnd", typeof(Point?), typeof(EdgeView), new PropertyMetadata(null, OnTmpEndPropertyChanged));

        /// <summary>
        /// Called when the TmpEnd depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnTmpEndPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            EdgeView Sender = o as EdgeView;
            Sender.OnTmpEndChanged((Point?)e.OldValue, (Point?)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the TmpEnd.
        /// </summary>
        /// <value>The layers.</value>
        public Point? TmpEnd
        {
            get
            {
                return (Point?)GetValue(TmpEndProperty);
            }
            set
            {
                SetValue(TmpEndProperty, value);
            }
        }

        /// <summary>
        /// Called when the TmpEnd dependency property changes.
        /// </summary>
        /// <param name="OldValue">The old value.</param>
        /// <param name="NewValue">The new value.</param>
        protected virtual void OnTmpEndChanged(Point? OldValue, Point? NewValue)
        {
            Draw();
        }
        #endregion

        #region IsSelected
        /// <summary>
        /// The backing field for the IsSelected dependency property.
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(EdgeView), new PropertyMetadata(false, OnIsSelectedPropertyChanged));

        /// <summary>
        /// Called when the IsSelected depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnIsSelectedPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            EdgeView Sender = o as EdgeView;
            Sender.OnIsSelectedChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the IsSelected.
        /// </summary>
        /// <value>The layers.</value>
        public bool IsSelected
        {
            get
            {
                return (bool)GetValue(IsSelectedProperty);
            }
            set
            {
                SetValue(IsSelectedProperty, value);
            }
        }

        /// <summary>
        /// Called when the IsSelected dependency property changes.
        /// </summary>
        /// <param name="OldValue">The old value.</param>
        /// <param name="NewValue">The new value.</param>
        protected virtual void OnIsSelectedChanged(bool OldValue, bool NewValue)
        {
            double thickness = (NewValue ? 2.5 : 1) * LineThickness;
            if (path != null)
                foreach (Line seq in path)
                    seq.StrokeDashArray = NewValue ? dotted : solid;
        }
        #endregion

        #region Hovering
        /// <summary>
        /// The backing field for the Hovering dependency property.
        /// </summary>
        public static readonly DependencyProperty HoveringProperty = DependencyProperty.Register("Hovering", typeof(HoverState), typeof(EdgeView), new PropertyMetadata(HoverState.NOT, OnHoveringPropertyChanged));

        /// <summary>
        /// Called when the Hovering depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnHoveringPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            EdgeView Sender = o as EdgeView;
            Sender.OnHoveringChanged((HoverState)e.OldValue, (HoverState)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the Hovering.
        /// </summary>
        /// <value>The layers.</value>
        public HoverState Hovering
        {
            get
            {
                return (HoverState)GetValue(HoveringProperty);
            }
            set
            {
                SetValue(HoveringProperty, value);
            }
        }

        /// <summary>
        /// Called when the Hovering dependency property changes.
        /// </summary>
        /// <param name="OldValue">The old value.</param>
        /// <param name="NewValue">The new value.</param>
        protected virtual void OnHoveringChanged(HoverState OldValue, HoverState NewValue)
        {
            Brush brush = GetHoverBrush(NewValue);
            if (path != null)
                foreach (Line seq in path)
                {
                    seq.Stroke = brush;
                }
            if (arrow != null)
            {
                arrow.Fill = brush;
                arrow.Stroke = brush;
            }
        }
        #endregion

        #region LineThickness
        /// <summary>
        /// The backing field for the LineThickness dependency property.
        /// </summary>
        public static readonly DependencyProperty LineThicknessProperty = DependencyProperty.Register("LineThickness", typeof(double), typeof(EdgeView), new PropertyMetadata(1d, OnLineThicknessPropertyChanged));

        /// <summary>
        /// Called when the LineThickness depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnLineThicknessPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            EdgeView Sender = o as EdgeView;
            Sender.OnLineThicknessChanged((double)e.OldValue, (double)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the LineThickness.
        /// </summary>
        /// <value>The layers.</value>
        public double LineThickness
        {
            get
            {
                return (double)GetValue(LineThicknessProperty);
            }
            set
            {
                SetValue(LineThicknessProperty, value);
            }
        }

        /// <summary>
        /// Called when the LineThickness dependency property changes.
        /// </summary>
        /// <param name="OldValue">The old value.</param>
        /// <param name="NewValue">The new value.</param>
        protected virtual void OnLineThicknessChanged(double OldValue, double NewValue)
        {
            double thickness = (IsSelected ? 1.5 : 1) * NewValue;
            if (path != null)
                foreach (Line seq in path)
                    seq.StrokeThickness = thickness;
        }
        #endregion

        #endregion

        #region [Methods]

        #region Draw
        private void Draw()
        {
            if (IsInInitMode)
                return;
            board.Children.Clear();
            if (Start == null || (End == null && TmpEnd == null) || (End != null && Start == End))
                return;
            leftColumn = Math.Min(Graph.GetColumnAssign(Start.Story), End != null ? Graph.GetColumnAssign(End.Story) : -1);
            rightColumn = Math.Max(Graph.GetColumnAssign(Start.Story), End != null ? Graph.GetColumnAssign(End.Story) : -1);
            Rect srcRect = new Rect(Canvas.GetLeft(Start), Canvas.GetTop(Start), Start.DesiredSize.Width, Start.DesiredSize.Height);
            Point srcPos = new Point(srcRect.Left + srcRect.Width / 2, srcRect.Top + srcRect.Height / 2);
            Rect destRect = End != null && Start != End ? new Rect(Canvas.GetLeft(End), Canvas.GetTop(End), End.DesiredSize.Width, End.DesiredSize.Height) : new Rect(TmpEnd.Value.X, TmpEnd.Value.Y, 1, 1);
            Point destPos = new Point(destRect.Left + destRect.Width / 2, destRect.Top + destRect.Height / 2);

            Line line = new Line() { X1 = srcPos.X, Y1 = srcPos.Y, X2 = destPos.X, Y2 = destPos.Y };
            Dictionary<Side, Point> srcIntersection = GeometryUtils.RectIntersection(srcRect, line);
            if (srcIntersection.Count > 0)
                srcPos = srcIntersection.First().Value;
            Dictionary<Side, Point> destIntersection = GeometryUtils.RectIntersection(destRect, line);
            if (destIntersection.Count > 0)
                destPos = destIntersection.First().Value;
            line = new Line() { X1 = srcPos.X, Y1 = srcPos.Y, X2 = destPos.X, Y2 = destPos.Y };

            path = new List<Line>();
            List<Line> queue = new List<Line>();
            queue.Add(line);
            while (queue.Count > 0)
            {
                Line item = queue.First();
                queue.RemoveAt(0);
                Line[] splitted = SplitLine(item);
                if (splitted != null)
                    queue.AddRange(splitted);
                else
                    path.Add(item);
            }

            foreach (Line seq in path)
            {
                seq.Stroke = GetHoverBrush(Hovering);
                seq.StrokeThickness = LineThickness;
                seq.StrokeDashArray = IsSelected ? dotted : solid;
                seq.StrokeStartLineCap = PenLineCap.Round;
                seq.StrokeEndLineCap = PenLineCap.Flat;
                board.Children.Add(seq);
            }
            Line lastSeq = path.First(x => x.X2 == destPos.X && x.Y2 == destPos.Y);
            double det = (lastSeq.Y2 - lastSeq.Y1) / (lastSeq.X2 - lastSeq.X1);
            double offset = lastSeq.X2 < lastSeq.X1 ? 180 : 0;
            double deg = Math.Atan(det) * DEG + offset;
            RotateTransform rotation = new RotateTransform(deg, destPos.X, destPos.Y);
            arrow = new Polygon();
            arrow.Points = new PointCollection();
            arrow.Points.Add(destPos);
            arrow.Points.Add(rotation.Transform(new Point(destPos.X - 10, destPos.Y - 5)));
            arrow.Points.Add(rotation.Transform(new Point(destPos.X - 10, destPos.Y + 5)));
            arrow.Fill = GetHoverBrush(Hovering);
            arrow.Stroke = GetHoverBrush(Hovering);
            arrow.StrokeThickness = 2;
            board.Children.Add(arrow);

            //Shorten last sequence
            Line arrowBack = new Line() { X1 = arrow.Points[1].X, Y1 = arrow.Points[1].Y, X2 = arrow.Points[2].X, Y2 = arrow.Points[2].Y };
            Point? docking = GeometryUtils.LineIntersection(lastSeq, arrowBack);
            if (docking != null)
            {
                lastSeq.X2 = docking.Value.X;
                lastSeq.Y2 = docking.Value.Y;
            }
        }

        private Line[] SplitLine(Line line)
        {
            Point p;
            foreach (NodeView obstacle in Graph.GetNodeViews())
            {
                if (obstacle == Start || obstacle == End)
                    continue;
                if (leftColumn != -1 && rightColumn != -1 && (Graph.GetColumnAssign(obstacle.Story) < leftColumn || rightColumn < Graph.GetColumnAssign(obstacle.Story)))
                    continue;
                Rect rect = new Rect(Canvas.GetLeft(obstacle), Canvas.GetTop(obstacle), obstacle.DesiredSize.Width, obstacle.DesiredSize.Height);
                Point topLeft = new Point(rect.TopLeft.X - ROUTING_OFFSET, rect.TopLeft.Y - ROUTING_OFFSET);
                Point topRight = new Point(rect.TopRight.X + ROUTING_OFFSET, rect.TopRight.Y - ROUTING_OFFSET);
                Point bottomLeft = new Point(rect.BottomLeft.X - ROUTING_OFFSET, rect.BottomLeft.Y + ROUTING_OFFSET);
                Point bottomRight = new Point(rect.BottomRight.X + ROUTING_OFFSET, rect.BottomRight.Y + ROUTING_OFFSET);
                Dictionary<Side, Point> intersection = GeometryUtils.RectIntersection(rect, line);
                if (intersection.Count == 2)
                {
                    if (intersection.Keys.Contains(Side.Top) && intersection.Keys.Contains(Side.Left))
                        p = topLeft;
                    else if (intersection.Keys.Contains(Side.Top) && intersection.Keys.Contains(Side.Right))
                        p = topRight;
                    else if (intersection.Keys.Contains(Side.Bottom) && intersection.Keys.Contains(Side.Left))
                        p = bottomLeft;
                    else if (intersection.Keys.Contains(Side.Bottom) && intersection.Keys.Contains(Side.Right))
                        p = bottomRight;
                    else if (intersection.Keys.Contains(Side.Left) && intersection.Keys.Contains(Side.Right))
                    #region Horizontally routing
                    {
                        Point refTop1 = line.X1 <= line.X2 ? topLeft : topRight;
                        Point refTop2 = line.X1 <= line.X2 ? topRight : topLeft;
                        Point refBottom1 = line.X1 <= line.X2 ? bottomLeft : bottomRight;
                        Point refBottom2 = line.X1 <= line.X2 ? bottomRight : bottomLeft;
                        Line[] routingTop = new Line[] {
                                    new Line() { X1 = line.X1, Y1 = line.Y1, X2 = refTop1.X, Y2 = refTop1.Y},
                                    new Line() { X1 = refTop1.X, Y1 = refTop1.Y, X2 = refTop2.X, Y2 = refTop2.Y},
                                    new Line() { X1 = refTop2.X, Y1 = refTop2.Y, X2 = line.X2, Y2 = line.Y2 } };
                        Line[] routingButtom = new Line[] {
                                    new Line() { X1 = line.X1, Y1 = line.Y1, X2 = refBottom1.X, Y2 = refBottom1.Y},
                                    new Line() { X1 = refBottom1.X, Y1 = refBottom1.Y, X2 = refBottom2.X, Y2 = refBottom2.Y},
                                    new Line() { X1 = refBottom2.X, Y1 = refBottom2.Y, X2 = line.X2, Y2 = line.Y2 } };

                        double distanceLeftTop = Math.Round(GeometryUtils.Distance(intersection[Side.Left], rect.TopLeft), ACCURACY);
                        double distanceLeftButtom = Math.Round(GeometryUtils.Distance(intersection[Side.Left], rect.BottomLeft), ACCURACY);
                        double distanceRightTop = Math.Round(GeometryUtils.Distance(intersection[Side.Right], rect.TopRight), ACCURACY);
                        double distanceRightButtom = Math.Round(GeometryUtils.Distance(intersection[Side.Right], rect.BottomRight), ACCURACY);
                        if (distanceLeftTop <= distanceRightButtom || distanceLeftButtom >= distanceRightTop)
                            return routingTop;
                        else
                            return routingButtom;

                    }
                    #endregion
                    else if (intersection.Keys.Contains(Side.Top) && intersection.Keys.Contains(Side.Bottom))
                    #region Vertically routing
                    {
                        Point refLeft1 = line.Y1 <= line.Y2 ? topLeft : bottomLeft;
                        Point refLeft2 = line.Y1 <= line.Y2 ? bottomLeft : topLeft;
                        Point refRight1 = line.Y1 <= line.Y2 ? topRight : bottomRight;
                        Point refRight2 = line.Y1 <= line.Y2 ? bottomRight : topRight;
                        Line[] routingLeft = new Line[] {
                                    new Line() { X1 = line.X1, Y1 = line.Y1, X2 = refLeft1.X, Y2 = refLeft1.Y},
                                    new Line() { X1 = refLeft1.X, Y1 = refLeft1.Y, X2 = refLeft2.X, Y2 = refLeft2.Y},
                                    new Line() { X1 = refLeft2.X, Y1 = refLeft2.Y, X2 = line.X2, Y2 = line.Y2 } };
                        Line[] routingRight = new Line[] {
                                    new Line() { X1 = line.X1, Y1 = line.Y1, X2 = refRight1.X, Y2 = refRight1.Y},
                                    new Line() { X1 = refRight1.X, Y1 = refRight1.Y, X2 = refRight2.X, Y2 = refRight2.Y},
                                    new Line() { X1 = refRight2.X, Y1 = refRight2.Y, X2 = line.X2, Y2 = line.Y2 } };

                        double distanceTopLeft = Math.Round(GeometryUtils.Distance(intersection[Side.Top], rect.TopLeft), ACCURACY);
                        double distanceTopRight = Math.Round(GeometryUtils.Distance(intersection[Side.Bottom], rect.TopRight), ACCURACY);
                        double distanceButtomLeft = Math.Round(GeometryUtils.Distance(intersection[Side.Top], rect.BottomLeft), ACCURACY);
                        double distanceButtomRight = Math.Round(GeometryUtils.Distance(intersection[Side.Bottom], rect.BottomRight), ACCURACY);
                        if (distanceTopLeft < distanceButtomRight || distanceButtomLeft < distanceTopRight)
                            return routingLeft;
                        else
                            return routingRight;
                    }
                    #endregion
                    else
                        p = new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
                    return new Line[] { new Line() { X1 = line.X1, Y1 = line.Y1, X2 = p.X, Y2 = p.Y }, new Line() { X1 = p.X, Y1 = p.Y, X2 = line.X2, Y2 = line.Y2 } };
                }
            }
            return null;
        }
        #endregion

        #region GetHoverBrush
        private Brush GetHoverBrush(HoverState hovering)
        {
            Brush brush = null;
            switch (hovering)
            {
                case HoverState.NOT:
                    brush = new SolidColorBrush(Colors.Black);
                    break;
                case HoverState.SELF:
                    brush = new SolidColorBrush(Colors.Blue);
                    break;
                case HoverState.SOURCE:
                    brush = new SolidColorBrush(Colors.Red);
                    break;
                case HoverState.DESTINATION:
                    brush = new SolidColorBrush(Colors.Green);
                    break;
                case HoverState.SOURCE | HoverState.DESTINATION:
                    brush = new SolidColorBrush(Colors.Purple);
                    break;
            }
            return brush;
        }
        #endregion

        #endregion
    }
}
