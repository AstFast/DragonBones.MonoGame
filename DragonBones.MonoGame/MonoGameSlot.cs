using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using DragonBones;
using System;
using System.Collections.Generic;
using MGBlendState = Microsoft.Xna.Framework.Graphics.BlendState;

namespace DragonBones.MonoGame
{
    public class MonoGameSlot : Slot
    {
        private Texture2D _texture;
        private Microsoft.Xna.Framework.Rectangle _sourceRect;
        private MonoGameArmature _proxy;

        // Cached render state from _UpdateColor / _UpdateBlendMode
        private Color _cachedColor = Color.White;
        private BlendMode _cachedBlendMode = BlendMode.Normal;
        private bool _textureRotated;

        // Mesh / FFD deformation
        private bool _isMeshDisplay;
        private VertexPositionColorTexture[] _meshVertices;
        private short[] _meshIndices;
        private int _meshVertexCount;
        private int _meshTriangleCount;
        // Cached BasicEffect for mesh drawing
        private static BasicEffect _meshEffect;

        // Exposed for MonoGameArmature blend-mode grouping
        internal BlendMode CachedBlendMode => _cachedBlendMode;
        internal bool IsMeshDisplay => _isMeshDisplay;

        protected override void _OnClear()
        {
            base._OnClear();
            _texture = null;
            _sourceRect = Microsoft.Xna.Framework.Rectangle.Empty;
            _proxy = null;
            _cachedColor = Color.White;
            _cachedBlendMode = BlendMode.Normal;
            _textureRotated = false;
            _isMeshDisplay = false;
            _meshVertices = null;
            _meshIndices = null;
            _meshVertexCount = 0;
            _meshTriangleCount = 0;
        }

        protected override void _InitDisplay(object value, bool isRetain)
        {
        }

        protected override void _DisposeDisplay(object value, bool isRelease)
        {
        }

        protected override void _OnUpdateDisplay()
        {
            _proxy = _armature as MonoGameArmature;
        }

        protected override void _AddDisplay()
        {
        }

        protected override void _ReplaceDisplay(object value)
        {
        }

        protected override void _RemoveDisplay()
        {
        }

        protected override void _UpdateZOrder()
        {
            // Z-order sorting is handled by the Armature via _slots.Sort() in AdvanceTime().
            // The _zOrder value on each slot is already updated by Armature._SortZOrder().
        }

        protected override void _UpdateColor()
        {
            _cachedColor = new Color(
                (byte)(this._colorTransform.redMultiplier * 255),
                (byte)(this._colorTransform.greenMultiplier * 255),
                (byte)(this._colorTransform.blueMultiplier * 255),
                (byte)(this._colorTransform.alphaMultiplier * 255)
            );
        }

        protected override void _UpdateFrame()
        {
            if (_textureData != null)
            {
                var textureAtlasData = _textureData.parent as MonoGameTextureAtlasData;
                if (textureAtlasData != null && textureAtlasData.texture != null)
                {
                    _texture = textureAtlasData.texture;
                    var region = _textureData.region;
                    int w = (int)region.width;
                    int h = (int)region.height;
                    _textureRotated = _textureData.rotated && _textureData.frame == null;
                    if (_textureRotated)
                    {
                        // Swap width/height for rotated textures packed in atlas
                        int tmp = w;
                        w = h;
                        h = tmp;
                    }
                    _sourceRect = new Microsoft.Xna.Framework.Rectangle(
                        (int)region.x,
                        (int)region.y,
                        w,
                        h
                    );
                }
            }
            else
            {
                _texture = null;
                _sourceRect = Microsoft.Xna.Framework.Rectangle.Empty;
                _textureRotated = false;
            }
        }

        protected override void _UpdateMesh()
        {
            if (_deformVertices == null || _deformVertices.verticesData == null)
            {
                _isMeshDisplay = false;
                return;
            }

            var verticesData = _deformVertices.verticesData;
            var data = verticesData.data;
            var intArray = data.intArray;
            var floatArray = data.floatArray;
            var deformVertices = _deformVertices.vertices;

            int offset = verticesData.offset;
            int vertexCount = intArray[offset + (int)BinaryOffset.MeshVertexCount];
            int triangleCount = intArray[offset + (int)BinaryOffset.MeshTriangleCount];
            int floatOffset = intArray[offset + (int)BinaryOffset.MeshFloatOffset];
            int weightOffset = intArray[offset + (int)BinaryOffset.MeshWeightOffset];

            var weight = verticesData.weight;
            bool isSkinned = weight != null;

            _meshVertexCount = vertexCount;
            _meshTriangleCount = triangleCount;
            _isMeshDisplay = true;

            // Allocate or reuse buffers
            if (_meshVertices == null || _meshVertices.Length < vertexCount)
                _meshVertices = new VertexPositionColorTexture[vertexCount];
            int indexCount = triangleCount * 3;
            if (_meshIndices == null || _meshIndices.Length < indexCount)
                _meshIndices = new short[indexCount];

            // Read triangle indices (3 consecutive shorts per triangle)
            for (int i = 0; i < indexCount; i++)
            {
                _meshIndices[i] = (short)intArray[offset + (int)BinaryOffset.MeshVertexIndices + i];
            }

            var region = _textureData.region;
            var atlasParent = _textureData.parent;
            int atlasW = atlasParent != null ? (int)atlasParent.width : 256;
            int atlasH = atlasParent != null ? (int)atlasParent.height : 256;

            float colR = this._colorTransform.redMultiplier;
            float colG = this._colorTransform.greenMultiplier;
            float colB = this._colorTransform.blueMultiplier;
            float colA = this._colorTransform.alphaMultiplier;
            Color vertexColor = new Color(colR, colG, colB, colA);

            if (isSkinned)
            {
                // Skinned mesh: weighted bone transforms
                var bones = _deformVertices.bones;
                int weightBoneCount = intArray[weightOffset + (int)BinaryOffset.WeigthBoneCount];
                int weightFloatOffset = intArray[weightOffset + (int)BinaryOffset.WeigthFloatOffset];
                int weightBoneIndicesOffset = weightOffset + (int)BinaryOffset.WeigthBoneIndices;

                for (int i = 0; i < vertexCount; i++)
                {
                    float baseX = floatArray[floatOffset + i * 2];
                    float baseY = floatArray[floatOffset + i * 2 + 1];
                    float defX = deformVertices != null ? deformVertices[i * 2] : 0;
                    float defY = deformVertices != null ? deformVertices[i * 2 + 1] : 0;

                    float worldX = 0, worldY = 0;

                    // Apply weighted bone transforms
                    int boneIndicesStart = weightBoneIndicesOffset + i * (weightBoneCount + 1);
                    int boneCount = intArray[boneIndicesStart];
                    if (boneCount == 0)
                    {
                        // No bones — use identity transform (rare)
                        worldX = baseX + defX;
                        worldY = baseY + defY;
                    }
                    else
                    {
                        float totalWeight = 0;
                        for (int b = 0; b < boneCount; b++)
                        {
                            int boneIndex = intArray[boneIndicesStart + 1 + b];
                            float w = floatArray[weightFloatOffset + i * weightBoneCount + b];
                            totalWeight += w;

                            if (boneIndex >= 0 && boneIndex < bones.Count && bones[boneIndex] != null)
                            {
                                var boneMatrix = bones[boneIndex].globalTransformMatrix;
                                float tx = boneMatrix.a * (baseX + defX) + boneMatrix.c * (baseY + defY) + boneMatrix.tx;
                                float ty = boneMatrix.b * (baseX + defX) + boneMatrix.d * (baseY + defY) + boneMatrix.ty;
                                worldX += tx * w;
                                worldY += ty * w;
                            }
                        }

                        // Normalize if weights don't sum to exactly 1
                        if (totalWeight > 0 && Math.Abs(totalWeight - 1.0f) > 0.0001f)
                        {
                            worldX /= totalWeight;
                            worldY /= totalWeight;
                        }
                    }

                    float u = (region.x + (i < vertexCount ? (float)i / (vertexCount - 1) * region.width : 0)) / atlasW;
                    float v = (region.y + (i < vertexCount ? (float)i / (vertexCount - 1) * region.height : 0)) / atlasH;

                    _meshVertices[i] = new VertexPositionColorTexture(
                        new Vector3(worldX, worldY, 0),
                        vertexColor,
                        new Vector2((region.x + (worldX - region.x) % region.width) / atlasW,
                                    (region.y + (worldY - region.y) % region.height) / atlasH)
                    );
                }

                // Skinned mesh vertices are already in world space; no _UpdateTransform needed
            }
            else
            {
                // Non-skinned (FFD): compute local-space deformed vertices + UV from floatArray
                for (int i = 0; i < vertexCount; i++)
                {
                    float baseX = floatArray[floatOffset + i * 2];
                    float baseY = floatArray[floatOffset + i * 2 + 1];
                    float defX = deformVertices != null ? deformVertices[i * 2] : 0;
                    float defY = deformVertices != null ? deformVertices[i * 2 + 1] : 0;

                    float localX = baseX + defX;
                    float localY = baseY + defY;

                    // Normalize UV from region relative to atlas
                    float u = (region.x + localX) / atlasW;
                    float v = (region.y + localY) / atlasH;

                    _meshVertices[i] = new VertexPositionColorTexture(
                        new Vector3(localX, localY, 0),
                        vertexColor,
                        new Vector2(u, v)
                    );
                }
                // _UpdateTransform will apply globalTransformMatrix
            }
        }

        protected override void _UpdateTransform()
        {
            if (!_isMeshDisplay || _meshVertices == null || _meshVertexCount == 0)
                return;

            // Non-skinned FFD: apply global transform matrix to all vertices
            var matrix = this.globalTransformMatrix;
            for (int i = 0; i < _meshVertexCount; i++)
            {
                var v = _meshVertices[i];
                float tx = matrix.a * v.Position.X + matrix.c * v.Position.Y + matrix.tx;
                float ty = matrix.b * v.Position.X + matrix.d * v.Position.Y + matrix.ty;
                _meshVertices[i].Position = new Vector3(tx, ty, 0);
            }
        }

        protected override void _IdentityTransform()
        {
            // Mesh vertices are re-computed each frame; no identity transform needed
        }

        internal override void _UpdateVisible()
        {
            _visible = _visible && _parent != null && _parent.visible;
        }

        internal override void _UpdateBlendMode()
        {
            _cachedBlendMode = this._blendMode;
            // Note: MonoGame SpriteBatch uses a single BlendState per Begin/End pair.
            // Per-slot blend modes require batching by blend mode (end/begin between groups)
            // or using a custom SpriteSortMode / custom shader.
            // For now, the blend mode is stored and available for external sorting/drawing.
        }

        public void Render(SpriteBatch spriteBatch)
        {
            if (!visible || _proxy == null)
                return;

            if (_isMeshDisplay && _meshVertices != null && _meshVertexCount > 0 && _texture != null)
            {
                RenderMesh(spriteBatch);
                return;
            }

            if (_texture == null)
                return;

            var matrix = this.globalTransformMatrix;

            float worldX = matrix.tx + _proxy.Position.X;
            float worldY = matrix.ty + _proxy.Position.Y;

            float scaleX = (float)Math.Sqrt(matrix.a * matrix.a + matrix.b * matrix.b);
            float scaleY = (float)Math.Sqrt(matrix.c * matrix.c + matrix.d * matrix.d);

            float rotation = (float)Math.Atan2(matrix.b, matrix.a);
            if (_textureRotated)
            {
                rotation += MathHelper.PiOver2;
            }

            // Use DragonBones-computed pivot (in texture-pixel space, pre-scaled).
            float pivotX = this._pivotX;
            float pivotY = this._pivotY;

            // Determine flip effects from the armature's flip flags.
            SpriteEffects effects = SpriteEffects.None;
            if (_armature.flipX) effects |= SpriteEffects.FlipHorizontally;
            if (_armature.flipY) effects |= SpriteEffects.FlipVertically;

            spriteBatch.Draw(
                _texture,
                new Vector2(worldX, worldY),
                _sourceRect,
                _cachedColor,
                rotation + _proxy.Rotation,
                new Vector2(pivotX, pivotY),
                new Vector2(scaleX * _proxy.Scale.X, scaleY * _proxy.Scale.Y),
                effects,
                0
            );
        }

        private void RenderMesh(SpriteBatch spriteBatch)
        {
            var gd = spriteBatch.GraphicsDevice;

            // Lazily create a shared BasicEffect for all mesh rendering
            if (_meshEffect == null || _meshEffect.IsDisposed)
            {
                _meshEffect = new BasicEffect(gd);
                _meshEffect.TextureEnabled = true;
                _meshEffect.VertexColorEnabled = true;
            }

            _meshEffect.Texture = _texture;

            // Match SpriteBatch's orthographic projection (top-left origin, Y-down)
            var vp = gd.Viewport;
            _meshEffect.World = Microsoft.Xna.Framework.Matrix.Identity;
            _meshEffect.View = Microsoft.Xna.Framework.Matrix.Identity;
            _meshEffect.Projection = Microsoft.Xna.Framework.Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, -1, 1);

            // Save existing render states to restore them after
            var prevBlendState = gd.BlendState;
            var prevDepthStencilState = gd.DepthStencilState;
            var prevRasterizerState = gd.RasterizerState;

            gd.BlendState = new MGBlendState()
            {
                ColorSourceBlend = Blend.SourceAlpha,
                ColorDestinationBlend = Blend.InverseSourceAlpha,
                AlphaSourceBlend = Blend.SourceAlpha,
                AlphaDestinationBlend = Blend.InverseSourceAlpha,
            };
            gd.DepthStencilState = DepthStencilState.None;
            gd.RasterizerState = RasterizerState.CullNone;

            foreach (var pass in _meshEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                gd.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _meshVertices,
                    0,
                    _meshVertexCount,
                    _meshIndices,
                    0,
                    _meshTriangleCount
                );
            }

            // Restore previous states
            gd.BlendState = prevBlendState;
            gd.DepthStencilState = prevDepthStencilState;
            gd.RasterizerState = prevRasterizerState;
        }
    }
}
