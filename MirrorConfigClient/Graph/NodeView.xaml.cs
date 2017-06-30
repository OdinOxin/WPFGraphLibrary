using MirrorConfigBL.Story;
using MirrorConfigClient.ValueConverter;
using MirrorConfigClient.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MirrorConfigClient.Graph
{
    /// <summary>
    /// Interaction logic for NodeView.xaml
    /// </summary>
    public partial class NodeView : UserControl, Selectable, Hoverable
    {
        #region [Attributes]
        private readonly DoubleCollection solid = new DoubleCollection(new double[] { 1, 0 }), dotted = new DoubleCollection(new double[] { 2, 1 });
        #endregion

        #region [ctor]
        public NodeView(StoryNode story)
        {
            InitializeComponent();
            Story = story;
            ViewModel<StoryNode> vm = (ViewModel<StoryNode>)new StoryNoteToViewModelConverter().Convert(Story, typeof(ViewModel<StoryNode>), null, null);
            vm.PropertyChanged += Story_PropertyChanged;
            DataContext = vm;
            Draw();
        }
        #endregion

        #region [Dependency Properties]

        #region Story
        /// <summary>
        /// The backing field for the Story dependency property.
        /// </summary>
        public static readonly DependencyProperty StoryProperty = DependencyProperty.Register("Story", typeof(StoryNode), typeof(NodeView), new PropertyMetadata(null, OnStoryPropertyChanged));

        /// <summary>
        /// Called when the Story depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnStoryPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            NodeView Sender = o as NodeView;
            Sender.OnStoryChanged((StoryNode)e.OldValue, (StoryNode)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the Story.
        /// </summary>
        /// <value>The layers.</value>
        public StoryNode Story
        {
            get
            {
                return (StoryNode)GetValue(StoryProperty);
            }
            set
            {
                SetValue(StoryProperty, value);
            }
        }

        /// <summary>
        /// Called when the Story dependency property changes.
        /// </summary>
        /// <param name="OldValue">The old value.</param>
        /// <param name="NewValue">The new value.</param>
        protected virtual void OnStoryChanged(StoryNode OldValue, StoryNode NewValue)
        {
            Draw();
        }
        #endregion

        #region IsSelected
        /// <summary>
        /// The backing field for the IsSelected dependency property.
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(NodeView), new PropertyMetadata(false, OnIsSelectedPropertyChanged));

        /// <summary>
        /// Called when the IsSelected depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnIsSelectedPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            NodeView Sender = o as NodeView;
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
            dash.StrokeDashArray = NewValue ? dotted : solid;
        }
        #endregion

        #region Hovering
        /// <summary>
        /// The backing field for the Hovering dependency property.
        /// </summary>
        public static readonly DependencyProperty HoveringProperty = DependencyProperty.Register("Hovering", typeof(HoverState), typeof(NodeView), new PropertyMetadata(HoverState.NOT, OnHoveringPropertyChanged));

        /// <summary>
        /// Called when the Hovering depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnHoveringPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            NodeView Sender = o as NodeView;
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
            switch (NewValue)
            {
                case HoverState.NOT:
                    dash.Stroke = new SolidColorBrush(Colors.Black);
                    break;
                case HoverState.SELF:
                    dash.Stroke = new SolidColorBrush(Colors.Blue);
                    break;
                case HoverState.SOURCE:
                    dash.Stroke = new SolidColorBrush(Colors.Red);
                    break;
                case HoverState.DESTINATION:
                    dash.Stroke = new SolidColorBrush(Colors.Green);
                    break;
                case HoverState.SOURCE | HoverState.DESTINATION:
                    dash.Stroke = new SolidColorBrush(Colors.Purple);
                    break;
            }
        }
        #endregion

        #region BorderThickness
        /// <summary>
        /// The backing field for the BorderThickness dependency property.
        /// </summary>
        public static new readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register("BorderThickness", typeof(double), typeof(NodeView), new PropertyMetadata(1d, OnBorderThicknessPropertyChanged));

        /// <summary>
        /// Called when the BorderThickness depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnBorderThicknessPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            NodeView Sender = o as NodeView;
            Sender.OnBorderThicknessChanged((double)e.OldValue, (double)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the BorderThickness.
        /// </summary>
        /// <value>The layers.</value>
        public new double BorderThickness
        {
            get
            {
                return (double)GetValue(BorderThicknessProperty);
            }
            set
            {
                SetValue(BorderThicknessProperty, value);
            }
        }

        /// <summary>
        /// Called when the BorderThickness dependency property changes.
        /// </summary>
        /// <param name="OldValue">The old value.</param>
        /// <param name="NewValue">The new value.</param>
        protected virtual void OnBorderThicknessChanged(double OldValue, double NewValue)
        {
            border.BorderThickness = new Thickness(NewValue);
            dash.StrokeThickness = NewValue;
        }
        #endregion

        #endregion

        #region [Methods]

        #region Draw
        private void Draw()
        {
            dash.StrokeDashArray = IsSelected ? dotted : solid;
            txtTitle.Text = Story.Title;
        }
        #endregion

        #region Story_PropertyChanged
        private void Story_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Draw();
        }
        #endregion

        #endregion
    }
}
