using MirrorConfigBL.Extender;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace MirrorConfigBL.Story
{
    [DataContract]
    public class StoryNode
    {
        #region [Needs]
        private bool _IsStartStory;
        #endregion

        #region [Ctor]
        public StoryNode()
        {
            StoryID = Guid.NewGuid();
            NextStoryIDsIntern = new List<Guid>();
        }
        #endregion

        #region [Properties]
        [DataMember]
        public Guid StoryID { get; private set; }

        [DataMember(Name = nameof(NextStoryIDs))]
        internal List<Guid> NextStoryIDsIntern { get; set; }

        public ReadOnlyCollection<Guid> NextStoryIDs => NextStoryIDsIntern.AsReadOnly();

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public int Ranking { get; set; }

        [DataMember]
        public bool IsStartStory
        {
            get
            {
                return _IsStartStory;
            }
            set
            {
                _IsStartStory = value;
            }
        }

        public bool IsEndStory
        {
            get
            {
                return NextStoryIDs.IsNullOrEmpty();
            }
        }
        #endregion
    }
}
