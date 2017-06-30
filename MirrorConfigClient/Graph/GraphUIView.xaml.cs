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
    /// Interaktionslogik für GraphUIView.xaml
    /// </summary>
    public partial class GraphUIView : UserControl
    {
        #region [Attributes]
        private const int TESTCOUNT = 64;
        private const int DRAG_PADDING = 5;
        private const int GAP = 50;
        private const int BOARD_BOARDERGAP = 1000;
        private static readonly StoryNodeViewModel EMPTY_VM = new StoryNodeViewModel(new StoryNode() { Title = "Kein Knoten ausgewählt" });
        #endregion

        #region [ctor]
        public GraphUIView()
        {
            InitializeComponent();

            AdminMode = false;

            RelayoutCmd = new RelayCommand((x) => true, (x) => GraphView.BuildGraph());
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

            keyAdd.Command = NewNodeCmd;
            keyCtrlN.Command = NewNodeCmd;
            keyDelete.Command = DeleteCmd;
            keyCtrlDelete.Command = DeleteAllCmd;
            keyF1.Command = HomeCmd;
            keyF5.Command = RelayoutCmd;
            keyF12.Command = ShuffleCmd;
            keyCtrlF12.Command = AdminModeCmd;
        }
        #endregion

        #region [Dependency Properties]

        #region Stories
        /// <summary>
        /// The backing field for the Stories dependency property.
        /// </summary>
        public static readonly DependencyProperty StoriesProperty = DependencyProperty.Register("Stories", typeof(ObservableCollection<StoryNode>), typeof(GraphUIView), new PropertyMetadata(new ObservableCollection<StoryNode>(), null));

        /// <summary>
        /// Gets or sets the Stories.
        /// </summary>
        /// <value>The layers.</value>
        public ObservableCollection<StoryNode> Stories
        {
            get
            {
                return GraphView.Stories;
            }
            set
            {
                GraphView.Stories = value;
            }
        }
        #endregion

        #region SelectedStory
        /// <summary>
        /// The backing field for the SelectedStory dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedStoryProperty = DependencyProperty.Register("SelectedStory", typeof(StoryNode), typeof(GraphUIView), new PropertyMetadata(null, OnSelectedStoryPropertyChanged));

        /// <summary>
        /// Called when the SelectedStory depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnSelectedStoryPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            GraphUIView Sender = o as GraphUIView;
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
            SelectedStoryVM = NewValue == null ? EMPTY_VM : (ViewModel<StoryNode>)GetNodeViews().FirstOrDefault(x => x.Story == NewValue).DataContext;
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
        public static readonly DependencyProperty SelectedStoryVMProperty = DependencyProperty.Register("SelectedStoryVM", typeof(ViewModel<StoryNode>), typeof(GraphUIView), new PropertyMetadata(null, null));
        
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
        #endregion

        #region Selection
        /// <summary>
        /// The backing field for the Selection dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectionProperty = DependencyProperty.Register("Selection", typeof(Selectable), typeof(GraphUIView), new PropertyMetadata(null, OnSelectionPropertyChanged));

        /// <summary>
        /// Called when the Selection depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnSelectionPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            GraphUIView Sender = o as GraphUIView;
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
        public static readonly DependencyProperty LineThicknessProperty = DependencyProperty.Register("LineThickness", typeof(double), typeof(GraphUIView), new PropertyMetadata(1d, null));

        /// <summary>
        /// Gets or sets the LineThickness.
        /// </summary>
        /// <value>The layers.</value>
        public double LineThickness
        {
            get
            {
                return GraphView.LineThickness;
            }
            set
            {
                GraphView.LineThickness = value;
            }
        }
        #endregion

        #region AdminMode
        /// <summary>
        /// The backing field for the AdminMode dependency property.
        /// </summary>
        public static readonly DependencyProperty AdminModeProperty = DependencyProperty.Register("AdminMode", typeof(bool), typeof(GraphUIView), new PropertyMetadata(false, OnAdminModePropertyChanged));

        /// <summary>
        /// Called when the AdminMode depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnAdminModePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            GraphUIView Sender = o as GraphUIView;
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
        public static readonly DependencyProperty IsEmptyProperty = DependencyProperty.Register("IsEmpty", typeof(bool), typeof(GraphUIView), new PropertyMetadata(true, null));
        
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
        #endregion

        #endregion

        #region [Properties]

        #region ChangeTracker
        public ChangeTracker ChangeTracker
        {
            get
            {
                return GraphView.ChangeTracker;
            }
            set
            {
                GraphView.ChangeTracker = value;
            }
        }
        #endregion

        #endregion

        #region [Commands]

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
            return GraphView.NewNode(source, columnAssin, track);
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
            return GraphView.NewEdge(from, to, track);
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
            return GraphView.DeleteNode(story, track);
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
            GraphView.DeleteEdge(from, to, track);
        }
        #endregion

        //#region ProcessStories
        //private void ProcessStories()
        //{
        //    board.Children.Clear();
        //    nodeViews.Clear();
        //    edgeViews.Clear();
        //    columnAssign.Clear();
        //    if (Stories == null || Stories.Count == 0)
        //        return;
        //    List<StoryNode> start = Stories.Where(x => x.IsStartStory).ToList();
        //    if (start.Count <= 0)
        //    {
        //        int maxOutgoing = Stories.Max(x => x.NextStoryIDs.Count);
        //        start = new List<StoryNode>() { Stories.First(x => x.NextStoryIDs.Count == maxOutgoing) };
        //    }
        //    BuildHirarchie(Stories, start, 0);
        //    List<StoryNode> unassigned = Stories.Where(x => !columnAssign.ContainsKey(x)).ToList();
        //    while (unassigned.Count > 0)
        //    {
        //        StoryNode tmpStart = unassigned.FirstOrDefault(x => x.NextStoryIDs.Count == unassigned.Max(y => y.NextStoryIDs.Count));
        //        BuildHirarchie(Stories, new List<StoryNode>() { tmpStart }, columnAssign.Count > 0 ? columnAssign.Max(x => x.Value) + 1 : 0);
        //        unassigned = Stories.Where(x => !columnAssign.ContainsKey(x)).ToList();
        //    }
        //    Draw(true);
        //}
        //#endregion

        //#region BuildHirarchie
        //private void BuildHirarchie(IEnumerable<StoryNode> source, IEnumerable<StoryNode> arrange, int minLayer)
        //{
        //    List<StoryNode> run = arrange.Where(x => !columnAssign.ContainsKey(x)).ToList();
        //    foreach (StoryNode story in run)
        //    {
        //        columnAssign.Add(story, minLayer);
        //    }
        //    foreach (StoryNode story in run)
        //    {
        //        BuildHirarchie(source, source.Where(x => story.NextStoryIDs.Contains(x.StoryID)), minLayer + 1);
        //    }
        //}
        //#endregion

        //#region Draw
        //private void Draw(bool async)
        //{
        //    Action<object> draw = new Action<object>((object Parameter) =>
        //    {
        //        if (columnAssign == null)
        //            return;
        //        board.Children.Clear();
        //        int maxColumns = columnAssign.Count > 0 ? columnAssign.Max(x => x.Value) + 1 : 0;
        //        int maxRows = columnAssign.Count > 0 ? columnAssign.GroupBy(x => x.Value).Max(x => x.Count()) : 0;
        //        double maxWidth = 0, maxHeight = 0;
        //        int row;
        //        List<NodeView> toRemove = nodeViews.Where(x => !Stories.Contains(x.Story)).ToList();
        //        foreach (NodeView nodeView in toRemove)
        //            nodeViews.Remove(nodeView);
        //        foreach (StoryNode story in Stories)
        //        {
        //            NodeView nodeView = nodeViews.FirstOrDefault(x => x.Story == story);
        //            if (nodeView == null)
        //            {
        //                nodeView = new NodeView(story);
        //                nodeView.SizeChanged += NodeView_SizeChanged;
        //                nodeViews.Add(nodeView);
        //            }
        //            nodeView.BorderThickness = LineThickness;
        //            nodeView.Hovering = HoverState.NOT;
        //            bool IsPreviewSize = nodeView.ActualWidth == 0 && nodeView.ActualHeight == 0;
        //            if (IsPreviewSize)
        //                nodeView.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        //            if ((IsPreviewSize ? nodeView.DesiredSize.Width : nodeView.ActualWidth) > maxWidth)
        //                maxWidth = IsPreviewSize ? nodeView.DesiredSize.Width : nodeView.ActualWidth;
        //            if ((IsPreviewSize ? nodeView.DesiredSize.Height : nodeView.ActualHeight) > maxHeight)
        //                maxHeight = IsPreviewSize ? nodeView.DesiredSize.Height : nodeView.ActualHeight;
        //            if (story == SelectedStory)
        //                Selection = nodeView;
        //        }
        //        maxWidth += GAP;
        //        maxHeight += GAP;
        //        board.Width = 2 * BOARD_BOARDERGAP + maxWidth * maxColumns;
        //        board.Height = 2 * BOARD_BOARDERGAP + maxHeight * maxRows;
        //        board.HorizontalAlignment = HorizontalAlignment.Center;
        //        for (int column = 0; column < maxColumns; column++)
        //        {
        //            IEnumerable<StoryNode> stories = columnAssign.Where(x => x.Value == column).Select(x => x.Key);
        //            row = 0;
        //            int maxRowsInColumn = stories.Count();
        //            double columnRowHeight = maxRows * maxHeight / maxRowsInColumn;
        //            foreach (StoryNode story in stories)
        //            {
        //                NodeView nodeView = nodeViews.First(x => x.Story == story);
        //                Canvas.SetLeft(nodeView, BOARD_BOARDERGAP + column * maxWidth + maxWidth / 2 - nodeView.DesiredSize.Width / 2);
        //                Canvas.SetTop(nodeView, BOARD_BOARDERGAP + row * columnRowHeight + columnRowHeight / 2 - nodeView.DesiredSize.Height / 2);
        //                board.Children.Add(nodeView);
        //                row++;
        //            }
        //        }
        //        edgeViews.Clear();
        //        foreach (NodeView srcView in nodeViews)
        //        {
        //            for (int i = 0; i < srcView.Story.NextStoryIDs.Count; i++)
        //            {
        //                NodeView destView = nodeViews.FirstOrDefault(x => x.Story.StoryID == srcView.Story.NextStoryIDs[i]);
        //                if (destView != null)
        //                {
        //                    bool isSelection = (Selection as EdgeView)?.Start.Story == srcView.Story && (Selection as EdgeView)?.End.Story == destView.Story;
        //                    EdgeView edgeView = new EdgeView(this, srcView, destView) { LineThickness = LineThickness, IsSelected = isSelection };
        //                    if (isSelection)
        //                        Selection = edgeView;
        //                    edgeView.MouseDown += Edge_MouseDown;
        //                    edgeViews.Add(edgeView);
        //                    board.Children.Add(edgeView);
        //                }
        //                else
        //                    System.Diagnostics.Debug.WriteLine("Node not found: " + srcView.Story.Title + " -> " + srcView.Story.NextStoryIDs[i]);
        //            }
        //        }

        //        if (centerAfterDraw)
        //        {
        //            centerAfterDraw = false;
        //            Center();
        //        }
        //    });
        //    if (async)
        //        DeferredAction.Execute(draw, null, 100);
        //    else
        //        draw(null);
        //}
        //#endregion

        #region Center
        public void Center()
        {
            GraphView.Center();
        }
        #endregion

        #region GetNodes
        public List<NodeView> GetNodeViews()
        {
            return GraphView.GetNodeViews();
        }
        #endregion

        #region GetEdges
        public List<EdgeView> GetEdgeViews()
        {
            return GraphView.GetEdgeViews();
        }
        #endregion

        #region GetColumnAssign
        public int GetColumnAssign(StoryNode story)
        {
            return GraphView.GetColumnAssign(story);
        }
        #endregion

        #endregion
    }
}
