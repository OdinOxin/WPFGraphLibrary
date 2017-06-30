using System.Collections.Generic;

namespace MirrorConfigClient.ChangeTracking
{
    public interface ChangeTracker
    {
        void AddUndoable(Transaction Undoable);

        void ForgetEverything();
    }
}
