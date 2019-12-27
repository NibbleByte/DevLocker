using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DevLocker.Animations
{
	/// <summary>
	/// Used with AnimatorOverrideController. Provides better interface and handles some corner cases like
	/// starting up with Animator that already has AnimatorOverrideController set for runtime controller.
	/// Check AnimatorClipOverrides.CreateAnimatorClipOverrides() for more info.
	/// https://docs.unity3d.com/ScriptReference/AnimatorOverrideController.html
	/// </summary>
	public class AnimatorClipOverrides : IEnumerable<KeyValuePair<AnimationClip, AnimationClip>>
	{
		public AnimatorOverrideController Controller { get; private set; }

		private readonly Dictionary<AnimationClip, AnimationClip> _overridden = new Dictionary<AnimationClip, AnimationClip>();
		private readonly Dictionary<AnimationClip, List<AnimationClip>> _remaps = new Dictionary<AnimationClip, List<AnimationClip>>();

		public AnimatorClipOverrides(AnimatorOverrideController controller)
		{
			Controller = controller;

			var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
			Controller.GetOverrides(overrides);

			foreach (var pair in overrides) {
				if (pair.Value == null)
					continue;
				AddRemap(pair.Value, pair.Key);
			}
		}

		private void AddRemap(AnimationClip from, AnimationClip to)
		{
			List<AnimationClip> toClips;
			if (!_remaps.TryGetValue(from, out toClips)) {
				toClips = new List<AnimationClip>();
				_remaps.Add(from, toClips);
			}
			toClips.Add(to);
		}

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<KeyValuePair<AnimationClip, AnimationClip>>)this).GetEnumerator();

		IEnumerator<KeyValuePair<AnimationClip, AnimationClip>> IEnumerable<KeyValuePair<AnimationClip, AnimationClip>>.GetEnumerator()
		{
			var pairs = GetOverridableClips()
				.Select(c => new KeyValuePair<AnimationClip, AnimationClip>(c, this[c]));
			return pairs.GetEnumerator();
		}

		public AnimationClip this[AnimationClip clipKey]
		{
			get
			{
				AnimationClip val;
				_overridden.TryGetValue(clipKey, out val);
				return val;
			}
			set
			{
				if (clipKey == null) {
					Debug.LogError($"Trying to assign clip {value?.name} to null key.", Controller);
					return;
				}
				if (value == null) {
					_overridden.Remove(clipKey);
				} else {
					_overridden[clipKey] = value;
				}
			}
		}

		public void ApplyOverrides()
		{
			var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
			foreach (var kv in _overridden) {
				List<AnimationClip> remapClips;
				if (_remaps.TryGetValue(kv.Key, out remapClips)) {
					foreach (var remap in remapClips)
						overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(remap, kv.Value));
					continue;
				}
				overrides.Add(kv);
			}

			foreach (var remap in _remaps) {
				if (_overridden.ContainsKey(remap.Key))
					continue;
				foreach (var remapClip in remap.Value)
					overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(remapClip, remap.Key));
			}

			Controller.ApplyOverrides(overrides);
		}

		public void ApplyOverrides(AnimationClip[] clipKeys, AnimationClip[] overrideWith)
		{
			Debug.Assert(clipKeys.Length == overrideWith.Length);

			for(int i = 0; i < clipKeys.Length; ++i) {

				// It's ok.
				if (clipKeys[i] == null && overrideWith[i] == null)
					continue;

				this[clipKeys[i]] = overrideWith[i];
			}

			ApplyOverrides();
		}

		public IEnumerable<AnimationClip> GetOverridableClips()
		{
			return
				Controller.runtimeAnimatorController.animationClips
					.Except(_remaps.Values.Cast<AnimationClip>())
					.Union(_remaps.Keys)
					.Distinct();
		}

		public static void TransferOverrides(AnimatorOverrideController source, AnimatorOverrideController dest)
		{
			List<KeyValuePair<AnimationClip, AnimationClip>> sourceOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(source.overridesCount);

			source.GetOverrides(sourceOverrides);

			dest.ApplyOverrides(sourceOverrides);
		}

		public static AnimatorClipOverrides CreateAnimatorClipOverrides(Animator animator)
		{
			AnimatorClipOverrides animatorOverrides = null;

			if (animator.runtimeAnimatorController == null) {
				Debug.LogError($"Animator Overrides: animator at {animator.name} did not have controller.", animator);
				return null;
			}

			if (animator.runtimeAnimatorController is AnimatorOverrideController) {
				// Using the original AnimatorOverrideController asset will change the asset as well (in editor). Make a copy.
				var assetOverrider = (AnimatorOverrideController)animator.runtimeAnimatorController;
				var runtimeOverrider = new AnimatorOverrideController(assetOverrider.runtimeAnimatorController);

				TransferOverrides(assetOverrider, runtimeOverrider);

				animatorOverrides = new AnimatorClipOverrides(runtimeOverrider);
			}
			else {
				animatorOverrides = new AnimatorClipOverrides(new AnimatorOverrideController(animator.runtimeAnimatorController));
			}

			animator.runtimeAnimatorController = animatorOverrides.Controller;
			animatorOverrides.Controller.name = "Overridden: " + animatorOverrides.Controller.runtimeAnimatorController.name;

			return animatorOverrides;
		}
	}
}
