namespace Core
{
    public class TriggerTileEnter : TriggerBase
    {
        public void HitTriggerEnter(TileWalker tileWalker)
        {
            if (WillHit())
            {
                base.HitTrigger();
            }
        }
    }
}