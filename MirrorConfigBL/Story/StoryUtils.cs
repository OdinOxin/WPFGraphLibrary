using MirrorConfigBL.Extender;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MirrorConfigBL.Story
{
    /// <summary>
    /// Utils for Story.
    /// This is implemented as Singelton.
    /// </summary>
    public class StoryUtils
    {
        #region [Needs]
        private static StoryUtils _instance;
        private Dictionary<Guid, List<Guid>> _previousStorys;
        #endregion

        #region [Ctor]
        private StoryUtils()
        {
            Stories = new Dictionary<Guid, StoryNode>();
            _previousStorys = new Dictionary<Guid, List<Guid>>();
        }
        #endregion

        #region [Properties]

        private Dictionary<Guid, StoryNode> Stories { get; set; }

        #endregion

        #region [Public Methods]
        public static StoryUtils GetUtilsInstance()
        {
            if (_instance == null)
                _instance = new StoryUtils();
            return _instance;
        }

        /// <summary>
        /// Gets all story nodes.
        /// </summary>
        /// <returns></returns>
        public List<StoryNode> GetAllStoryNodes()
        {
            return Stories.Values.ToList();
        }

        /// <summary>
        /// Gets the start stories.
        /// </summary>
        /// <returns></returns>
        public List<StoryNode> GetStartStories()
        {
            return Stories.Where(x => x.Value.IsStartStory).Select(x => x.Value).ToList();
        }

        /// <summary>
        /// Gets the story by ID.
        /// </summary>
        /// <param name="StoryID">The story identifier.</param>
        /// <returns></returns>
        public StoryNode GetStory(Guid StoryID)
        {
            StoryNode Result = null;
            Stories.TryGetValue(StoryID, out Result);
            return Result;
        }

        /// <summary>
        /// Gets the stories by IDs.
        /// </summary>
        /// <param name="StoryIDs">The story i ds.</param>
        /// <returns></returns>
        public List<StoryNode> GetStories(ICollection<Guid> StoryIDs)
        {
            return Stories.Where(x => StoryIDs.Contains(x.Key)).Select(x => x.Value).ToList();
        }

        /// <summary>
        /// Gets the previous story nodes.
        /// </summary>
        /// <param name="StoryID">The story identifier.</param>
        /// <returns></returns>
        public List<StoryNode> GetPreviousStoryNodes(Guid StoryID)
        {
            return GetStories(_previousStorys[StoryID]);
        }

        /// <summary>
        /// Gets the next story nodes.
        /// </summary>
        /// <param name="StoryID">The story identifier.</param>
        /// <returns></returns>
        public List<StoryNode> GetNextStoryNodes(Guid StoryID)
        {
            return GetStories(GetStory(StoryID).NextStoryIDs);
        }

        /// <summary>
        /// Adds the story edge.
        /// </summary>
        /// <param name="From">From.</param>
        /// <param name="To">To.</param>
        /// <returns></returns>
        public StoryNode AddStoryEdge(StoryNode From, StoryNode To)
        {
            return AddStoryEdge(From.StoryID, To.StoryID);
        }

        /// <summary>
        /// Adds the story edge.
        /// </summary>
        /// <param name="From">From.</param>
        /// <param name="To">To.</param>
        /// <returns></returns>
        public StoryNode AddStoryEdge(Guid From, Guid To)
        {
            var FromStory = GetStory(From);
            FromStory.NextStoryIDsIntern.AddIfNeeded(To);
            _previousStorys[To].AddIfNeeded(From);
            return FromStory;
        }

        /// <summary>
        /// Removes the story edge.
        /// </summary>
        /// <param name="From">From.</param>
        /// <param name="To">To.</param>
        /// <returns></returns>
        public StoryNode RemoveStoryEdge(StoryNode From, StoryNode To)
        {
            return RemoveStoryEdge(From.StoryID, To.StoryID);
        }

        /// <summary>
        /// Removes the story edge.
        /// </summary>
        /// <param name="From">From.</param>
        /// <param name="To">To.</param>
        /// <returns></returns>
        public StoryNode RemoveStoryEdge(Guid From, Guid To)
        {
            var FromStory = GetStory(From);
            FromStory.NextStoryIDsIntern.Remove(To);
            _previousStorys[To].Remove(From);
            return FromStory;
        }

        /// <summary>
        /// Adds the story node.
        /// </summary>
        /// <param name="NewStoryNode">The new story node.</param>
        /// <returns></returns>
        public StoryNode AddStoryNode(StoryNode NewStoryNode)
        {
            Stories.Add(NewStoryNode.StoryID, NewStoryNode);
            _previousStorys.Add(NewStoryNode.StoryID, new List<Guid>());
            return NewStoryNode;
        }

        /// <summary>
        /// Removes the story node.
        /// </summary>
        /// <param name="Story">The story.</param>
        public void RemoveStoryNode(StoryNode Story)
        {
            RemoveStoryNode(Story.StoryID);
        }

        /// <summary>
        /// Removes the story node.
        /// </summary>
        /// <param name="StoryID">The story identifier.</param>
        public void RemoveStoryNode(Guid StoryID)
        {
            foreach (var pre in GetPreviousStoryNodes(StoryID))
            {
                pre.NextStoryIDsIntern.Remove(StoryID);
            }
            Stories.Remove(StoryID);
            _previousStorys.Remove(StoryID);
        }

        public void ClearAll()
        {
            Stories.Clear();
            _previousStorys.Clear();
        }

        /// <summary>
        /// Creates the random story configuration in the current empty Singelton.
        /// </summary>
        /// <param name="Count">The count of new Stories.</param>
        /// <exception cref="System.NotSupportedException">CreateRandomStoryConfig not supports not empty existing Configs.</exception>
        public static void CreateRandomStoryConfig(int Count)
        {
            var Utils = StoryUtils.GetUtilsInstance();

            //Damit niemand ausversehen eine aktuelle Config überschreibt
            if (Utils.GetAllStoryNodes().IsNotNullOrEmpty())
                throw new NotSupportedException("CreateRandomStoryConfig not supports not empty existing Configs.");

            StoryNode nextStory;
            for (int i = 0; i < Count; i++)
            {
                nextStory = new StoryNode();
                nextStory.IsStartStory = i % 10 == 0;
                nextStory.Title = i.ToString();
                Utils.AddStoryNode(nextStory);
            }
            var rnd = new Random();

            var connectedNodeIDs = new List<Guid>();
            foreach (var item in Utils.GetAllStoryNodes())
            {
                connectedNodeIDs.AddRangeIfNeeded(item.NextStoryIDs);
            }
            var notConnectedNoedIds = Utils.GetAllStoryNodes().Select(x => x.StoryID).Except(connectedNodeIDs);
            foreach (var item in notConnectedNoedIds)
            {
                var story = Utils.GetStory(notConnectedNoedIds.ElementAt(rnd.Next(notConnectedNoedIds.Count())));
                Utils.AddStoryEdge(story.StoryID, item);
            }

        }
        #endregion

        #region [Private Methods]

        private void __InitPreviousStorys()
        {
            _previousStorys = new Dictionary<Guid, List<Guid>>();

            foreach (var item in Stories.Keys)
            {
                _previousStorys.Add(item, new List<Guid>());
            }

            foreach (var preStory in Stories)
            {
                foreach (var item in preStory.Value.NextStoryIDs)
                {
                    _previousStorys[item].AddIfNeeded(preStory.Key);
                }
            }

        }

        #endregion
    }
}
