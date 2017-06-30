namespace MirrorConfigClient
{
    public interface INotifyPropertyChangedExtended
    {
        event PropertyChangedExtendedEventHandler PropertyChangedExtended;
    }

    public delegate void PropertyChangedExtendedEventHandler(object sender, PropertyChangedExtendedEventArgs e);
}
