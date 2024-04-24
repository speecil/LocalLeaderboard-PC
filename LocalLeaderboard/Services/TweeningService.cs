using System;
using System.Collections.Generic;
using TMPro;
using Tweening;
using UnityEngine;
using Zenject;
using EaseTyped = EaseType;
namespace LocalLeaderboard.Services
{
    internal class TweeningService
    {
        [Inject] private TimeTweeningManager _tweeningManager;
        private HashSet<Transform> activeRotationTweens = new HashSet<Transform>();

        public void RotateTransform(Transform transform, float rotationAmount, float time, Action callback = null)
        {
            if (activeRotationTweens.Contains(transform)) return;
            float startRotation = transform.rotation.eulerAngles.z;
            float endRotation = startRotation + rotationAmount;

            Tween tween = new FloatTween(startRotation, endRotation, (float u) =>
            {
                transform.rotation = Quaternion.Euler(0f, 0f, u);
            }, 0.1f, EaseTyped.Linear, 0f);
            tween.onCompleted = () =>
            {
                callback?.Invoke();
                activeRotationTweens.Remove(transform);
            };
            tween.onKilled = () =>
            {
                if (transform != null) transform.rotation = Quaternion.Euler(0f, 0f, endRotation);
                callback?.Invoke();
                activeRotationTweens.Remove(transform);
            };
            activeRotationTweens.Add(transform);
            _tweeningManager.AddTween(tween, transform);
        }

        public void FadeText(TextMeshProUGUI text, bool fadeIn, float time)
        {
            float startAlpha = fadeIn ? 0f : 1f;
            float endAlpha = fadeIn ? 1f : 0f;

            Tween tween = new FloatTween(startAlpha, endAlpha, (float u) =>
            {
                text.color = text.color.ColorWithAlpha(u);
            }, 0.4f, EaseTyped.Linear, 0f);
            tween.onCompleted = () =>
            {
                if (text == null) return;
                text.gameObject.SetActive(fadeIn);
            };
            tween.onKilled = () =>
            {
                if (text == null) return;
                text.gameObject.SetActive(fadeIn);
                text.color = text.color.ColorWithAlpha(endAlpha);
            };
            text.gameObject.SetActive(true);
            _tweeningManager.AddTween(tween, text);
        }
    }
}
