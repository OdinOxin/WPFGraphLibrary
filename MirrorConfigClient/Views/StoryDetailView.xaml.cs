using MirrorConfigClient.ChangeTracking;
using MirrorConfigClient.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace MirrorConfigClient.Views
{
    /// <summary>
    /// Interaction logic for StoryNodeDetailView.xaml
    /// </summary>
    public partial class StoryDetailView : UserControl
    {
        #region [Needs]
        private bool SkipTracking = false;
        #endregion

        #region ctor
        public StoryDetailView()
        {
            InitializeComponent();
            DataContextChanged += StoryDetailView_DataContextChanged;
        }
        #endregion

        #region Events
        private void StoryDetailView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            StoryNodeViewModel vm = DataContext as StoryNodeViewModel;
            if (vm != null)
            {
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += delegate (object bwSender, DoWorkEventArgs bwE)
                {
                    txtTitle.Dispatcher.Invoke(() =>
                    {
                        if (txtTitle.Text == "Neuer Knoten")
                        {
                            txtTitle.Focus();
                            txtTitle.SelectAll();
                        }
                    });
                };
                bw.RunWorkerAsync();
                vm.PropertyChangedExtended += Vm_PropertyChangedExtended;
            }
        }

        private void Vm_PropertyChangedExtended(object sender, PropertyChangedExtendedEventArgs e)
        {
            StoryNodeViewModel vm = DataContext as StoryNodeViewModel;
            if (vm != null)
            {
                if (!SkipTracking)
                    ChangeTracker?.AddUndoable(new Transaction(
                        new Action(() =>
                        {
                            SkipTracking = true;
                            vm[e.PropertyName] = e.OldValue;
                            SkipTracking = false;
                        }),
                        new Action(() =>
                        {
                            SkipTracking = true;
                            vm[e.PropertyName] = e.NewValue;
                            SkipTracking = false;
                        }),
                        this, sender as Control
                    ));
            }
        }
        #endregion

        #region [Properties]

        #region ChangeTracker
        public ConfigClient ChangeTracker { get; set; }
        #endregion

        #endregion

        #region [DependencyProperties]

        #region IsContentEnabled
        /// <summary>
        /// The backing field for the IsContentEnabled dependency property.
        /// </summary>
        public static readonly DependencyProperty IsContentEnabledProperty = DependencyProperty.Register("IsContentEnabled", typeof(bool), typeof(StoryDetailView), new PropertyMetadata(true, OnIsContentEnabledPropertyChanged));

        /// <summary>
        /// Called when the IsContentEnabled depenency property changed.
        /// </summary>
        /// <param name="o">The dependency object that owns the dependency property.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnIsContentEnabledPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            StoryDetailView Sender = o as StoryDetailView;
            Sender.OnIsContentEnabledChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the IsContentEnabled.
        /// </summary>
        /// <value>The layers.</value>
        public bool IsContentEnabled
        {
            get
            {
                return (bool)GetValue(IsContentEnabledProperty);
            }
            set
            {
                SetValue(IsContentEnabledProperty, value);
            }
        }

        /// <summary>
        /// Called when the IsContentEnabled dependency property changes.
        /// </summary>
        /// <param name="OldValue">The old value.</param>
        /// <param name="NewValue">The new value.</param>
        protected virtual void OnIsContentEnabledChanged(bool OldValue, bool NewValue)
        {
            BaseGrid.IsEnabled = NewValue;
        }
        #endregion

        #endregion
    }
}
