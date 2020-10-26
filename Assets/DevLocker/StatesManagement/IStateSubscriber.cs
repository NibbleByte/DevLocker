
/// <summary>
/// Implement this indicating that your class wants to subscribe for the state change.
/// </summary>
public interface IStateSubscriber
{
	void SubscribeState();
	void UnsubscribeState();
}
