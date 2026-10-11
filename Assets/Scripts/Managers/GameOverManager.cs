using UnityEngine;

// Reuses the tutorial component as the end-of-round presenter.
public class GameOverManager : MonoBehaviour
{
    public float presentationDelay = 1.4f;
    float showAt = float.PositiveInfinity;
    public bool Visible { get { return Time.unscaledTime >= showAt; } }
    public void ShowResult() { showAt = Time.unscaledTime + presentationDelay; }
}
