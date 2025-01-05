using Cysharp.Threading.Tasks;
using DG.Tweening;

public static class DOTweenExtensions
{
    // Converts a Tween's completion into a UniTask for awaiting.
    public static UniTask ToUniTask(this Tween tween)
    {
        var tcs = new UniTaskCompletionSource();
        tween.OnComplete(() => tcs.TrySetResult());
        return tcs.Task;
    }

    // Converts a Sequence's completion into a UniTask for awaiting.
    public static UniTask ToUniTask(this Sequence sequence)
    {
        var tcs = new UniTaskCompletionSource();
        sequence.OnComplete(() => tcs.TrySetResult());
        return tcs.Task;
    }
}