using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MokomoGamesLib.Runtime.Barcode
{
    public class BarcodeDecodeView : MonoBehaviour
    {
        [SerializeField] private RawImage outputTexture;
        [SerializeField] private TextMeshProUGUI decodeResultText;
        public RawImage OutputTexture => outputTexture;

        public void ApplyOutputImage(Texture texture)
        {
            outputTexture.texture = texture;
        }

        public void ApplyDecodeResultText(string text)
        {
            decodeResultText.text = text;
        }
    }
}