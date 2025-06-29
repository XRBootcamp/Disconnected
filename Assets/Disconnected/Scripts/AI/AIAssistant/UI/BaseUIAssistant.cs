using UnityEngine;

public abstract class BaseUIAssistant : MonoBehaviour {
    public abstract void Bind(BaseAssistant assistant);
    public abstract void Unbind();
}
