using MirrorConfigBL.Extender;
using MirrorConfigBL.Story;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MirrorConfigClient.ViewModels
{
    #region TagList
    public class TagListModel
    {
        bool _isActive = false;
        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                if (value != _isActive)
                {
                    _isActive = value;
                    ActiveChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        public event EventHandler ActiveChanged;
    }

    public class TagListViewModel : ViewModel<TagListModel>
    {
        public TagListViewModel(TagListModel Model)
            : base(Model)
        {
        }
    }
    #endregion

    public class StoryNodeViewModel : ViewModel<StoryNode>
    {
        #region [ctor]
        public StoryNodeViewModel(StoryNode BusinessObject)
            : base(BusinessObject)
        {
            AddProperty(nameof(IsSelected), GetType().GetProperty(nameof(IsSelected)), this, false);
        }
        #endregion

        #region [Properties]
        public bool IsSelected { get; set; }
        #endregion
    }
}
