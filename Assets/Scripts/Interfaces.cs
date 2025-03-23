public interface IInteractable
{
    void Interact ();
}

// Any time the level needs reseting, we find objects that implement this interface.
public interface IResetable {
    void Reset();
}
