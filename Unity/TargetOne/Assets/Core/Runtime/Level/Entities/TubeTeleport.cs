using Core;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Assertions;

public class TubeTeleport : MonoBehaviour
{
    [Required]
    public TubeTeleport ConnectedTube;
    public Transform WaypointsContainer;
    [Required]
    public TubeTeleportAnimator TubeAnimator;
    public Tile Tile;

    private void OnDrawGizmos()
    {
        if (ConnectedTube != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, ConnectedTube.transform.position);
            Gizmos.DrawSphere(ConnectedTube.transform.position, 0.2f);
        }
    }

    public void StartTeleporting()
    {
        Assert.IsNotNull(ConnectedTube);
        Teleporting(null).Forget();
    }

    public async UniTask DisappearWalker(TileWalker walker)
    {
        var playerController = walker.PlayerController;
        Assert.IsNotNull(playerController);
        
        // Disable walker interactions
        playerController.EnableInteractions(false);
        
        // Disable hovering
        playerController.Hover.SetActive(false);
        
        // Disable walker logic
        walker.SetActive(false);
        
        // Play animations of player disappearing
        await playerController.CharacterAnimator.PlayDisappearInTube(walker.CurrentTile);

        // Disable walker visual (including attached effects)
        playerController.EnableVisual(false);
        
        // Detach from tile (to restart triggers for later reuse)
        walker.DetachFromTile();
    }
    
    public async UniTask RevealWalker(TileWalker walker)
    {
        // Get player controller from tile walker
        var playerController = walker.PlayerController;
        Assert.IsNotNull(playerController);
        
        // Disable connected teleport trigger for current entry
        ConnectedTube.gameObject.GetComponent<TriggerTubeTeleport>().IsWalkerEntered = true;
        
        // Attach walker to tile
        walker.PutOnTile(ConnectedTube.Tile);
        
        // Play connected tube animation (note: the starting tube animation is played by trigger handler)
        var connectedTeleport = ConnectedTube.gameObject.GetComponent<TubeTeleport>();
        Assert.IsNotNull(connectedTeleport);
        connectedTeleport.TubeAnimator.AnimateThrowUp();
        
        // Enable visual
        playerController.EnableVisual(true);

        // Play character appearing animation
        await playerController.CharacterAnimator.PlayAppearFromTube(walker.CurrentTile, playerController.Hover.NormalValues.height);
        
        // Enable hovering
        playerController.Hover.SetActive(true);
        
        walker.GoToState(TileWalker.State.Walking);
        playerController.EnableInteractions(true);
        walker.SetActive(true);
        
    }

    public async UniTask NavigateTubeWaypoints(TileWalker walker)
    {
        await UniTask.Delay(1000); // 1 second delay
    }
    
    private async UniTask Teleporting(TileWalker walker)
    {
        Debug.Log("Teleporting process started...");
        
        await UniTask.Yield(); // in case we are still inside the Walker.Update (for Walker.AutoWalk)
        
        if (walker == null)
            walker = GetComponentInChildren<TileWalker>();
        Assert.IsNotNull(walker);
        
        
        await DisappearWalker(walker);
        await NavigateTubeWaypoints(walker);
        await RevealWalker(walker);

        
        Debug.Log("Teleporting process finished.");
    }
}