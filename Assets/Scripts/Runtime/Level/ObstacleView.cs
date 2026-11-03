using System;
using System.Collections;
using BallGame.Common;
using UnityEngine;

namespace BallGame.Level
{
    public sealed class ObstacleView : MonoBehaviour
    {
        // Unity capsule mesh is 2 units tall and 1 unit wide at scale 1.
        private const float CapsuleMeshHeight = 2f;

        private const float InfectedSwell = 1.15f;

        [SerializeField]
        private Material _infectedMaterial;

        [SerializeField]
        private MeshRenderer _renderer;

        private Vector3 _baseScale;

        private Action<ObstacleView> _exploded;

        public void Infect(float delay, float explodeDelay)
        {
            StartCoroutine(InfectRoutine(delay, explodeDelay));
        }

        public void Setup(Vector2 position, float radius, float height, Action<ObstacleView> exploded)
        {
            _exploded = exploded;
            _baseScale = new Vector3(radius * 2f, height / CapsuleMeshHeight, radius * 2f);

            transform.SetLocalPositionAndRotation(position.ToXZ(height * 0.5f), Quaternion.identity);
            transform.localScale = _baseScale;
        }

        private IEnumerator InfectRoutine(float delay, float explodeDelay)
        {
            yield return new WaitForSeconds(delay);

            _renderer.sharedMaterial = _infectedMaterial;

            for (var time = 0f; time < explodeDelay; time += Time.deltaTime)
            {
                transform.localScale = _baseScale * Mathf.Lerp(1f, InfectedSwell, time / explodeDelay);
                yield return null;
            }

            gameObject.SetActive(false);
            _exploded?.Invoke(this);
        }
    }
}
