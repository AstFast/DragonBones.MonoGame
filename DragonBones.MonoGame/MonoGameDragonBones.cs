using Microsoft.Xna.Framework.Graphics;
using DragonBones;

namespace DragonBones.MonoGame
{
    public class MonoGameDragonBones : DragonBones
    {
        public MonoGameDragonBones(IEventDispatcher<EventObject> eventManager) : base(eventManager)
        {}

        public void Update(float deltaTime)
        {
            AdvanceTime(deltaTime);
        }
    }
}
