

using DG.Tweening;


// goblin animations
public enum GoblinAnimations
{
    GoblinMove,
}




public static class TweenChecker
{
    public static bool IsTweenPlaying(string id)
    {

        if (DOTween.IsTweening(id)) return true;

        return false;
    }
}
