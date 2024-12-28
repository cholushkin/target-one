namespace Core
{
    public class TriggerTileExit : TriggerBase
    {
        public void HitTriggerExit(TileWalker tileWalker)
        {
            if (WillHit())
            {
                base.HitTrigger();
            }
        }
    }
}