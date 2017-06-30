using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MirrorConfigBL.Story
{
    [DataContract]
    public class StoryConfig
    {
        [DataMember]
        public List<StoryNode> StoryNodes { get; set; }

    }
}
