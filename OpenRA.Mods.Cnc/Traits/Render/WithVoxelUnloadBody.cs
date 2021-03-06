#region Copyright & License Information
/*
 * Copyright 2007-2017 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.Cnc.Traits.Render
{
	// TODO: This trait is hacky and should go away as soon as we support granting a condition on docking, in favor of toggling two regular WithVoxelBodies
	public class WithVoxelUnloadBodyInfo : ITraitInfo, IRenderActorPreviewVoxelsInfo, Requires<RenderVoxelsInfo>, IAutoSelectionSizeInfo, IAutoRenderSizeInfo
	{
		[Desc("Voxel sequence name to use when docked to a refinery.")]
		public readonly string UnloadSequence = "unload";

		[Desc("Voxel sequence name to use when undocked from a refinery.")]
		public readonly string IdleSequence = "idle";

		[Desc("Defines if the Voxel should have a shadow.")]
		public readonly bool ShowShadow = true;

		public object Create(ActorInitializer init) { return new WithVoxelUnloadBody(init.Self, this); }

		public IEnumerable<ModelAnimation> RenderPreviewVoxels(
			ActorPreviewInitializer init, RenderVoxelsInfo rv, string image, Func<WRot> orientation, int facings, PaletteReference p)
		{
			var body = init.Actor.TraitInfo<BodyOrientationInfo>();
			var model = init.World.ModelCache.GetModelSequence(image, IdleSequence);
			yield return new ModelAnimation(model, () => WVec.Zero,
				() => new[] { body.QuantizeOrientation(orientation(), facings) },
				() => false, () => 0, ShowShadow);
		}
	}

	public class WithVoxelUnloadBody : IAutoSelectionSize, IAutoRenderSize
	{
		public bool Docked;

		readonly int2 size;

		public WithVoxelUnloadBody(Actor self, WithVoxelUnloadBodyInfo info)
		{
			var body = self.Trait<BodyOrientation>();
			var rv = self.Trait<RenderVoxels>();

			var idleModel = self.World.ModelCache.GetModelSequence(rv.Image, info.IdleSequence);
			rv.Add(new ModelAnimation(idleModel, () => WVec.Zero,
				() => new[] { body.QuantizeOrientation(self, self.Orientation) },
				() => Docked,
				() => 0, info.ShowShadow));

			// Selection size
			var rvi = self.Info.TraitInfo<RenderVoxelsInfo>();
			var s = (int)(rvi.Scale * idleModel.Size.Aggregate(Math.Max));
			size = new int2(s, s);

			var unloadModel = self.World.ModelCache.GetModelSequence(rv.Image, info.UnloadSequence);
			rv.Add(new ModelAnimation(unloadModel, () => WVec.Zero,
				() => new[] { body.QuantizeOrientation(self, self.Orientation) },
				() => !Docked,
				() => 0, info.ShowShadow));
		}

		int2 IAutoSelectionSize.SelectionSize(Actor self) { return size; }
		int2 IAutoRenderSize.RenderSize(Actor self) { return size; }
	}
}
