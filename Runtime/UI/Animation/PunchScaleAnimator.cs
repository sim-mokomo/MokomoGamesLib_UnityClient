using DG.Tweening;
using UnityEngine;

namespace MokomoGamesLib.Runtime.UI.Animation
{
    public class PunchScaleAnimator : MonoBehaviour
    {
        [SerializeField] private float punchScale;
        [SerializeField] private float animDuration;

        public void Anim()
        {
            transform.DOPunchScale(Vector3.one * punchScale, animDuration);
        }
    }
}