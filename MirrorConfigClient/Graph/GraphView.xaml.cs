using MirrorConfigBL.Story;
using MirrorConfigClient.ChangeTracking;
using MirrorConfigClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MirrorConfigClient.Graph
{
    /// <summary>
    /// Interaction logic for GraphView.xaml
    /// </summary>
    public partial class GraphView : UserControl
    {
        #region [Attributes]
        private const int TESTCOUNT = 64;
        private const int DRAG_PADDING = 5;
        private const int GAP = 50;
        private const int BOARD_BOARDERGAP = 1000;
        private static readonly StoryNodeViewModel EMPTY_VM = new StoryNodeViewModel(new StoryNode() { Title = "Kein Knoten ausgewählt" });

        private List<NodeView> nodeViews = new List<NodeView>();
        private List<EdgeView> edgeViews = new List<EdgeView>();
        private Dictionary<StoryNode, int> columnAssign = new Dictionary<StoryNode, int>();
        private Ellipse dot;
        private EdgeView dragging;
        private readonly MatrixTransform transform = new MatrixTransform();
        private Point? draggedFrom;

        private bool centerAfterDraw = false;
        #endregion

        #region [ctor]
        public GraphView()
        {
            InitializeComponent();
            Cursor = ((FrameworkElement)Resources["Grab"]).Cursor;

            AdminMode = false;
            MouseDown += GraphView_MouseDown;
            MouseUp += GraphView_MouseUp;
            MouseMove += GraphView_MouseMove;
            MouseWheel += GraphView_MouseWheel;
            board.MouseMove += Board_MouseMove;
            board.MouseDown += Board_MouseDown;
            board.MouseUp += Board_MouseUp;

            EscCmd = new RelayCommand((x) => true, (x) =>
            {
                Selection = null;
                if (dragging != null)
                {
                    board.Children.Remove(dragging);
                    dragging = null;
                }
            });

            RelayoutCmd = new RelayCommand((x) => true, (x) => BuildGraph());
            ShuffleCmd = new RelayCommand((x) => true, (x) =>
            {
                if (Debugger.IsAttached)
                {
                    AdminMode = true;
                    StoryUtils.GetUtilsInstance().ClearAll();
                    StoryUtils.CreateRandomStoryConfig(TESTCOUNT);
                    ChangeTracker?.ForgetEverything();
                    Stories = new ObservableCollection<StoryNode>(StoryUtils.GetUtilsInstance().GetAllStoryNodes());
                }
            });
            HomeCmd = new RelayCommand((x) => true, (x) => Center());

            NewNodeCmd = new RelayCommand((x) => true, (x) => NewNode());
            DeleteCmd = new RelayCommand((x) => Selection != null, (x) =>
            {
                if (Selection is NodeView)
                    DeleteNode((Selection as NodeView).Story);
                else if (Selection is EdgeView)
                    DeleteEdge((Selection as EdgeView).Start.Story, (Selection as EdgeView).End.Story);
            });
            DeleteAllCmd = new RelayCommand((x) => Stories.Count > 0, (x) =>
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Wirklich alles löschen?\nDies kann nicht rückgängig gemacht werden!", "Löschen bestätigen\u2026", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    StoryUtils.GetUtilsInstance().ClearAll();
                    ChangeTracker?.ForgetEverything();
                    Stories = new ObservableCollection<StoryNode>(StoryUtils.GetUtilsInstance().GetAllStoryNodes());
                }
            });

            AdminModeCmd = new RelayCommand((x) => true, (x) =>
            {
                if (Debugger.IsAttached)
                {
                    AdminMode = !AdminMode;
                }
            });

            keyEsc.Command = EscCmd;
            keyAdd.Command = NewNodeCmd;
            keyCtrlN.Command = NewNodeCmd;
            keyDelete.Command = DeleteCmd;
            keyCtrlDelete.Command = DeleteAllCmd;
            keyF1.Command = HomeCmd;
            keyF5.Command = RelayoutCmd;
            keyF12.Command = ShuffleCmd;
            keyCtrlF12.Command = AdminModeCmd;

            MouseCatcher.DataContext = this;
            Stories = new ObservableCollection<StoryNode>(StoryUtils.GetUtilsInstance().GetAllStoryNodes());
            OnSelectedStoryChanged(null, null);
        }
        #endregion

        #region [Mouse on GraphView]
        private void GraphView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            draggedFrom = transform.Inverse.Transform(e.GetPosition(this));
            Cursor = ((FrameworkElement)Resources["Grabbed"]).Cursor;
        }

        private void GraphView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            draggedFrom = null;
            Cursor = ((FrameworkElement)Resources["Grab"]).Cursor;
        }

        private void GraphView_MouseMove(object sender, MouseEventArgs e)
        {
            if (draggedFrom != null)
            {
                Point draggedTo = transform.Inverse.Transform(e.GetPosition(this));
                Vector delta = Point.Subtract(draggedTo, draggedFrom.Value);
                var translate = new TranslateTransform(delta.X, delta.Y);
                transform.Matrix = translate.Value * transform.Matrix;
                board.RenderTransform = transform;
                e.Handled = true;
            }
        }

        private void GraphView_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            float scale = 1.1f;
            if (e.Delta < 0) scale = 1f / scale;
            Point mouse = e.GetPosition(board);

            Matrix matrix = transform.Matrix;
            matrix.ScaleAt(scale, scale, mouse.X, mouse.Y);
            transform.Matrix = matrix;

            board.RenderTransform = transform;
            e.Handled = true;
        }
        #endregion

        #region [Mouse on Canvas]
        private void Board_MouseMove(object sender, MouseEventArgs e)
        {
            Rect rect = new Rect();
            NodeView hovered = nodeViews.FirstOrDefault(x =>
            {
                rect = new Rect(Canvas.GetLeft(x) - DRAG_PADDING, Canvas.GetTop(x) - DRAG_PADDING, x.ActualWidth + 2 * DRAG_PADDING, x.ActualHeight + 2 * DRAG_PADDING);
                return rect.Contains(e.GetPosition(board));
            });
            HoverNode(hovered?.Story);
            if (hovered != null)
            {
                if (dot == null)
                {
                    dot = new Ellipse()
                    {
                        Width = 5 * LineThickness,
                        Height = 5 * LineThickness,
                        Fill = new SolidColorBrush(Colors.Magenta),
                        Cursor = Cursors.Arrow,
                    };
                    dot.MouseDown += Board_MouseDown;
                    dot.MouseUp += Board_MouseUp;
                    dot.MouseMove += Board_MouseMove;
                    board.Children.Add(dot);
                }
                double diffL = Math.Abs(e.GetPosition(board).X - rect.Left),
                       diffT = Math.Abs(e.GetPosition(board).Y - rect.Top),
                       diffR = Math.Abs(e.GetPosition(board).X - rect.Right),
                       diffB = Math.Abs(e.GetPosition(board).Y - rect.Bottom),
                       diffMin = diffL,
                       left = rect.Left + DRAG_PADDING,
                       top = e.GetPosition(board).Y;
                if (diffT < diffMin)
                {
                    left = e.GetPosition(board).X;
                    top = rect.Top + DRAG_PADDING;
                    diffMin = diffT;
                }
                if (diffR < diffMin)
                {
                    left = rect.Right - DRAG_PADDING;
                    top = e.GetPosition(board).Y;
                    diffMin = diffR;
                }
                if (diffB < diffMin)
                {
                    left = e.GetPosition(board).X;
                    top = rect.Bottom - DRAG_PADDING;
                }
                Canvas.SetLeft(dot, left - dot.Width / 2);
                Canvas.SetTop(dot, top - dot.Height / 2);
                Canvas.SetZIndex(dot, int.MaxValue);
                e.Handled = true;
            }
            else if (dot != null)
            {
                board.Children.Remove(dot);
                dot = null;
                e.Handled = true;
            }


            if (dragging != null)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    dragging.End = hovered;
                    dragging.TmpEnd = e.GetPosition(board);
                }
                else
                {
                    board.Children.Remove(dragging);
                    dragging = null;
                }
                e.Handled = true;
            }
        }

        private void Board_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Rect rect = new Rect();
            NodeView clicked = nodeViews.FirstOrDefault(x =>
            {
                rect = new Rect(Canvas.GetLeft(x) - DRAG_PADDING, Canvas.GetTop(x) - DRAG_PADDING, x.ActualWidth + 2 * DRAG_PADDING, x.ActualHeight + 2 * DRAG_PADDING);
                return rect.Contains(e.GetPosition(board));
            });
            Selection = clicked;
            if (clicked != null)
            {
                if (dragging != null)
                    board.Children.Remove(dragging);
                dragging = new EdgeView(this, clicked, null) { TmpEnd = e.GetPosition(board), LineThickness = LineThickness };
                board.Children.Add(dragging);
                Cursor = Cursors.Arrow;
                e.Handled = true;
            }
        }

        private void Board_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Rect rect = new Rect();
            NodeView released = nodeViews.FirstOrDefault(x =>
            {
                rect = new Rect(Canvas.GetLeft(x) - DRAG_PADDING, Canvas.GetTop(x) - DRAG_PADDING, x.ActualWidth + 2 * DRAG_PADDING, x.ActualHeight + 2 * DRAG_PADDING);
                return rect.Contains(e.GetPosition(board));
            });
            board.Children.Remove(dragging);
            if (released != null && dragging != null && released != dragging.Start)
            {
                NewEdge(dragging.Start.Story, released.Story);
                dragging = null;
                Cursor = ((FrameworkElement)Resources["Grab"]).Cursor;
                e.Handled = true;
            }
        }
        #endregion

        #region [Mouse on Edge]
        private void Edge_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Selection = sender as EdgeView;
            e.Handled = true;
        }
        #endregion

        #region [Dependency Properties]

        #region Stories
        /// <summary>
        /// The backing field for the Stories dependency property.
        /// </summary>
        public static readonly DependencyProperty StoriesProperty = DependencyProperty.Register("Stories", typeof(ObservableCollection<StoryNode>), typeof(GraphView), new PropertyMetadata(new ObservableCollection<StoryNode>(), OnStoriesPropertyChanged));

        /// <summary>
        /// Called when the Stories depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnStoriesPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            GraphView Sender = o as GraphView;
            Sender.OnStoriesChanged((ObservableCollection<StoryNode>)e.OldValue, (ObservableCollection<StoryNode>)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the Stories.
        /// </summary>
        /// <value>The layers.</value>
        public ObservableCollection<StoryNode> Stories
        {
            get
            {
                return (ObservableCollection<StoryNode>)GetValue(StoriesProperty);
            }
            set
            {
                SetValue(StoriesProperty, value);
            }
        }

        /// <summary>
        /// Called when the Stories dependency property changes.
        /// </summary>
        /// <param name="OldValue">The old value.</param>
        /// <param name="NewValue">The new value.</param>
        protected virtual void OnStoriesChanged(ObservableCollection<StoryNode> OldValue, ObservableCollection<StoryNode> NewValue)
        {
            Selection = null;
            if (OldValue != null)
                OldValue.CollectionChanged -= Stories_CollectionChanged;
            IsEmpty = NewValue == null || NewValue.Count == 0;
            if (NewValue != null)
                NewValue.CollectionChanged += Stories_CollectionChanged;
            BuildGraph();
            centerAfterDraw = true;
        }
        #endregion

        #region SelectedStory
        /// <summary>
        /// The backing field for the SelectedStory dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedStoryProperty = DependencyProperty.Register("SelectedStory", typeof(StoryNode), typeof(GraphView), new PropertyMetadata(null, OnSelectedStoryPropertyChanged));

        /// <summary>
        /// Called when the SelectedStory depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnSelectedStoryPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            GraphView Sender = o as GraphView;
            Sender.OnSelectedStoryChanged((StoryNode)e.OldValue, (StoryNode)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the SelectedStory.
        /// </summary>
        /// <value>The layers.</value>
        public StoryNode SelectedStory
        {
            get
            {
                return (StoryNode)GetValue(SelectedStoryProperty);
            }
            set
            {
                SetValue(SelectedStoryProperty, value);
            }
        }

        /// <summary>
        /// Called when the SelectedStory dependency property changes.
        /// </summary>
        /// <param name="OldValue">The old value.</param>
        /// <param name="NewValue">The new value.</param>
        protected virtual void OnSelectedStoryChanged(StoryNode OldValue, StoryNode NewValue)
        {
            if (SelectedStoryVM != null)
            {
                var vm = (StoryNodeViewModel)SelectedStoryVM;
                vm[nameof(vm.IsSelected)] = false;
            }
            SelectedStoryVM = NewValue == null ? EMPTY_VM : (ViewModel<StoryNode>)nodeViews.FirstOrDefault(x => x.Story == NewValue).DataContext;
            if (SelectedStoryVM != null)
            {
                var vm = (StoryNodeViewModel)SelectedStoryVM;
                vm[nameof(vm.IsSelected)] = true;
            }
            CommandManager.InvalidateRequerySuggested();
        }
        #endregion

        #region SelectedStoryVM
        /// <summary>
        /// The backing field for the SelectedStoryVM dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedStoryVMProperty = DependencyProperty.Register("SelectedStoryVM", typeof(ViewModel<StoryNode>), typeof(GraphView), new PropertyMetadata(null, OnSelectedStoryVMPropertyChanged));

        /// <summary>
        /// Called when the SelectedStoryVM depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnSelectedStoryVMPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            GraphView Sender = o as GraphView;
            Sender.OnSelectedStoryVMChanged((ViewModel<StoryNode>)e.OldValue, (ViewModel<StoryNode>)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the SelectedStoryVM.
        /// </summary>
        /// <value>The layers.</value>
        public ViewModel<StoryNode> SelectedStoryVM
        {
            get
            {
                return (ViewModel<StoryNode>)GetValue(SelectedStoryVMProperty);
            }
            set
            {
                SetValue(SelectedStoryVMProperty, value);
            }
        }

        /// <summary>
        /// Called when the SelectedStoryVM dependency property changes.
        /// </summary>
        /// <param name="OldValue">The old value.</param>
        /// <param name="NewValue">The new value.</param>
        protected virtual void OnSelectedStoryVMChanged(ViewModel<StoryNode> OldValue, ViewModel<StoryNode> NewValue)
        {
            if (OldValue != null)
                OldValue.PropertyChanged -= SelectedStory_PropertyChanged;
            if (NewValue != null)
                NewValue.PropertyChanged += SelectedStory_PropertyChanged;
        }
        #endregion

        #region Selection
        /// <summary>
        /// The backing field for the Selection dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectionProperty = DependencyProperty.Register("Selection", typeof(Selectable), typeof(GraphView), new PropertyMetadata(null, OnSelectionPropertyChanged));

        /// <summary>
        /// Called when the Selection depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnSelectionPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            GraphView Sender = o as GraphView;
            Sender.OnSelectionChanged((Selectable)e.OldValue, (Selectable)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the Selection.
        /// </summary>
        /// <value>The layers.</value>
        public Selectable Selection
        {
            get
            {
                return (Selectable)GetValue(SelectionProperty);
            }
            set
            {
                SetValue(SelectionProperty, value);
            }
        }

        /// <summary>
        /// Called when the Selection dependency property changes.
        /// </summary>
        /// <param name="OldValue">The old value.</param>
        /// <param name="NewValue">The new value.</param>
        protected virtual void OnSelectionChanged(Selectable OldValue, Selectable NewValue)
        {
            if (OldValue != null)
                OldValue.IsSelected = false;
            if (NewValue != null)
                NewValue.IsSelected = true;
            SelectedStory = (NewValue as NodeView)?.Story;
        }
        #endregion

        #region LineThickness
        /// <summary>
        /// The backing field for the LineThickness dependency property.
        /// </summary>
        public static readonly DependencyProperty LineThicknessProperty = DependencyProperty.Register("LineThickness", typeof(double), typeof(GraphView), new PropertyMetadata(1d, OnLineThicknessPropertyChanged));

        /// <summary>
        /// Called when the LineThickness depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnLineThicknessPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            GraphView Sender = o as GraphView;
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
            Draw(true);
        }
        #endregion

        #region AdminMode
        /// <summary>
        /// The backing field for the AdminMode dependency property.
        /// </summary>
        public static readonly DependencyProperty AdminModeProperty = DependencyProperty.Register("AdminMode", typeof(bool), typeof(GraphView), new PropertyMetadata(false, OnAdminModePropertyChanged));

        /// <summary>
        /// Called when the AdminMode depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnAdminModePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            GraphView Sender = o as GraphView;
            Sender.OnAdminModeChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the AdminMode.
        /// </summary>
        /// <value>The layers.</value>
        public bool AdminMode
        {
            get
            {
                return (bool)GetValue(AdminModeProperty);
            }
            set
            {
                SetValue(AdminModeProperty, value);
            }
        }

        /// <summary>
        /// Called when the AdminMode dependency property changes.
        /// </summary>
        /// <param name="OldValue">The old value.</param>
        /// <param name="NewValue">The new value.</param>
        protected virtual void OnAdminModeChanged(bool OldValue, bool NewValue)
        {

        }
        #endregion

        #region IsEmpty
        /// <summary>
        /// The backing field for the IsEmpty dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEmptyProperty = DependencyProperty.Register("IsEmpty", typeof(bool), typeof(GraphView), new PropertyMetadata(true, OnIsEmptyPropertyChanged));

        /// <summary>
        /// Called when the IsEmpty depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnIsEmptyPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            GraphView Sender = o as GraphView;
            Sender.OnIsEmptyChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the IsEmpty.
        /// </summary>
        /// <value>The layers.</value>
        public bool IsEmpty
        {
            get
            {
                return (bool)GetValue(IsEmptyProperty);
            }
            private set
            {
                SetValue(IsEmptyProperty, value);
            }
        }

        /// <summary>
        /// Called when the IsEmpty dependency property changes.
        /// </summary>
        /// <param name="OldValue">The old value.</param>
        /// <param name="NewValue">The new value.</param>
        protected virtual void OnIsEmptyChanged(bool OldValue, bool NewValue)
        {

        }
        #endregion

        #endregion

        #region [Properties]

        #region ChangeTracker
        public ChangeTracker ChangeTracker { get; set; }
        #endregion

        #endregion

        #region [Commands]

        #region EscCmd
        public ICommand EscCmd { get; private set; }
        #endregion

        #region RelayoutCmd
        public ICommand RelayoutCmd { get; private set; }
        #endregion

        #region ShuffleCmd
        public ICommand ShuffleCmd { get; private set; }
        #endregion

        #region HomeCmd
        public ICommand HomeCmd { get; private set; }
        #endregion

        #region NewNodeCmd
        public ICommand NewNodeCmd { get; private set; }
        #endregion

        #region DeleteCmd
        public ICommand DeleteCmd { get; private set; }
        #endregion

        #region DeleteAllCmd
        public ICommand DeleteAllCmd { get; private set; }
        #endregion

        #region AdminModeCmd
        public ICommand AdminModeCmd { get; private set; }
        #endregion

        #endregion

        #region [Methods]

        #region NewNode
        /// <summary>
        /// Adds a new NodeView to the graph.
        /// </summary>
        /// <param name="source">NodeView used as soruce</param>
        /// <param name="track">Whether change should be traked to undo / redo</param>
        public NodeView NewNode(StoryNode source = null, int columnAssin = -1, bool track = true)
        {
            StoryNode story = StoryUtils.GetUtilsInstance().AddStoryNode(source == null ? new StoryNode() : source);
            if (!Stories.Contains(story))
            {
                if (source == null)
                    story.Title = "Neuer Knoten";
                Stories.Add(story);
                columnAssign.Add(story, columnAssin >= 0 ? columnAssin : columnAssign.Count > 0 ? columnAssign.Max(x => x.Value) + 1 : 0);
                Draw(false);
                NodeView newNodeView = nodeViews.First(x => x.Story == story);
                Selection = newNodeView;
                if (track)
                {
                    int assign = GetColumnAssign(newNodeView.Story);
                    List<StoryNode> previous = Stories.Where(x => x.NextStoryIDs.Contains(newNodeView.Story.StoryID)).ToList();
                    List<StoryNode> next = Stories.Where(x => newNodeView.Story.NextStoryIDs.Contains(x.StoryID)).ToList();
                    ChangeTracker?.AddUndoable(new Transaction(
                        new Action(() => DeleteNode(story, false)),
                        new Action(() =>
                        {
                            NewNode(story, assign, false);
                            foreach (StoryNode from in previous)
                                NewEdge(from, story, false);
                            foreach (StoryNode to in next)
                                NewEdge(story, to, false);
                        }),
                        this, null
                    ));
                }
                Center();
                return newNodeView;
            }
            return null;
        }
        #endregion

        #region NewEdge
        /// <summary>
        /// Adds a new EdgeView to the graph.
        /// </summary>
        /// <param name="from">NodeView where to start</param>
        /// <param name="to">NodeView where to end</param>
        /// <param name="track">Whether change should be traked to undo / redo</param>
        public EdgeView NewEdge(StoryNode from, StoryNode to, bool track = true)
        {
            if (from != null && to != null && !from.NextStoryIDs.Contains(to.StoryID))
            {
                NodeView srcView = nodeViews.FirstOrDefault(x => x.Story == from);
                NodeView destView = nodeViews.FirstOrDefault(x => x.Story == to);
                if (srcView != null && destView != null)
                {
                    StoryUtils.GetUtilsInstance().AddStoryEdge(from, to);
                    ((ViewModel<StoryNode>)srcView.DataContext)?.OnPropertyChanged(nameof(from.NextStoryIDs), null, null);
                    EdgeView edgeView = new EdgeView(this, srcView, destView) { LineThickness = LineThickness };
                    edgeView.MouseDown += Edge_MouseDown;
                    edgeViews.Add(edgeView);
                    board.Children.Add(edgeView);
                    if (track)
                        ChangeTracker?.AddUndoable(new Transaction(
                            new Action(() => DeleteEdge(from, to, false)),
                            new Action(() => NewEdge(from, to, false)),
                            this, null
                        ));
                    return edgeView;
                }
            }
            return null;
        }
        #endregion

        #region DeleteNode
        /// <summary>
        /// Deletes the given NodeView.
        /// </summary>
        /// <param name="nodeView">NodeView to be deleted</param>
        /// <param name="track">Whether change should be traked to undo / redo</param>
        public bool DeleteNode(StoryNode story, bool track = true)
        {
            if (story == null)
                return false;
            MessageBoxResult messageBoxResult = MessageBox.Show($"Knoten \"{story.Title}\" wirklich löschen?", "Löschen bestätigen\u2026", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                int assign = columnAssign[story];
                List<StoryNode> previous = Stories.Where(x => x.NextStoryIDs.Contains(story.StoryID)).ToList();
                List<StoryNode> next = Stories.Where(x => story.NextStoryIDs.Contains(x.StoryID)).ToList();
                List<EdgeView> toRemove = edgeViews.Where(edge => edge.Start.Story == story || edge.End.Story == story).ToList();
                foreach (EdgeView edgeView in toRemove)
                    DeleteEdge(edgeView.Start.Story, edgeView.End.Story, false);
                if ((Selection as NodeView)?.Story == story)
                    Selection = null;
                StoryUtils.GetUtilsInstance().RemoveStoryNode(story.StoryID);
                Stories.Remove(story);
                columnAssign.Remove(story);
                NodeView nodeView = nodeViews.First(x => x.Story == story);
                nodeViews.Remove(nodeView);
                board.Children.Remove(nodeView);
                if (track)
                    ChangeTracker?.AddUndoable(new Transaction(
                        new Action(() =>
                        {
                            NewNode(story, assign, false);
                            foreach (StoryNode from in previous)
                                NewEdge(from, story, false);
                            foreach (StoryNode to in next)
                                NewEdge(story, to, false);
                        }),
                        new Action(() => DeleteNode(story, false)),
                        this, null
                    ));
                return true;
            }
            return false;
        }
        #endregion

        #region DeleteEdge
        /// <summary>
        /// Deletes the specified EdgeView.
        /// </summary>
        /// <param name="edgeView">EdgeView to be deleted</param>
        /// <param name="track">Whether change should be traked to undo / redo</param>
        public void DeleteEdge(StoryNode from, StoryNode to, bool track = true)
        {
            EdgeView edgeView = edgeViews.First(x => x.Start.Story == from && x.End.Story == to);
            NodeView srcView = nodeViews.First(x => x.Story == from);
            if (Selection == edgeView)
                Selection = null;
            StoryUtils.GetUtilsInstance().RemoveStoryEdge(from, to);
            ((ViewModel<StoryNode>)srcView.DataContext)?.OnPropertyChanged(nameof(from.NextStoryIDs), null, null);
            edgeViews.Remove(edgeView);
            board.Children.Remove(edgeView);
            if (track)
                ChangeTracker?.AddUndoable(new Transaction(
                    new Action(() => NewEdge(from, to, false)),
                    new Action(() => DeleteEdge(from, to, false)),
                    this, null
                ));
        }
        #endregion

        #region HoverNode
        private void HoverNode(StoryNode story)
        {
            foreach (EdgeView edgeView in edgeViews)
            {
                edgeView.Hovering = HoverState.NOT;
                Canvas.SetZIndex(edgeView, 0);
            }
            for (int i = 0; i < nodeViews.Count; i++)
            {
                HoverState hovering = HoverState.NOT;
                if (story != null)
                {
                    if (nodeViews[i].Story == story)
                        hovering = hovering | HoverState.SELF;
                    else
                    {
                        if (nodeViews[i].Story.NextStoryIDs.Contains(story.StoryID))
                            hovering = hovering | HoverState.SOURCE;
                        if (story.NextStoryIDs.Contains(nodeViews[i].Story.StoryID))
                            hovering = hovering | HoverState.DESTINATION;
                    }
                }
                nodeViews[i].Hovering = hovering;
                foreach (EdgeView edgeView in edgeViews.Where(x => (x.Start == nodeViews[i] && x.End.Story == story) || (x.Start.Story == story && x.End == nodeViews[i])))
                {
                    edgeView.Hovering = hovering;
                    Canvas.SetZIndex(edgeView, 1);
                }
            }
        }
        #endregion

        #region ProcessStories
        public void BuildGraph()
        {
            board.Children.Clear();
            nodeViews.Clear();
            edgeViews.Clear();
            columnAssign.Clear();
            if (Stories == null || Stories.Count == 0)
                return;
            List<StoryNode> start = Stories.Where(x => x.IsStartStory).ToList();
            if (start.Count <= 0)
            {
                int maxOutgoing = Stories.Max(x => x.NextStoryIDs.Count);
                start = new List<StoryNode>() { Stories.First(x => x.NextStoryIDs.Count == maxOutgoing) };
            }
            BuildHirarchie(Stories, start, 0);
            List<StoryNode> unassigned = Stories.Where(x => !columnAssign.ContainsKey(x)).ToList();
            while (unassigned.Count > 0)
            {
                StoryNode tmpStart = unassigned.FirstOrDefault(x => x.NextStoryIDs.Count == unassigned.Max(y => y.NextStoryIDs.Count));
                BuildHirarchie(Stories, new List<StoryNode>() { tmpStart }, columnAssign.Count > 0 ? columnAssign.Max(x => x.Value) + 1 : 0);
                unassigned = Stories.Where(x => !columnAssign.ContainsKey(x)).ToList();
            }
            Draw(true);
        }
        #endregion

        #region BuildHirarchie
        private void BuildHirarchie(IEnumerable<StoryNode> source, IEnumerable<StoryNode> arrange, int minLayer)
        {
            List<StoryNode> run = arrange.Where(x => !columnAssign.ContainsKey(x)).ToList();
            foreach (StoryNode story in run)
            {
                columnAssign.Add(story, minLayer);
            }
            foreach (StoryNode story in run)
            {
                BuildHirarchie(source, source.Where(x => story.NextStoryIDs.Contains(x.StoryID)), minLayer + 1);
            }
        }
        #endregion

        #region Draw
        private void Draw(bool async)
        {
            Action<object> draw = new Action<object>((object Parameter) =>
            {
                if (columnAssign == null)
                    return;
                board.Children.Clear();
                int maxColumns = columnAssign.Count > 0 ? columnAssign.Max(x => x.Value) + 1 : 0;
                int maxRows = columnAssign.Count > 0 ? columnAssign.GroupBy(x => x.Value).Max(x => x.Count()) : 0;
                double maxWidth = 0, maxHeight = 0;
                int row;
                List<NodeView> toRemove = nodeViews.Where(x => !Stories.Contains(x.Story)).ToList();
                foreach (NodeView nodeView in toRemove)
                    nodeViews.Remove(nodeView);
                foreach (StoryNode story in Stories)
                {
                    NodeView nodeView = nodeViews.FirstOrDefault(x => x.Story == story);
                    if (nodeView == null)
                    {
                        nodeView = new NodeView(story);
                        nodeView.SizeChanged += NodeView_SizeChanged;
                        nodeViews.Add(nodeView);
                    }
                    nodeView.BorderThickness = LineThickness;
                    nodeView.Hovering = HoverState.NOT;
                    bool IsPreviewSize = nodeView.ActualWidth == 0 && nodeView.ActualHeight == 0;
                    if (IsPreviewSize)
                        nodeView.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    if ((IsPreviewSize ? nodeView.DesiredSize.Width : nodeView.ActualWidth) > maxWidth)
                        maxWidth = IsPreviewSize ? nodeView.DesiredSize.Width : nodeView.ActualWidth;
                    if ((IsPreviewSize ? nodeView.DesiredSize.Height : nodeView.ActualHeight) > maxHeight)
                        maxHeight = IsPreviewSize ? nodeView.DesiredSize.Height : nodeView.ActualHeight;
                    if (story == SelectedStory)
                        Selection = nodeView;
                }
                maxWidth += GAP;
                maxHeight += GAP;
                board.Width = 2 * BOARD_BOARDERGAP + maxWidth * maxColumns;
                board.Height = 2 * BOARD_BOARDERGAP + maxHeight * maxRows;
                board.HorizontalAlignment = HorizontalAlignment.Center;
                for (int column = 0; column < maxColumns; column++)
                {
                    IEnumerable<StoryNode> stories = columnAssign.Where(x => x.Value == column).Select(x => x.Key);
                    row = 0;
                    int maxRowsInColumn = stories.Count();
                    double columnRowHeight = maxRows * maxHeight / maxRowsInColumn;
                    foreach (StoryNode story in stories)
                    {
                        NodeView nodeView = nodeViews.First(x => x.Story == story);
                        Canvas.SetLeft(nodeView, BOARD_BOARDERGAP + column * maxWidth + maxWidth / 2 - nodeView.DesiredSize.Width / 2);
                        Canvas.SetTop(nodeView, BOARD_BOARDERGAP + row * columnRowHeight + columnRowHeight / 2 - nodeView.DesiredSize.Height / 2);
                        board.Children.Add(nodeView);
                        row++;
                    }
                }
                edgeViews.Clear();
                foreach (NodeView srcView in nodeViews)
                {
                    for (int i = 0; i < srcView.Story.NextStoryIDs.Count; i++)
                    {
                        NodeView destView = nodeViews.FirstOrDefault(x => x.Story.StoryID == srcView.Story.NextStoryIDs[i]);
                        if (destView != null)
                        {
                            bool isSelection = (Selection as EdgeView)?.Start.Story == srcView.Story && (Selection as EdgeView)?.End.Story == destView.Story;
                            EdgeView edgeView = new EdgeView(this, srcView, destView) { LineThickness = LineThickness, IsSelected = isSelection };
                            if (isSelection)
                                Selection = edgeView;
                            edgeView.MouseDown += Edge_MouseDown;
                            edgeViews.Add(edgeView);
                            board.Children.Add(edgeView);
                        }
                        else
                            System.Diagnostics.Debug.WriteLine("Node not found: " + srcView.Story.Title + " -> " + srcView.Story.NextStoryIDs[i]);
                    }
                }

                if (centerAfterDraw)
                {
                    centerAfterDraw = false;
                    Center();
                }
            });
            if (async)
                DeferredAction.Execute(draw, null, 100);
            else
                draw(null);
        }
        #endregion

        #region Center
        public void Center()
        {
            Matrix m = new Matrix(1, 0, 0, 1, 0, 0);
            double scale = Math.Min(ActualWidth / (board.Width - 2 * BOARD_BOARDERGAP), ActualHeight / (board.Height - 2 * BOARD_BOARDERGAP));
            m.ScaleAt(scale, scale, board.Width / 2, board.Height / 2);
            transform.Matrix = m;
            board.RenderTransform = transform;
        }
        #endregion

        #region GetNodes
        public List<NodeView> GetNodeViews()
        {
            return nodeViews;
        }
        #endregion

        #region GetEdges
        public List<EdgeView> GetEdgeViews()
        {
            return edgeViews;
        }
        #endregion

        #region GetColumnAssign
        public int GetColumnAssign(StoryNode story)
        {
            if (columnAssign.ContainsKey(story))
                return columnAssign[story];
            return -1;
        }
        #endregion

        #endregion

        #region [Events]

        #region NodeView_SizeChanged
        private void NodeView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender as NodeView != null)
                Draw(true);
        }
        #endregion

        #region SelectedStory_PropertyChanged
        private void SelectedStory_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender as StoryNode != null)
                Draw(true);
        }
        #endregion

        #region Stories_CollectionChanged
        private void Stories_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsEmpty = Stories.Count == 0;
        }
        #endregion

        #endregion
    }
}
