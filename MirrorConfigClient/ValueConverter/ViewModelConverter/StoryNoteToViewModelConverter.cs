using MirrorConfigBL.Story;
using MirrorConfigClient.ViewModels;
using System;
using System.Globalization;

namespace MirrorConfigClient.ValueConverter
{
    public class StoryNoteToViewModelConverter : BusinessObjectToViewModelConverter<StoryNode>
    {
        public new object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StoryNode)
                return new StoryNodeViewModel((StoryNode)value);
            else
                return null;
        }

        public new object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StoryNodeViewModel)
                return ((StoryNodeViewModel)value).BusinessObject;
            else
                return null;
        }
    }
}
