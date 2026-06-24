using Microsoft.Xna.Framework.Graphics;
using DragonBones;

namespace DragonBones.MonoGame
{
    public class MonoGameFactory : BaseFactory
    {
        private readonly GraphicsDevice _graphicsDevice;

        public MonoGameFactory(GraphicsDevice graphicsDevice, DataParser dataParser = null) : base(dataParser)
        {
            _graphicsDevice = graphicsDevice;
        }

        protected override TextureAtlasData _BuildTextureAtlasData(TextureAtlasData textureAtlasData, object textureAtlas)
        {
            if (textureAtlasData == null)
            {
                textureAtlasData = BaseObject.BorrowObject<MonoGameTextureAtlasData>();
            }
            
            if (textureAtlasData != null && textureAtlas is Texture2D)
            {
                var monoGameTextureAtlasData = textureAtlasData as MonoGameTextureAtlasData;
                if (monoGameTextureAtlasData != null)
                {
                    monoGameTextureAtlasData.texture = textureAtlas as Texture2D;
                }
            }
            
            return textureAtlasData;
        }

        protected override Armature _BuildArmature(BuildArmaturePackage dataPackage)
        {
            var armature = BaseObject.BorrowObject<MonoGameArmature>();
            armature.Init(dataPackage.armature, armature, armature, this._dragonBones);
            return armature;
        }

        protected override Slot _BuildSlot(BuildArmaturePackage dataPackage, SlotData slotData, Armature armature)
        {
            var slot = BaseObject.BorrowObject<MonoGameSlot>();
            slot.Init(slotData, armature, slot, slot);
            return slot;
        }
    }
}
